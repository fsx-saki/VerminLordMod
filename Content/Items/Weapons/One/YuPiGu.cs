using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.One
{
    /// <summary>
    /// 一转月道蛊虫 — 玉皮蛊
    /// 一转防御蛊虫，用于合炼白玉蛊和月霓裳。
    /// </summary>
    public class YuPiGu : MoonWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 5;
        protected override int _useTime => 22;
        protected override int _guLevel => 1;
        protected override int controlQiCost => 3;
        protected override float unitConntrolRate => 30;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.LifeSteal };
        public float DoTDuration => 3f;
        public float DoTDamage => 6f;
        public float SlowPercent => 0.3f;
        public int SlowDuration => 90;
        public float ArmorShredAmount => 5f;
        public int ArmorShredDuration => 90;
        public float WeakenPercent => 0.15f;
        public float LifeStealPercent => 0.1f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 12;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 2f;
            Item.crit = 2;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 1;
            Item.value = 500;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 22;
            Item.useTime = 22;
            Item.UseSound = SoundID.Item4;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<YuPiProj>();
            Item.shootSpeed = 10f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }
    }
}
