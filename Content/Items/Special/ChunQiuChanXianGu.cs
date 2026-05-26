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
    [ImplStatus(ImplStatus.Implemented, "六转宙道仙蛊-核心蛊虫", "六转", "宙")]
    public class ChunQiuChanXianGu : TimeWeapon, IOnHitEffectProvider
    {
        protected override int _guLevel => 6;
        protected override int qiCost => 80;
        protected override int controlQiCost => 40;
        protected override int _useTime => 60;
        protected override float unitConntrolRate => 5;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Slow, DaoEffectTags.DoT };
        public float DoTDuration => 3f;
        public float DoTDamage => 5f;
        public float SlowPercent => 0.5f;
        public int SlowDuration => 300;
        public float ArmorShredAmount => 0f;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0f;
        public float LifeStealPercent => 0f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 120;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 2f;
            Item.crit = 8;
            Item.rare = ItemRarityID.Yellow;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 60;
            Item.useTime = 60;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<ChunQiuChanProj>();
            Item.shootSpeed = 7f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = false;
        }
    }
}
