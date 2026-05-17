using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class DustOnHitBehavior : IBulletBehavior
    {
        public string Name => "DustOnHit";

        public int DustType { get; set; } = DustID.Blood;

        public int DustCount { get; set; } = 10;

        public float SpeedMin { get; set; } = 1f;

        public float SpeedMax { get; set; } = 4f;

        public float ScaleMin { get; set; } = 0.5f;

        public float ScaleMax { get; set; } = 1.5f;

        public int Alpha { get; set; } = 100;

        public Color Color { get; set; } = Color.White;

        public bool NoGravity { get; set; } = true;

        public DustOnHitBehavior() { }

        public DustOnHitBehavior(int dustType, int dustCount = 10, float speedMin = 1f, float speedMax = 4f)
        {
            DustType = dustType;
            DustCount = dustCount;
            SpeedMin = speedMin;
            SpeedMax = speedMax;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < DustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(SpeedMin, SpeedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    target.Center,
                    DustType,
                    vel,
                    Alpha,
                    Color,
                    Main.rand.NextFloat(ScaleMin, ScaleMax)
                );
                d.noGravity = NoGravity;
            }
        }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}