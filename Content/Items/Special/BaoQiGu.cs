using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "爆气蛊完整实现", plannedTurn: "三转", daoType: "气")]
    public class BaoQiGu : QiWeapon, IOnHitEffectProvider
    {
        protected override int _guLevel => 3;
        protected override int qiCost => 25;
        protected override int controlQiCost => 18;
        protected override int _useTime => 35;
        protected override float unitConntrolRate => 12;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
        public float DoTDuration => 2f;
        public float DoTDamage => 8f;
        public float SlowPercent => 0;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.damage = 65;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 5f;
            Item.crit = 6;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 20000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 35;
            Item.useTime = 35;
            Item.UseSound = SoundID.Item14;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<BaoQiExplosionProj>();
            Item.shootSpeed = 6f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
                return false;

            int numProjectiles = Main.rand.Next(8, 13);
            float angleStep = MathHelper.TwoPi / numProjectiles;
            float baseAngle = Main.rand.NextFloat() * MathHelper.TwoPi;

            for (int i = 0; i < numProjectiles; i++)
            {
                float angle = baseAngle + angleStep * i;
                Vector2 dir = angle.ToRotationVector2();
                Vector2 spawnPos = player.Center + dir * 10f;
                Vector2 vel = dir * Item.shootSpeed;
                Projectile.NewProjectile(source, spawnPos, vel, type, damage, knockback, player.whoAmI);
            }

            return false;
        }
    }
}
