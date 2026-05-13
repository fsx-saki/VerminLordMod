using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class SpaceTrailBehavior : IBulletBehavior
    {
        public string Name => "SpaceTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public SpaceTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(160, 120, 220, 160);

        public int MaxFoldLines { get; set; } = 8;
        public int FoldLineLife { get; set; } = 40;
        public float FoldLineWidth { get; set; } = 0.5f;
        public float FoldLineSpawnChance { get; set; } = 0.04f;
        public float FoldLineRevealSpeed { get; set; } = 2.5f;
        public float FoldLineReach { get; set; } = 28f;
        public int FoldLineSegments { get; set; } = 3;
        public float FoldLineAngleRange { get; set; } = 0.8f;
        public Color FoldLineColor { get; set; } = new Color(180, 140, 240, 210);

        public int MaxWarpPoints { get; set; } = 6;
        public int WarpPointLife { get; set; } = 50;
        public float WarpPointStartSize { get; set; } = 0.3f;
        public float WarpPointEndSize { get; set; } = 1.5f;
        public float WarpPointSpawnChance { get; set; } = 0.02f;
        public float WarpPointExpandSpeed { get; set; } = 1.8f;
        public float WarpPointRotSpeed { get; set; } = 0.04f;
        public float WarpPointDistortStrength { get; set; } = 8f;
        public Color WarpPointColor { get; set; } = new Color(140, 100, 200, 180);

        public int MaxMirrorShards { get; set; } = 12;
        public int MirrorShardLife { get; set; } = 35;
        public float MirrorShardSize { get; set; } = 0.5f;
        public float MirrorShardSpawnChance { get; set; } = 0.06f;
        public float MirrorShardDriftSpeed { get; set; } = 0.1f;
        public float MirrorShardRotSpeed { get; set; } = 0.04f;
        public float MirrorShardHueSpeed { get; set; } = 0.5f;
        public Color MirrorShardColor { get; set; } = new Color(200, 180, 255, 220);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public SpaceTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new SpaceTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxFoldLines = MaxFoldLines,
                FoldLineLife = FoldLineLife,
                FoldLineWidth = FoldLineWidth,
                FoldLineSpawnChance = FoldLineSpawnChance,
                FoldLineRevealSpeed = FoldLineRevealSpeed,
                FoldLineReach = FoldLineReach,
                FoldLineSegments = FoldLineSegments,
                FoldLineAngleRange = FoldLineAngleRange,
                FoldLineColor = FoldLineColor,

                MaxWarpPoints = MaxWarpPoints,
                WarpPointLife = WarpPointLife,
                WarpPointStartSize = WarpPointStartSize,
                WarpPointEndSize = WarpPointEndSize,
                WarpPointSpawnChance = WarpPointSpawnChance,
                WarpPointExpandSpeed = WarpPointExpandSpeed,
                WarpPointRotSpeed = WarpPointRotSpeed,
                WarpPointDistortStrength = WarpPointDistortStrength,
                WarpPointColor = WarpPointColor,

                MaxMirrorShards = MaxMirrorShards,
                MirrorShardLife = MirrorShardLife,
                MirrorShardSize = MirrorShardSize,
                MirrorShardSpawnChance = MirrorShardSpawnChance,
                MirrorShardDriftSpeed = MirrorShardDriftSpeed,
                MirrorShardRotSpeed = MirrorShardRotSpeed,
                MirrorShardHueSpeed = MirrorShardHueSpeed,
                MirrorShardColor = MirrorShardColor,

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
