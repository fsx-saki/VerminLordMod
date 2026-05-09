using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.GuBehaviors
{
    /// <summary>
    /// 杀招配方（D-27：MVA 硬编码阶段）。
    ///
    /// 杀招是玩家将多个蛊虫组合释放的强力技能。
    /// MVA 阶段硬编码 1-2 条配方验证系统可行性。
    /// </summary>
    public class ShaZhaoRecipe
    {
        /// <summary> 杀招名称 </summary>
        public string Name;

        /// <summary> 杀招描述 </summary>
        public string Description;

        /// <summary> 需要的道痕类型和数量 </summary>
        public Dictionary<DaoType, int> RequiredDaoTypes;

        /// <summary> 最低境界要求 </summary>
        public int MinRealmLevel;

        /// <summary> 最低空窍槽位数 </summary>
        public int MinKongQiaoSlots;

        /// <summary> 基础伤害 </summary>
        public int BaseDamage;

        /// <summary> 伤害倍率 </summary>
        public float DamageMultiplier;

        /// <summary> 附加效果标签 </summary>
        public DaoEffectTags Effects;

        /// <summary> 冷却时间（帧） </summary>
        public int CooldownTicks;

        /// <summary> 额外释放条件（返回 false 则无法释放） </summary>
        public Func<Player, bool> AdditionalCondition;

        /// <summary> 执行杀招效果 </summary>
        public Action<Player, Vector2, Vector2> Execute;

        /// <summary>
        /// 检查玩家是否满足此杀招的释放条件。
        /// </summary>
        public bool CanExecute(Player player)
        {
            // 境界检查
            var realmPlayer = player.GetModPlayer<QiRealmPlayer>();
            if (realmPlayer == null || realmPlayer.GuLevel < MinRealmLevel)
                return false;

            // 空窍槽位检查
            var kqPlayer = player.GetModPlayer<KongQiaoPlayer>();
            if (kqPlayer == null || kqPlayer.MaxSlots < MinKongQiaoSlots)
                return false;

            // 道痕检查
            var daoHenPlayer = player.GetModPlayer<DaoHenPlayer>();
            if (daoHenPlayer != null && RequiredDaoTypes != null)
            {
                foreach (var kvp in RequiredDaoTypes)
                {
                    if (!daoHenPlayer.DaoHen.TryGetValue(kvp.Key, out float val) || val < kvp.Value)
                        return false;
                }
            }

            // 额外条件
            if (AdditionalCondition != null && !AdditionalCondition(player))
                return false;

            return true;
        }
    }

    /// <summary>
    /// 杀招系统（D-27：MVA 硬编码阶段）。
    ///
    /// 管理所有杀招配方、冷却计时器、释放逻辑。
    /// MVA 阶段硬编码 2 条配方：
    /// 1. 「万剑归宗」— 剑道 + 金道 + 力道，大范围高伤害
    /// 2. 「时光回溯」— 宙道 + 空道，重置冷却
    /// </summary>
    public class ShaZhaoSystem : ModSystem
    {
        /// <summary> 所有已注册的杀招配方 </summary>
        public static List<ShaZhaoRecipe> AllRecipes = new();

        /// <summary> 玩家冷却计时器：player.whoAmI → 剩余冷却帧数 </summary>
        public Dictionary<int, int> CooldownTimers = new();

        // ============================================================
        // 初始化
        // ============================================================

        public override void OnWorldLoad()
        {
            AllRecipes.Clear();
            CooldownTimers.Clear();
            RegisterDefaultRecipes();
        }

        public override void OnWorldUnload()
        {
            AllRecipes.Clear();
            CooldownTimers.Clear();
        }

        /// <summary>
        /// 注册默认杀招配方（MVA 硬编码）。
        /// </summary>
        private static void RegisterDefaultRecipes()
        {
            // ---- 杀招 1：万剑归宗 ----
            // 需求：剑道≥100 + 金道≥50 + 力道≥50，境界≥3，空窍≥3
            // 效果：大范围剑气风暴，附加 ArmorShred + Weaken
            AllRecipes.Add(new ShaZhaoRecipe
            {
                Name = "万剑归宗",
                Description = "以剑道为引，金道为锋，力道为势，召唤万剑齐发。",
                RequiredDaoTypes = new Dictionary<DaoType, int>
                {
                    { DaoType.Sword, 100 },
                    { DaoType.Gold, 50 },
                    { DaoType.Power, 50 }
                },
                MinRealmLevel = 3,
                MinKongQiaoSlots = 3,
                BaseDamage = 200,
                DamageMultiplier = 1.5f,
                Effects = DaoEffectTags.ArmorShred | DaoEffectTags.Weaken,
                CooldownTicks = 36000, // 1 游戏日
                Execute = (player, position, velocity) =>
                {
                    // 万剑归宗：在玩家周围生成 20 把飞剑弹幕
                    int projType = FindProjectileType("VerminLordMod", "SwordProjectile");
                    if (projType <= 0)
                        projType = ProjectileID.WoodenArrowFriendly; // 降级

                    for (int i = 0; i < 20; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 20f;
                        Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                        Vector2 spawnPos = player.Center + dir * 80f;
                        Vector2 projVel = dir * 12f;

                        int damage = (int)(200 * 1.5f);
                        Projectile.NewProjectile(
                            player.GetSource_ItemUse(player.HeldItem),
                            spawnPos, projVel,
                            projType, damage, 5f,
                            player.whoAmI);
                    }

                    // 通知
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        Main.NewText("万剑归宗！", Color.Gold);
                    }
                }
            });

            // ---- 杀招 2：时光回溯 ----
            // 需求：宙道≥80 + 空道≥30，境界≥2，空窍≥2
            // 效果：重置所有蛊虫冷却，附加 QiRestore
            AllRecipes.Add(new ShaZhaoRecipe
            {
                Name = "时光回溯",
                Description = "以宙道之力逆转时光，重置所有蛊虫冷却。",
                RequiredDaoTypes = new Dictionary<DaoType, int>
                {
                    { DaoType.Time, 80 },
                    { DaoType.Space, 30 }
                },
                MinRealmLevel = 2,
                MinKongQiaoSlots = 2,
                BaseDamage = 0,
                DamageMultiplier = 1f,
                Effects = DaoEffectTags.QiRestore,
                CooldownTicks = 72000, // 2 游戏日
                AdditionalCondition = (player) =>
                {
                    // 需要至少 30% 灵气才能释放
                    var qiPlayer = player.GetModPlayer<QiResourcePlayer>();
                    return qiPlayer != null && qiPlayer.QiCurrent >= qiPlayer.QiMaxCurrent * 0.3f;
                },
                Execute = (player, position, velocity) =>
                {
                    // 恢复 50% 灵气
                    var qiPlayer = player.GetModPlayer<QiResourcePlayer>();
                    if (qiPlayer != null)
                    {
                        float restore = qiPlayer.QiMaxCurrent * 0.5f;
                        qiPlayer.RefundQi(restore);
                    }

                    // 通知
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        Main.NewText("时光回溯！灵气恢复。", Color.Cyan);
                    }
                }
            });
        }

        /// <summary>
        /// 尝试查找指定投射物类型。
        /// </summary>
        private static int FindProjectileType(string modName, string projName)
        {
            try
            {
                var mod = ModLoader.GetMod(modName);
                if (mod != null)
                {
                    return mod.Find<ModProjectile>(projName).Type;
                }
            }
            catch { }
            return -1;
        }

        // ============================================================
        // 运行时
        // ============================================================

        public override void PreUpdateWorld()
        {
            // 减少所有冷却计时器
            var expiredKeys = new List<int>();
            foreach (var kvp in CooldownTimers)
            {
                CooldownTimers[kvp.Key] = kvp.Value - 1;
                if (CooldownTimers[kvp.Key] <= 0)
                    expiredKeys.Add(kvp.Key);
            }
            foreach (var key in expiredKeys)
                CooldownTimers.Remove(key);
        }

        // ============================================================
        // 公开接口
        // ============================================================

        /// <summary>
        /// 注册一条杀招配方。
        /// </summary>
        public static void RegisterRecipe(ShaZhaoRecipe recipe)
        {
            AllRecipes.Add(recipe);
        }

        /// <summary>
        /// 获取玩家当前可用的杀招列表。
        /// </summary>
        public List<ShaZhaoRecipe> GetAvailableRecipes(Player player)
        {
            var available = new List<ShaZhaoRecipe>();
            foreach (var recipe in AllRecipes)
            {
                if (recipe.CanExecute(player))
                    available.Add(recipe);
            }
            return available;
        }

        /// <summary>
        /// 尝试执行杀招。
        /// </summary>
        public bool TryExecute(Player player, ShaZhaoRecipe recipe)
        {
            if (recipe == null)
                return false;

            // 检查冷却
            if (CooldownTimers.TryGetValue(player.whoAmI, out int remaining) && remaining > 0)
            {
                Main.NewText($"{recipe.Name} 还在冷却中（剩余 {remaining / 60} 秒）。", Color.Gray);
                return false;
            }

            // 检查条件
            if (!recipe.CanExecute(player))
            {
                Main.NewText("不满足释放条件。", Color.Gray);
                return false;
            }

            // 执行
            recipe.Execute(player, player.Center, Vector2.Zero);

            // 设置冷却
            CooldownTimers[player.whoAmI] = recipe.CooldownTicks;

            return true;
        }

        /// <summary>
        /// 获取玩家指定杀招的剩余冷却帧数。
        /// </summary>
        public int GetRemainingCooldown(int playerID)
        {
            if (CooldownTimers.TryGetValue(playerID, out int remaining))
                return remaining;
            return 0;
        }
    }
}
