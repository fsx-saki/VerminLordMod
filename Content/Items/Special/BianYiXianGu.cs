using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;
using Terraria.DataStructures;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转变化道仙蛊", "五转", "变")]
    public class BianYiXianGu : VariationWeapon
    {
        protected override int qiCost => 50;
        protected override int controlQiCost => 25;
        protected override int _useTime => 36;
        protected override int _guLevel => 5;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.damage = 85;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 1;
            Item.value = 1000000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 36;
            Item.useTime = 36;
            Item.UseSound = SoundID.Item4;
            Item.scale = 1f;
            Item.shootSpeed = 10f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float roll = Main.rand.NextFloat();

            if (roll < 0.33f)
            {
                Projectile.NewProjectile(source, position, velocity.SafeNormalize(Vector2.Zero) * 12f,
                    ModContent.ProjectileType<BianYiPierceProj>(), damage, knockback, player.whoAmI);
            }
            else if (roll < 0.66f)
            {
                Projectile.NewProjectile(source, position, velocity.SafeNormalize(Vector2.Zero) * 7f,
                    ModContent.ProjectileType<BianYiHomingProj>(), damage, knockback, player.whoAmI);
            }
            else
            {
                Projectile.NewProjectile(source, position, velocity.SafeNormalize(Vector2.Zero) * 8f,
                    ModContent.ProjectileType<BianYiExplosiveProj>(), damage, knockback, player.whoAmI);
            }

            return false;
        }
    }
}
