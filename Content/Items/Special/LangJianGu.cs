using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转战道攻击蛊", "一转", "战")]
    public class LangJianGu : WarWeapon
    {
        protected override int _guLevel => 1;
        protected override int qiCost => 8;
        protected override int controlQiCost => 8;
        protected override int _useTime => 20;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 1;
            Item.value = 8000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item1;
            Item.damage = 18;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 4f;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<LangJianProj>();
            Item.shootSpeed = 10f;
        }
    }
}
