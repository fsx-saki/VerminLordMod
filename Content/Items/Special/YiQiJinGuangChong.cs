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
    [ImplStatus(ImplStatus.Implemented, "一转金道攻击蛊", "一转", "金")]
    public class YiQiJinGuangChong : GoldWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 8;
        protected override int controlQiCost => 8;
        protected override int _useTime => 20;
        protected override int _guLevel => 1;
        protected override float unitConntrolRate => 25;

        public DaoEffectTags[] OnHitEffects => new DaoEffectTags[0];
        public float DoTDuration => 0f;
        public float DoTDamage => 0f;
        public float SlowPercent => 0f;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0f;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0f;
        public float LifeStealPercent => 0f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.damage = 18;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 2f;
            Item.crit = 4;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 1;
            Item.value = 1000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<YiQiJinGuangProj>();
            Item.shootSpeed = 10f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
