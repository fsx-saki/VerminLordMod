using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class MetalTrailBehavior : IBulletBehavior
    {
        public string Name => "MetalTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public MetalTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.18f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(200, 180, 120, 160);

        public int MaxGrindSparks { get; set; } = 50;
        public int GrindSparkLife { get; set; } = 12;
        public float GrindSparkSize { get; set; } = 0.4f;
        public int GrindSparkSpawnInterval { get; set; } = 1;
        public float GrindSparkSpeed { get; set; } = 3.5f;
        public float GrindSparkSpread { get; set; } = 6f;
        public Color GrindSparkColor { get; set; } = new Color(255, 230, 150, 255);

        public int MaxShards { get; set; } = 18;
        public int ShardLife { get; set; } = 30;
        public float ShardSize { get; set; } = 0.5f;
        public float ShardStretch { get; set; } = 2.5f;
        public float ShardSpawnChance { get; set; } = 0.12f;
        public float ShardSpinSpeed { get; set; } = 0.2f;
        public float ShardDriftSpeed { get; set; } = 0.3f;
        public Color ShardColor { get; set; } = new Color(220, 200, 140, 230);

        public int MaxWhetStreaks { get; set; } = 10;
        public int WhetStreakLife { get; set; } = 18;
        public float WhetStreakLength { get; set; } = 22f;
        public float WhetStreakWidth { get; set; } = 0.3f;
        public float WhetStreakSpawnChance { get; set; } = 0.06f;
        public float WhetStreakGrowSpeed { get; set; } = 4f;
        public float WhetStreakShrinkSpeed { get; set; } = 3f;
        public float WhetStreakDriftSpeed { get; set; } = 0.08f;
        public Color WhetStreakColor { get; set; } = new Color(240, 220, 160, 200);

        public float InertiaFactor { get; set; } = 0.18f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public MetalTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new MetalTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxGrindSparks = MaxGrindSparks,
                GrindSparkLife = GrindSparkLife,
                GrindSparkSize = GrindSparkSize,
                GrindSparkSpawnInterval = GrindSparkSpawnInterval,
                GrindSparkSpeed = GrindSparkSpeed,
                GrindSparkSpread = GrindSparkSpread,
                GrindSparkColor = GrindSparkColor,

                MaxShards = MaxShards,
                ShardLife = ShardLife,
                ShardSize = ShardSize,
                ShardStretch = ShardStretch,
                ShardSpawnChance = ShardSpawnChance,
                ShardSpinSpeed = ShardSpinSpeed,
                ShardDriftSpeed = ShardDriftSpeed,
                ShardColor = ShardColor,

                MaxWhetStreaks = MaxWhetStreaks,
                WhetStreakLife = WhetStreakLife,
                WhetStreakLength = WhetStreakLength,
                WhetStreakWidth = WhetStreakWidth,
                WhetStreakSpawnChance = WhetStreakSpawnChance,
                WhetStreakGrowSpeed = WhetStreakGrowSpeed,
                WhetStreakShrinkSpeed = WhetStreakShrinkSpeed,
                WhetStreakDriftSpeed = WhetStreakDriftSpeed,
                WhetStreakColor = WhetStreakColor,

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
