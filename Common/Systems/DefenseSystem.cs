using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// DefenseSystem — 迷踪阵与守卫（P2 MVA 阶段）
    /// 
    /// 职责：
    /// 1. 迷踪阵 Tile：降低 NPC 对玩家位置的感知精度
    /// 2. 守卫 NPC：提高 NPC 对玩家的风险阈值（让 NPC 更谨慎）
    /// 3. DefensePlayer：玩家状态标记（是否在迷踪阵中、守卫数量）
    /// 
    /// MVA 阶段：
    /// - 迷踪阵：放置后范围内玩家获得 Buff，NPC 感知距离 -50%
    /// - 守卫：通过 DefensePlayer 标记，P1 再实现具体 NPC
    /// - 无 NPC 感知精度修改（P1 扩展）
    /// 
    /// 依赖：
    /// - GuMasterBase（NPC 感知系统）
    /// - ModTile / ModBuff 系统
    /// </summary>
    public class DefenseSystem : ModSystem
    {
        // ===== 单例访问 =====
        public static DefenseSystem Instance => ModContent.GetInstance<DefenseSystem>();

        // ===== 配置常量 =====
        /// <summary> 迷踪阵有效范围（Tile 格数） </summary>
        public const int MIZONG_RANGE_TILES = 30;

        /// <summary> 迷踪阵有效范围（像素） </summary>
        public const float MIZONG_RANGE_PIXELS = MIZONG_RANGE_TILES * 16f;

        /// <summary> 迷踪阵 NPC 感知距离缩减比例 </summary>
        public const float MIZONG_PERCEPTION_REDUCTION = 0.5f;

        /// <summary> 每个守卫提供的风险阈值加成 </summary>
        public const float GUARD_RISK_THRESHOLD_BONUS = 0.1f;

        /// <summary> 守卫最大数量 </summary>
        public const int MAX_GUARDS = 5;

        // ============================================================
        // 运行时数据
        // ============================================================

        /// <summary> 所有活跃的迷踪阵位置列表 </summary>
        public System.Collections.Generic.List<Point> ActiveMiZongPositions = new();

        // ============================================================
        // 迷踪阵管理
        // ============================================================

        /// <summary>
        /// 注册迷踪阵位置。
        /// </summary>
        public void RegisterMiZong(Point tilePos)
        {
            if (!ActiveMiZongPositions.Contains(tilePos))
            {
                ActiveMiZongPositions.Add(tilePos);
            }
        }

        /// <summary>
        /// 移除迷踪阵位置。
        /// </summary>
        public void UnregisterMiZong(Point tilePos)
        {
            ActiveMiZongPositions.Remove(tilePos);
        }

        /// <summary>
        /// 检查玩家是否在迷踪阵范围内。
        /// </summary>
        public bool IsPlayerInMiZong(Player player)
        {
            foreach (var pos in ActiveMiZongPositions)
            {
                Vector2 tileCenter = new Vector2(pos.X * 16 + 8, pos.Y * 16 + 8);
                if (Vector2.Distance(player.Center, tileCenter) < MIZONG_RANGE_PIXELS)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取迷踪阵对 NPC 感知距离的修正系数。
        /// </summary>
        public float GetPerceptionModifier(Player player)
        {
            if (IsPlayerInMiZong(player))
                return MIZONG_PERCEPTION_REDUCTION;
            return 1f;
        }

        // ============================================================
        // 守卫管理
        // ============================================================

        /// <summary>
        /// 获取守卫提供的风险阈值加成。
        /// </summary>
        public float GetGuardRiskBonus(int guardCount)
        {
            return System.Math.Min(guardCount, MAX_GUARDS) * GUARD_RISK_THRESHOLD_BONUS;
        }

        // ============================================================
        // ModSystem 生命周期
        // ============================================================

        public override void OnWorldLoad()
        {
            ActiveMiZongPositions.Clear();
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var posData = new System.Collections.Generic.List<TagCompound>();
            foreach (var pos in ActiveMiZongPositions)
            {
                posData.Add(new TagCompound
                {
                    ["x"] = pos.X,
                    ["y"] = pos.Y
                });
            }
            tag["miZongPositions"] = posData;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            ActiveMiZongPositions.Clear();
            if (tag.TryGet("miZongPositions", out System.Collections.Generic.List<TagCompound> posData))
            {
                foreach (var entry in posData)
                {
                    ActiveMiZongPositions.Add(new Point(entry.GetInt("x"), entry.GetInt("y")));
                }
            }
        }
    }

    // ============================================================
    // DefensePlayer — 玩家防御状态
    // ============================================================

    /// <summary>
    /// 玩家防御状态 ModPlayer。
    /// 记录玩家是否在迷踪阵中、雇佣的守卫数量。
    /// </summary>
    public class DefensePlayer : ModPlayer
    {
        /// <summary> 是否在迷踪阵中（每帧由 MiZongBuff 设置） </summary>
        public bool IsInMiZong;

        /// <summary> 雇佣的守卫数量 </summary>
        public int GuardCount;

        /// <summary> 守卫提供的风险阈值加成 </summary>
        public float GuardRiskBonus;

        public override void ResetEffects()
        {
            IsInMiZong = false;
            GuardRiskBonus = 0f;
        }

        public override void PostUpdate()
        {
            // 计算守卫提供的风险阈值加成
            if (GuardCount > 0)
            {
                GuardRiskBonus = DefenseSystem.Instance.GetGuardRiskBonus(GuardCount);
            }

            // 迷踪阵效果：由 MiZongBuff 设置 IsInMiZong
            // 此处预留扩展点
        }

        public override void SaveData(TagCompound tag)
        {
            tag["guardCount"] = GuardCount;
        }

        public override void LoadData(TagCompound tag)
        {
            GuardCount = tag.GetInt("guardCount");
        }
    }

    // ============================================================
    // MiZongBuff — 迷踪阵 Buff
    // ============================================================

    /// <summary>
    /// 迷踪阵 Buff。
    /// 玩家在迷踪阵范围内时获得此 Buff，标记 IsInMiZong。
    /// </summary>
    public class MiZongBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // 不随时间减少（使用 BuffID.Sets 替代过时的 Main.buffNoTimeLoss）
            Main.debuff[Type] = false; // 不是减益
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 标记玩家处于迷踪阵中
            player.GetModPlayer<DefensePlayer>().IsInMiZong = true;

            // 视觉效果：淡紫色光晕
            if (Main.rand.NextBool(4))
            {
                Dust.NewDust(
                    player.position,
                    player.width,
                    player.height,
                    DustID.Shadowflame,
                    0f, 0f,
                    100,
                    Color.Purple,
                    0.5f);
            }
        }
    }

    // ============================================================
    // MiZongZhenTile — 迷踪阵 Tile
    // ============================================================

    /// <summary>
    /// 迷踪阵 Tile。
    /// 消耗元石运转，降低 NPC 对玩家位置的感知精度。
    /// </summary>
    public class MiZongZhenTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            AddMapEntry(new Color(100, 50, 150), CreateMapEntryName());

            DustType = DustID.Shadowflame;
            HitSound = SoundID.Tink;
            MineResist = 2f;
            MinPick = 50;
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            // 注册迷踪阵位置
            DefenseSystem.Instance.RegisterMiZong(new Point(i, j));

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText("迷踪阵已布置。附近的 NPC 将难以感知你的精确位置。", Color.Cyan);
            }
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (!fail && !effectOnly)
            {
                DefenseSystem.Instance.UnregisterMiZong(new Point(i, j));
            }
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            // 范围内玩家获得 Buff
            foreach (Player player in Main.player)
            {
                if (player.active && !player.dead)
                {
                    if (Vector2.Distance(
                            new Vector2(i * 16 + 8, j * 16 + 8),
                            player.Center) < DefenseSystem.MIZONG_RANGE_PIXELS)
                    {
                        player.AddBuff(ModContent.BuffType<MiZongBuff>(), 2);
                    }
                }
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // 淡紫色微光
            r = 0.2f;
            g = 0.1f;
            b = 0.3f;
        }
    }
}
