using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Five
{    /// <summary>
    /// 五转道蛊虫 — HuoGuangZhuTianGu
    /// 蛊虫
    /// </summary>
    public class HuoGuangZhuTianGu : FireWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 60;
        protected override int _useTime => 35;
        protected override int _guLevel => 5;
        protected override int controlQiCost => 30;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
        public float DoTDuration => 5f;
        public float DoTDamage => 12f;
        public float SlowPercent => 0;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 150;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 5f;
            Item.crit = 8;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
            Item.value = 28000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 35;
            Item.useTime = 35;
            Item.UseSound = SoundID.Item14;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<HuoGuangZhuTianProj>();
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
