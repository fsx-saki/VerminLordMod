using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;
using Terraria.DataStructures;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转毒道攻击蛊", "二转", "毒")]
    public class FengDuGu : PoisonWeapon
    {
        protected override int _guLevel => 2;
        protected override int qiCost => 12;
        protected override int controlQiCost => 10;
        protected override int _useTime => 22;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.damage = 25;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 1f;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 5000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 22;
            Item.useTime = 22;
            Item.UseSound = SoundID.Item17;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<FengDuProj>();
            Item.shootSpeed = 10f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
                return false;
            return true;
        }
    }
}
