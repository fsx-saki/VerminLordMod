using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class CharmTrailBehavior : IBulletBehavior
    {
        public string Name => "CharmTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public CharmTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.18f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(255, 180, 210, 140);

        public int MaxHearts { get; set; } = 10;
        public int HeartLife { get; set; } = 35;
        public float HeartSize { get; set; } = 0.5f;
        public float HeartSpawnChance { get; set; } = 0.06f;
        public float HeartPulseSpeed { get; set; } = 0.08f;
        public float HeartDriftSpeed { get; set; } = 0.2f;
        public Color HeartColor { get; set; } = new Color(255, 120, 160, 230);

        public int MaxRings { get; set; } = 5;
        public int RingLife { get; set; } = 45;
        public float RingStartRadius { get; set; } = 2f;
        public float RingEndRadius { get; set; } = 25f;
        public float RingWidth { get; set; } = 0.3f;
        public float RingSpawnChance { get; set; } = 0.02f;
        public float RingExpandSpeed { get; set; } = 2f;
        public float RingRotSpeed { get; set; } = 0.03f;
        public Color RingColor { get; set; } = new Color(255, 160, 200, 180);

        public int MaxMists { get; set; } = 15;
        public int MistLife { get; set; } = 50;
        public float MistSize { get; set; } = 0.6f;
        public float MistSpawnChance { get; set; } = 0.08f;
        public float MistDriftSpeed { get; set; } = 0.1f;
        public Color MistColor { get; set; } = new Color(255, 180, 220, 160);

        public float InertiaFactor { get; set; } = 0.1f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public CharmTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new CharmTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxHearts = MaxHearts,
                HeartLife = HeartLife,
                HeartSize = HeartSize,
                HeartSpawnChance = HeartSpawnChance,
                HeartPulseSpeed = HeartPulseSpeed,
                HeartDriftSpeed = HeartDriftSpeed,
                HeartColor = HeartColor,

                MaxRings = MaxRings,
                RingLife = RingLife,
                RingStartRadius = RingStartRadius,
                RingEndRadius = RingEndRadius,
                RingWidth = RingWidth,
                RingSpawnChance = RingSpawnChance,
                RingExpandSpeed = RingExpandSpeed,
                RingRotSpeed = RingRotSpeed,
                RingColor = RingColor,

                MaxMists = MaxMists,
                MistLife = MistLife,
                MistSize = MistSize,
                MistSpawnChance = MistSpawnChance,
                MistDriftSpeed = MistDriftSpeed,
                MistColor = MistColor,

                InertiaFactor = InertiaFactor,
                SpawnOffset = SpawnOffset,
            };
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
                TrailManager.Draw(spriteBatch);
            return !SuppressDefaultDraw;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}