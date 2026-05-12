using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles.Zero;

namespace VerminLordMod.Content.Items.Weapons.Zero
{
    /// <summary>
    /// 冰道基础蛊 — 冰晶蛊
    /// 冰道技术储备库的武器端实现。
    /// 聚焦冰系特性：冰晶、冰锥散射、冰霜领域、暴风雪、冰爆术。
    /// 手持时按 R 键切换当前攻击模式。
    ///
    /// 攻击模式（5种截然不同的冰系效果）：
    ///   模式0 - 冰晶：发射基础冰晶弹，命中后冻结敌人
    ///   模式1 - 冰锥散射：发射后分裂成多个冰锥，扇形散射
    ///   模式2 - 冰霜领域：在鼠标位置生成冰霜区域，持续减速+冻结
    ///   模式3 - 暴风雪：从天而降大量冰晶，覆盖范围攻击
    ///   模式4 - 冰爆术：发射冰爆弹，命中后范围冰爆+冻结
    /// </summary>
    public class IceSnowBaseGu : IceSnowWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 8;
        protected override int _useTime => 20;
        protected override int _guLevel => 1;
        protected override int controlQiCost => 5;
        protected override float unitConntrolRate => 25;

        // ===== 攻击方式切换系统 =====

        /// <summary>当前攻击模式索引</summary>
        public int attackMode = 0;

        /// <summary>所有攻击模式的名称（用于提示）</summary>
        private static readonly string[] AttackModeNames = new[]
        {
            "冰晶",         // 模式0：基础冰晶弹，冻结效果
            "冰锥散射",     // 模式1：扇形散射多个冰锥
            "冰霜领域",     // 模式2：滞留冰霜区域，持续减速+冻结
            "暴风雪",       // 模式3：从天而降大量冰晶
            "冰爆术",       // 模式4：范围冰爆+冻结
        };

        /// <summary>所有攻击模式对应的弹幕类型</summary>
        private int[] _modeProjectileTypes;

        /// <summary>所有攻击模式对应的 shootSpeed</summary>
        private readonly float[] _modeShootSpeeds = new[]
        {
            12f, // 模式0：冰晶
            12f,  // 模式1：冰锥散射
            0f,   // 模式2：冰霜领域（在鼠标位置直接生成）
            0f,   // 模式3：暴风雪（在鼠标上方生成）
            9f,   // 模式4：冰爆术
        };

        /// <summary>所有攻击模式对应的伤害倍率</summary>
        private readonly float[] _modeDamageMultipliers = new[]
        {
            1.0f,  // 模式0：冰晶
            0.7f,  // 模式1：冰锥散射（多个冰锥）
            0.5f,  // 模式2：冰霜领域（持续范围伤害）
            0.6f,  // 模式3：暴风雪（大量冰晶）
            1.5f,  // 模式4：冰爆术（高单发伤害）
        };

        /// <summary>所有攻击模式对应的使用间隔（帧）</summary>
        private readonly int[] _modeUseTimes = new[]
        {
            20,  // 模式0：冰晶，快速
            30,  // 模式1：冰锥散射，较慢
            30,  // 模式2：冰霜领域，较慢
            35,  // 模式3：暴风雪，慢
            35,  // 模式4：冰爆术，较慢
        };

