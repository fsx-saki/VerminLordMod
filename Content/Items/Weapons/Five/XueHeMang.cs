using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Five
{    /// <summary>
    /// 五转道蛊虫 — XueHeMang
    /// 蛊虫
    /// </summary>
    public class XueHeMang : BloodWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 52;
        protected override int _useTime => 28;
        protected override int _guLevel => 5;
        protected override int controlQiCost => 26;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
        public float DoTDuration => 6f;
        public float DoTDamage => 8f;
        public float SlowPercent => 0;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0.06f;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.damage = 75;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 4f;
            Item.crit = 4;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
            Item.value = 24000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 28;
            Item.useTime = 28;
            Item.UseSound = SoundID.Item20;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<XueHeMangProj>();
            Item.shootSpeed = 9f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
