using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons
{
    /// <summary>
    /// 蛊道媒介 - 唯一媒介武器（空壳）
    /// 所有伤害来自空窍中启用的蛊虫数据
    /// MVA 硬编码杀招配方：月光蛊 + 酒虫 = 酒月斩
    /// </summary>
    public class GuMediumWeapon : ModItem
    {
        // 媒介武器冷却追踪
        private int lastShaZhaoFrame = -9999;

        public override void SetDefaults()
        {
            // === 空壳属性 ===
            Item.damage = 0;                                              // 自身无伤害
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>(); // 蛊术伤害类型
            Item.width = 28;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.knockBack = 0f;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Green;
            Item.noMelee = true;
            Item.noUseGraphic = false;

            // 关键：必须设置 Item.shoot > 0，否则 Shoot() 不会被调用！
            // 使用木箭作为占位符，实际弹幕由空窍中的蛊虫动态决定
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 8f;
        }

        public override void AddRecipes()
        {
            // MVA 阶段：开局赠送或开窍时自动获得
        }

        public override bool CanUseItem(Player player)
        {
            // 只有已开窍的玩家才能使用媒介
            var realm = player.GetModPlayer<QiRealmPlayer>();
            return realm.GuLevel > 0;
        }

        // MVA 硬编码：P1 时抽取到 ShaZhaoRecipeSystem
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var kongQiao = player.GetModPlayer<KongQiaoPlayer>();
            var qiResource = player.GetModPlayer<QiResourcePlayer>();

            // 1. 获取启用的攻击蛊
            var activeAttackGus = kongQiao.GetActiveAttackGus();
            if (activeAttackGus.Count == 0)
            {
                // 无蛊可用：哑火
                return false;
            }

            // 2. 检查杀招配方（MVA 硬编码）
            var shaZhao = MatchShaZhao(activeAttackGus);
            if (shaZhao != null)
            {
                // 检查冷却
                int cooldownFrames = 300; // 5 秒冷却
                if (Main.GameUpdateCount - lastShaZhaoFrame >= cooldownFrames)
                {
                    // 检查真元是否足够
                    if (ConsumeQiSafe(qiResource, shaZhao.QiCost))
                    {
                        // ===== 释放杀招 =====
                        FireShaZhao(source, position, velocity, shaZhao, activeAttackGus, player);
                        lastShaZhaoFrame = (int)Main.GameUpdateCount;

                        // D-10: 仅参与者休眠
                        foreach (var gu in activeAttackGus.Where(g => shaZhao.RequiredGuTypes.Contains(g.GuTypeID)))
                        {
                            int index = kongQiao.KongQiao.IndexOf(gu);
                            if (index >= 0) kongQiao.SetGuActive(index, false);
                        }

                        return false;
                    }
                }
            }

            // 3. 平 A 齐射（D-12: 同时散射）
            FireSalvo(source, position, velocity, activeAttackGus, qiResource, player);

            return false;
        }

        /// <summary>
        /// 安全消耗真元（不发布事件，避免事件风暴）
        /// </summary>
        private bool ConsumeQiSafe(QiResourcePlayer qiResource, float amount)
        {
            if (qiResource.QiCurrent < amount) return false;
            qiResource.QiCurrent -= amount;
            return true;
        }

        #region 杀招匹配（MVA 硬编码）

        /// <summary>
        /// 匹配杀招配方
        /// </summary>
        private ShaZhaoRecipe MatchShaZhao(List<KongQiaoSlot> activeGus)
        {
            var activeTypes = activeGus.Select(g => g.GuTypeID).ToHashSet();

            // 硬编码配方：月光蛊 + 酒虫 = 酒月斩
            int moonlightType = ModContent.ItemType<Content.Items.Weapons.One.Moonlight>();
            int wineBugType = ModContent.ItemType<Content.Items.Weapons.One.WineBugWeapon>();

            if (activeTypes.Contains(moonlightType) && activeTypes.Contains(wineBugType))
            {
                return new ShaZhaoRecipe
                {
                    Name = "酒月斩",
                    RequiredGuTypes = new List<int> { moonlightType, wineBugType },
                    QiCost = 30f,
                    DamageMultiplier = 3f,
                    MainProjectile = ModContent.ProjectileType<ShaZhaoJiuYueZhan>()
                };
            }

            // MVA 阶段只有 1 条配方
            return null;
        }

        #endregion

        #region 杀招发射

        /// <summary>
        /// 发射杀招弹幕
        /// </summary>
        private void FireShaZhao(EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
            ShaZhaoRecipe recipe, List<KongQiaoSlot> participants, Player player)
        {
            // 计算总伤害：参与者蛊虫伤害之和 × 倍率
            int totalDamage = (int)(participants
                .Where(g => recipe.RequiredGuTypes.Contains(g.GuTypeID))
                .Sum(g => g.GuItem.damage) * recipe.DamageMultiplier);

            // 发射杀招弹幕
            var proj = Projectile.NewProjectileDirect(source, position, velocity * 1.5f,
                recipe.MainProjectile, totalDamage, 6f, player.whoAmI);

            // 特效：弹幕放大
            proj.scale = 1.5f;

            // 播放杀招音效
            SoundEngine.PlaySound(SoundID.Item20, position);

            // 播放特效文字
            Main.NewText($"【杀招·{recipe.Name}】", new Color(255, 200, 100));
        }

        #endregion

        #region 平 A 齐射

        /// <summary>
        /// 平 A 齐射（D-12: 同时散射）
        /// </summary>
        private void FireSalvo(EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
            List<KongQiaoSlot> activeGus, QiResourcePlayer qiResource, Player player)
        {
            int count = activeGus.Count;
            float spreadAngle = 0.15f;           // 总散射弧度
            float angleStep = count > 1 ? spreadAngle / (count - 1) : 0f;
            float startAngle = -spreadAngle / 2f;

            for (int i = 0; i < count; i++)
            {
                var gu = activeGus[i];

                // 角度微调
                float individualAngle = startAngle + angleStep * i;
                Vector2 shootVel = velocity.RotatedBy(individualAngle);

                // 速度随机波动
                shootVel *= 0.8f + Main.rand.NextFloat(0.4f);

                // 使用蛊虫对应的弹幕类型（炼化时已从原始物品获取并存入 ProjectileType）
                int projectileType = gu.ProjectileType;
                if (projectileType <= 0)
                {
                    // 未设置弹幕类型，使用默认木箭
                    projectileType = ProjectileID.WoodenArrowFriendly;
                }

                // 发射弹幕
                Projectile.NewProjectile(source, position, shootVel,
                    projectileType, gu.GuItem.damage, gu.GuItem.knockBack, player.whoAmI);

                // 每只蛊虫消耗真元：占据额度的 10%
                float cost = gu.QiOccupation * 0.1f;
                ConsumeQiSafe(qiResource, cost);
            }
        }

        #endregion
    }

    /// <summary>
    /// 杀招配方数据结构（MVA 硬编码用）
    /// </summary>
    public class ShaZhaoRecipe
    {
        public string Name;
        public List<int> RequiredGuTypes;
        public float QiCost;
        public float DamageMultiplier;
        public int MainProjectile;
    }
}
