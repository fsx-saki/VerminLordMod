using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent;

namespace VerminLordMod.Content.Projectiles
{
    class MoonlightProj : ModProjectile
    {
        private Texture2D mainTexture;
        private readonly TrailManager trailManager = new TrailManager();

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 99;
            Projectile.timeLeft = 60;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            mainTexture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D trailTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTail").Value;

            // 初始化发光虚影拖尾（兼容原 Fucs.DrawGlowingProjectile 风格）
            trailManager.AddGhostTrail(trailTex,
                color: new Color(120, 200, 255),
                maxPositions: 16,
                widthScale: 1f,
                lengthScale: 1f,
                alpha: 1f,
                recordInterval: 2,
                enableGlow: true);
        }

        public override void AI()
        {
            trailManager.Update(Projectile.Center, Projectile.velocity);

            Lighting.AddLight(Projectile.Center, 1.8f, 1.9f, 2.0f);
            Projectile.rotation = Projectile.velocity.ToRotation() + (float)(0.5 * MathHelper.Pi);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            trailManager.Draw(Main.spriteBatch);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 30; i++)
            {
                int dustId = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.BlueFairy,
                    Main.rand.NextFloat(-5f, 5f),
                    Main.rand.NextFloat(-5f, 5f),
                    100, default, 1.5f
                );
                Main.dust[dustId].noGravity = true;
            }
        }
    }
}
