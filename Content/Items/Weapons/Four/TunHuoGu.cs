using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Four
{
    /// <summary>
    /// 四转火道蛊虫 — 吞火蛊
    /// 四转存储蛊，可以吞噬火焰。
    /// </summary>
    public class TunHuoGu : FireWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 35;
        protected override int _useTime => 16;
        protected override int _guLevel => 4;
        protected override int controlQiCost => 18;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
        public float DoTDuration => 3f;
        public float DoTDamage => 12f;
        public float SlowPercent => 0.3f;
        public int SlowDuration => 180;
        public float ArmorShredAmount => 11f;
        public int ArmorShredDuration => 180;
        public float WeakenPercent => 0.15f;
        public float LifeStealPercent => 0.1f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 55;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 5f;
            Item.crit = 6;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 12000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 16;
            Item.useTime = 16;
            Item.UseSound = SoundID.Item4;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<TunHuoProj>();
            Item.shootSpeed = 13f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }
    }
}
