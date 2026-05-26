using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转风道攻击蛊", "一转", "风")]
    public class LongJuanGu : WindWeapon
    {
        protected override int _guLevel => 1;
        protected override int qiCost => 8;
        protected override int controlQiCost => 8;
        protected override int _useTime => 22;

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 18;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 6f;
            Item.crit = 0;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 1;
            Item.value = 3000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 22;
            Item.useTime = 22;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<LongJuanProj>();
            Item.shootSpeed = 7f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }
    }
}