        // ===== IOnHitEffectProvider =====
        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Freeze };
        public float DoTDuration => 3f;
        public float DoTDamage => 4f;
        public float SlowPercent => 0.3f;
        public int SlowDuration => 120;
        public float ArmorShredAmount => 5f;
        public int ArmorShredDuration => 180;
        public float WeakenPercent => 0.15f;
        public float LifeStealPercent => 0.1f;
        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 1;
            Item.value = 50000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item1;
            Item.damage = 14;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.crit = 4;
            Item.noMelee = true;

            // 初始化弹幕类型数组
            _modeProjectileTypes = new[]
            {
                ModContent.ProjectileType<IceSnowBaseProj>(),       // 模式0：冰晶
                ModContent.ProjectileType<IceShardScatterProj>(),   // 模式1：冰锥散射
                ModContent.ProjectileType<IceFieldProj>(),          // 模式2：冰霜领域
                ModContent.ProjectileType<IceBlizzardProj>(),       // 模式3：暴风雪
                ModContent.ProjectileType<IceExplosionProj>(),      // 模式4：冰爆术
            };

            // 默认使用模式0
            Item.shoot = _modeProjectileTypes[0];
            Item.shootSpeed = _modeShootSpeeds[0];
        }

        // ===== 切换攻击方式（R 键切换）=====

        /// <summary>上次 R 键状态，用于检测按下事件</summary>
        private bool _lastRKeyState = false;

        public override void UpdateInventory(Player player)
        {
            base.UpdateInventory(player);

            // 只在手持该物品时检测 R 键切换
            if (player.HeldItem.type == Item.type)
            {
                bool currentRState = Main.keyState.IsKeyDown(Keys.R);
                if (currentRState && !_lastRKeyState)
                {
                    // R 键按下事件：切换攻击模式
                    SwitchAttackMode(player);
                }
                _lastRKeyState = currentRState;
            }
            else
            {
                _lastRKeyState = false;
            }
        }

        private void SwitchAttackMode(Player player)
        {
            // 切换到下一个攻击模式
            attackMode = (attackMode + 1) % _modeProjectileTypes.Length;

            // 更新物品属性
            Item.shoot = _modeProjectileTypes[attackMode];
            Item.shootSpeed = _modeShootSpeeds[attackMode];
            Item.damage = (int)(Item.OriginalDamage * _modeDamageMultipliers[attackMode]);

            // 显示切换提示
            string modeName = AttackModeNames[attackMode];
            CombatText.NewText(player.getRect(), Color.Cyan, $"切换至：{modeName}");
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);

            // 显示当前攻击模式
            string modeName = AttackModeNames[attackMode];
            tooltips.Add(new TooltipLine(Mod, "AttackMode", $"当前攻击方式：[c/66CCFF:{modeName}]"));
            tooltips.Add(new TooltipLine(Mod, "SwitchHint", "手持时按 [c/66CCFF:R] 键切换攻击方式"));
        }

        public override bool CanUseItem(Player player)
        {
            // 确保当前弹幕类型正确（防止加载后未同步）
            if (attackMode >= 0 && attackMode < _modeProjectileTypes.Length)
            {
                Item.shoot = _modeProjectileTypes[attackMode];
                Item.shootSpeed = _modeShootSpeeds[attackMode];
            }

            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (attackMode == 1)
            {
                // 模式1：冰锥散射 — 发射扇形散射的冰锥
                IceShardScatterProj.SpawnScatter(
                    player, source, position, velocity,
                    type, damage, knockback
                );

                return false; // 阻止默认发射
            }

            if (attackMode == 2)
            {
                // 模式2：冰霜领域 — 在鼠标位置生成冰霜区域
                Vector2 mousePos = Main.MouseWorld;

                Projectile.NewProjectile(
                    source,
                    mousePos,
                    Vector2.Zero,
                    type,
                    damage,
                    0f,
                    player.whoAmI
                );

                return false;
            }

            if (attackMode == 3)
            {
                // 模式3：暴风雪 — 在鼠标上方生成大量冰晶
                Vector2 mousePos = Main.MouseWorld;

                IceBlizzardProj.SpawnBlizzard(
                    player, source, mousePos,
                    type, damage, knockback
                );

                return false;
            }

            // 模式0/4：使用默认发射逻辑
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }

        // ===== 持久化 =====

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);
            tag["attackMode"] = attackMode;
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            attackMode = tag.GetInt("attackMode");

            // 恢复弹幕类型
            if (_modeProjectileTypes != null && attackMode >= 0 && attackMode < _modeProjectileTypes.Length)
            {
                Item.shoot = _modeProjectileTypes[attackMode];
                Item.shootSpeed = _modeShootSpeeds[attackMode];
                Item.damage = (int)(Item.OriginalDamage * _modeDamageMultipliers[attackMode]);
            }
        }
    }
}
