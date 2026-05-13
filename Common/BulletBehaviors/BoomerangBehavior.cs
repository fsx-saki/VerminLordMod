using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class BoomerangBehavior : IBulletBehavior
    {
        public string Name => "Boomerang";

        public float OutwardSpeed { get; set; } = 14f;

        public float ReturnSpeed { get; set; } = 16f;

        public float ReturnAccel { get; set; } = 0.6f;

        public int OutwardFrames { get; set; } = 25;

        public float SpinSpeed { get; set; } = 0.4f;

        public bool AutoRotate { get; set; } = false;

        public float RotationOffset { get; set; } = 0f;

        public bool EnableLight { get; set; } = false;

        public Vector3 LightColor { get; set; } = Vector3.Zero;

        public Action<Projectile> OnReturnStart { get; set; } = null;

        private enum Phase { Outward, Returning }

        private Phase _phase;
        private int _timer;
        private bool _returnStarted;

        public BoomerangBehavior() { }

        public BoomerangBehavior(float outwardSpeed, float returnSpeed, int outwardFrames)
        {
            OutwardSpeed = outwardSpeed;
            ReturnSpeed = returnSpeed;
            OutwardFrames = outwardFrames;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _phase = Phase.Outward;
            _timer = 0;
            _returnStarted = false;
        }

        public void Update(Projectile projectile)
        {
            _timer++;
            projectile.rotation += SpinSpeed;

            if (_phase == Phase.Outward)
            {
                float speed = projectile.velocity.Length();
                if (speed > 0.1f)
                    projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * speed;

                if (_timer >= OutwardFrames)
                {
                    _phase = Phase.Returning;
                    _returnStarted = false;
                }
            }
            else if (_phase == Phase.Returning)
            {
                if (!_returnStarted)
                {
                    _returnStarted = true;
                    OnReturnStart?.Invoke(projectile);
                }

                Player owner = Main.player[projectile.owner];
                if (owner != null && owner.active)
                {
                    Vector2 toOwner = owner.Center - projectile.Center;
                    float dist = toOwner.Length();

                    if (dist < 20f)
                    {
                        projectile.Kill();
                        return;
                    }

                    Vector2 desiredVel = toOwner.SafeNormalize(Vector2.Zero) * ReturnSpeed;
                    projectile.velocity += (desiredVel - projectile.velocity) * (ReturnAccel / ReturnSpeed);

                    float currentSpeed = projectile.velocity.Length();
                    if (currentSpeed > ReturnSpeed)
                        projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * ReturnSpeed;
                }
            }

            if (AutoRotate && projectile.velocity.Length() > 0.1f)
                projectile.rotation = projectile.velocity.ToRotation() + RotationOffset;

            if (EnableLight && LightColor != Vector3.Zero)
                Lighting.AddLight(projectile.Center, LightColor.X, LightColor.Y, LightColor.Z);
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;

        public bool IsReturning => _phase == Phase.Returning;
    }
}
