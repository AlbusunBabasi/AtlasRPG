// ═══════════════════════════════════════════════════════════════════
// PASSIVE TREE ICON DOSYALARI
// Klasör: AtlasRPG.Web/wwwroot/images/passives/
//
// Aşağıdaki dosyaları bu klasöre koy.
// SVG veya PNG olabilir — JS içindeki getNodeIcon() fonksiyonu
// bu isimleri kullanıyor.
//
// ItemIconMapper.cs gibi bir C# helper GEREKMEZ;
// icon eşleştirmesi PassiveTree.cshtml içindeki JS'de yapılıyor.
// ═══════════════════════════════════════════════════════════════════

// ─── KEYSTONE İKONLARI (büyük, altın renkli, 5 adet) ────────────────
// keystone-crit.svg       → Kritik hasar keystone (Assassin's Mark vb.)
// keystone-poison.svg     → Zehir keystone (Viper Strike vb.)
// keystone-burn.svg       → Ateş/yanma keystone (Pyromancer)
// keystone-block.svg      → Blok keystone (Duelist)
// keystone-evasion.svg    → Kaçınma keystone (Ghost Walker vb.)
// keystone-bleed.svg      → Kanama keystone
// keystone-generic.svg    → Genel keystone fallback (yıldız/elmas)

// ─── NOTABLE İKONLARI (orta, mavi renkli, ~10 adet) ─────────────────
// notable-accuracy.svg    → Doğruluk (hedef nişan)
// notable-crit.svg        → Kritik (crosshair / kılıç)
// notable-mana.svg        → Mana (su damlası / kristal)
// notable-cooldown.svg    → Cooldown azaltma (saat / girdap)
// notable-block.svg       → Block (kalkan)
// notable-bleed.svg       → Kanama (kan damlası)
// notable-burn.svg        → Yanma (alev)
// notable-initiative.svg  → Inisiyatif (şimşek)
// notable-generic.svg     → Genel notable fallback (hilal)

// ─── MINOR İKONLARI (küçük, gri renkli, ~12 adet) ───────────────────
// minor-damage.svg        → Hasar (kılıç)
// minor-armor.svg         → Zırh (plaka)
// minor-evasion.svg       → Kaçınma (rüzgar / tüy)
// minor-crit.svg          → Crit şansı (çarpı)
// minor-critmulti.svg     → Crit çarpanı (iki kılıç)
// minor-hp.svg            → Can (kalp)
// minor-mana.svg          → Mana (damla)
// minor-accuracy.svg      → Doğruluk (göz)
// minor-initiative.svg    → Hız (şimşek)
// minor-ward.svg          → Ward (büyü kalkanı)
// minor-block.svg         → Blok (kalkan mini)
// minor-bleed.svg         → Kanama mini
// minor-burn.svg          → Yanma mini
// minor-poison.svg        → Zehir mini
// minor-generic.svg       → Genel minor fallback (yıldız)

// ─── TOPLAM: ~31 dosya ──────────────────────────────────────────────
//
// KULLANIM:
// 1) Resimlerini yukarıdaki isimlere rename et (veya kopya al)
// 2) wwwroot/images/passives/ klasörüne koy
// 3) Proje çalıştır — JS otomatik yükler
// 4) Resim yoksa fallback emoji gösterilir (⚔️ 🛡️ 💥 vb.)
//
// FARKLI UZANTI?
// PassiveTree.cshtml içindeki getNodeIcon() fonksiyonunda
// .svg yerine .png yaz (tek yerden değiştir, ~31 satır)
