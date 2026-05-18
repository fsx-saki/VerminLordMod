using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Five
{    /// <summary>
    /// 五转金道蛊虫 — 金龙蛊
    /// 五转攻伐蛊虫，化为金龙攻击。
    /// </summary>
    public class JinLongGu : GoldWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 55;
        protected override int _useTime => 30;
        protected override int _guLevel => 5;
        protected override int controlQiCost => 28;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.ArmorShred };
        public float DoTDuration => 0;
        public float DoTDamage => 0;
        public float SlowPercent => 0;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0.2f;
        public int ArmorShredDuration => 180;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 120;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 6f;
            Item.crit = 6;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
            Item.value = 25000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.Item20;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<JinLongProj>();
            Item.shootSpeed = 9f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
