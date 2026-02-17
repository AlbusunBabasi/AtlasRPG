// AtlasRPG.Web/Controllers/RunController.cs
using System.Security.Claims;
using AtlasRPG.Application.Services;
using AtlasRPG.Core.Entities.GameData;
using AtlasRPG.Core.Entities.Runs;
using AtlasRPG.Core.ValueObjects;
using AtlasRPG.Infrastructure.Data;
using AtlasRPG.Web.Models.RunViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AtlasRPG.Web.Controllers
{
    [Authorize]
    public class RunController : Controller
    {
        private readonly RunService _runService;
        private readonly ApplicationDbContext _context;
        private readonly BotService _botService;
        private readonly CombatService _combatService;
        private readonly PveEncounterService _pveEncounterService;
        private readonly LootService _lootService;
        private readonly ShopService _shopService;
        private readonly StatCalculatorService _statCalculator;


        public RunController(
            RunService runService,
            ApplicationDbContext context,
            CombatService combatService,
            PveEncounterService pveEncounterService,
            BotService botService,
            LootService lootService,
            ShopService shopService,
            StatCalculatorService statCalculatorService)
        {
            _runService = runService;
            _context = context;
            _combatService = combatService;
            _pveEncounterService = pveEncounterService;
            _botService = botService;
            _lootService = lootService;
            _shopService = shopService;
            _statCalculator = statCalculatorService;
        }

        // GET: /Run/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRunViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "User not authenticated";
                return RedirectToAction("Login", "Account");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                TempData["Error"] = $"User not found: '{userId}'";
                ModelState.AddModelError("", "User not found in database.");
                return View(model);
            }

            try
            {
                var run = await _runService.CreateNewRun(userId, model.Race!.Value, model.Class!.Value);
                TempData["Success"] = "Run created successfully!";
                return RedirectToAction("TurnHub", new { id = run.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // GET: /Run/TurnHub/{id}
        public async Task<IActionResult> TurnHub(Guid id)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var run = await _context.Runs
                .Include(r => r.Turns)
                .Include(r => r.Inventory)
                    .ThenInclude(i => i.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment)
                    .ThenInclude(e => e.Weapon)
                    .ThenInclude(w => w!.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment)
                    .ThenInclude(e => e.Armor)
                    .ThenInclude(a => a!.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment)
                    .ThenInclude(e => e.Belt)
                    .ThenInclude(b => b!.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment)
                    .ThenInclude(e => e.Offhand)
                    .ThenInclude(o => o!.Item)
                    .ThenInclude (i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .Include(r => r.AllocatedPassives)  // ← ekle
                .AsSplitQuery()
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (run == null)
                return NotFound();

            if (!run.IsActive)
                return RedirectToAction("Summary", new { id = run.Id });

            var currentTurn = await _runService.GetCurrentTurn(run.Id);

            if (currentTurn == null)
                return RedirectToAction("Summary", new { id = run.Id });

            // Get available skills for equipped weapon
            var availableSkills = new List<SkillDefinition>();
            if (run.Equipment?.Weapon?.Item?.WeaponType != null)
            {
                availableSkills = await _context.SkillDefinitions
                    .Where(s => s.WeaponType == run.Equipment.Weapon.Item.WeaponType)
                    .Where(s => s.RequiredLevel <= run.CurrentLevel)
                    .ToListAsync();
            }

            var shopItems = await _shopService.GenerateShopInventory(run);
            foreach (var si in shopItems)
            {
                await _context.Entry(si)
                    .Collection(i => i.Affixes)
                    .Query()
                    .Include(a => a.AffixDefinition)
                    .LoadAsync();
            }
            ViewBag.ShopItems = shopItems;
            ViewBag.ShopPrices = shopItems.ToDictionary(i => i.Id, i => _shopService.CalculateItemPrice(i));

            // ── Stats hesapla ────────────────────────────────────────────────
            var baseStat = await _context.BaseStatDefinitions
                .FirstOrDefaultAsync(b => b.Race == run.Race);

            CharacterStats calculatedStats = new();

            if (baseStat != null)
            {
                var allocatedIds = run.AllocatedPassives?
                    .Select(p => p.NodeId).ToList() ?? new List<string>();

                var allocatedDefs = (await _context.PassiveNodeDefinitions.ToListAsync())
                    .Where(nd => allocatedIds.Contains(nd.NodeId))
                    .ToList();

                calculatedStats = _statCalculator.CalculateRunStats(run, baseStat, allocatedDefs);
            }

            var lastSkillId = run.Turns
                .Where(t => t.IsCompleted && t.SelectedActiveSkillId.HasValue)
                .OrderByDescending(t => t.TurnNumber)
                .FirstOrDefault()
                ?.SelectedActiveSkillId;

            var viewModel = new TurnHubViewModel
            {
                Run = run,
                CurrentTurn = currentTurn,
                Equipment = run.Equipment,
                AvailableSkills = availableSkills,
                Inventory = run.Inventory.ToList(),
                LastSelectedSkillId = lastSkillId,   // ← YENİ
                Stats = calculatedStats,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartCombat(StartCombatViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please select a skill";
                return RedirectToAction("TurnHub", new { id = model.TurnId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Turn + Run birlikte yükle
            var turn = await _context.RunTurns
                .Include(t => t.Run)
                .FirstOrDefaultAsync(t => t.Id == model.TurnId && t.Run.UserId == userId);

            if (turn == null || turn.IsCompleted)
                return NotFound();

            // Skill doğrula
            var skill = await _context.SkillDefinitions.FindAsync(model.SelectedSkillId);
            if (skill == null)
            {
                TempData["Error"] = "Invalid skill selected";
                return RedirectToAction("TurnHub", new { id = turn.RunId });
            }

            turn.SelectedActiveSkillId = model.SelectedSkillId;

            // ✅ PVP: BotService → SimulateCombat (Run vs Run)
            // ✅ PVE: PveEncounterService → SimulatePveCombat (Run vs Monster)
            Core.Entities.Combat.CombatResult combatResult;

            if (turn.IsPvp)
            {
                var botRun = await _botService.GeneratePveBot(turn.TurnNumber, turn.Run.CurrentLevel);
                combatResult = await _combatService.SimulateCombat(turn.Run, botRun, model.SelectedSkillId);
                combatResult.OpponentUsername = $"{botRun.Race} {botRun.Class} (Bot)";
            }
            else
            {
                var monster = _pveEncounterService.GenerateMonster(turn.TurnNumber, turn.Run.CurrentLevel);
                combatResult = await _combatService.SimulatePveCombat(turn.Run, monster, model.SelectedSkillId);
                combatResult.OpponentUsername = monster.Name;
            }

            // Combat result'u kaydet
            combatResult.RunTurnId = turn.Id;
            _context.CombatResults.Add(combatResult);
            turn.CombatResultId = combatResult.Id;


            await _context.SaveChangesAsync();

            // ✅ isVictory kaynağı: combat sonucu — forma asla güvenme
            // Bu sayede "3 galibiyet = 3 can bitti" bug'ı ortadan kalkar
            await _runService.CompleteTurn(turn.Id, combatResult.IsVictory);

            // ── PVE LOOT (PVP'de item drop olmaz) ─────────────────────
            if (!turn.IsPvp)
            {
                var droppedItems = await _lootService.GeneratePveLoot(
                    turn.Run,
                    turn.TurnNumber,
                    combatResult.IsVictory);

                // Loot ID'lerini TempData'ya yaz → CombatResult action okur
                TempData["LootItemIds"] = string.Join(",",
                    droppedItems.Select(ri => ri.Id.ToString()));
            }

            return RedirectToAction("CombatResult", new { id = combatResult.Id });

        }


        [HttpGet]
        public async Task<IActionResult> CombatResult(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var combatResult = await _context.CombatResults
                .Include(c => c.Rounds)
                .Include(c => c.RunTurn)
                    .ThenInclude(t => t.Run)
                .FirstOrDefaultAsync(c => c.Id == id && c.RunTurn.Run.UserId == userId);

            if (combatResult == null) return NotFound();

            var sortedRounds = combatResult.Rounds.OrderBy(r => r.RoundNumber).ToList();
            combatResult.Rounds = sortedRounds;

            ViewBag.RunIsActive = combatResult.RunTurn.Run.IsActive;
            ViewBag.RunId = combatResult.RunTurn.Run.Id;
            ViewBag.IsPveTurn = !combatResult.RunTurn.IsPvp;

            // ── Loot (sadece PVE turn'de) ──────────────────────────
            var lootResult = new AtlasRPG.Application.Services.LootDropResult
            {
                IsPveLoot = !combatResult.RunTurn.IsPvp,
                IsVictory = combatResult.IsVictory,
            };

            if (!combatResult.RunTurn.IsPvp && TempData["LootItemIds"] is string lootIds
                && !string.IsNullOrEmpty(lootIds))
            {
                var ids = lootIds.Split(',')
                    .Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
                    .Where(g => g.HasValue)
                    .Select(g => g!.Value)
                    .ToList();

                var runItems = new List<RunItem>();
                foreach (var runItemId in ids)
                {
                    var ri = await _context.RunItems
                        .Include(ri => ri.Item)
                            .ThenInclude(i => i.Affixes)
                            .ThenInclude(a => a.AffixDefinition)
                        .FirstOrDefaultAsync(ri => ri.Id == runItemId);
                    if (ri != null) runItems.Add(ri);
                }

                lootResult.Items = runItems
                    .Select(LootItemDto.FromRunItem)
                    .ToList();
            }

            ViewBag.Loot = lootResult;

            return View(combatResult);
        }

        // ✅ Continue butonu bu action'a yönlendiriyor.
        // CompleteTurn işlemi artık StartCombat içinde yapılıyor —
        // burada sadece yönlendirme var.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContinueRun(Guid runId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var run = await _context.Runs
                .FirstOrDefaultAsync(r => r.Id == runId && r.UserId == userId);

            if (run == null)
                return NotFound();

            if (!run.IsActive)
                return RedirectToAction("Summary", new { id = run.Id });

            return RedirectToAction("TurnHub", new { id = run.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Summary(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Run + Turns + CombatResults + AllocatedPassives
            var run = await _context.Runs
                .Include(r => r.Turns)
                    .ThenInclude(t => t.CombatResult)
                .Include(r => r.AllocatedPassives)
                .FirstOrDefaultAsync(r => r.Id == id
                                       && r.UserId == userId
                                       && !r.IsActive);

            if (run == null)
                return NotFound();

            var completedTurns = run.Turns.Where(t => t.IsCompleted).ToList();
            var combatResults = completedTurns
                .Where(t => t.CombatResult != null)
                .Select(t => t.CombatResult!)
                .ToList();

            // ── Win / Loss ──────────────────────────────────────────
            var pvpTurns = completedTurns.Where(t => t.IsPvp).ToList();
            var pveTurns = completedTurns.Where(t => !t.IsPvp).ToList();

            int pvpWins = pvpTurns.Count(t => t.IsVictory == true);
            int pvpLosses = pvpTurns.Count(t => t.IsVictory == false);
            int pveWins = pveTurns.Count(t => t.IsVictory == true);
            int pveLosses = pveTurns.Count(t => t.IsVictory == false);

            double pvpWinRate = pvpTurns.Count > 0
                ? (double)pvpWins / pvpTurns.Count * 100 : 0;

            // ── Sudden Death Rate ───────────────────────────────────
            int suddenDeathCount = combatResults.Count(r => r.WasSuddenDeath);
            double suddenDeathRate = combatResults.Count > 0
                ? (double)suddenDeathCount / combatResults.Count * 100 : 0;

            // ── Avg Combat Rounds ───────────────────────────────────
            double avgRounds = combatResults.Count > 0
                ? combatResults.Average(r => r.TotalRounds) : 0;

            // ── Toplam Hasar / Crit ─────────────────────────────────
            decimal totalDmgDealt = combatResults.Sum(r => r.PlayerTotalDamageDealt);
            decimal totalDmgTaken = combatResults.Sum(r => r.PlayerTotalDamageTaken);
            int totalCrits = combatResults.Sum(r => r.PlayerCriticalHits);
            int totalHits = completedTurns.Count > 0 ? (int)avgRounds * completedTurns.Count : 0;

            // ── En çok kullanılan skill ─────────────────────────────
            // SelectedActiveSkillId → SkillDefinition
            var usedSkillIds = completedTurns
                .Where(t => t.SelectedActiveSkillId.HasValue)
                .Select(t => t.SelectedActiveSkillId!.Value)
                .ToList();

            string mostUsedSkillName = "—";
            int mostUsedSkillCount = 0;
            if (usedSkillIds.Any())
            {
                var mostUsedSkillId = usedSkillIds
                    .GroupBy(x => x)
                    .OrderByDescending(g => g.Count())
                    .First();

                var skillDef = await _context.SkillDefinitions
                    .FirstOrDefaultAsync(s => s.Id == mostUsedSkillId.Key);

                mostUsedSkillName = skillDef?.DisplayName ?? mostUsedSkillId.Key.ToString();
                mostUsedSkillCount = mostUsedSkillId.Count();
            }

            // ── En çok kullanılan weapon ────────────────────────────
            // SelectedWeaponId → RunItem → Item.WeaponType
            var usedWeaponIds = completedTurns
                .Where(t => t.SelectedWeaponId.HasValue)
                .Select(t => t.SelectedWeaponId!.Value)
                .Distinct()
                .ToList();

            string mostUsedWeaponType = "—";
            int mostUsedWeaponCount = 0;
            if (usedWeaponIds.Any())
            {
                // RunItem'ı Item ile çek
                var weaponItems = await _context.RunItems
                    .Include(ri => ri.Item)
                    .Where(ri => usedWeaponIds.Contains(ri.Id))
                    .ToListAsync();

                // Turn bazlı weapon type sayısı
                var weaponTypeCounts = completedTurns
                    .Where(t => t.SelectedWeaponId.HasValue)
                    .Select(t =>
                    {
                        var ri = weaponItems.FirstOrDefault(w => w.Id == t.SelectedWeaponId!.Value);
                        return ri?.Item.WeaponType?.ToString() ?? "Unknown";
                    })
                    .GroupBy(x => x)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                if (weaponTypeCounts != null)
                {
                    mostUsedWeaponType = weaponTypeCounts.Key;
                    mostUsedWeaponCount = weaponTypeCounts.Count();
                }
            }

            // ── Passive özeti ───────────────────────────────────────
            var allocatedNodeIds = run.AllocatedPassives.Select(p => p.NodeId).ToList();
            int allocatedNodeCount = allocatedNodeIds.Count;

            string? keystoneNodeId = null;
            if (allocatedNodeIds.Any())
            {
                var allKeystones = await _context.PassiveNodeDefinitions
                    .Where(nd => nd.NodeType == "Keystone")
                    .AsNoTracking()
                    .ToListAsync();

                keystoneNodeId = allKeystones
                    .FirstOrDefault(nd => allocatedNodeIds.Contains(nd.NodeId))
                    ?.DisplayName;
            }

            // ── Run tamamlanma durumu ───────────────────────────────
            bool didComplete = run.CurrentTurn >= 20 && pvpWins + pveWins > 0;

            // ── ViewBag ─────────────────────────────────────────────
            ViewBag.PvpWins = pvpWins;
            ViewBag.PvpLosses = pvpLosses;
            ViewBag.PveWins = pveWins;
            ViewBag.PveLosses = pveLosses;
            ViewBag.PvpWinRate = pvpWinRate.ToString("F1");
            ViewBag.TotalGold = run.Gold;
            ViewBag.FinalLevel = run.CurrentLevel;
            ViewBag.PvpPoints = run.PvpPoints;
            ViewBag.DidComplete = didComplete;

            ViewBag.SuddenDeathRate = suddenDeathRate.ToString("F1");
            ViewBag.SuddenDeathCount = suddenDeathCount;
            ViewBag.AvgRounds = avgRounds.ToString("F1");

            ViewBag.TotalDmgDealt = totalDmgDealt.ToString("F0");
            ViewBag.TotalDmgTaken = totalDmgTaken.ToString("F0");
            ViewBag.TotalCrits = totalCrits;

            ViewBag.MostUsedSkill = mostUsedSkillName;
            ViewBag.MostUsedSkillCount = mostUsedSkillCount;
            ViewBag.MostUsedWeapon = mostUsedWeaponType;
            ViewBag.MostUsedWeaponCount = mostUsedWeaponCount;

            ViewBag.AllocatedNodeCount = allocatedNodeCount;
            ViewBag.KeystoneNode = keystoneNodeId ?? "None";

            ViewBag.CompletedTurnCount = completedTurns.Count;

            return View(run);
        }

        [HttpGet]
        public async Task<IActionResult> AllocateStats(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var run = await _context.Runs
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId && r.IsActive);

            if (run == null)
                return NotFound();


            var viewModel = new AllocateStatsViewModel
            {
                RunId = run.Id,
                AvailablePoints = run.AvailableStatPoints,
                CurrentStrength = run.Strength,
                CurrentDexterity = run.Dexterity,
                CurrentAgility = run.Agility,
                CurrentIntelligence = run.Intelligence,
                CurrentVitality = run.Vitality,
                CurrentWisdom = run.Wisdom,
                CurrentLuck = run.Luck
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AllocateStats(AllocateStatsViewModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var run = await _context.Runs
                .FirstOrDefaultAsync(r => r.Id == model.RunId && r.UserId == userId && r.IsActive);

            if (run == null)
                return NotFound();

            int totalToSpend = model.StrengthToAdd + model.DexterityToAdd + model.AgilityToAdd +
                               model.IntelligenceToAdd + model.VitalityToAdd + model.WisdomToAdd +
                               model.LuckToAdd;

            if (totalToSpend > run.AvailableStatPoints)
            {
                ModelState.AddModelError("", "Not enough stat points available");
                model.AvailablePoints = run.AvailableStatPoints;
                return View(model);
            }

            if (totalToSpend == 0)
            {
                ModelState.AddModelError("", "You must spend at least 1 stat point");
                model.AvailablePoints = run.AvailableStatPoints;
                return View(model);
            }

            run.Strength += model.StrengthToAdd;
            run.Dexterity += model.DexterityToAdd;
            run.Agility += model.AgilityToAdd;
            run.Intelligence += model.IntelligenceToAdd;
            run.Vitality += model.VitalityToAdd;
            run.Wisdom += model.WisdomToAdd;
            run.Luck += model.LuckToAdd;

            run.AvailableStatPoints -= totalToSpend;


            await _context.SaveChangesAsync();

            TempData["Success"] = $"Allocated {totalToSpend} stat points successfully!";
            return RedirectToAction("TurnHub", new { id = run.Id });
        }
        // GET: /Run/PassiveTree/{id}
        [HttpGet]
        public async Task<IActionResult> PassiveTree(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var run = await _context.Runs
                .Include(r => r.AllocatedPassives)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId && r.IsActive);

            if (run == null) return NotFound();

            if (run.AvailableSkillPoints <= 0)
            {
                TempData["Info"] = "No passive skill points available";
                return RedirectToAction("TurnHub", new { id = run.Id });
            }

            var allNodes = await _context.PassiveNodeDefinitions
                .OrderBy(n => n.NodeId)
                .ToListAsync();

            var allocatedIds = run.AllocatedPassives
                .Select(p => p.NodeId)
                .ToHashSet();

            var nodeVMs = allNodes.Select(n =>
            {
                var prereqs = string.IsNullOrWhiteSpace(n.PrerequisiteNodeIds)
                    ? new List<string>()
                    : n.PrerequisiteNodeIds.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();

                bool prereqsMet = prereqs.Count == 0 || prereqs.Any(p => allocatedIds.Contains(p));
                bool levelMet = run.CurrentLevel >= n.RequiredLevel;
                bool isAllocated = allocatedIds.Contains(n.NodeId);

                return new PassiveNodeViewModel
                {
                    NodeId = n.NodeId,
                    DisplayName = n.DisplayName,
                    NodeType = n.NodeType,
                    Description = n.Description,
                    RequiredLevel = n.RequiredLevel,
                    PrerequisiteNodeIds = prereqs,
                    EffectJson = n.EffectJson,
                    IsAllocated = isAllocated,
                    IsAllocatable = !isAllocated && levelMet && prereqsMet && run.AvailableSkillPoints > 0
                };
            }).ToList();

            var viewModel = new PassiveTreeViewModel
            {
                RunId = run.Id,
                AvailablePoints = run.AvailableSkillPoints,
                CurrentLevel = run.CurrentLevel,
                Nodes = nodeVMs,
                AllocatedNodeIds = allocatedIds
            };

            return View(viewModel);
        }

        // POST: /Run/AllocateNode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AllocateNode(Guid runId, string nodeId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // ✅ AsNoTracking — validation için yükle, track etme
            var run = await _context.Runs
                .Include(r => r.AllocatedPassives)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == runId && r.UserId == userId && r.IsActive);

            if (run == null) return NotFound();

            // Guard: puan var mı?
            if (run.AvailableSkillPoints <= 0)
            {
                TempData["Error"] = "No skill points available";
                return RedirectToAction("PassiveTree", new { id = runId });
            }

            // Guard: zaten alındı mı?
            if (run.AllocatedPassives.Any(p => p.NodeId == nodeId))
            {
                TempData["Error"] = "Node already allocated";
                return RedirectToAction("PassiveTree", new { id = runId });
            }

            var nodeDef = await _context.PassiveNodeDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.NodeId == nodeId);

            if (nodeDef == null) return NotFound();

            // Guard: level yeterli mi?
            if (run.CurrentLevel < nodeDef.RequiredLevel)
            {
                TempData["Error"] = $"Requires level {nodeDef.RequiredLevel} (you are {run.CurrentLevel})";
                return RedirectToAction("PassiveTree", new { id = runId });
            }

            // Guard: prerequisite karşılandı mı?
            var allocatedIds = run.AllocatedPassives.Select(p => p.NodeId).ToHashSet();
            if (!string.IsNullOrEmpty(nodeDef.PrerequisiteNodeIds))
            {
                var prereqs = nodeDef.PrerequisiteNodeIds
                    .Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                if (prereqs.Any() && !prereqs.Any(p => allocatedIds.Contains(p)))
                {
                    TempData["Error"] = "Prerequisites not met";
                    return RedirectToAction("PassiveTree", new { id = runId });
                }
            }

            // ✅ DÜZELTME: RunPassiveNode'u doğrudan DbSet üzerinden ekle (navigation property bypass)
            _context.RunPassiveNodes.Add(new RunPassiveNode
            {
                RunId = runId,
                NodeId = nodeId,
                AllocatedAtLevel = run.CurrentLevel
            });

            // ✅ DÜZELTME: AvailableSkillPoints için ExecuteUpdate — doğrudan SQL UPDATE,
            // EF Change Tracker'ı tamamen bypass eder, 0-row problemi olmaz
            await _context.Runs
                .Where(r => r.Id == runId && r.AvailableSkillPoints > 0)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.AvailableSkillPoints, r => r.AvailableSkillPoints - 1));

            await _context.SaveChangesAsync(); // sadece RunPassiveNode INSERT'i

            TempData["Success"] = $"✅ Allocated: {nodeDef.DisplayName}";
            return RedirectToAction("PassiveTree", new { id = runId });
        }

        // POST: /Run/ResetPassiveTree
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassiveTree(Guid runId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var run = await _context.Runs
                .Include(r => r.AllocatedPassives)
                .FirstOrDefaultAsync(r => r.Id == runId && r.UserId == userId && r.IsActive);

            if (run == null) return NotFound();

            int refunded = run.AllocatedPassives.Count;
            run.AllocatedPassives.Clear();
            run.AvailableSkillPoints += refunded;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Tree reset — {refunded} point(s) refunded";
            return RedirectToAction("PassiveTree", new { id = runId });
        }

    }
}
