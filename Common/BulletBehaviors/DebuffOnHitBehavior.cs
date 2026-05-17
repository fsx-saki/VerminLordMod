using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class DebuffOnHitBehavior : IBulletBehavior
    {
        public string Name => "DebuffOnHit";

        public List<(int buffType, int duration)> Buffs { get; set; } = new();

        public DebuffOnHitBehavior() { }

        public DebuffOnHitBehavior(int buffType, int duration)
        {
            Buffs.Add((buffType, duration));
        }

        public DebuffOnHitBehavior(List<(int, int)> buffs)
        {
            Buffs = buffs;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            foreach (var (buffType, duration) in Buffs)
            {
                target.AddBuff(buffType, duration);
            }
        }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}