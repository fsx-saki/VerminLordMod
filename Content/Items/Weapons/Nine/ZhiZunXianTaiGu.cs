using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Nine
{
    /// <summary>
    /// 九转变化道蛊虫 — 至尊仙胎蛊
    /// 九转至高仙蛊，传说中可炼成至尊仙体，拥有无穷变化。
    /// 攻击时释放至尊之力，毁天灭地。
    /// </summary>
    [ImplStatus(ImplStatus.Implemented, "九转仙蛊示例 — 至尊仙胎蛊", "九转", "变化")]
    public class ZhiZunXianTaiGu : VariationWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 2000;
        protected override int _useTime => 10;
        protected override int _guLevel => 9;
        protected override int controlQiCost => 800;
        protected override float unitConntrolRate => 1;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.ArmorShred, DaoEffectTags.Weaken, DaoEffectTags.LifeSteal };
        public float DoTDuration => 8f;
        public float DoTDamage => 100f;
        public float SlowPercent => 0.6f;
        public int SlowDuration => 480;
        public float ArmorShredAmount => 50f;
        public int ArmorShredDuration => 480;
        public float WeakenPercent => 0.4f;
        public float LifeStealPercent => 0.35f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 2000;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 12f;
            Item.crit = 30;
            Item.rare = ItemRarityID.Red;
            Item.maxStack = 1;
            Item.value = 2000000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<ZhiZunXianTaiGuProj>();
            Item.shootSpeed = 25f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
