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
    [ImplStatus(ImplStatus.Implemented, "六转规则道仙蛊", "六转", "规则")]
    public class DaoKeDaoXianGu : RuleWeapon, IOnHitEffectProvider
    {
        protected override int _guLevel => 6;
        protected override int qiCost => 80;
        protected override int controlQiCost => 35;
        protected override int _useTime => 42;
        protected override float unitConntrolRate => 5;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Weaken, DaoEffectTags.ArmorShred };
        public float DoTDuration => 0f;
        public float DoTDamage => 0f;
        public float SlowPercent => 0f;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 8f;
        public int ArmorShredDuration => 300;
        public float WeakenPercent => 0.2f;
        public float LifeStealPercent => 0f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 130;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.rare = ItemRarityID.Yellow;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 42;
            Item.useTime = 42;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<DaoKeDaoProj>();
            Item.shootSpeed = 8f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = false;
        }
    }
}
