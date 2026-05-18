using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{    /// <summary>
    /// 三转道蛊虫 — FlyingSoulGu
    /// 蛊虫
    /// </summary>

    public class FlyingSoulGu : SoulWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 22;
        protected override int _useTime => 24;
        protected override int _guLevel => 3;
        protected override int controlQiCost => 10;
        protected override float unitConntrolRate => 18;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Fear, DaoEffectTags.DoT };
        public float DoTDuration => 2.5f;
        public float DoTDamage => 4f;
        public float SlowPercent => 0;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0.1f;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.damage = 22;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 1.5f;
            Item.crit = 4;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 4500;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 24;
            Item.useTime = 24;
            Item.UseSound = SoundID.NPCHit36;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<FlyingSoulProj>();
            Item.shootSpeed = 7f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}