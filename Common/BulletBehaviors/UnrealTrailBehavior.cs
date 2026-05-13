using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class UnrealTrailBehavior : IBulletBehavior
    {
        public string Name => "UnrealTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public UnrealTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 12;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.18f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.2f;
        public Color GhostColor { get; set; } = new Color(160, 180, 220, 100);

        public int MaxIllusionWaves { get; set; } = 15;
        public int IllusionWaveLife { get; set; } = 25;
        public float IllusionWaveSize { get; set; } = 0.6f;
        public float IllusionWaveExpandRate { get; set; } = 2.5f;
        public int IllusionWaveSpawnInterval { get; set; } = 3;
        public float IllusionWaveDistortSpeed { get; set; } = 0.1f;
        public float IllusionWaveSpread { get; set; } = 5f;
        public Color IllusionWaveColor { get; set; } = new Color(180, 200, 240, 200);

        public int MaxAfterImages { get; set; } = 10;
        public int AfterImageLife { get; set; } = 40;
        public float AfterImageSize { get; set; } = 0.5f;
        public float AfterImageSpawnChance { get; set; } = 0.05f;
        public float AfterImageFadeDelay { get; set; } = 0.3f;
        public float AfterImageDriftSpeed { get; set; } = 0.3f;
        public float AfterImageSpread { get; set; } = 6f;
        public Color AfterImageColor { get; set; } = new Color(140, 180, 220, 180);

        public int MaxMirrorShards { get; set; } = 12;
        public int MirrorShardLife { get; set; } = 22;
        public float MirrorShardSize { get; set; } = 0.4f;
        public float MirrorShardSpawnChance { get; set; } = 0.07f;
        public float MirrorShardSpinSpeed { get; set; } = 0.15f;
        public float MirrorShardReflectSpeed { get; set; } = 0.1f;
        public float MirrorShardSpeed { get; set; } = 2f;
        public float MirrorShardSpread { get; set; } = 4f;
        public Color MirrorShardColor { get; set; } = new Color(200, 220, 255, 230);

        public float InertiaFactor { get; set; } = 0.15f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public UnrealTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new UnrealTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxIllusionWaves = MaxIllusionWaves,
                IllusionWaveLife = IllusionWaveLife,
                IllusionWaveSize = IllusionWaveSize,
                IllusionWaveExpandRate = IllusionWaveExpandRate,
                IllusionWaveSpawnInterval = IllusionWaveSpawnInterval,
                IllusionWaveDistortSpeed = IllusionWaveDistortSpeed,
                IllusionWaveSpread = IllusionWaveSpread,
                IllusionWaveColor = IllusionWaveColor,

                MaxAfterImages = MaxAfterImages,
                AfterImageLife = AfterImageLife,
                AfterImageSize = AfterImageSize,
                AfterImageSpawnChance = AfterImageSpawnChance,
                AfterImageFadeDelay = AfterImageFadeDelay,
                AfterImageDriftSpeed = AfterImageDriftSpeed,
                AfterImageSpread = AfterImageSpread,
                AfterImageColor = AfterImageColor,

                MaxMirrorShards = MaxMirrorShards,
                MirrorShardLife = MirrorShardLife,
                MirrorShardSize = MirrorShardSize,
                MirrorShardSpawnChance = MirrorShardSpawnChance,
                MirrorShardSpinSpeed = MirrorShardSpinSpeed,
                MirrorShardReflectSpeed = MirrorShardReflectSpeed,
                MirrorShardSpeed = MirrorShardSpeed,
                MirrorShardSpread = MirrorShardSpread,
                MirrorShardColor = MirrorShardColor,

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