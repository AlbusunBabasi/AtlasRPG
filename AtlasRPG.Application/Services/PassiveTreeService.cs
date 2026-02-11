// AtlasRPG.Application/Services/PassiveTreeService.cs
using AtlasRPG.Core.Entities.Runs;
using AtlasRPG.Core.Entities.GameData;
using AtlasRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AtlasRPG.Application.Services
{
    public class PassiveTreeService
    {
        private readonly ApplicationDbContext _context;

        public PassiveTreeService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────
        // Tüm node tanımlarını + run state'ini birlikte döner
        // ─────────────────────────────────────────────────────
        public async Task<PassiveTreeData> GetTreeDataAsync(Guid runId)
        {
            var run = await _context.Runs
                .Include(r => r.AllocatedPassives)
                .FirstOrDefaultAsync(r => r.Id == runId)
                ?? throw new InvalidOperationException("Run not found.");

            var allNodes = await _context.PassiveNodeDefinitions
                .OrderBy(n => n.NodeId)
                .ToListAsync();

            var allocatedIds = run.AllocatedPassives
                .Select(p => p.NodeId)
                .ToHashSet();

            return new PassiveTreeData
            {
                RunId = runId,
                CurrentLevel = run.CurrentLevel,
                AvailablePoints = run.AvailableSkillPoints,
                AllNodes = allNodes,
                AllocatedIds = allocatedIds
            };
        }

        // ─────────────────────────────────────────────────────
        // Tek bir node'u allocate eder
        // ─────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> AllocateNodeAsync(
            Guid runId, string nodeId, string userId)
        {
            var run = await _context.Runs
                .Include(r => r.AllocatedPassives)
                .FirstOrDefaultAsync(r => r.Id == runId)
                ?? throw new InvalidOperationException("Run not found.");

            // Ownership check
            if (run.UserId != userId)
                return (false, "Unauthorized.");

            if (!run.IsActive)
                return (false, "This run is no longer active.");

            if (run.AvailableSkillPoints <= 0)
                return (false, "No skill points available.");

            // Node tanımını bul
            var nodeDef = await _context.PassiveNodeDefinitions
                .FirstOrDefaultAsync(n => n.NodeId == nodeId)
                ?? throw new InvalidOperationException($"Node '{nodeId}' not found.");

            // Level kontrolü
            if (run.CurrentLevel < nodeDef.RequiredLevel)
                return (false, $"Required level {nodeDef.RequiredLevel} not reached.");

            // Zaten alınmış mı?
            var alreadyAllocated = run.AllocatedPassives.Any(p => p.NodeId == nodeId);
            if (alreadyAllocated)
                return (false, "Node already allocated.");

            // Prerequisite kontrolü
            if (!string.IsNullOrWhiteSpace(nodeDef.PrerequisiteNodeIds))
            {
                var prereqs = nodeDef.PrerequisiteNodeIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim());

                foreach (var prereq in prereqs)
                {
                    if (!run.AllocatedPassives.Any(p => p.NodeId == prereq))
                        return (false, $"Prerequisite node '{prereq}' must be allocated first.");
                }
            }

            // Keystone: sadece 1 keystone
            if (nodeDef.NodeType == "Keystone")
            {
                var keystoneNodeIds = await _context.PassiveNodeDefinitions
                    .Where(nd => nd.NodeType == "Keystone")
                    .Select(nd => nd.NodeId)
                    .ToListAsync();

                bool hasKeystone = run.AllocatedPassives
                    .Any(p => keystoneNodeIds.Contains(p.NodeId));

                if (hasKeystone)
                    return (false, "You can only have one Keystone node per run.");
            }

            // Allocate
            var passiveNode = new RunPassiveNode
            {
                RunId = runId,
                NodeId = nodeId,
                AllocatedAtLevel = run.CurrentLevel
            };

            run.AvailableSkillPoints--;
            _context.RunPassiveNodes.Add(passiveNode);
            await _context.SaveChangesAsync();

            return (true, $"Node '{nodeDef.DisplayName}' allocated successfully.");
        }

        // ─────────────────────────────────────────────────────
        // Tree reset (V1: free)
        // ─────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> ResetTreeAsync(
            Guid runId, string userId)
        {
            var run = await _context.Runs
                .Include(r => r.AllocatedPassives)
                .FirstOrDefaultAsync(r => r.Id == runId)
                ?? throw new InvalidOperationException("Run not found.");

            if (run.UserId != userId)
                return (false, "Unauthorized.");

            if (!run.IsActive)
                return (false, "This run is no longer active.");

            int refundPoints = run.AllocatedPassives.Count;

            _context.RunPassiveNodes.RemoveRange(run.AllocatedPassives);
            run.AvailableSkillPoints += refundPoints;

            await _context.SaveChangesAsync();
            return (true, $"Tree reset. {refundPoints} point(s) refunded.");
        }

        // ─────────────────────────────────────────────────────
        // Helper: bir node unlock olabilir mi?
        //   - Level yeterli
        //   - Prereq'ler alınmış (veya prereq yok)
        // ─────────────────────────────────────────────────────
        public static bool IsNodeUnlocked(
            PassiveNodeDefinition node,
            int currentLevel,
            HashSet<string> allocatedIds)
        {
            if (currentLevel < node.RequiredLevel) return false;

            if (string.IsNullOrWhiteSpace(node.PrerequisiteNodeIds)) return true;

            return node.PrerequisiteNodeIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .All(prereq => allocatedIds.Contains(prereq));
        }
    }

    // ─────────────────────────────────────────────────────
    // DTO – Controller / View arasında taşınan veri
    // ─────────────────────────────────────────────────────
    public class PassiveTreeData
    {
        public Guid RunId { get; set; }
        public int CurrentLevel { get; set; }
        public int AvailablePoints { get; set; }
        public List<PassiveNodeDefinition> AllNodes { get; set; } = new();
        public HashSet<string> AllocatedIds { get; set; } = new();
    }
}
