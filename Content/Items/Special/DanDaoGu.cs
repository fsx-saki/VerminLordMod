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
    [ImplStatus(ImplStatus.Implemented, "一转刀道攻击蛊", "一转", "刀")]
    public class DanDaoGu : KnifeWeapon
    {
        protected override int qiCost => 8;
        protected override int _useTime => 18;
        protected override int _guLevel => 1;
        protected override int controlQiCost => 8;
        protected override float unitConntrolRate => 20;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.damage = 22;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 5f;
            Item.crit = 4;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<DanDaoProj>();
            Item.shootSpeed = 11f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
                return false;
            return true;
        }
    }
}
