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
    /// 水道基础蛊 — 水弹蛊
    /// 水道技术储备库的武器端实现。
    /// 聚焦水系特性：水弹、漩涡吸附、治疗回复、摆动追踪、法阵连击、圆环汇聚。
    /// 手持时按 R 键切换当前攻击模式。
    ///
    /// 攻击模式（6种截然不同的水系效果）：
    ///   模式0 - 水弹：发射基础水弹，命中后水花爆裂
    ///   模式1 - 漩涡：在鼠标位置生成漩涡，吸附并伤害敌人
    ///   模式2 - 治疗：发射治疗水弹，命中敌人时回复玩家生命
    ///   模式3 - 摆弹：发射小水弹，小角度左右摆动并以弧线追踪鼠标指针
    ///   模式4 - 法阵：在鼠标位置生成水法阵，从四周汇聚水弹攻击法阵中心
    ///   模式5 - 圆环汇聚：从鼠标周围大圆环上随机位置生成水弹，向鼠标汇聚
    /// </summary>
    public class WaterBaseGu : WaterWeapon, IOnHitEffectProvider
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
            "水弹",         // 模式0：基础水弹，水花爆裂
            "漩涡",         // 模式1：漩涡吸附，持续范围伤害
            "治疗",         // 模式2：治疗水弹，命中回复生命
            "摆弹",         // 模式3：摆动水弹，弧线追踪鼠标
            "法阵",         // 模式4：水法阵，汇聚水弹连击
            "圆环汇聚",     // 模式5：圆环随机生成水弹，向鼠标汇聚
        };

        /// <summary>所有攻击模式对应的弹幕类型</summary>
        private int[] _modeProjectileTypes;

        /// <summary>所有攻击模式对应的 shootSpeed</summary>
        private readonly float[] _modeShootSpeeds = new[]
        {
            10f,  // 模式0：水弹
            0f,   // 模式1：漩涡（在鼠标位置直接生成）
            8f,   // 模式2：治疗（中等速度）
            9f,   // 模式3：摆弹（摆动追踪，中等速度）
            0f,   // 模式4：法阵（在鼠标位置直接生成）
            0f,   // 模式5：圆环汇聚（在鼠标位置周围圆环上生成）
        };

        /// <summary>所有攻击模式对应的伤害倍率</summary>
        private readonly float[] _modeDamageMultipliers = new[]
        {
            1.0f,  // 模式0：水弹
            0.5f,  // 模式1：漩涡（持续范围伤害）
            0.7f,  // 模式2：治疗（兼顾伤害和治疗）
            0.75f, // 模式3：摆弹（小水弹，追踪，伤害略低）
            0.6f,  // 模式4：法阵（法阵本体无直接伤害，汇聚水弹多次伤害）
            0.7f,  // 模式5：圆环汇聚（多弹汇聚，单发伤害略低）
        };

        /// <summary>所有攻击模式对应的使用间隔（帧）</summary>
        private readonly int[] _modeUseTimes = new[]
        {
            20,  // 模式0：水弹，快速
            30,  // 模式1：漩涡，较慢
            25,  // 模式2：治疗，中等
            18,  // 模式3：摆弹，快速（小水弹，快速连发）
            35,  // 模式4：法阵，较慢（法阵持续 2.5 秒，需要冷却）
            14,  // 模式5：圆环汇聚，快速
        };

        // ===== IOnHitEffectProvider =====
        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Slow };
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
                ModContent.ProjectileType<WaterBaseProj>(),          // 模式0：水弹
                ModContent.ProjectileType<WaterVortexProj>(),        // 模式1：漩涡
                ModContent.ProjectileType<WaterHealProj>(),          // 模式2：治疗
                ModContent.ProjectileType<WaterSwingProj>(),         // 模式3：摆弹
                ModContent.ProjectileType<WaterFormationProj>(),     // 模式4：法阵
                ModContent.ProjectileType<WaterRingConvergeProj>(),  // 模式5：圆环汇聚
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

            int mode = attackMode;
            if (mode < 0 || mode >= AttackModeNames.Length)
                mode = 0;

            string modeName = AttackModeNames[mode];
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

        /// <summary>
        /// 判断当前模式是否需要在鼠标位置直接生成弹幕（而非从玩家位置发射）
        /// </summary>
        private bool IsMouseSpawnMode(int mode)
        {
            // 模式1（漩涡）、模式4（法阵）、模式5（圆环汇聚）在鼠标位置生成
            return mode == 1 || mode == 4 || mode == 5;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (IsMouseSpawnMode(attackMode))
            {
                // 在鼠标位置生成弹幕
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

            // 模式0/2/3：使用默认发射逻辑
            // 模式0：WaterBaseProj（水弹）— 抛物线弹道，受重力影响，碰物块即破
            // 模式2：WaterHealProj（治疗）— 沿鼠标方向飞行，命中回复生命
            // 模式3：WaterSwingProj（摆弹）— 沿鼠标方向发射，SwingHomingBehavior 接管后摆动追踪
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

            if (attackMode < 0 || attackMode >= AttackModeNames.Length)
                attackMode = 0;

            if (_modeProjectileTypes != null && attackMode >= 0 && attackMode < _modeProjectileTypes.Length)
            {
                Item.shoot = _modeProjectileTypes[attackMode];
                Item.shootSpeed = _modeShootSpeeds[attackMode];
                Item.damage = (int)(Item.OriginalDamage * _modeDamageMultipliers[attackMode]);
            }
        }
    }
}
