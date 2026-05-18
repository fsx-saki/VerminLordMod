using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{    /// <summary>
    /// 三转道蛊虫 — DaoChiXueFuGu
    /// 蛊虫
    /// </summary>
    public class DaoChiXueFuGu : BloodWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 28;
        protected override int _useTime => 18;
        protected override int _guLevel => 3;
        protected override int controlQiCost => 15;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
        public float DoTDuration => 3f;
        public float DoTDamage => 5f;
        public float SlowPercent => 0;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0.02f;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 28;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 2f;
            Item.crit = 8;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 5200;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<DaoChiXueFuProj>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
