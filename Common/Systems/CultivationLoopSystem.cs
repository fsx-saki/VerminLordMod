using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Items.Consumables;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // CultivationLoopSystem — 修为进阶闭环大框
    //
    // 系统定位：
    // 修为进阶是蛊师的核心成长路径。当前 QiRealmPlayer 有破境逻辑，
    // 但缺乏完整的闭环：开窍→修炼→破境→天劫→新境界→新空窍→更多蛊虫。
    // 此系统串联所有修为相关的子系统。
    //
    // 功能规划：
    // 1. 开窍觉醒（AwakeningSystem 已有，需串联）
    // 2. 日常修炼：真元积累、空窍巩固
    // 3. 小境界突破（QiRealmPlayer.StageUp 已有）
    // 4. 大境界突破（QiRealmPlayer.LevelUp 已有）
    // 5. 天劫渡劫（HeavenTribulationSystem 已有，需串联）
    // 6. 破境后的闭环：新空窍格数→更多蛊虫→更强修为→再次破境
    // 7. 修为瓶颈：每个大境界后期有瓶颈期，需要特殊突破条件
    //
    // 闭环流程：
    //   真元修炼 → 真元满 → 消耗元石破境 → 
    //   破境成功 → 天劫预警 → 天劫降临 → 
    //   渡劫成功 → 新境界解锁 → 空窍扩容 →
    //   炼化新蛊虫 → 继续修炼 → 循环
    //
    // TODO:
    //   - 创建修炼台Tile
    // ============================================================

    public class CultivationLoopSystem : ModSystem
    {
        public static CultivationLoopSystem Instance => ModContent.GetInstance<CultivationLoopSystem>();

        public static int GetMaxKongQiaoSlots(int guLevel)
        {
            return guLevel switch
            {
                0 => 0,
                1 => 3,
                2 => 5,
                3 => 7,
                4 => 9,
                5 => 12,
                6 => 15,
                7 => 18,
                8 => 21,
                9 => 25,
                _ => 25
            };
        }

        public static int GetMaxQiForLevel(int guLevel, int stage)
        {
            int baseQi = guLevel * 100 + stage * 25;
            return baseQi;
        }

        public static bool HasBreakthroughBottleneck(int guLevel, int stage)
        {
            if (guLevel == 3 && stage == 3) return true;
            if (guLevel == 5 && stage == 3) return true;
            if (guLevel == 7 && stage == 3) return true;
            return false;
        }

        public static string GetBottleneckRequirement(int guLevel)
        {
            return guLevel switch
            {
                3 => "需要成功渡过天劫并获得族长批准",
                5 => "需要完成家族战争任务或获得稀有突破材料",
                7 => "需要领悟道痕印记",
                _ => "未知条件"
            };
        }

        public static string GetUnlockedContent(int guLevel)
        {
            return guLevel switch
            {
                1 => "一转：基础蛊虫、学堂训练、青茅山探索",
                2 => "二转：中级蛊虫、家族委托、天劫预警、小型阵法",
                3 => "三转：高级蛊虫、家族职位、中型阵法、领地进入",
                4 => "四转：稀有蛊虫、族长级权限、大型阵法、家族战争",
                5 => "五转：传说蛊虫、南疆通行、顶阶阵法",
                6 => "六转：仙蛊雏形、跨域传送、秘境探索",
                7 => "七转：仙蛊炼制、道痕觉醒、天劫掌控",
                8 => "八转：仙蛊大成、世界法则、家族称霸",
                9 => "九转：至尊蛊师、天地共鸣、万蛊朝拜",
                _ => "未知境界"
            };
        }

        public static float GetCultivationEfficiency(Player player)
        {
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            float efficiency = 1.0f;
            efficiency += qiRealm.GuLevel * 0.05f;

            var formationSystem = FormationSystem.Instance;
            if (formationSystem != null && formationSystem.IsPlayerInFormation(player, FormationType.SpiritZhen))
                efficiency += 0.2f;

            var weatherSystem = WeatherSystem.Instance;
            if (weatherSystem != null)
                efficiency += weatherSystem.GetCultivationBonus();

            if (IsPlayerNearCultivationPlatform(player))
                efficiency += 0.15f;

            return efficiency;
        }

        public static bool IsPlayerNearCultivationPlatform(Player player)
        {
            int tileX = (int)(player.Center.X / 16);
            int tileY = (int)(player.Center.Y / 16);
            int radius = 5;

            for (int x = tileX - radius; x <= tileX + radius; x++)
            {
                for (int y = tileY - radius; y <= tileY + radius; y++)
                {
                    if (!WorldGen.InWorld(x, y)) continue;
                    Tile tile = Main.tile[x, y];
                    if (tile.HasTile && tile.TileType == ModContent.TileType<Content.Tiles.CultivationPlatformTile>())
                        return true;
                }
            }
            return false;
        }

        public static bool TryBreakthroughBottleneck(Player player, int guLevel)
        {
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            if (!HasBreakthroughBottleneck(guLevel, qiRealm.LevelStage)) return true;

            switch (guLevel)
            {
                case 3:
                    var tribulation = HeavenTribulationSystem.Instance;
                    if (tribulation != null && tribulation.HasPlayerSurvivedTribulation(player))
                        return true;
                    if (player.whoAmI == Main.myPlayer)
                        Main.NewText("瓶颈：需要成功渡过天劫并获得族长批准", Microsoft.Xna.Framework.Color.Orange);
                    return false;

                case 5:
                    var warSystem = FactionWarSystem.Instance;
                    var guWorld = player.GetModPlayer<GuWorldPlayer>();
                    if (warSystem != null && guWorld != null)
                    {
                        var faction = guWorld.CurrentAlly;
                        if (warSystem.GetFactionWarParticipation(faction) >= 1)
                            return true;
                    }
                    if (player.whoAmI == Main.myPlayer)
                        Main.NewText("瓶颈：需要完成家族战争任务或获得稀有突破材料", Microsoft.Xna.Framework.Color.Orange);
                    return false;

                case 7:
                    var daoMark = player.GetModPlayer<DaoHenPlayer>();
                    if (daoMark != null && daoMark.HasDaoMark())
                        return true;
                    if (player.whoAmI == Main.myPlayer)
                        Main.NewText("瓶颈：需要领悟道痕印记", Microsoft.Xna.Framework.Color.Orange);
                    return false;

                default:
                    return true;
            }
        }

        public static void OnTribulationSurvived(Player player)
        {
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            qiRealm.BreakthroughProgress += 30f;

            int bonusSlots = qiRealm.GuLevel >= 5 ? 1 : 0;
            if (bonusSlots > 0 && player.whoAmI == Main.myPlayer)
                Main.NewText($"天劫淬体！空窍扩容+{bonusSlots}格！", Microsoft.Xna.Framework.Color.Cyan);
        }

        public static void OnTribulationFailed(Player player)
        {
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            qiRealm.BreakthroughProgress = System.Math.Max(0, qiRealm.BreakthroughProgress - 50f);

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.QiCurrent = qiResource.QiMaxCurrent * 0.3f;

            if (player.whoAmI == Main.myPlayer)
                Main.NewText("天劫失败！修为受损，真元大减...", Microsoft.Xna.Framework.Color.Red);
        }

        private int _lastDay = -1;

        public override void PostUpdateWorld()
        {
            if (!WorldTimeHelper.IsNewDay(ref _lastDay)) return;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var player = Main.player[i];
                if (!player.active) continue;

                var qiRealm = player.GetModPlayer<QiRealmPlayer>();
                if (qiRealm.GuLevel <= 0) continue;

                var qiResource = player.GetModPlayer<QiResourcePlayer>();
                float efficiency = GetCultivationEfficiency(player);
                float dailyQiGain = 10f * efficiency;

                qiResource.QiCurrent = System.Math.Min(qiResource.QiMaxCurrent, qiResource.QiCurrent + dailyQiGain);

                if (qiResource.QiCurrent >= qiResource.QiMaxCurrent * 0.9f)
                {
                    qiRealm.BreakthroughProgress += efficiency * 2f;
                    if (qiRealm.BreakthroughProgress >= 100f)
                    {
                        qiRealm.BreakthroughProgress = 0f;
                        if (qiRealm.LevelStage < 3)
                        {
                            qiRealm.StageUp();
                        }
                        else if (!HasBreakthroughBottleneck(qiRealm.GuLevel, qiRealm.LevelStage))
                        {
                            qiRealm.LevelUp();
                        }
                        else if (TryBreakthroughBottleneck(player, qiRealm.GuLevel))
                        {
                            qiRealm.LevelUp();
                        }
                    }
                }
            }
        }
    }
}