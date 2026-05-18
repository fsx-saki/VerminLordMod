using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{    /// <summary>
    /// 三转道蛊虫 — BaoRanGu
    /// 蛊虫
    /// </summary>
    public class BaoRanGu : FireWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 32;
        protected override int _useTime => 25;
        protected override int _guLevel => 3;
        protected override int controlQiCost => 16;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
        public float DoTDuration => 4f;
        public float DoTDamage => 8f;
        public float SlowPercent => 0;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 50;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 8f;
            Item.crit = 6;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 5800;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 25;
            Item.useTime = 25;
            Item.UseSound = SoundID.Item14;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<BaoRanProj>();
            Item.shootSpeed = 7f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
