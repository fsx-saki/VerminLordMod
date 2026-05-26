using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转梦道攻击蛊", "一转", "梦")]
    public class MengDaoFanGu : DreamWeapon
    {
        protected override int _guLevel => 1;
        protected override int qiCost => 8;
        protected override int controlQiCost => 8;
        protected override int _useTime => 24;

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 16;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 1f;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 1;
            Item.value = 500;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 24;
            Item.useTime = 24;
            Item.UseSound = SoundID.Item46;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<MengDaoFanProj>();
            Item.shootSpeed = 6f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }
    }
}
