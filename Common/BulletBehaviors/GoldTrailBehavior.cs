using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class GoldTrailBehavior : IBulletBehavior
    {
        public string Name => "GoldTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public GoldTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.22f;
        public float GhostLengthScale { get; set; } = 1.6f;
        public float GhostAlpha { get; set; } = 0.45f;
        public Color GhostColor { get; set; } = new Color(255, 215, 80, 180);

        public int MaxShards { get; set; } = 40;
        public int ShardLife { get; set; } = 22;
        public float ShardSize { get; set; } = 0.5f;
        public float ShardStretch { get; set; } = 2.0f;
        public int ShardSpawnInterval { get; set; } = 1;
        public float ShardSpinSpeed { get; set; } = 0.15f;
        public float ShardDriftSpeed { get; set; } = 0.3f;
        public float ShardSpread { get; set; } = 4f;
        public Color ShardColor { get; set; } = new Color(255, 220, 100, 220);

        public int MaxSparks { get; set; } = 25;
        public int SparkLife { get; set; } = 30;
        public float SparkSize { get; set; } = 0.5f;
        public float SparkSpawnChance { get; set; } = 0.2f;
        public float SparkDriftSpeed { get; set; } = 0.25f;
        public Color SparkColor { get; set; } = new Color(255, 240, 160, 240);

        public int MaxRings { get; set; } = 6;
        public int RingLife { get; set; } = 40;
        public float RingStartSize { get; set; } = 0.3f;
        public float RingEndSize { get; set; } = 1.5f;
        public float RingSpawnChance { get; set; } = 0.03f;
        public float RingRotSpeed { get; set; } = 0.06f;
        public float RingDriftSpeed { get; set; } = 0.1f;
        public Color RingColor { get; set; } = new Color(255, 200, 60, 160);

        public int MaxDust { get; set; } = 35;
        public int DustLife { get; set; } = 28;
        public float DustSize { get; set; } = 0.2f;
        public float DustSpawnChance { get; set; } = 0.3f;
        public float DustDriftSpeed { get; set; } = 0.4f;
        public Color DustColor { get; set; } = new Color(255, 210, 80, 180);

        public float InertiaFactor { get; set; } = 0.18f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public GoldTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new GoldTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxShards = MaxShards,
                ShardLife = ShardLife,
                ShardSize = ShardSize,
                ShardStretch = ShardStretch,
                ShardSpawnInterval = ShardSpawnInterval,
                ShardSpinSpeed = ShardSpinSpeed,
                ShardDriftSpeed = ShardDriftSpeed,
                ShardSpread = ShardSpread,
                ShardColor = ShardColor,

                MaxSparks = MaxSparks,
                SparkLife = SparkLife,
                SparkSize = SparkSize,
                SparkSpawnChance = SparkSpawnChance,
                SparkDriftSpeed = SparkDriftSpeed,
                SparkColor = SparkColor,

                MaxRings = MaxRings,
                RingLife = RingLife,
                RingStartSize = RingStartSize,
                RingEndSize = RingEndSize,
                RingSpawnChance = RingSpawnChance,
                RingRotSpeed = RingRotSpeed,
                RingDriftSpeed = RingDriftSpeed,
                RingColor = RingColor,

                MaxDust = MaxDust,
                DustLife = DustLife,
                DustSize = DustSize,
                DustSpawnChance = DustSpawnChance,
                DustDriftSpeed = DustDriftSpeed,
                DustColor = DustColor,

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
