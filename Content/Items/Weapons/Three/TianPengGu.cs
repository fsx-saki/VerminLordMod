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
    /// 三转骨道蛊虫 — 天蓬蛊
    /// 方源使用的防御蛊，形成白光虚甲。
    /// </summary>

    public class TianPengGu : LightWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 20;
        protected override int _useTime => 35;
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
            Item.width = 30;
            Item.height = 30;
            Item.damage = 0;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 0f;
            Item.crit = 0;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 6000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 35;
            Item.useTime = 35;
            Item.UseSound = SoundID.Item4;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<TianPengProj>();
            Item.shootSpeed = 0f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2 velocity,
            int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.Center, Microsoft.Xna.Framework.Vector2.Zero,
                type, damage, knockback, player.whoAmI);
            return false;
        }
    }
}
