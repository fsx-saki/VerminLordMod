using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class HuanHunXianProj : BaseBullet
    {
        private const float FlySpeed = 8f;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.8f, 0.8f, 1.0f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(220, 220, 255),
                GlowLayers = 3,
                GlowBaseScale = 1.4f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.6f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(0.8f, 0.8f, 1.0f)
            });
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Player player = Main.player[Projectile.owner];
                if (player.active && !player.dead)
                {
                    Vector2 playerPos = player.Center;
                    Vector2 npcPos = target.Center;

                    player.Center = npcPos;
                    target.Center = playerPos;

                    player.velocity = Vector2.Zero;
                    target.velocity = Vector2.Zero;

                    player.immune = true;
                    player.immuneTime = 60;

                    target.immune[Projectile.owner] = 60;

                    NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, target.whoAmI);

                    for (int i = 0; i < 20; i++)
                    {
                        Dust d1 = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Ghost);
                        d1.noGravity = true;
                        d1.scale = Main.rand.NextFloat(1f, 2f);
                        d1.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));

                        Dust d2 = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Ghost);
                        d2.noGravity = true;
                        d2.scale = Main.rand.NextFloat(1f, 2f);
                        d2.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                    }
                }
            }
        }
    }
}
