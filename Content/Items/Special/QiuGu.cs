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
    [ImplStatus(ImplStatus.Implemented, "七转宙道仙蛊", "七转", "宙")]
    public class QiuGu : TimeWeapon, IOnHitEffectProvider
    {
        protected override int _guLevel => 7;
        protected override int qiCost => 90;
        protected override int controlQiCost => 45;
        protected override int _useTime => 35;
        protected override float unitConntrolRate => 4;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.LifeSteal, DaoEffectTags.DoT, DaoEffectTags.ArmorShred };
        public float DoTDuration => 4f;
        public float DoTDamage => 10f;
        public float SlowPercent => 0f;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 8f;
        public int ArmorShredDuration => 300;
        public float WeakenPercent => 0f;
        public float LifeStealPercent => 0.15f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 130;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.crit = 10;
            Item.rare = ItemRarityID.Lime;
            Item.maxStack = 1;
            Item.value = 500000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 35;
            Item.useTime = 35;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<QiuGuProj>();
            Item.shootSpeed = 8f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
