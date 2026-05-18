using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Four
{    /// <summary>
    /// 四转道蛊虫 — DieYingGu
    /// 蛊虫
    /// </summary>

    public class DieYingGu : ShadowWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 38;
        protected override int _useTime => 16;
        protected override int _guLevel => 4;
        protected override int controlQiCost => 19;
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
            Item.damage = 45;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 1f;
            Item.crit = 12;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 14000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 16;
            Item.useTime = 16;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<DieYingProj>();
            Item.shootSpeed = 14f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
