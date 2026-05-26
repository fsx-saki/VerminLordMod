using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;
using VerminLordMod.Common.ImplementationTracker;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转毒道攻击蛊", "五转", "毒")]
    public class TianDuXianGu : PoisonWeapon, IOnHitEffectProvider
    {
        protected override int _guLevel => 5;
        protected override int qiCost => 50;
        protected override int controlQiCost => 25;
        protected override int _useTime => 32;
        protected override float unitConntrolRate => 8;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT, DaoEffectTags.Weaken };
        public float DoTDuration => 6f;
        public float DoTDamage => 12f;
        public float SlowPercent => 0.3f;
        public int SlowDuration => 180;
        public float ArmorShredAmount => 10f;
        public int ArmorShredDuration => 180;
        public float WeakenPercent => 0.1f;
        public float LifeStealPercent => 0.1f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 95;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.crit = 6;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 50000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 32;
            Item.useTime = 32;
            Item.UseSound = SoundID.Item17;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<TianDuProj>();
            Item.shootSpeed = 10f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
