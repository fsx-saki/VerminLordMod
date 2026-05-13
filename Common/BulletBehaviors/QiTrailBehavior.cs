using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class QiTrailBehavior : IBulletBehavior
    {
        public string Name => "QiTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public QiTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 12;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.25f;
        public float GhostLengthScale { get; set; } = 1.6f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(100, 200, 180, 140);

        public int MaxChiStreams { get; set; } = 12;
        public int ChiStreamLife { get; set; } = 35;
        public float ChiStreamSize { get; set; } = 0.3f;
        public float ChiStreamLength { get; set; } = 24f;
        public float ChiStreamSpawnChance { get; set; } = 0.08f;
        public float ChiStreamFlowSpeed { get; set; } = 0.15f;
        public float ChiStreamFlowAmplitude { get; set; } = 0.25f;
        public float ChiStreamGrowSpeed { get; set; } = 3f;
        public float ChiStreamDriftSpeed { get; set; } = 0.06f;
        public Color ChiStreamColor { get; set; } = new Color(80, 220, 180, 200);

        public int MaxAcupoints { get; set; } = 16;
        public int AcupointLife { get; set; } = 40;
        public float AcupointSize { get; set; } = 0.35f;
        public float AcupointSpawnChance { get; set; } = 0.12f;
        public float AcupointOrbitSpeed { get; set; } = 0.06f;
        public float AcupointOrbitRadius { get; set; } = 15f;
        public float AcupointDriftSpeed { get; set; } = 0.08f;
        public Color AcupointColor { get; set; } = new Color(120, 255, 200, 240);

        public int MaxMeridianPulses { get; set; } = 6;
        public int MeridianPulseLife { get; set; } = 45;
        public float MeridianPulseStartSize { get; set; } = 0.2f;
        public float MeridianPulseEndSize { get; set; } = 1.5f;
        public float MeridianPulseSpawnChance { get; set; } = 0.025f;
        public float MeridianPulseExpandSpeed { get; set; } = 2f;
        public float MeridianPulseRingWidth { get; set; } = 0.4f;
        public float MeridianPulseDriftSpeed { get; set; } = 0.04f;
        public Color MeridianPulseColor { get; set; } = new Color(60, 180, 160, 160);

        public float InertiaFactor { get; set; } = 0.12f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public QiTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new QiTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxChiStreams = MaxChiStreams,
                ChiStreamLife = ChiStreamLife,
                ChiStreamSize = ChiStreamSize,
                ChiStreamLength = ChiStreamLength,
                ChiStreamSpawnChance = ChiStreamSpawnChance,
                ChiStreamFlowSpeed = ChiStreamFlowSpeed,
                ChiStreamFlowAmplitude = ChiStreamFlowAmplitude,
                ChiStreamGrowSpeed = ChiStreamGrowSpeed,
                ChiStreamDriftSpeed = ChiStreamDriftSpeed,
                ChiStreamColor = ChiStreamColor,

                MaxAcupoints = MaxAcupoints,
                AcupointLife = AcupointLife,
                AcupointSize = AcupointSize,
                AcupointSpawnChance = AcupointSpawnChance,
                AcupointOrbitSpeed = AcupointOrbitSpeed,
                AcupointOrbitRadius = AcupointOrbitRadius,
                AcupointDriftSpeed = AcupointDriftSpeed,
                AcupointColor = AcupointColor,

                MaxMeridianPulses = MaxMeridianPulses,
                MeridianPulseLife = MeridianPulseLife,
                MeridianPulseStartSize = MeridianPulseStartSize,
                MeridianPulseEndSize = MeridianPulseEndSize,
                MeridianPulseSpawnChance = MeridianPulseSpawnChance,
                MeridianPulseExpandSpeed = MeridianPulseExpandSpeed,
                MeridianPulseRingWidth = MeridianPulseRingWidth,
                MeridianPulseDriftSpeed = MeridianPulseDriftSpeed,
                MeridianPulseColor = MeridianPulseColor,

                InertiaFactor = InertiaFactor,
                RandomSpread = RandomSpread,
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
