using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Eight
{
    /// <summary>
    /// 八转运道蛊虫 — 鸿运齐天蛊
    /// 八转运道仙蛊，可汇聚天地气运，使持有者鸿运齐天。
    /// 攻击时释放气运之力，大幅提升暴击率和伤害。
    /// </summary>
    [ImplStatus(ImplStatus.Implemented, "八转仙蛊示例 — 鸿运齐天蛊", "八转", "运")]
    public class HongYunQiTianGu : LuckWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 550;
        protected override int _useTime => 14;
        protected override int _guLevel => 8;
        protected override int controlQiCost => 220;
        protected override float unitConntrolRate => 2;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.LifeSteal, DaoEffectTags.Weaken };
        public float DoTDuration => 5f;
        public float DoTDamage => 45f;
        public float SlowPercent => 0.45f;
        public int SlowDuration => 340;
        public float ArmorShredAmount => 30f;
        public int ArmorShredDuration => 340;
        public float WeakenPercent => 0.28f;
        public float LifeStealPercent => 0.3f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.damage = 650;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 9f;
            Item.crit = 25;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 1;
            Item.value = 550000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 14;
            Item.useTime = 14;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<HongYunQiTianGuProj>();
            Item.shootSpeed = 22f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
