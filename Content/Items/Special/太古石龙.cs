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
    [ImplStatus(ImplStatus.Implemented, "五转泥道攻击仙蛊", "五转", "泥")]
    public class 太古石龙 : MudWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 63;
        protected override int _useTime => 32;
        protected override int _guLevel => 5;
        protected override int controlQiCost => 36;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.ArmorShred };
        public float DoTDuration => 3f;
        public float DoTDamage => 10f;
        public float SlowPercent => 0f;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 5f;
        public int ArmorShredDuration => 180;
        public float WeakenPercent => 0f;
        public float LifeStealPercent => 0f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 136;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 7f;
            Item.crit = 4;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 32;
            Item.useTime = 32;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<ShiLongProj>();
            Item.shootSpeed = 7f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
