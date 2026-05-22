using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{
    /// <summary>
    /// 三转血道蛊虫 — 雷翼蛊
    /// 方源曾用的移动蛊，雷翅形态，后被血狂蛊污染而舍去。
    /// </summary>
    public class LeiYiGu : BloodWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 20;
        protected override int _useTime => 18;
        protected override int _guLevel => 3;
        protected override int controlQiCost => 12;
        protected override float unitConntrolRate => 15;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT, DaoEffectTags.LifeSteal };
        public float DoTDuration => 3f;
        public float DoTDamage => 10f;
        public float SlowPercent => 0.3f;
        public int SlowDuration => 150;
        public float ArmorShredAmount => 9f;
        public int ArmorShredDuration => 150;
        public float WeakenPercent => 0.15f;
        public float LifeStealPercent => 0.1f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 35;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 4f;
            Item.crit = 4;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 5000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.UseSound = SoundID.Item4;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<LeiYiProj>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }
    }
}
