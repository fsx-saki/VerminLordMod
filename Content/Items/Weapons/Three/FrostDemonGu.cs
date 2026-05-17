using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{
    public class FrostDemonGu : IceSnowWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 35;
        protected override int _useTime => 36;
        protected override int _guLevel => 3;
        protected override int controlQiCost => 18;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT, DaoEffectTags.Slow };
        public float DoTDuration => 4f;
        public float DoTDamage => 7f;
        public float SlowPercent => 0.6f;
        public int SlowDuration => 180;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.damage = 35;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 6f;
            Item.crit = 2;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 6500;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 36;
            Item.useTime = 36;
            Item.UseSound = SoundID.Item30;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<FrostDemonProj>();
            Item.shootSpeed = 0f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2 velocity,
            int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.Center, Microsoft.Xna.Framework.Vector2.Zero,
                type, damage, knockback, player.whoAmI);
            return false;
        }
    }
}