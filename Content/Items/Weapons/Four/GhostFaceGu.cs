using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Four
{    /// <summary>
    /// 四转道蛊虫 — GhostFaceGu
    /// 蛊虫
    /// </summary>
    public class GhostFaceGu : SoulWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 40;
        protected override int _useTime => 30;
        protected override int _guLevel => 4;
        protected override int controlQiCost => 20;
        protected override float unitConntrolRate => 12;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Fear, DaoEffectTags.DoT };
        public float DoTDuration => 3f;
        public float DoTDamage => 6f;
        public float SlowPercent => 0;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0.15f;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }
        public float LifeStealPercent => 0;

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.damage = 35;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 2f;
            Item.crit = 4;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 9000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.NPCHit36;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<GhostFaceProj>();
            Item.shootSpeed = 8f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}