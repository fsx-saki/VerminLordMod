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
    /// 八转光道蛊虫 — 宝光蛊
    /// 八转光道仙蛊，可释放璀璨宝光，照耀天地。
    /// 攻击时释放强光弹幕，对敌人造成巨额光道伤害并致盲。
    /// </summary>
    [ImplStatus(ImplStatus.Implemented, "八转仙蛊示例 — 宝光蛊", "八转", "光")]
    public class BaoGuangGu : LightWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 500;
        protected override int _useTime => 16;
        protected override int _guLevel => 8;
        protected override int controlQiCost => 200;
        protected override float unitConntrolRate => 2;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.ArmorShred, DaoEffectTags.Weaken, DaoEffectTags.LifeSteal };
        public float DoTDuration => 6f;
        public float DoTDamage => 50f;
        public float SlowPercent => 0.5f;
        public int SlowDuration => 360;
        public float ArmorShredAmount => 35f;
        public int ArmorShredDuration => 360;
        public float WeakenPercent => 0.3f;
        public float LifeStealPercent => 0.25f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.damage = 600;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 10f;
            Item.crit = 20;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 1;
            Item.value = 500000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 16;
            Item.useTime = 16;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<BaoGuangGuProj>();
            Item.shootSpeed = 20f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
