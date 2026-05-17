using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{
    public class FlyingBoneShieldGu : BoneWeapon
    {
        protected override int qiCost => 30;
        protected override int _useTime => 40;
        protected override int _guLevel => 3;
        protected override int controlQiCost => 15;
        protected override float unitConntrolRate => 15;

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.damage = 20;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.crit = 2;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 6000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 40;
            Item.useTime = 40;
            Item.UseSound = SoundID.Item28;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<FlyingBoneShieldProj>();
            Item.shootSpeed = 0f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int shieldCount = 3;
            for (int i = 0; i < shieldCount; i++)
            {
                float angle = MathHelper.TwoPi * i / shieldCount;
                Vector2 offset = angle.ToRotationVector2() * 60f;
                Projectile.NewProjectile(source, player.Center + offset, Vector2.Zero,
                    type, damage, knockback, player.whoAmI, i, angle);
            }
            return false;
        }
    }
}