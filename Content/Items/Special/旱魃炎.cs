using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;
using VerminLordMod.Common.ImplementationTracker;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转火道攻击蛊", "三转", "火")]
    public class 旱魃炎 : FireWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 25;
        protected override int _useTime => 30;
        protected override int _guLevel => 3;
        protected override int controlQiCost => 15;
        protected override float unitConntrolRate => 15;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
        public float DoTDuration => 8f;
        public float DoTDamage => 4f;
        public float SlowPercent => 0f;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0f;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0f;
        public float LifeStealPercent => 0f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 55;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 50000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.Item20;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<HanBaYanProj>();
            Item.shootSpeed = 8f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }
    }
}
