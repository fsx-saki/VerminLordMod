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
    /// 炎道基础蛊 — 火苗蛊
    /// 炎道技术储备库的武器端实现。
    /// 聚焦火焰特性：爆炸、滞留燃烧、陨石天降、火焰喷射、星火燎原。
    /// 手持时按 R 键切换当前攻击模式。
    ///
    /// 攻击模式（5种截然不同的火焰效果）：
    ///   模式0 - 星火燎原：发射星火弹，命中/碰撞后爆炸产生更多星火弹
    ///   模式1 - 爆炸冲击：发射爆炸弹，落地产生大爆炸 + 火焰墙 + 星火散射
    ///   模式2 - 陨石天降：召唤陨石从天而降，落地大爆炸 + 多个火焰墙
    ///   模式3 - 火焰喷射：持续喷射火焰，路径上产生火焰墙
    ///   模式4 - 烈焰风暴：在鼠标位置召唤火焰漩涡，持续燃烧范围内敌人
    /// </summary>
    public class FireBaseGu : FireWeapon, IOnHitEffectProvider
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
            "星火燎原",     // 模式0：星火弹 → 爆炸 → 更多星火弹（连锁爆炸）
            "爆炸冲击",     // 模式1：爆炸弹 → 大爆炸 + 火焰墙 + 星火散射
            "陨石天降",     // 模式2：召唤陨石从天而降 → 大爆炸 + 多个火焰墙
            "火焰喷射",     // 模式3：持续喷射火焰 → 路径产生火焰墙
            "烈焰风暴",     // 模式4：在鼠标位置召唤火焰漩涡，持续燃烧
        };

        /// <summary>所有攻击模式对应的弹幕类型</summary>
        private int[] _modeProjectileTypes;

        /// <summary>所有攻击模式对应的 shootSpeed</summary>
        private readonly float[] _modeShootSpeeds = new[]
        {
            12f,  // 模式0：星火燎原
            8f,   // 模式1：爆炸冲击
            0f,   // 模式2：陨石天降（速度由重力控制）
            16f,  // 模式3：火焰喷射
            0f,   // 模式4：烈焰风暴（在鼠标位置直接生成）
        };

        /// <summary>所有攻击模式对应的伤害倍率</summary>
        private readonly float[] _modeDamageMultipliers = new[]
        {
            1.0f,  // 模式0
            1.5f,  // 模式1：高伤害爆炸
            2.0f,  // 模式2：陨石，最高单发伤害
            0.8f,  // 模式3：火焰喷射，持续伤害
            0.6f,  // 模式4：烈焰风暴，持续范围伤害
        };

        /// <summary>所有攻击模式对应的使用间隔（帧）</summary>
        private readonly int[] _modeUseTimes = new[]
        {
            20,  // 模式0：快速连发
            35,  // 模式1：爆炸弹，较慢
            45,  // 模式2：陨石，最慢
            10,  // 模式3：火焰喷射，快速
            30,  // 模式4：烈焰风暴，中等
        };

        // ===== IOnHitEffectProvider =====
        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
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
                ModContent.ProjectileType<FireBaseProj>(),       // 模式0：星火燎原
                ModContent.ProjectileType<FireExplosionProj>(),  // 模式1：爆炸冲击
                ModContent.ProjectileType<FireMeteorProj>(),     // 模式2：陨石天降
                ModContent.ProjectileType<FireStreamProj>(),     // 模式3：火焰喷射
                ModContent.ProjectileType<FireBaseProj>(),       // 模式4：烈焰风暴（占位，实际由 Shoot 处理）
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
            CombatText.NewText(player.getRect(), Color.OrangeRed, $"切换至：{modeName}");
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);

            // 显示当前攻击模式
            string modeName = AttackModeNames[attackMode];
            tooltips.Add(new TooltipLine(Mod, "AttackMode", $"当前攻击方式：[c/FF6600:{modeName}]"));
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
                // 模式2：陨石天降 — 在鼠标上方高空生成陨石
                Vector2 mousePos = Main.MouseWorld;
                // 在鼠标上方 400~600 像素高空
                float offsetY = Main.rand.NextFloat(400f, 600f);
                // 水平随机偏移 ±80 像素
                float offsetX = Main.rand.NextFloat(-80f, 80f);
                Vector2 spawnPos = new Vector2(mousePos.X + offsetX, mousePos.Y - offsetY);

                // 初始速度：轻微水平偏移 + 无垂直速度（重力会拉下来）
                Vector2 initVel = new Vector2(Main.rand.NextFloat(-1f, 1f), 0f);

                Projectile.NewProjectile(
                    source,
                    spawnPos,
                    initVel,
                    type,
                    damage,
                    knockback,
                    player.whoAmI
                );

                return false; // 阻止默认发射
            }

            if (attackMode == 4)
            {
                // 模式4：烈焰风暴 — 在鼠标位置召唤火焰漩涡
                // 生成多个 FireFlameWallProj 围绕鼠标位置呈环形分布
                Vector2 mousePos = Main.MouseWorld;
                int flameWallType = ModContent.ProjectileType<FireFlameWallProj>();
                int wallCount = 6;

                for (int i = 0; i < wallCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / wallCount;
                    float radius = 50f;
                    Vector2 spawnPos = mousePos + angle.ToRotationVector2() * radius;

                    Projectile.NewProjectile(
                        source,
                        spawnPos,
                        Vector2.Zero,
                        flameWallType,
                        (int)(damage * 0.8f),
                        0f,
                        player.whoAmI
                    );
                }

                // 额外在中心生成一个火焰墙
                Projectile.NewProjectile(
                    source,
                    mousePos,
                    Vector2.Zero,
                    flameWallType,
                    damage,
                    0f,
                    player.whoAmI
                );

                return false; // 阻止默认发射
            }

            // 模式0/1/3：使用默认发射逻辑
            // 模式0：FireBaseProj（星火燎原）— 沿鼠标方向直线飞行
            // 模式1：FireExplosionProj（爆炸冲击）— 飞行到目标位置后爆炸
            // 模式3：FireStreamProj（火焰喷射）— 沿鼠标方向高速飞行
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
