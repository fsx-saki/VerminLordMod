using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class LifeDeathTrailBehavior : IBulletBehavior
    {
        public string Name => "LifeDeathTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public LifeDeathTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.22f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(180, 160, 200, 140);

        public int MaxWitherPetals { get; set; } = 20;
        public int WitherPetalLife { get; set; } = 40;
        public float WitherPetalSize { get; set; } = 0.5f;
        public float WitherPetalSpawnChance { get; set; } = 0.15f;
        public float WitherPetalSpinSpeed { get; set; } = 0.06f;
        public float WitherPetalGravity { get; set; } = 0.08f;
        public float WitherPetalCurlSpeed { get; set; } = 0.1f;
        public float WitherPetalCurlAmplitude { get; set; } = 0.4f;
        public float WitherPetalDriftSpeed { get; set; } = 0.2f;
        public Color WitherPetalColor { get; set; } = new Color(160, 80, 100, 200);

        public int MaxBloomFlowers { get; set; } = 8;
        public int BloomFlowerLife { get; set; } = 55;
        public float BloomFlowerStartSize { get; set; } = 0.2f;
        public float BloomFlowerEndSize { get; set; } = 1.2f;
        public float BloomFlowerSpawnChance { get; set; } = 0.04f;
        public float BloomFlowerBloomSpeed { get; set; } = 2f;
        public float BloomFlowerDriftSpeed { get; set; } = 0.05f;
        public Color BloomFlowerColor { get; set; } = new Color(120, 220, 140, 200);

        public int MaxSamsaraRings { get; set; } = 5;
        public int SamsaraRingLife { get; set; } = 50;
        public float SamsaraRingStartSize { get; set; } = 0.3f;
        public float SamsaraRingEndSize { get; set; } = 1.6f;
        public float SamsaraRingSpawnChance { get; set; } = 0.02f;
        public float SamsaraRingRotSpeed { get; set; } = 0.04f;
        public float SamsaraRingExpandSpeed { get; set; } = 1.8f;
        public float SamsaraRingDriftSpeed { get; set; } = 0.04f;
        public Color SamsaraRingLifeColor { get; set; } = new Color(100, 220, 130, 180);
        public Color SamsaraRingDeathColor { get; set; } = new Color(180, 60, 90, 180);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public LifeDeathTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new LifeDeathTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxWitherPetals = MaxWitherPetals,
                WitherPetalLife = WitherPetalLife,
                WitherPetalSize = WitherPetalSize,
                WitherPetalSpawnChance = WitherPetalSpawnChance,
                WitherPetalSpinSpeed = WitherPetalSpinSpeed,
                WitherPetalGravity = WitherPetalGravity,
                WitherPetalCurlSpeed = WitherPetalCurlSpeed,
                WitherPetalCurlAmplitude = WitherPetalCurlAmplitude,
                WitherPetalDriftSpeed = WitherPetalDriftSpeed,
                WitherPetalColor = WitherPetalColor,

                MaxBloomFlowers = MaxBloomFlowers,
                BloomFlowerLife = BloomFlowerLife,
                BloomFlowerStartSize = BloomFlowerStartSize,
                BloomFlowerEndSize = BloomFlowerEndSize,
                BloomFlowerSpawnChance = BloomFlowerSpawnChance,
                BloomFlowerBloomSpeed = BloomFlowerBloomSpeed,
                BloomFlowerDriftSpeed = BloomFlowerDriftSpeed,
                BloomFlowerColor = BloomFlowerColor,

                MaxSamsaraRings = MaxSamsaraRings,
                SamsaraRingLife = SamsaraRingLife,
                SamsaraRingStartSize = SamsaraRingStartSize,
                SamsaraRingEndSize = SamsaraRingEndSize,
                SamsaraRingSpawnChance = SamsaraRingSpawnChance,
                SamsaraRingRotSpeed = SamsaraRingRotSpeed,
                SamsaraRingExpandSpeed = SamsaraRingExpandSpeed,
                SamsaraRingDriftSpeed = SamsaraRingDriftSpeed,
                SamsaraRingLifeColor = SamsaraRingLifeColor,
                SamsaraRingDeathColor = SamsaraRingDeathColor,

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
