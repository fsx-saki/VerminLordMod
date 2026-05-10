using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
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
    /// 风道基础蛊 — 风刃蛊
    /// 风道技术储备库的武器端实现。
    /// 聚焦风系特性：风刃、旋风、风墙、风卷残云、风爆。
    /// 手持时按 R 键切换当前攻击模式。
    ///
    /// 攻击模式（5种截然不同的风系效果）：
    ///   模式0 - 风刃：高速直线飞行，穿透敌人
    ///   模式1 - 旋风：旋转前进，持续伤害
    ///   模式2 - 风墙：滞留风墙，击退敌人
    ///   模式3 - 风卷残云：追踪+波浪飞行，多重伤害
    ///   模式4 - 风爆：范围风爆，大范围击退+伤害
    /// </summary>
    public class WindBaseGu : WindWeapon, IOnHitEffectProvider
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
            "风刃",         // 模式0：高速风刃，穿透敌人
            "旋风",         // 模式1：旋转前进，持续伤害
            "风墙",         // 模式2：滞留风墙，击退敌人
            "风卷残云",     // 模式3：追踪+波浪，多重伤害
            "风爆",         // 模式4：范围风爆，大范围击退
        };

        /// <summary>所有攻击模式对应的弹幕类型</summary>
        private int[] _modeProjectileTypes;

        /// <summary>所有攻击模式对应的 shootSpeed</summary>
        private readonly float[] _modeShootSpeeds = new[]
        {
            14f,  // 模式0：风刃（高速）
            8f,   // 模式1：旋风（中等速度）
            0f,   // 模式2：风墙（在鼠标位置直接生成）
            10f,  // 模式3：风卷残云（追踪）
            10f,  // 模式4：风爆（飞行到目标后爆炸）
        };

        /// <summary>所有攻击模式对应的伤害倍率</summary>
        private readonly float[] _modeDamageMultipliers = new[]
        {
            1.0f,  // 模式0：风刃
            0.7f,  // 模式1：旋风（持续伤害）
            0.5f,  // 模式2：风墙（持续范围伤害）
            0.8f,  // 模式3：风卷残云（追踪+多重伤害）
            1.5f,  // 模式4：风爆（高单发伤害）
        };

        /// <summary>所有攻击模式对应的使用间隔（帧）</summary>
        private readonly int[] _modeUseTimes = new[]
        {
            20,  // 模式0：风刃，快速
            25,  // 模式1：旋风，中等
            30,  // 模式2：风墙，较慢
            25,  // 模式3：风卷残云，中等
            35,  // 模式4：风爆，较慢
        };

        // ===== IOnHitEffectProvider =====
        public DaoEffectTags[] OnHitEffects => Array.Empty<DaoEffectTags>();
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
            Item.damage = 16;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.crit = 4;
            Item.noMelee = true;

            // 初始化弹幕类型数组
            _modeProjectileTypes = new[]
            {
                ModContent.ProjectileType<WindBaseProj>(),       // 模式0：风刃
                ModContent.ProjectileType<WindCycloneProj>(),    // 模式1：旋风
                ModContent.ProjectileType<WindWallProj>(),       // 模式2：风墙
                ModContent.ProjectileType<WindStormProj>(),      // 模式3：风卷残云
                ModContent.ProjectileType<WindBurstProj>(),      // 模式4：风爆
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
            if (attackMode == 2)
            {
                // 模式2：风墙 — 在鼠标位置生成风墙
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

                return false; // 阻止默认发射
            }

            // 模式0/1/3/4：使用默认发射逻辑
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
