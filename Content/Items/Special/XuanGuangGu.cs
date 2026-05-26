using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转光道攻击蛊", "一转", "光")]
    public class XuanGuangGu : LightWeapon
    {
        protected override int qiCost => 8;
        protected override int controlQiCost => 8;
        protected override int _useTime => 18;
        protected override int _guLevel => 1;

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 15;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 1f;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 1;
            Item.value = 500;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.UseSound = SoundID.Item8;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<XuanGuangProj>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
