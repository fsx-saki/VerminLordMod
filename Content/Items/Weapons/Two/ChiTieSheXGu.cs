using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Two
{
    /// <summary>
    /// 二转气道蛊虫 — 赤铁舍利蛊
    /// 用于提升蛊师修为的蛊虫，共两只（后追加三只），由常飚赏赐给马鸿运。
    /// </summary>
    public class ChiTieSheXGu : QiWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 10;
        protected override int _useTime => 20;
        protected override int _guLevel => 2;
        protected override int controlQiCost => 6;
        protected override float unitConntrolRate => 25;

        public DaoEffectTags[] OnHitEffects => Array.Empty<DaoEffectTags>();
        public float DoTDuration => 3f;
        public float DoTDamage => 8f;
        public float SlowPercent => 0.3f;
        public int SlowDuration => 120;
        public float ArmorShredAmount => 7f;
        public int ArmorShredDuration => 120;
        public float WeakenPercent => 0.15f;
        public float LifeStealPercent => 0.1f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 20;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.crit = 3;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 1;
            Item.value = 1500;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<ChiTieSheXProj>();
            Item.shootSpeed = 11f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
