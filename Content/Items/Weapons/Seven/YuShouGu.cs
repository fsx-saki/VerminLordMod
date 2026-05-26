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
    /// 七转驭兽蛊 — 驭兽仙蛊
    /// 七转仙蛊，可操控荒兽甚至太古荒兽，是兽道至高仙蛊之一。
    /// 攻击时召唤荒兽虚影协同作战，对敌人造成大量伤害。
    /// </summary>
    [ImplStatus(ImplStatus.Implemented, "七转仙蛊示例 — 驭兽蛊", "七转", "兽")]
    public class YuShouGu : SlaveWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 200;
        protected override int _useTime => 18;
        protected override int _guLevel => 7;
        protected override int controlQiCost => 80;
        protected override float unitConntrolRate => 3;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.ArmorShred, DaoEffectTags.Weaken };
        public float DoTDuration => 5f;
        public float DoTDamage => 30f;
        public float SlowPercent => 0.4f;
        public int SlowDuration => 300;
        public float ArmorShredAmount => 25f;
        public int ArmorShredDuration => 300;
        public float WeakenPercent => 0.2f;
        public float LifeStealPercent => 0.15f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.damage = 300;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 9f;
            Item.crit = 15;
            Item.rare = ItemRarityID.Lime;
            Item.maxStack = 1;
            Item.value = 200000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<YuShouGuProj>();
            Item.shootSpeed = 18f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
