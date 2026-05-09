using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Common.GlobalProjectiles;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Items.Weapons.Daos
{
    public abstract class DaoWeapon : GuWeaponItem, IKinematicProvider, IOnHitEffectProvider, ITacticalTriggerProvider
    {
        // ============================================================
        // 流派标识
        // ============================================================
        public abstract DaoType DaoType { get; }

        // ============================================================
        // Dust 类型映射
        // ============================================================
        private static readonly Dictionary<DaoType, int> DustTypeMap = new()
        {
            { DaoType.Ban, ModContent.DustType<BanDust>() },
            { DaoType.Blood, ModContent.DustType<BloodDust>() },
            { DaoType.Bone, ModContent.DustType<BoneDust>() },
            { DaoType.Charm, ModContent.DustType<CharmDust>() },
            { DaoType.Cloud, ModContent.DustType<CloudDust>() },
            { DaoType.Dark, ModContent.DustType<DarkDust>() },
            { DaoType.Draw, ModContent.DustType<DrawDust>() },
            { DaoType.Dream, ModContent.DustType<DreamDust>() },
            { DaoType.Eating, ModContent.DustType<EatingDust>() },
            { DaoType.Fire, ModContent.DustType<FireDust>() },
            { DaoType.Flying, ModContent.DustType<FlyingDust>() },
            { DaoType.Gold, ModContent.DustType<GoldDust>() },
            { DaoType.IceSnow, ModContent.DustType<IceSnowDust>() },
            { DaoType.Info, ModContent.DustType<InfoDust>() },
            { DaoType.Killing, ModContent.DustType<KillingDust>() },
            { DaoType.Knife, ModContent.DustType<KnifeDust>() },
            { DaoType.LifeDeath, ModContent.DustType<LifeDeathDust>() },
            { DaoType.Light, ModContent.DustType<LightDust>() },
            { DaoType.Lightning, ModContent.DustType<LightningDust>() },
            { DaoType.Love, ModContent.DustType<LoveDust>() },
            { DaoType.Luck, ModContent.DustType<LuckDust>() },
            { DaoType.Moon, ModContent.DustType<MoonDust>() },
            { DaoType.Mud, ModContent.DustType<MudDust>() },
            { DaoType.Pellet, ModContent.DustType<PelletDust>() },
            { DaoType.Person, ModContent.DustType<PersonDust>() },
            { DaoType.Poison, ModContent.DustType<PoisonDust>() },
            { DaoType.Power, ModContent.DustType<PowerDust>() },
            { DaoType.Practise, ModContent.DustType<PractiseDust>() },
            { DaoType.Qi, ModContent.DustType<QiDust>() },
            { DaoType.Rule, ModContent.DustType<RuleDust>() },
            { DaoType.Shadow, ModContent.DustType<ShadowDust>() },
            { DaoType.Sky, ModContent.DustType<SkyDust>() },
            { DaoType.Slave, ModContent.DustType<SlaveDust>() },
            { DaoType.Soul, ModContent.DustType<SoulDust>() },
            { DaoType.Space, ModContent.DustType<SpaceDust>() },
            { DaoType.Star, ModContent.DustType<StarDust>() },
            { DaoType.Stealing, ModContent.DustType<StealingDust>() },
            { DaoType.SuccessFailure, ModContent.DustType<SuccessFailureDust>() },
            { DaoType.Sword, ModContent.DustType<SwordDust>() },
            { DaoType.Tactical, ModContent.DustType<TacticalDust>() },
            { DaoType.Time, ModContent.DustType<TimeDust>() },
            { DaoType.Unreal, ModContent.DustType<UnrealDust>() },
            { DaoType.Variation, ModContent.DustType<VariationDust>() },
            { DaoType.Voice, ModContent.DustType<VoiceDust>() },
            { DaoType.Void, ModContent.DustType<VoidDust>() },
            { DaoType.War, ModContent.DustType<WarDust>() },
            { DaoType.Water, ModContent.DustType<WaterDust>() },
            { DaoType.Wind, ModContent.DustType<WindDust>() },
            { DaoType.Wisdom, ModContent.DustType<WisdomDust>() },
            { DaoType.Wood, ModContent.DustType<WoodDust>() },
            { DaoType.YinYang, ModContent.DustType<YinYangDust>() },
        };

        protected override int moddustType
        {
            get
            {
                if (DustTypeMap.TryGetValue(DaoType, out int dustType))
                    return dustType;
                return -1;
            }
        }

        // ============================================================
        // IKinematicProvider 默认实现
        // ============================================================
        public virtual int ProjectileType => Item.shoot;
        public virtual float ShootSpeed => Item.shootSpeed;
        public virtual int ShootCount => 1;
        public virtual float SpreadAngle => 0f;
        public virtual bool OverrideShootLogic => false;
        public virtual void ModifyProjectile(Projectile projectile, Player player) { }
        public virtual void CustomShoot(Player player, Vector2 position, Vector2 velocity, int damage, float knockback)
        {
            throw new NotImplementedException();
        }

        // ============================================================
        // IOnHitEffectProvider 默认实现
        // ============================================================
        public virtual DaoEffectTags[] OnHitEffects => Array.Empty<DaoEffectTags>();
        public virtual float DoTDuration => 0;
        public virtual float DoTDamage => 0;
        public virtual float SlowPercent => 0;
        public virtual int SlowDuration => 0;
        public virtual float ArmorShredAmount => 0;
        public virtual int ArmorShredDuration => 0;
        public virtual float WeakenPercent => 0;
        public virtual float LifeStealPercent => 0;
        public virtual void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        // ============================================================
        // ITacticalTriggerProvider 默认实现
        // ============================================================
        public virtual TacticalTrigger[] Triggers => Array.Empty<TacticalTrigger>();
        public virtual void CheckTriggers(Player player, out List<ActiveTacticalBuff> activeBuffs)
        {
            activeBuffs = new List<ActiveTacticalBuff>();
            foreach (var trigger in Triggers)
            {
                switch (trigger)
                {
                    case TacticalTrigger.OnLowHealth:
                        if ((float)player.statLife / player.statLifeMax2 < 0.3f)
                            activeBuffs.Add(new ActiveTacticalBuff { Trigger = trigger, DamageMultiplier = 1.4f, RemainingFrames = 2 });
                        break;
                    case TacticalTrigger.OnNightTime:
                        if (!Main.dayTime)
                            activeBuffs.Add(new ActiveTacticalBuff { Trigger = trigger, DamageMultiplier = 1.2f, RemainingFrames = 2 });
                        break;
                    case TacticalTrigger.OnAirborne:
                        if (!player.velocity.Y.Equals(0f) && !player.pulley)
                            activeBuffs.Add(new ActiveTacticalBuff { Trigger = trigger, CritBonus = 0.1f, RemainingFrames = 2 });
                        break;
                    case TacticalTrigger.OnEmptyQi:
                        var qiPlayer = player.GetModPlayer<QiResourcePlayer>();
                        if (qiPlayer.QiCurrent <= 0)
                            activeBuffs.Add(new ActiveTacticalBuff { Trigger = trigger, DamageMultiplier = 1.3f, RemainingFrames = 2 });
                        break;
                    case TacticalTrigger.OnStandingStill2s:
                        if (player.velocity.Length() < 0.1f && !player.controlLeft && !player.controlRight && !player.controlUp && !player.controlDown)
                            activeBuffs.Add(new ActiveTacticalBuff { Trigger = trigger, DamageMultiplier = 1.15f, SpeedMultiplier = 0.8f, RemainingFrames = 2 });
                        break;
                    // OnCombo10, OnPerfectDodge, OnBackstab, OnKill, OnHitTaken5, OnAllyDeath
                    // 由 TacticalTriggerSystem 通过 TacticalTriggerPlayer 状态追踪处理（D-29）
                    // 此处不再重复实现，由系统在 OnProjectileHit 中自动合并事件触发Buff
                    case TacticalTrigger.OnCombo10:
                    case TacticalTrigger.OnPerfectDodge:
                    case TacticalTrigger.OnBackstab:
                    case TacticalTrigger.OnKill:
                    case TacticalTrigger.OnHitTaken5:
                    case TacticalTrigger.OnAllyDeath:
                    default:
                        break;
                }
            }
        }

        // ============================================================
        // Shoot 重写
        // ============================================================
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 道痕增幅
            float daoMult = player.GetModPlayer<DaoHenPlayer>().GetMultiplier(DaoType);
            int finalDamage = (int)(damage * daoMult);

            if (OverrideShootLogic)
            {
                CustomShoot(player, position, velocity, finalDamage, knockback);
                return false;
            }

            // 散射/默认发射
            for (int i = 0; i < ShootCount; i++)
            {
                Vector2 shootVel = velocity;
                if (ShootCount > 1 && SpreadAngle > 0)
                {
                    float rotOffset = MathHelper.Lerp(-SpreadAngle / 2, SpreadAngle / 2, (float)i / (ShootCount - 1));
                    shootVel = shootVel.RotatedBy(rotOffset);
                }

                Projectile p = Projectile.NewProjectileDirect(source, position, shootVel, ProjectileType, finalDamage, knockback, player.whoAmI);

                // 注入命中效应数据
                if (p.TryGetGlobalProjectile(out GuProjectileInfo info))
                {
                    info.EffectsOnHit = OnHitEffects != null ? CombineEffectTags(OnHitEffects) : DaoEffectTags.None;
                    info.DoTDamage = DoTDamage;
                    info.DoTDuration = DoTDuration;
                    info.SlowPercent = SlowPercent;
                    info.SlowDuration = SlowDuration;
                    info.ArmorShred = ArmorShredAmount;
                    info.ArmorShredDuration = ArmorShredDuration;
                    info.WeakenPercent = WeakenPercent;
                    info.LifeStealPercent = LifeStealPercent;
                    info.DaoMultiplier = daoMult;
                    info.SourceDao = DaoType;
                }

                // 应用弹道修改
                ModifyProjectile(p, player);
            }

            return false;
        }

        private DaoEffectTags CombineEffectTags(DaoEffectTags[] tags)
        {
            DaoEffectTags result = DaoEffectTags.None;
            foreach (var tag in tags) result |= tag;
            return result;
        }
    }
}
