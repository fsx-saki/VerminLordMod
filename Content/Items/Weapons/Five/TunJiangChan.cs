using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Five
{    /// <summary>
    /// 五转道蛊虫 — TunJiangChan
    /// 蛊虫
    /// </summary>

    public class TunJiangChan : WaterWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 55;
        protected override int _useTime => 35;
        protected override int _guLevel => 5;
        protected override int controlQiCost => 28;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Slow };
        public float DoTDuration => 0;
        public float DoTDamage => 0;
        public float SlowPercent => 0.4f;
        public int SlowDuration => 180;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 90;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 8f;
            Item.crit = 2;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
            Item.value = 23000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 35;
            Item.useTime = 35;
            Item.UseSound = SoundID.Item21;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<TunJiangChanProj>();
            Item.shootSpeed = 6f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
