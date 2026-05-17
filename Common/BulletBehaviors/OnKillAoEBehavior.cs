using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class OnKillAoEBehavior : IBulletBehavior
    {
        public string Name => "OnKillAoE";

        public float Radius { get; set; } = 120f;

        public float DamageMultiplier { get; set; } = 1f;

        public float Knockback { get; set; } = 6f;

        public bool DirectionalKnockback { get; set; } = true;

        public List<(int buffType, int duration)> Buffs { get; set; } = new();

        public OnKillAoEBehavior() { }

        public OnKillAoEBehavior(float radius, List<(int, int)> buffs = null, float damageMultiplier = 1f, float knockback = 6f)
        {
            Radius = radius;
            DamageMultiplier = damageMultiplier;
            Knockback = knockback;
            if (buffs != null)
                Buffs = buffs;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            Vector2 center = projectile.Center;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.CanBeChasedBy() || npc.friendly)
                    continue;

                float dist = Vector2.Distance(center, npc.Center);
                if (dist > Radius)
                    continue;

                foreach (var (buffType, duration) in Buffs)
                {
                    npc.AddBuff(buffType, duration);
                }

                if (DamageMultiplier > 0f)
                {
                    int hitDir = DirectionalKnockback
                        ? (npc.Center.X > center.X ? 1 : -1)
                        : 0;
                    bool crit = Main.rand.Next(100) < projectile.CritChance;
                    npc.StrikeNPC(new NPC.HitInfo
                    {
                        Damage = (int)(projectile.damage * DamageMultiplier),
                        Knockback = Knockback,
                        HitDirection = hitDir,
                        Crit = crit
                    });
                }
            }
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}