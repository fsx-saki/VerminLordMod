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
    [ImplStatus(ImplStatus.Implemented, "三转冰雪道攻击蛊", "三转", "冰雪")]
    public class BingXiao : IceSnowWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 22;
        protected override int controlQiCost => 15;
        protected override int _useTime => 26;
        protected override int _guLevel => 3;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Slow };
        public float DoTDuration => 0;
        public float DoTDamage => 0;
        public float SlowPercent => 0.2f;
        public int SlowDuration => 180;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.damage = 50;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 4f;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 26;
            Item.useTime = 26;
            Item.UseSound = SoundID.Item30;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<BingXiaoProj>();
            Item.shootSpeed = 9f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }
    }
}
