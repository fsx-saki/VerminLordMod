using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{    /// <summary>
    /// 三转光道蛊虫 — 光虹蛊
    /// 三转移动蛊，可令蛊师化光遁走，速度在三转中号称第一。
    /// </summary>
    public class GuangHongGu : LightWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 22;
        protected override int _useTime => 15;
        protected override int _guLevel => 3;
        protected override int controlQiCost => 14;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => Array.Empty<DaoEffectTags>();
        public float DoTDuration => 0;
        public float DoTDamage => 0;
        public float SlowPercent => 0;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 25;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 1f;
            Item.crit = 10;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 5800;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.UseSound = SoundID.Item8;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<GuangHongProj>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
