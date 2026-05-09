using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Consumables;

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
    /// D-37 扩展（防御工事经济）：
    /// - 每个迷踪阵有维护成本（元石消耗）
    /// - 每 MAINTENANCE_INTERVAL 帧消耗 MAINTENANCE_COST 个元石
    /// - 消耗从放置者的背包中扣除
    /// - 元石不足时迷踪阵失效（buff 停止工作）
    /// - 失效后玩家靠近迷踪阵会收到提示
    ///
    /// 依赖：
    /// - GuMasterBase（NPC 感知系统）
    /// - ModTile / ModBuff 系统
    /// - YuanS（元石物品，用于维护消耗）
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

        // ===== D-37：维护成本配置 =====
        /// <summary> 维护间隔（帧数，约 1 游戏日 = 36000 帧的 1/3） </summary>
        public const int MAINTENANCE_INTERVAL = 12000; // ~2 分钟现实时间

        /// <summary> 每次维护消耗的元石数量 </summary>
        public const int MAINTENANCE_COST = 1;

        /// <summary> 元石不足时迷踪阵失效的最大连续帧数 </summary>
        public const int MAINTENANCE_GRACE_PERIOD = 600; // 10 秒宽限期

        // ============================================================
        // 运行时数据
        // ============================================================

        /// <summary> 迷踪阵所有者信息（D-37） </summary>
        public struct MiZongInstance
        {
            public Point TilePos;
            public int OwnerPlayerID;       // 放置者的 player.whoAmI
            public int MaintenanceTimer;     // 距下次维护的剩余帧数
            public int InsufficientTimer;    // 元石不足的连续帧数
            public bool IsActive;            // 是否因维护不足而失效

            public MiZongInstance(Point pos, int ownerID)
            {
                TilePos = pos;
                OwnerPlayerID = ownerID;
                MaintenanceTimer = MAINTENANCE_INTERVAL;
                InsufficientTimer = 0;
                IsActive = true;
            }
        }

        /// <summary> 所有迷踪阵实例（D-37：替换 ActiveMiZongPositions） </summary>
        public System.Collections.Generic.List<MiZongInstance> ActiveMiZongs = new();

        // ============================================================
        // 迷踪阵管理
        // ============================================================

        /// <summary>
        /// 注册迷踪阵位置（D-37：记录所有者）。
        /// </summary>
        public void RegisterMiZong(Point tilePos, int ownerPlayerID = -1)
        {
            // 检查是否已存在
            foreach (var mz in ActiveMiZongs)
            {
                if (mz.TilePos == tilePos)
                    return;
            }
            ActiveMiZongs.Add(new MiZongInstance(tilePos, ownerPlayerID));
        }

        /// <summary>
        /// 移除迷踪阵位置。
        /// </summary>
        public void UnregisterMiZong(Point tilePos)
        {
            ActiveMiZongs.RemoveAll(mz => mz.TilePos == tilePos);
        }

        /// <summary>
        /// 检查玩家是否在活跃的迷踪阵范围内（D-37：只考虑 IsActive 的迷踪阵）。
        /// </summary>
        public bool IsPlayerInMiZong(Player player)
        {
            for (int i = 0; i < ActiveMiZongs.Count; i++)
            {
                var mz = ActiveMiZongs[i];
                if (!mz.IsActive)
                    continue; // 维护不足的迷踪阵失效

                Vector2 tileCenter = new Vector2(mz.TilePos.X * 16 + 8, mz.TilePos.Y * 16 + 8);
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
        // D-37：维护成本逻辑
        // ============================================================

        /// <summary>
        /// 从玩家背包中扣除指定数量的元石。
        /// </summary>
        private static bool TryConsumeYuanStones(Player player, int amount)
        {
            if (player == null || !player.active)
                return false;

            int total = 0;
            foreach (var item in player.inventory)
            {
                if (item.IsAir) continue;
                if (item.type == ModContent.ItemType<YuanS>())
                {
                    total += item.stack;
                }
            }

            if (total < amount)
                return false;

            int remaining = amount;
            for (int i = 0; i < player.inventory.Length && remaining > 0; i++)
            {
                var item = player.inventory[i];
                if (item.IsAir) continue;
                if (item.type == ModContent.ItemType<YuanS>())
                {
                    int take = System.Math.Min(remaining, item.stack);
                    item.stack -= take;
                    remaining -= take;
                    if (item.stack <= 0)
                        item.TurnToAir();
                }
            }
            return true;
        }

        /// <summary>
        /// 每帧更新维护计时器（D-37）。
        /// </summary>
        public override void PreUpdateWorld()
        {
            for (int i = 0; i < ActiveMiZongs.Count; i++)
            {
                var mz = ActiveMiZongs[i];
                mz.MaintenanceTimer--;

                if (mz.MaintenanceTimer <= 0)
                {
                    // 到达维护时间点
                    Player owner = null;
                    if (mz.OwnerPlayerID >= 0 && mz.OwnerPlayerID < Main.maxPlayers)
                    {
                        owner = Main.player[mz.OwnerPlayerID];
                    }

                    if (owner != null && owner.active)
                    {
                        if (TryConsumeYuanStones(owner, MAINTENANCE_COST))
                        {
                            // 维护成功
                            mz.MaintenanceTimer = MAINTENANCE_INTERVAL;
                            mz.InsufficientTimer = 0;
                            if (!mz.IsActive)
                            {
                                mz.IsActive = true;
                                if (Main.netMode == NetmodeID.SinglePlayer)
                                {
                                    Main.NewText("迷踪阵恢复运转。", Color.Cyan);
                                }
                            }
                        }
                        else
                        {
                            // 元石不足
                            mz.InsufficientTimer++;
                            if (mz.InsufficientTimer >= MAINTENANCE_GRACE_PERIOD)
                            {
                                if (mz.IsActive)
                                {
                                    mz.IsActive = false;
                                    if (Main.netMode == NetmodeID.SinglePlayer)
                                    {
                                        Main.NewText("迷踪阵因缺乏元石维护而失效。", Color.OrangeRed);
                                    }
                                }
                            }
                            // 继续尝试（每帧减到 0 以下会继续尝试）
                            mz.MaintenanceTimer = 60; // 1 秒后重试
                        }
                    }
                    else
                    {
                        // 所有者不在线，迷踪阵进入待机状态
                        mz.MaintenanceTimer = MAINTENANCE_INTERVAL;
                        if (mz.IsActive)
                        {
                            mz.IsActive = false;
                        }
                    }
                }

                ActiveMiZongs[i] = mz; // 写回 struct
            }
        }

        // ============================================================
        // ModSystem 生命周期
        // ============================================================

        public override void OnWorldLoad()
        {
            ActiveMiZongs.Clear();
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var posData = new System.Collections.Generic.List<TagCompound>();
            foreach (var mz in ActiveMiZongs)
            {
                posData.Add(new TagCompound
                {
                    ["x"] = mz.TilePos.X,
                    ["y"] = mz.TilePos.Y,
                    ["owner"] = mz.OwnerPlayerID,
                    ["timer"] = mz.MaintenanceTimer,
                    ["insufficient"] = mz.InsufficientTimer,
                    ["active"] = mz.IsActive
                });
            }
            tag["miZongInstances"] = posData;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            ActiveMiZongs.Clear();
            if (tag.TryGet("miZongInstances", out System.Collections.Generic.List<TagCompound> posData))
            {
                foreach (var entry in posData)
                {
                    ActiveMiZongs.Add(new MiZongInstance
                    {
                        TilePos = new Point(entry.GetInt("x"), entry.GetInt("y")),
                        OwnerPlayerID = entry.GetInt("owner"),
                        MaintenanceTimer = entry.GetInt("timer"),
                        InsufficientTimer = entry.GetInt("insufficient"),
                        IsActive = entry.GetBool("active")
                    });
                }
            }
            // 兼容旧格式（D-37 之前）
            else if (tag.TryGet("miZongPositions", out System.Collections.Generic.List<TagCompound> oldData))
            {
                foreach (var entry in oldData)
                {
                    ActiveMiZongs.Add(new MiZongInstance(
                        new Point(entry.GetInt("x"), entry.GetInt("y")),
                        -1 // 旧数据无所有者信息
                    ));
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

        // ===== D-37：维护状态 =====
        /// <summary> 玩家拥有的迷踪阵是否因维护不足而失效 </summary>
        public bool HasInactiveMiZong;

        /// <summary> 玩家拥有的迷踪阵数量 </summary>
        public int OwnedMiZongCount;

        /// <summary> 下次维护还需的帧数（仅当只有一个迷踪阵时显示） </summary>
        public int NextMaintenanceIn;

        public override void ResetEffects()
        {
            IsInMiZong = false;
            GuardRiskBonus = 0f;
            HasInactiveMiZong = false;
            OwnedMiZongCount = 0;
            NextMaintenanceIn = int.MaxValue;
        }

        public override void PostUpdate()
        {
            // 计算守卫提供的风险阈值加成
            if (GuardCount > 0)
            {
                GuardRiskBonus = DefenseSystem.Instance.GetGuardRiskBonus(GuardCount);
            }

            // D-37：检查玩家拥有的迷踪阵维护状态
            OwnedMiZongCount = 0;
            HasInactiveMiZong = false;
            NextMaintenanceIn = int.MaxValue;

            foreach (var mz in DefenseSystem.Instance.ActiveMiZongs)
            {
                if (mz.OwnerPlayerID == Player.whoAmI)
                {
                    OwnedMiZongCount++;
                    if (!mz.IsActive)
                        HasInactiveMiZong = true;
                    if (mz.MaintenanceTimer < NextMaintenanceIn)
                        NextMaintenanceIn = mz.MaintenanceTimer;
                }
            }
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
            // D-37：注册迷踪阵，记录放置者作为所有者（用于维护扣费）
            int ownerID = -1;
            // 尝试从 Main.LocalPlayer 获取放置者
            if (Main.LocalPlayer != null && Main.LocalPlayer.active && !Main.LocalPlayer.dead)
            {
                // 检查玩家是否持有该物品
                for (int p = 0; p < Main.maxPlayers; p++)
                {
                    var player = Main.player[p];
                    if (player != null && player.active && player.HeldItem.type == item.type)
                    {
                        ownerID = p;
                        break;
                    }
                }
                // 兜底：使用本地玩家
                if (ownerID < 0)
                    ownerID = Main.myPlayer;
            }

            DefenseSystem.Instance.RegisterMiZong(new Point(i, j), ownerID);

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText("迷踪阵已布置。消耗元石维持运转。", Color.Cyan);
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
