using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;

namespace VerminLordMod.Content.Items.Weapons.Test
{
    public class DaggerTestGu : DaoWeapon
    {
        public override DaoType DaoType => DaoType.Fire;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.value = 50000;

            // 蛊术伤害类型
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.damage = 10;
            Item.knockBack = 3f;
            Item.crit = 0;
            Item.noMelee = true;

            // 使用属性
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item1;

            // 弹射物
            Item.shootSpeed = 10f;
            Item.shoot = ModContent.ProjectileType<Projectiles.StarArrowProj>();

            base.SetDefaults();
        }

        public override void AddRecipes()
        {
            // 测试用，不添加合成配方
        }
    }
}
