using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{    /// <summary>
    /// 三转冰雪道蛊虫 — 冰爆蛊
    /// 白凝冰的创新蛊虫，能引爆冰块造成范围爆炸。
    /// </summary>
    public class BingBaoGu : IceSnowWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 30;
        protected override int _useTime => 30;
        protected override int _guLevel => 3;
        protected override int controlQiCost => 16;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Slow };
        public float DoTDuration => 0;
        public float DoTDamage => 0;
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
            Item.knockBack = 5f;
            Item.crit = 2;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 5500;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.Item30;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<BingBaoProj>();
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
