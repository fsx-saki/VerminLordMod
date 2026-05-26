using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;
using Terraria.DataStructures;

namespace VerminLordMod.Content.Items.Weapons.Four
{    /// <summary>
    /// 四转道蛊虫 — WindFlowerGu
    /// 蛊虫
    /// </summary>
    public class WindFlowerGu : WindWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 35;
        protected override int _useTime => 22;
        protected override int _guLevel => 4;
        protected override int controlQiCost => 18;
        protected override float unitConntrolRate => 15;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT, DaoEffectTags.Slow };
        public float DoTDuration => 2.5f;
        public float DoTDamage => 5f;
        public float SlowPercent => 0.25f;
        public int SlowDuration => 90;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.damage = 28;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 2f;
            Item.crit = 4;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 8000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 22;
            Item.useTime = 22;
            Item.UseSound = SoundID.Item29;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<WindFlowerProj>();
            Item.shootSpeed = 9f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2 velocity,
            int type, int damage, float knockback)
        {
            int count = 5;
            float spread = MathHelper.ToRadians(25);
            for (int i = 0; i < count; i++)
            {
                float rotOffset = MathHelper.Lerp(-spread / 2, spread / 2, (float)i / (count - 1));
                Vector2 vel = velocity.RotatedBy(rotOffset);
                Projectile.NewProjectile(source, position, vel, type, damage, knockback, player.whoAmI);
            }
            return false;
        }
    }
}