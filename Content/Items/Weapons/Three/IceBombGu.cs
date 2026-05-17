using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{
    public class IceBombGu : IceSnowWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 30;
        protected override int _useTime => 28;
        protected override int _guLevel => 3;
        protected override int controlQiCost => 15;
        protected override float unitConntrolRate => 12;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT, DaoEffectTags.Slow };
        public float DoTDuration => 3f;
        public float DoTDamage => 6f;
        public float SlowPercent => 0.5f;
        public int SlowDuration => 150;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.damage = 30;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 4f;
            Item.crit = 4;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 5500;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 28;
            Item.useTime = 28;
            Item.UseSound = SoundID.Item28;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<IceBombProj>();
            Item.shootSpeed = 8f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}