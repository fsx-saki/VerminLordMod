using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.Weapons;
using VerminLordMod.Content.Items.Weapons.Daos;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 蓄力武器模板 — 抽象出 BloodHandprintGu 的蓄力模式。
    ///
    /// 使用方式：
    /// <code>
    /// class MyChargeGu : ChargeWeaponTemplate
    /// {
    ///     protected override int ProjType => ModContent.ProjectileType<MyChargeProj>();
    ///     protected override int BloodCostInterval => 60; // 每秒扣 1 滴
    ///     protected override int qiCost => 300;
    ///     protected override int _guLevel => 5;
    /// }
    /// </code>
    ///
    /// 核心机制：
    /// - channel = true，长按蓄力
    /// - 每秒扣血（由 BloodCostInterval 控制间隔）
    /// - 只生成一个弹幕（_activeProjIndex 追踪）
    /// - 一轮一次使用（_roundComplete 标志，弹幕推出后必须松手再按）
    /// - 弹幕通过 ai[2] = 1f 通知武器端"本轮完成"
    /// </summary>
    public abstract class ChargeWeaponTemplate : BloodWeapon
    {
        // ===== 子类必须重写的参数 =====

        /// <summary>蓄力弹幕的类型 ID</summary>
        protected abstract int ProjType { get; }

        /// <summary>扣血间隔（帧），60 = 1秒</summary>
        protected abstract int BloodCostInterval { get; }

        // ===== 可选重写的参数 =====

        /// <summary>蓄力时是否扣血（默认 true）</summary>
        protected virtual bool EnableBloodCost => true;

        /// <summary>扣血时的粒子效果（默认 Blood dust）</summary>
        protected virtual void OnBloodCost(Player player)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustPerfect(
                    player.Center + Main.rand.NextVector2Circular(25f, 25f),
                    DustID.Blood,
                    Main.rand.NextVector2Circular(4f, 4f),
                    100, Color.DarkRed, Main.rand.NextFloat(1f, 2f)
                );
                d.noGravity = true;
            }
        }

        /// <summary>扣血时的音效（默认 NPCHit18）</summary>
        protected virtual SoundStyle? BloodCostSound => SoundID.NPCHit18;

        /// <summary>弹幕推出后，通过 ai[2] 通知完成的标志值（默认 1f）</summary>
        protected virtual float RoundCompleteFlag => 1f;

        // ===== 运行时状态 =====

        private int _chargeTimer;
        private int _activeProjIndex = -1;
        private bool _roundComplete;

        /// <summary>
        /// 子类在此方法中设置 Item 的基础属性（width, height, damage, DamageType 等）。
        /// 在 SetDefaults 中，先调用此方法，再设置 channel/shoot 等蓄力相关属性。
        /// </summary>
        protected virtual void SetupItemDefaults() { }

        public override void SetDefaults()
        {
            base.SetDefaults();
            SetupItemDefaults();
            Item.channel = true;
            Item.shoot = ProjType;
            Item.shootSpeed = 12f;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.noMelee = true;
            Item.noUseGraphic = false;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
                return false;

            // 本轮已完成，不允许生成新弹幕
            if (_roundComplete)
                return false;

            // 检查是否已有活跃的蓄力弹幕
            bool hasActiveProj = false;
            if (_activeProjIndex >= 0 && _activeProjIndex < Main.maxProjectiles)
            {
                Projectile existing = Main.projectile[_activeProjIndex];
                if (existing.active && existing.type == type && existing.owner == player.whoAmI)
                {
                    hasActiveProj = true;
                }
            }

            if (!hasActiveProj)
            {
                _chargeTimer = 0;
                int proj = Projectile.NewProjectile(source, position, velocity,
                    type, damage, knockback, player.whoAmI);
                _activeProjIndex = proj;
            }

            return false; // 阻止默认射击
        }

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            if (player.channel && player.altFunctionUse != 2 && player.HeldItem.type == Item.type)
            {
                // 本轮已完成，蓄力阶段不再扣血
                if (_roundComplete)
                    return;

                // 扣血逻辑
                if (EnableBloodCost)
                {
                    _chargeTimer++;
                    if (_chargeTimer % BloodCostInterval == 0)
                    {
                        player.statLife--;
                        if (player.statLife <= 0)
                        {
                            player.statLife = 0;
                            player.KillMe(PlayerDeathReason.ByCustomReason(
                                player.name + "的血被蛊虫吸干了"), 10, 0);
                        }

                        OnBloodCost(player);
                        if (BloodCostSound.HasValue)
                            SoundEngine.PlaySound(BloodCostSound.Value, player.Center);
                    }
                }
            }
            else
            {
                // 不在蓄力时（松手了）
                CheckRoundComplete();
                _chargeTimer = 0;
            }
        }

        /// <summary>
        /// 检查弹幕是否已推出，更新 _roundComplete 状态。
        /// 弹幕推出时设置 ai[2] = RoundCompleteFlag，武器端检测到后标记本轮完成。
        /// </summary>
        private void CheckRoundComplete()
        {
            if (_activeProjIndex >= 0 && _activeProjIndex < Main.maxProjectiles)
            {
                Projectile existing = Main.projectile[_activeProjIndex];
                if (existing.active && existing.type == Item.shoot)
                {
                    if (existing.ai[2] == RoundCompleteFlag)
                        _roundComplete = true;
                }
                else
                {
                    // 弹幕已死亡 → 完全重置
                    ResetRound();
                }
            }
            else
            {
                ResetRound();
            }
        }

        /// <summary>重置本轮状态（弹幕死亡或松手后）</summary>
        protected void ResetRound()
        {
            _roundComplete = false;
            _activeProjIndex = -1;
        }

        /// <summary>强制结束本轮（外部调用，如玩家死亡）</summary>
        protected void ForceEndRound()
        {
            if (_activeProjIndex >= 0 && _activeProjIndex < Main.maxProjectiles)
            {
                Projectile existing = Main.projectile[_activeProjIndex];
                if (existing.active && existing.type == Item.shoot)
                {
                    existing.Kill();
                }
            }
            ResetRound();
        }

        /// <summary>当前是否有活跃的蓄力弹幕</summary>
        protected bool HasActiveProj => _activeProjIndex >= 0 &&
            _activeProjIndex < Main.maxProjectiles &&
            Main.projectile[_activeProjIndex].active &&
            Main.projectile[_activeProjIndex].type == Item.shoot;
    }
}
