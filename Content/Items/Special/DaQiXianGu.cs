using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转气道仙蛊", "五转", "气")]
    public class DaQiXianGu : QiWeapon, IOnHitEffectProvider
    {
        protected override int _guLevel => 5;
        protected override int qiCost => 50;
        protected override int controlQiCost => 25;
        protected override int _useTime => 35;
        protected override float unitConntrolRate => 7;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.LifeSteal, DaoEffectTags.DoT };
        public float DoTDuration => 6f;
        public float DoTDamage => 4f;
        public float SlowPercent => 0f;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0f;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0f;
        public float LifeStealPercent => 0.05f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 90;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 1;
            Item.value = 60000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 35;
            Item.useTime = 35;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<DaQiXianProj>();
            Item.shootSpeed = 7f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = false;
        }
    }
}
