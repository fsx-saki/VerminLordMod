using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class ScaleOverLifeBehavior : IBulletBehavior
    {
        public string Name => "ScaleOverLife";

        public float StartScale { get; set; } = 0.2f;

        public float EndScale { get; set; } = 1.5f;

        public int StartAlpha { get; set; } = 150;

        public int EndAlpha { get; set; } = 0;

        public bool AnimateAlpha { get; set; } = false;

        public bool EnableLight { get; set; } = false;

        public Vector3 LightColor { get; set; } = Vector3.Zero;

        private int _timer;
        private int _maxLife;

        public ScaleOverLifeBehavior() { }

        public ScaleOverLifeBehavior(float startScale, float endScale, bool animateAlpha = false, int startAlpha = 150, int endAlpha = 0)
        {
            StartScale = startScale;
            EndScale = endScale;
            AnimateAlpha = animateAlpha;
            StartAlpha = startAlpha;
            EndAlpha = endAlpha;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _timer = 0;
            _maxLife = projectile.timeLeft;
        }

        public void Update(Projectile projectile)
        {
            _timer++;
            float progress = MathHelper.Clamp(_timer / (float)_maxLife, 0f, 1f);
            projectile.scale = MathHelper.Lerp(StartScale, EndScale, progress);

            if (AnimateAlpha)
            {
                projectile.alpha = (int)MathHelper.Lerp(StartAlpha, EndAlpha, progress);
            }

            if (EnableLight && LightColor != Vector3.Zero)
            {
                Lighting.AddLight(projectile.Center,
                    LightColor.X * progress,
                    LightColor.Y * progress,
                    LightColor.Z * progress);
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}