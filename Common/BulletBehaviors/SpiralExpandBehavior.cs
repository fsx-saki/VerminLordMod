using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class SpiralExpandBehavior : IBulletBehavior
    {
        public string Name => "SpiralExpand";

        public enum SpiralPhase { Charge, Expand, Fade }

        public int ChargeFrames { get; set; } = 30;

        public int ExpandFrames { get; set; } = 20;

        public int FadeFrames { get; set; } = 15;

        public float ExpandSpeed { get; set; } = 6f;

        public float SpinSpeed { get; set; } = 0.15f;

        public int ArmCount { get; set; } = 4;

        public float HitRadius { get; set; } = 120f;

        public int HitInterval { get; set; } = 8;

        public float Knockback { get; set; } = 8f;

        public bool EnableLight { get; set; } = true;

        public Vector3 LightColor { get; set; } = new Vector3(0.3f, 0.6f, 0.5f);

        public int DustType { get; set; } = DustID.Cloud;

        public Color DustColor { get; set; } = new Color(180, 240, 220, 200);

        public int DustCountPerArm { get; set; } = 3;

        public Action<Projectile> OnExpandStart { get; set; } = null;

        private SpiralPhase _phase;
        private int _timer;
        private float _currentRadius;
        private float _rotationAngle;
        private bool _expandStarted;

        public SpiralExpandBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _phase = SpiralPhase.Charge;
            _timer = 0;
            _currentRadius = 0f;
            _rotationAngle = 0f;
            _expandStarted = false;
            projectile.velocity = Vector2.Zero;
        }

        public void Update(Projectile projectile)
        {
            _timer++;
            _rotationAngle += SpinSpeed;

            switch (_phase)
            {
                case SpiralPhase.Charge:
                    UpdateCharge(projectile);
                    break;
                case SpiralPhase.Expand:
                    UpdateExpand(projectile);
                    break;
                case SpiralPhase.Fade:
                    UpdateFade(projectile);
                    break;
            }

            if (EnableLight && LightColor != Vector3.Zero)
                Lighting.AddLight(projectile.Center, LightColor.X, LightColor.Y, LightColor.Z);
        }

        private void UpdateCharge(Projectile projectile)
        {
            float progress = (float)_timer / ChargeFrames;
            float chargeRadius = 20f * progress;

            for (int i = 0; i < ArmCount; i++)
            {
                float armAngle = _rotationAngle + MathHelper.TwoPi * i / ArmCount;
                Vector2 pos = projectile.Center + armAngle.ToRotationVector2() * chargeRadius;
                SpawnDust(pos, armAngle.ToRotationVector2() * 0.5f, 0.6f);
            }

            if (_timer % 3 == 0)
            {
                Vector2 centerPos = projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                SpawnDust(centerPos, Vector2.Zero, 0.8f);
            }

            if (_timer >= ChargeFrames)
            {
                _phase = SpiralPhase.Expand;
                _timer = 0;
                _currentRadius = 0f;
                _expandStarted = false;
            }
        }

        private void UpdateExpand(Projectile projectile)
        {
            if (!_expandStarted)
            {
                _expandStarted = true;
                OnExpandStart?.Invoke(projectile);
            }

            float progress = (float)_timer / ExpandFrames;
            _currentRadius = HitRadius * progress;

            for (int arm = 0; arm < ArmCount; arm++)
            {
                float armBaseAngle = _rotationAngle + MathHelper.TwoPi * arm / ArmCount;

                for (int j = 0; j < DustCountPerArm; j++)
                {
                    float t = (float)j / DustCountPerArm;
                    float r = _currentRadius * (0.3f + t * 0.7f);
                    float spiralAngle = armBaseAngle - r * 0.02f;
                    Vector2 pos = projectile.Center + spiralAngle.ToRotationVector2() * r;
                    Vector2 vel = spiralAngle.ToRotationVector2() * ExpandSpeed * 0.3f;
                    SpawnDust(pos, vel, 1.0f - t * 0.3f);
                }
            }

            if (_timer % HitInterval == 0 && _currentRadius > 20f)
            {
                HitEnemiesInRange(projectile);
            }

            if (_timer >= ExpandFrames)
            {
                _phase = SpiralPhase.Fade;
                _timer = 0;
            }
        }

        private void UpdateFade(Projectile projectile)
        {
            float progress = (float)_timer / FadeFrames;

            for (int arm = 0; arm < ArmCount; arm++)
            {
                float armBaseAngle = _rotationAngle + MathHelper.TwoPi * arm / ArmCount;
                float r = _currentRadius * (1f + progress * 0.3f);
                Vector2 pos = projectile.Center + armBaseAngle.ToRotationVector2() * r;
                if (Main.rand.NextFloat() > progress)
                    SpawnDust(pos, armBaseAngle.ToRotationVector2() * 1f, 0.5f * (1f - progress));
            }

            if (_timer >= FadeFrames)
            {
                projectile.Kill();
            }
        }

        private void HitEnemiesInRange(Projectile projectile)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.CanBeChasedBy() || npc.friendly)
                    continue;

                float dist = Vector2.Distance(projectile.Center, npc.Center);
                if (dist <= _currentRadius && dist > 10f)
                {
                    Vector2 pushDir = (npc.Center - projectile.Center).SafeNormalize(Vector2.Zero);
                    float pushForce = Knockback * (1f - dist / _currentRadius);
                    npc.velocity += pushDir * pushForce;

                    Player owner = Main.player[projectile.owner];
                    if (owner != null && owner.active)
                    {
                        bool crit = Main.rand.Next(100) < projectile.CritChance;
                        npc.StrikeNPC(new NPC.HitInfo
                        {
                            Damage = projectile.damage,
                            Knockback = pushForce,
                            HitDirection = pushDir.X > 0 ? 1 : -1,
                            Crit = crit
                        });
                    }
                }
            }
        }

        private void SpawnDust(Vector2 pos, Vector2 vel, float scale)
        {
            Dust d = Dust.NewDustPerfect(pos, DustType, vel, 0, DustColor, scale);
            d.noGravity = true;
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool SuppressDefaultDraw { get; set; } = false;

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return !SuppressDefaultDraw;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;

        public SpiralPhase CurrentPhase => _phase;
        public float CurrentRadius => _currentRadius;
    }
}
