using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Five
{    /// <summary>
    /// 五转道蛊虫 — BloodFrenzyGu
    /// 蛊虫
    /// </summary>
    public class BloodFrenzyGu : BloodWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 50;
        protected override int _useTime => 25;
        protected override int _guLevel => 5;
        protected override int controlQiCost => 25;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT, DaoEffectTags.Slow };
        public float DoTDuration => 4f;
        public float DoTDamage => 8f;
        public float SlowPercent => 0.4f;
        public int SlowDuration => 120;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0.05f;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 45;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.crit = 6;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
            Item.value = 15000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 25;
            Item.useTime = 25;
            Item.UseSound = SoundID.Item20;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<BloodFrenzyProj>();
            Item.shootSpeed = 10f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}