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
    /// 支持多种攻击方式切换，手持时按 R 键切换当前攻击模式。
    /// 体现炎道"星火燎原，变化万千"的核心特点。
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
            "火弹",       // 模式0：火焰弹跳弹
            "火升腾",     // 模式1：升起→冲刺 - 3枚弹幕从角色中心升起后飞向鼠标
            "火散射",     // 模式2：散射追踪 - 向四周散射带平滑追踪的火焰弹
            "火爆弹",     // 模式3：大型爆弹 - 碰到物块或销毁时爆出一圈星火弹
            "火雨",       // 模式4：天降火雨 - 从天而降的3~5个大小不一的火焰弹
            "蓄力爆弹",   // 模式5：蓄力爆弹 - 长按蓄力，松手发射巨大火弹，产生巨大爆炸
        };

        /// <summary>所有攻击模式对应的弹幕类型</summary>
        private int[] _modeProjectileTypes;

        /// <summary>所有攻击模式对应的 shootSpeed</summary>
        private readonly float[] _modeShootSpeeds = new[]
        {
            12f,  // 模式0：火弹
            0f,   // 模式1：火升腾（速度由弹幕自身控制）
            6f,   // 模式2：火散射（初始速度，追踪会接管）
            10f,  // 模式3：火爆弹（直线飞行）
            0f,   // 模式4：火雨（速度由重力控制，武器不赋予初速度）
            0f,   // 模式5：蓄力爆弹（速度由弹幕自身控制，蓄力后推出）
        };

        /// <summary>所有攻击模式对应的伤害倍率</summary>
        private readonly float[] _modeDamageMultipliers = new[]
        {
            1.0f,  // 模式0
            0.8f,  // 模式1：3枚弹幕，单发伤害略低
            0.6f,  // 模式2：散射，单发伤害较低但数量多
            1.5f,  // 模式3：大型爆弹，单发高伤害
            0.7f,  // 模式4：火雨，3~5枚弹幕，单发伤害较低
            1.0f,  // 模式5：蓄力爆弹，基础伤害 1x，蓄力后最高 3x（由弹幕自身控制）
        };

        /// <summary>所有攻击模式对应的使用间隔（帧）</summary>
        private readonly int[] _modeUseTimes = new[]
        {
            20,  // 模式0
            30,  // 模式1：稍慢
            15,  // 模式2：快速散射
            35,  // 模式3：爆弹，较慢
            30,  // 模式4：火雨，中等速度
            10,  // 模式5：蓄力爆弹，快速响应蓄力（实际蓄力时间由弹幕控制）
        };

        // ===== 蓄力模式状态（模式5：蓄力爆弹）=====

        /// <summary>蓄力模式活跃弹幕索引</summary>
        private int _chargeActiveProjIndex = -1;

        /// <summary>
        /// 蓄力模式本轮是否已完成（弹幕已推出）。
        /// 当弹幕推出后设为 true，阻止 Shoot 生成新弹幕。
        /// 当玩家松手再按（channel 上升沿）时重置。
        /// </summary>
        private bool _chargeRoundComplete = false;

        /// <summary>上一帧的 channel 状态，用于检测上升沿</summary>
        private bool _lastChannelState = false;

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
                ModContent.ProjectileType<FireBaseProj>(),
                ModContent.ProjectileType<FireRiseProj>(),
                ModContent.ProjectileType<FireScatterProj>(),
                ModContent.ProjectileType<FireBombProj>(),
                ModContent.ProjectileType<FireRainProj>(),
                ModContent.ProjectileType<FireChargeProj>(),
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
            // 离开当前模式时，清理蓄力状态
            if (attackMode == 5)
            {
                ResetChargeRound();
                Item.channel = false;
                _lastChannelState = false;
            }

            // 切换到下一个攻击模式
            attackMode = (attackMode + 1) % _modeProjectileTypes.Length;

            // 更新物品属性
            Item.shoot = _modeProjectileTypes[attackMode];
            Item.shootSpeed = _modeShootSpeeds[attackMode];
            Item.damage = (int)(Item.OriginalDamage * _modeDamageMultipliers[attackMode]);

            // 蓄力模式需要 channel = true（长按蓄力）
            if (attackMode == 5)
            {
                Item.channel = true;
                _lastChannelState = false; // 确保进入模式5时 channel 上升沿检测正常
            }

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

        // ===== 蓄力模式处理（模式5）=====

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            // 只在模式5（蓄力爆弹）时处理蓄力逻辑
            if (attackMode != 5)
            {
                // 非蓄力模式确保 channel = false（避免干扰其他模式）
                if (Item.channel)
                    Item.channel = false;
                _lastChannelState = false;
                return;
            }

            if (player.HeldItem.type != Item.type) return;

            // 确保 channel = true（蓄力模式需要长按）
            if (!Item.channel)
                Item.channel = true;

            // 检测 channel 上升沿（松手→再按）：重置蓄力状态，允许生成新弹幕
            if (player.channel && !_lastChannelState)
            {
                // 检查旧弹幕是否已推出或死亡，如果是则重置
                bool oldProjDone = false;
                if (_chargeActiveProjIndex >= 0 && _chargeActiveProjIndex < Main.maxProjectiles)
                {
                    Projectile existing = Main.projectile[_chargeActiveProjIndex];
                    if (!existing.active || existing.type != Item.shoot || existing.ai[2] == 1f)
                        oldProjDone = true;
                }
                else
                {
                    oldProjDone = true;
                }

                if (oldProjDone)
                {
                    ResetChargeRound();
                }
            }

            // 更新上一帧 channel 状态
            _lastChannelState = player.channel;

            if (!player.channel)
            {
                // 松手时检查弹幕是否已推出（通过 ai[2] 标志）
                CheckChargeRoundComplete();
            }
        }

        /// <summary>
        /// 检查蓄力弹幕是否已推出（ai[2] == 1f），更新 _chargeRoundComplete 状态。
        /// </summary>
        private void CheckChargeRoundComplete()
        {
            if (_chargeActiveProjIndex >= 0 && _chargeActiveProjIndex < Main.maxProjectiles)
            {
                Projectile existing = Main.projectile[_chargeActiveProjIndex];
                if (existing.active && existing.type == Item.shoot)
                {
                    if (existing.ai[2] == 1f)
                        _chargeRoundComplete = true;
                }
                else
                {
                    // 弹幕已死亡 → 完全重置
                    ResetChargeRound();
                }
            }
            else
            {
                ResetChargeRound();
            }
        }

        /// <summary>重置蓄力本轮状态</summary>
        private void ResetChargeRound()
        {
            _chargeRoundComplete = false;
            _chargeActiveProjIndex = -1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (attackMode == 1)
            {
                // 模式1：发射 3 枚升起弹幕
                // 以玩家为中心呈扇形发射，初始速度向上并带角度散开
                Vector2 spawnPos = player.Center;

                for (int i = 0; i < 3; i++)
                {
                    // 以正上方（-90°）为中心，左右各 60° 扇形散开
                    // 角度：-150°, -90°, -30°（左、中、右）
                    float angle = -MathHelper.PiOver2 - MathHelper.Pi / 3f + i * MathHelper.Pi / 3f;
                    Vector2 initVel = angle.ToRotationVector2() * 3f;

                    // 弹幕自身会处理升起→冲刺逻辑
                    Projectile.NewProjectile(
                        source,
                        spawnPos,
                        initVel,
                        type,
                        damage,
                        knockback,
                        player.whoAmI
                    );
                }

                return false; // 阻止默认发射
            }

            if (attackMode == 2)
            {
                // 模式2：向四周散射 6 枚追踪弹幕（导弹式）
                // 每次点击发射 6 枚弹幕，呈圆形均匀分布
                // 弹幕先以散射初速度飞散，然后指数平滑追踪鼠标指针
                // 模拟"导弹发射→先散开→转大弯追踪目标"的效果
                Vector2 spawnPos = player.Center;
                int scatterCount = 6;

                for (int i = 0; i < scatterCount; i++)
                {
                    // 圆形均匀分布：每枚弹幕间隔 60°
                    float angle = MathHelper.TwoPi * i / scatterCount;
                    // 散射初速度（弹幕会先以此速度飞散，再转向追踪鼠标）
                    Vector2 initVel = angle.ToRotationVector2() * Item.shootSpeed;

                    Projectile.NewProjectile(
                        source,
                        spawnPos,
                        initVel,
                        type,
                        damage,
                        knockback,
                        player.whoAmI
                    );
                }

                return false; // 阻止默认发射
            }

            if (attackMode == 4)
            {
                // 模式4：天降火雨 — 在鼠标上方随机位置生成 3~5 个大小不一的火焰弹
                // 弹幕从高空自由落体下落，落地反弹后爆炸
                // 每个弹幕的大小、伤害随机，模拟"陨石雨"效果
                Vector2 mousePos = Main.MouseWorld;
                int rainCount = Main.rand.Next(3, 6); // 3~5 枚

                for (int i = 0; i < rainCount; i++)
                {
                    // 在鼠标位置上方 300~500 像素范围内随机水平偏移
                    float offsetX = Main.rand.NextFloat(-120f, 120f);
                    // 垂直位置在鼠标上方 300~500 像素（高空）
                    float offsetY = Main.rand.NextFloat(300f, 500f);
                    Vector2 spawnPos = new Vector2(mousePos.X + offsetX, mousePos.Y - offsetY);

                    // 初始速度：轻微随机水平速度 + 无垂直速度（重力会拉下来）
                    Vector2 initVel = new Vector2(Main.rand.NextFloat(-1f, 1f), 0f);

                    // 生成弹幕
                    int projIdx = Projectile.NewProjectile(
                        source,
                        spawnPos,
                        initVel,
                        type,
                        damage,
                        knockback,
                        player.whoAmI
                    );

                    // 设置随机大小（0.6~1.2）
                    if (Main.projectile[projIdx].ModProjectile is FireRainProj rainProj)
                    {
                        float sizeScale = Main.rand.NextFloat(0.6f, 1.2f);
                        rainProj.SetSizeScale(sizeScale);
                    }
                }

                return false; // 阻止默认发射
            }

            if (attackMode == 5)
            {
                // 模式5：蓄力爆弹 — 长按蓄力，松手发射巨大火弹，产生巨大爆炸
                // 蓄力弹幕由 HoldItem 中的 channel 机制控制生成和发射
                // Shoot 仅在首次按下时生成弹幕，后续由 ChargeProjectileBehavior 接管

                // 检查旧弹幕状态：如果已推出（ai[2]==1f）或已死亡，重置状态允许生成新弹幕
                if (_chargeActiveProjIndex >= 0 && _chargeActiveProjIndex < Main.maxProjectiles)
                {
                    Projectile existing = Main.projectile[_chargeActiveProjIndex];
                    if (!existing.active || existing.type != type || existing.owner != player.whoAmI || existing.ai[2] == 1f)
                    {
                        // 旧弹幕已推出或死亡 → 重置，允许生成新弹幕
                        ResetChargeRound();
                    }
                }
                else
                {
                    // 索引无效 → 重置
                    ResetChargeRound();
                }

                // 本轮已完成（弹幕已推出且玩家尚未松手再按），阻止再次生成
                if (_chargeRoundComplete)
                    return false;

                // 检查是否已有活跃的蓄力弹幕（正在蓄力中）
                bool hasActiveProj = false;
                if (_chargeActiveProjIndex >= 0 && _chargeActiveProjIndex < Main.maxProjectiles)
                {
                    Projectile existing = Main.projectile[_chargeActiveProjIndex];
                    if (existing.active && existing.type == type && existing.owner == player.whoAmI)
                        hasActiveProj = true;
                }

                if (!hasActiveProj)
                {
                    // 生成新的蓄力弹幕（初始位置在玩家中心，ChargeProjectileBehavior 会立即将其移到玩家前方）
                    int proj = Projectile.NewProjectile(
                        source,
                        player.Center,
                        Vector2.Zero,
                        type,
                        damage,
                        knockback,
                        player.whoAmI
                    );
                    _chargeActiveProjIndex = proj;
                }

                return false; // 阻止默认发射
            }

            // 模式0/3：使用默认发射逻辑
            // 模式0：FireBaseProj（星火弹）— 沿鼠标方向直线飞行
            // 模式3：FireBombProj（火爆弹）— 大型弹幕，销毁时爆出一圈星火弹
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
