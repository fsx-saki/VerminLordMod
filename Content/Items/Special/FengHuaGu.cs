using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "四转风道攻击蛊", "四转", "风")]
    public class FengHuaGu : WindWeapon, IOnHitEffectProvider
    {
        protected override int _guLevel => 4;
        protected override int qiCost => 35;
        protected override int controlQiCost => 20;
        protected override int _useTime => 30;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Slow };
        public float DoTDuration => 0f;
        public float DoTDamage => 0f;
        public float SlowPercent => 0.2f;
        public int SlowDuration => 180;
        public float ArmorShredAmount => 0f;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0f;
        public float LifeStealPercent => 0f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 70;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 5f;
            Item.crit = 4;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 50000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.Item1;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<FengHuaProj>();
            Item.shootSpeed = 10f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2) return false;

            float daoMult = player.GetModPlayer<DaoHenPlayer>().GetMultiplier(DaoType.Wind);
            int finalDamage = (int)(damage * daoMult);

            Vector2 plrToMouse = Main.MouseWorld - player.Center;
            float r = (float)Math.Atan2(plrToMouse.Y, plrToMouse.X);

            for (int i = -1; i <= 1; i++)
            {
                float r2 = r + i * MathHelper.Pi / 18f;
                Vector2 shootVel = r2.ToRotationVector2() * 10f;
                Projectile p = Projectile.NewProjectileDirect(source, position, shootVel, type, finalDamage, knockback, player.whoAmI);

                if (p.TryGetGlobalProjectile(out Common.GlobalProjectiles.GuProjectileInfo info))
                {
                    info.EffectsOnHit = DaoEffectTags.Slow;
                    info.SlowPercent = SlowPercent;
                    info.SlowDuration = SlowDuration;
                    info.DaoMultiplier = daoMult;
                    info.SourceDao = DaoType.Wind;
                }
            }

            return false;
        }
    }
}
