using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Six
{
    /// <summary>
    /// 六转运道蛊虫 — 察运仙蛊
    /// 六转侦查仙蛊，用于观察气运。
    /// </summary>
    public class ChaYunXianGu : LuckWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 80;
        protected override int _useTime => 14;
        protected override int _guLevel => 6;
        protected override int controlQiCost => 35;
        protected override float unitConntrolRate => 5;

        public DaoEffectTags[] OnHitEffects => Array.Empty<DaoEffectTags>();
        public float DoTDuration => 3f;
        public float DoTDamage => 16f;
        public float SlowPercent => 0.3f;
        public int SlowDuration => 240;
        public float ArmorShredAmount => 15f;
        public int ArmorShredDuration => 240;
        public float WeakenPercent => 0.15f;
        public float LifeStealPercent => 0.1f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 130;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 7f;
            Item.crit = 10;
            Item.rare = ItemRarityID.Yellow;
            Item.maxStack = 1;
            Item.value = 80000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 14;
            Item.useTime = 14;
            Item.UseSound = SoundID.Item4;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<ChaYunXianProj>();
            Item.shootSpeed = 15f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }
    }
}
