using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public abstract class ElementalTrailBehavior<TTrail> : IBulletBehavior where TTrail : ITrail, new()
    {
        public abstract string Name { get; }

        public TrailManager TrailManager { get; } = new();

        public TTrail Trail { get; private set; }

        public bool AutoDraw { get; set; } = true;

        public bool SuppressDefaultDraw { get; set; } = false;

        protected abstract void ConfigureTrail(TTrail trail);

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new TTrail();
            ConfigureTrail(Trail);
            TrailManager.Add(Trail);
        }

        public void Update(Projectile projectile)
        {
            TrailManager.Update(projectile.Center, projectile.velocity);
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            TrailManager.Clear();
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            if (AutoDraw)
            {
                TrailManager.Draw(spriteBatch);
            }
            return !SuppressDefaultDraw;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
