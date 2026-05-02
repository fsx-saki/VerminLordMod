using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Five
{
    /// <summary>
    /// 血手印蛊 — 蓄力型武器。
    ///
    /// 使用 ChargeWeaponTemplate 模板实现：
    /// - 长按蓄力，每秒扣 1 滴血
    /// - 只生成一个弹幕，一轮一次使用
    /// - 弹幕推出后必须松手再按才能开始新一轮
    /// </summary>
    class BloodHandprintGu : ChargeWeaponTemplate
    {
        protected override int qiCost => 300;
        protected override int _useTime => 30;
        protected override int _guLevel => 5;

        // ChargeWeaponTemplate 参数
        protected override int ProjType => ModContent.ProjectileType<BloodHandprintsProj>();
        protected override int BloodCostInterval => 60; // 每秒扣 1 滴

        protected override void SetupItemDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.damage = 30;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 2f;
            Item.crit = 4;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
            Item.value = 10000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item17;
            Item.scale = 1f;
        }
    }
}
