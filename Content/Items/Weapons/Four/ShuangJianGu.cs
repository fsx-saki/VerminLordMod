using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Four
{    /// <summary>
    /// 四转冰雪道蛊虫 — 霜箭蛊
    /// 发射霜箭的冰道蛊虫
    /// </summary>
    public class ShuangJianGu : IceSnowWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 38;
        protected override int _useTime => 20;
        protected override int _guLevel => 4;
        protected override int controlQiCost => 19;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Slow, DaoEffectTags.DoT };
        public float DoTDuration => 3f;
        public float DoTDamage => 5f;
        public float SlowPercent => 0.4f;
        public int SlowDuration => 150;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 70;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.crit = 8;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 14000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.UseSound = SoundID.Item27;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<ShuangJianProj>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
