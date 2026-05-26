using System;
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
    [ImplStatus(ImplStatus.Implemented, "五转宙道攻击仙蛊", "五转", "宙")]
    public class 太古年兽 : TimeWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 63;
        protected override int _useTime => 30;
        protected override int _guLevel => 5;
        protected override int controlQiCost => 36;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Slow };
        public float DoTDuration => 3f;
        public float DoTDamage => 10f;
        public float SlowPercent => 0.2f;
        public int SlowDuration => 240;
        public float ArmorShredAmount => 0f;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0f;
        public float LifeStealPercent => 0f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 120;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 5f;
            Item.crit = 4;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<TaiGuNianProj>();
            Item.shootSpeed = 8f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
