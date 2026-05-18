using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{    /// <summary>
    /// 三转道蛊虫 — ThunderWingGu
    /// 蛊虫
    /// </summary>
    public class ThunderWingGu : LightningWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 25;
        protected override int _useTime => 18;
        protected override int _guLevel => 3;
        protected override int controlQiCost => 12;
        protected override float unitConntrolRate => 20;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT, DaoEffectTags.Slow };
        public float DoTDuration => 2f;
        public float DoTDamage => 5f;
        public float SlowPercent => 0.3f;
        public int SlowDuration => 60;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.damage = 25;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 5f;
            Item.crit = 4;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 5000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.UseSound = SoundID.Item93;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<ThunderWingProj>();
            Item.shootSpeed = 14f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}