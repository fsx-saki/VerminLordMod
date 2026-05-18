using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Four
{    /// <summary>
    /// 四转道蛊虫 — TieShouQinNaGu
    /// 蛊虫
    /// </summary>
    public class TieShouQinNaGu : GoldWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 35;
        protected override int _useTime => 24;
        protected override int _guLevel => 4;
        protected override int controlQiCost => 18;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Slow };
        public float DoTDuration => 0;
        public float DoTDamage => 0;
        public float SlowPercent => 0.5f;
        public int SlowDuration => 180;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.damage = 55;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 8f;
            Item.crit = 2;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 13000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 24;
            Item.useTime = 24;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<TieShouQinNaProj>();
            Item.shootSpeed = 10f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
