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
    /// 三转水道蛊虫 — 螺旋水箭蛊
    /// 三转攻击蛊，发射螺旋水箭
    /// </summary>
    public class LuoXuanShuiJianGu : WaterWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 20;
        protected override int _useTime => 18;
        protected override int _guLevel => 3;
        protected override int controlQiCost => 12;
        protected override float unitConntrolRate => 15;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Slow };
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
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<LuoXuanShuiJianProj>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
