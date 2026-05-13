using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class PoisonTrailBehavior : IBulletBehavior
    {
        public string Name => "PoisonTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public PoisonTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(80, 180, 60, 140);

        public int MaxSporeBubbles { get; set; } = 12;
        public int SporeBubbleLife { get; set; } = 50;
        public float SporeBubbleStartSize { get; set; } = 0.3f;
        public float SporeBubbleEndSize { get; set; } = 1.2f;
        public float SporeBubbleSpawnChance { get; set; } = 0.03f;
        public float SporeBubbleExpandSpeed { get; set; } = 1.5f;
        public float SporeBubbleDriftSpeed { get; set; } = 0.06f;
        public int SporeBubbleBurstCount { get; set; } = 3;
        public Color SporeBubbleColor { get; set; } = new Color(100, 200, 60, 200);

        public int MaxCorrosionDrips { get; set; } = 15;
        public int CorrosionDripLife { get; set; } = 40;
        public float CorrosionDripSize { get; set; } = 0.4f;
        public float CorrosionDripSpawnChance { get; set; } = 0.06f;
        public float CorrosionDripGravity { get; set; } = 0.18f;
        public float CorrosionDripSpread { get; set; } = 4f;
        public float CorrosionDripCorrodeRadius { get; set; } = 2f;
        public float CorrosionDripCorrodeSpeed { get; set; } = 3f;
        public Color CorrosionDripColor { get; set; } = new Color(120, 220, 50, 220);

        public int MaxMiasmaClouds { get; set; } = 8;
        public int MiasmaCloudLife { get; set; } = 60;
        public float MiasmaCloudStartSize { get; set; } = 0.4f;
        public float MiasmaCloudEndSize { get; set; } = 2.0f;
        public float MiasmaCloudSpawnChance { get; set; } = 0.02f;
        public float MiasmaCloudExpandSpeed { get; set; } = 1.2f;
        public float MiasmaCloudRotSpeed { get; set; } = 0.015f;
        public float MiasmaCloudSinkSpeed { get; set; } = 0.05f;
        public float MiasmaCloudDriftSpeed { get; set; } = 0.1f;
        public Color MiasmaCloudColor { get; set; } = new Color(60, 160, 40, 160);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public PoisonTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new PoisonTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxSporeBubbles = MaxSporeBubbles,
                SporeBubbleLife = SporeBubbleLife,
                SporeBubbleStartSize = SporeBubbleStartSize,
                SporeBubbleEndSize = SporeBubbleEndSize,
                SporeBubbleSpawnChance = SporeBubbleSpawnChance,
                SporeBubbleExpandSpeed = SporeBubbleExpandSpeed,
                SporeBubbleDriftSpeed = SporeBubbleDriftSpeed,
                SporeBubbleBurstCount = SporeBubbleBurstCount,
                SporeBubbleColor = SporeBubbleColor,

                MaxCorrosionDrips = MaxCorrosionDrips,
                CorrosionDripLife = CorrosionDripLife,
                CorrosionDripSize = CorrosionDripSize,
                CorrosionDripSpawnChance = CorrosionDripSpawnChance,
                CorrosionDripGravity = CorrosionDripGravity,
                CorrosionDripSpread = CorrosionDripSpread,
                CorrosionDripCorrodeRadius = CorrosionDripCorrodeRadius,
                CorrosionDripCorrodeSpeed = CorrosionDripCorrodeSpeed,
                CorrosionDripColor = CorrosionDripColor,

                MaxMiasmaClouds = MaxMiasmaClouds,
                MiasmaCloudLife = MiasmaCloudLife,
                MiasmaCloudStartSize = MiasmaCloudStartSize,
                MiasmaCloudEndSize = MiasmaCloudEndSize,
                MiasmaCloudSpawnChance = MiasmaCloudSpawnChance,
                MiasmaCloudExpandSpeed = MiasmaCloudExpandSpeed,
                MiasmaCloudRotSpeed = MiasmaCloudRotSpeed,
                MiasmaCloudSinkSpeed = MiasmaCloudSinkSpeed,
                MiasmaCloudDriftSpeed = MiasmaCloudDriftSpeed,
                MiasmaCloudColor = MiasmaCloudColor,

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
