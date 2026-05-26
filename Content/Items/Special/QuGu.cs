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
    [ImplStatus(ImplStatus.Implemented, "八转律道仙蛊", "八转", "规则")]
    public class QuGu : RuleWeapon, IOnHitEffectProvider
    {
        protected override int _guLevel => 8;
        protected override int qiCost => 160;
        protected override int controlQiCost => 55;
        protected override int _useTime => 45;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.ArmorShred, DaoEffectTags.DoT };
        public float DoTDuration => 300f;
        public float DoTDamage => 5f;
        public float SlowPercent => 0f;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 12f;
        public int ArmorShredDuration => 300;
        public float WeakenPercent => 0f;
        public float LifeStealPercent => 0f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 200;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 4f;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 1;
            Item.value = 1000000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<QuGuProj>();
            Item.shootSpeed = 9f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }
    }
}
