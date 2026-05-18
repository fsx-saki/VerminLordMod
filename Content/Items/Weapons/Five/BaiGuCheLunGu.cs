using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Five
{
    /// <summary>
    /// 五转火道蛊虫 — 白骨车轮
    /// 五转蛊，哈突骨的核心蛊，可化为白骨战车
    /// </summary>
    public class BaiGuCheLunGu : FireWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 55;
        protected override int _useTime => 15;
        protected override int _guLevel => 5;
        protected override int controlQiCost => 25;
        protected override float unitConntrolRate => 8;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
        public float DoTDuration => 3f;
        public float DoTDamage => 14f;
        public float SlowPercent => 0.3f;
        public int SlowDuration => 210;
        public float ArmorShredAmount => 13f;
        public int ArmorShredDuration => 210;
        public float WeakenPercent => 0.15f;
        public float LifeStealPercent => 0.1f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 85;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 6f;
            Item.crit = 8;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 30000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<BaiProjCheLunGu>();
            Item.shootSpeed = 14f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
