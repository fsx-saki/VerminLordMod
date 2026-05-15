using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
    public enum KarmaType
    {
        None = 0,
        Good,               // 善业 — 帮助凡人、守约、护族
        Evil,               // 恶业 — 杀戮无辜、背信、叛族
        Cause,              // 因 — 种下因果（承诺/恩惠/仇恨）
        Effect,             // 果 — 因果报应（回报/报复）
        Heaven,             // 天道 — 顺应天道的行为
        Defiance,           // 逆天 — 违背天道的行为
    }

    public enum KarmaLevel
    {
        Pure = 5,           // 至善 — 天劫难度降低，NPC好感
        Virtuous = 4,       // 善 — 运势提升
        Neutral = 3,        // 中 — 无额外影响
        Tainted = 2,        // 恶 — 运势降低
        Corrupted = 1,      // 极恶 — 天劫难度增加，NPC敌意
        Damned = 0,         // 天谴 — 天劫必来，众叛亲离
    }

    public class KarmaRecord
    {
        public KarmaType Type;
        public string Description;
        public int Value;
        public int GameDay;
        public FactionID AffectedFaction;
        public int? AffectedNPCType;
    }

    public class CauseEffectBond
    {
        public string BondID;
        public string CauseDescription;
        public string EffectDescription;
        public bool IsResolved;
        public int Strength;
        public int GameDayCreated;
        public int? ResolutionDay;
    }

    public class KarmaSystem : ModSystem
    {
        public static KarmaSystem Instance => ModContent.GetInstance<KarmaSystem>();

        private int _lastDay = -1;

        public override void PostUpdateWorld()
        {
            int currentDay = (int)(Main.GameUpdateCount / 36000);
            if (currentDay <= _lastDay) return;
            _lastDay = currentDay;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var player = Main.player[i];
                if (!player.active) continue;

                var karmaPlayer = player.GetModPlayer<KarmaPlayer>();

                if (karmaPlayer.GoodKarma > 0)
                    karmaPlayer.GoodKarma = System.Math.Max(0, karmaPlayer.GoodKarma - 1);
                if (karmaPlayer.EvilKarma > 0)
                    karmaPlayer.EvilKarma = System.Math.Max(0, karmaPlayer.EvilKarma - 1);

                CheckKarmaRetribution(player, karmaPlayer);
            }
        }

        private void CheckKarmaRetribution(Player player, KarmaPlayer karmaPlayer)
        {
            var level = karmaPlayer.GetKarmaLevel();

            if (level == KarmaLevel.Damned && Main.rand.NextFloat() < 0.02f)
            {
                var tribSystem = HeavenTribulationSystem.Instance;
                if (tribSystem != null && tribSystem.CanTriggerTribulation(player))
                {
                    tribSystem.TriggerTribulation(player);
                    if (player.whoAmI == Main.myPlayer)
                        Main.NewText("天谴降临！你的恶行引来了天劫！", Color.DarkRed);
                }
            }

            if (level <= KarmaLevel.Corrupted && Main.rand.NextFloat() < 0.01f)
            {
                player.AddBuff(Terraria.ID.BuffID.Cursed, 600);
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText("你感到一股不祥的预兆...", Color.Gray);
            }

            if (level >= KarmaLevel.Virtuous && Main.rand.NextFloat() < 0.01f)
            {
                player.AddBuff(Terraria.ID.BuffID.Regeneration, 600);
            }
        }

        public static float GetTribulationModifier(KarmaLevel level)
        {
            return level switch
            {
                KarmaLevel.Pure => 0.7f,
                KarmaLevel.Virtuous => 0.85f,
                KarmaLevel.Neutral => 1.0f,
                KarmaLevel.Tainted => 1.3f,
                KarmaLevel.Corrupted => 1.6f,
                KarmaLevel.Damned => 2.0f,
                _ => 1.0f,
            };
        }

        public static float GetLuckModifier(KarmaLevel level)
        {
            return level switch
            {
                KarmaLevel.Pure => 1.5f,
                KarmaLevel.Virtuous => 1.2f,
                KarmaLevel.Neutral => 1.0f,
                KarmaLevel.Tainted => 0.8f,
                KarmaLevel.Corrupted => 0.5f,
                KarmaLevel.Damned => 0.2f,
                _ => 1.0f,
            };
        }

        public static float GetNPCAttitudeModifier(KarmaLevel level)
        {
            return level switch
            {
                KarmaLevel.Pure => 1.5f,
                KarmaLevel.Virtuous => 1.2f,
                KarmaLevel.Neutral => 1.0f,
                KarmaLevel.Tainted => 0.7f,
                KarmaLevel.Corrupted => 0.4f,
                KarmaLevel.Damned => 0.1f,
                _ => 1.0f,
            };
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["karmaDayCounter"] = _lastDay;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            _lastDay = tag.GetInt("karmaDayCounter");
        }
    }

    public class KarmaPlayer : ModPlayer
    {
        public int GoodKarma;
        public int EvilKarma;
        public int NetKarma => GoodKarma - EvilKarma;
        public List<KarmaRecord> KarmaHistory = new();
        public List<CauseEffectBond> ActiveBonds = new();

        public KarmaLevel GetKarmaLevel()
        {
            int net = NetKarma;
            if (net >= 500) return KarmaLevel.Pure;
            if (net >= 200) return KarmaLevel.Virtuous;
            if (net >= -200) return KarmaLevel.Neutral;
            if (net >= -500) return KarmaLevel.Tainted;
            if (net >= -1000) return KarmaLevel.Corrupted;
            return KarmaLevel.Damned;
        }

        public void AddKarma(KarmaType type, int value, string description, FactionID faction = FactionID.None)
        {
            var record = new KarmaRecord
            {
                Type = type,
                Description = description,
                Value = value,
                GameDay = (int)(Main.GameUpdateCount / 36000),
                AffectedFaction = faction,
            };

            if (type == KarmaType.Good || type == KarmaType.Heaven)
                GoodKarma += value;
            else if (type == KarmaType.Evil || type == KarmaType.Defiance)
                EvilKarma += value;

            KarmaHistory.Add(record);

            if (type == KarmaType.Cause)
            {
                ActiveBonds.Add(new CauseEffectBond
                {
                    BondID = $"bond_{Player.whoAmI}_{ActiveBonds.Count}",
                    CauseDescription = description,
                    Strength = value,
                    GameDayCreated = record.GameDay,
                });
            }

            EventBus.Publish(new KarmaChangedEvent
            {
                PlayerID = Player.whoAmI,
                Type = type,
                Value = value,
                NewLevel = GetKarmaLevel(),
            });
        }

        public void ResolveCause(string bondID, string effectDescription)
        {
            var bond = ActiveBonds.Find(b => b.BondID == bondID);
            if (bond == null || bond.IsResolved) return;

            bond.IsResolved = true;
            bond.EffectDescription = effectDescription;
            bond.ResolutionDay = (int)(Main.GameUpdateCount / 36000);
        }

        public override void SaveData(TagCompound tag)
        {
            tag["goodKarma"] = GoodKarma;
            tag["evilKarma"] = EvilKarma;
        }

        public override void LoadData(TagCompound tag)
        {
            GoodKarma = tag.GetInt("goodKarma");
            EvilKarma = tag.GetInt("evilKarma");
            KarmaHistory.Clear();
            ActiveBonds.Clear();
        }
    }

    public class KarmaChangedEvent : GuWorldEvent
    {
        public int PlayerID;
        public KarmaType Type;
        public int Value;
        public KarmaLevel NewLevel;
    }
}
