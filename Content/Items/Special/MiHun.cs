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
    [ImplStatus(ImplStatus.Implemented, "七转魂道仙蛊", "七转", "魂")]
    public class MiHun : SoulWeapon, IOnHitEffectProvider
    {
        protected override int _guLevel => 7;
        protected override int qiCost => 100;
        protected override int controlQiCost => 50;
        protected override int _useTime => 40;
        protected override float unitConntrolRate => 4;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Weaken, DaoEffectTags.DoT };
        public float DoTDuration => 5f;
        public float DoTDamage => 8f;
        public float SlowPercent => 0f;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0f;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0.25f;
        public float LifeStealPercent => 0f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 150;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 1f;
            Item.crit = 12;
            Item.rare = ItemRarityID.Lime;
            Item.maxStack = 1;
            Item.value = 500000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 40;
            Item.useTime = 40;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<MiHunProj>();
            Item.shootSpeed = 6f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = false;
        }
    }
}
