using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Five
{    /// <summary>
    /// 五转力道蛊虫 — 定音蛊
    /// 黑楼兰的五转蛊，能发出无形声波束缚目标。
    /// </summary>

    public class DingYinGu : VoiceWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 48;
        protected override int _useTime => 32;
        protected override int _guLevel => 5;
        protected override int controlQiCost => 24;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Slow, DaoEffectTags.Weaken };
        public float DoTDuration => 0;
        public float DoTDamage => 0;
        public float SlowPercent => 0.6f;
        public int SlowDuration => 240;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0.15f;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.damage = 60;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.crit = 2;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
            Item.value = 20000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 32;
            Item.useTime = 32;
            Item.UseSound = SoundID.Item28;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<DingYinProj>();
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
