using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Seven
{
    /// <summary>
    /// 七转运道蛊虫 — 招灾蛊
    /// 七转运道仙蛊，可招来灾祸降临敌身，大幅降低敌人运势。
    /// 攻击附带厄运效果，使敌人受到额外伤害。
    /// </summary>
    [ImplStatus(ImplStatus.Implemented, "七转仙蛊示例 — 招灾蛊", "七转", "运")]
    public class ZhaoZaiGu : LuckWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 180;
        protected override int _useTime => 22;
        protected override int _guLevel => 7;
        protected override int controlQiCost => 75;
        protected override float unitConntrolRate => 3;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Weaken, DaoEffectTags.LifeSteal };
        public float DoTDuration => 4f;
        public float DoTDamage => 25f;
        public float SlowPercent => 0.35f;
        public int SlowDuration => 280;
        public float ArmorShredAmount => 20f;
        public int ArmorShredDuration => 280;
        public float WeakenPercent => 0.25f;
        public float LifeStealPercent => 0.2f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.damage = 280;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 8f;
            Item.crit = 18;
            Item.rare = ItemRarityID.Lime;
            Item.maxStack = 1;
            Item.value = 200000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 22;
            Item.useTime = 22;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<ZhaoZaiGuProj>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
