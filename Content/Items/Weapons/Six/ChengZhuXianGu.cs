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
    /// 六转木道蛊虫 — 成竹仙蛊
    /// 木道六转仙蛊，形如萤火虫，体长扁平，头小，眼半圆球形，尾端放深绿光辉。可用于元莲真传杀招。
    /// </summary>
    public class ChengZhuXianGu : WoodWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 80;
        protected override int _useTime => 14;
        protected override int _guLevel => 6;
        protected override int controlQiCost => 35;
        protected override float unitConntrolRate => 5;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
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
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 14;
            Item.useTime = 14;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<ChengZhuXianProj>();
            Item.shootSpeed = 15f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
