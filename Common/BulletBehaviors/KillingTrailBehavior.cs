using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class KillingTrailBehavior : IBulletBehavior
    {
        public string Name => "KillingTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public KillingTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.14f;
        public float GhostLengthScale { get; set; } = 1.3f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(180, 60, 60, 120);

        public int MaxBloodStreaks { get; set; } = 15;
        public int BloodStreakLife { get; set; } = 22;
        public float BloodStreakLength { get; set; } = 28f;
        public float BloodStreakWidth { get; set; } = 0.22f;
        public float BloodStreakSpawnChance { get; set; } = 0.1f;
        public float BloodStreakDripSpeed { get; set; } = 0.08f;
        public float BloodStreakDriftSpeed { get; set; } = 0.15f;
        public Color BloodStreakColor { get; set; } = new Color(200, 40, 40, 230);

        public int MaxKillingAuras { get; set; } = 12;
        public int KillingAuraLife { get; set; } = 20;
        public float KillingAuraSize { get; set; } = 0.6f;
        public float KillingAuraExpandRate { get; set; } = 2f;
        public float KillingAuraSpawnChance { get; set; } = 0.07f;
        public float KillingAuraPulseSpeed { get; set; } = 0.1f;
        public float KillingAuraSpread { get; set; } = 4f;
        public Color KillingAuraColor { get; set; } = new Color(180, 30, 30, 200);

        public int MaxDeathShadows { get; set; } = 10;
        public int DeathShadowLife { get; set; } = 45;
        public float DeathShadowSize { get; set; } = 0.7f;
        public float DeathShadowSpawnChance { get; set; } = 0.04f;
        public float DeathShadowFadeDelay { get; set; } = 0.5f;
        public float DeathShadowShimmerSpeed { get; set; } = 0.05f;
        public float DeathShadowSpread { get; set; } = 5f;
        public Color DeathShadowColor { get; set; } = new Color(100, 20, 40, 180);

        public float InertiaFactor { get; set; } = 0.2f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public KillingTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new KillingTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxBloodStreaks = MaxBloodStreaks,
                BloodStreakLife = BloodStreakLife,
                BloodStreakLength = BloodStreakLength,
                BloodStreakWidth = BloodStreakWidth,
                BloodStreakSpawnChance = BloodStreakSpawnChance,
                BloodStreakDripSpeed = BloodStreakDripSpeed,
                BloodStreakDriftSpeed = BloodStreakDriftSpeed,
                BloodStreakColor = BloodStreakColor,

                MaxKillingAuras = MaxKillingAuras,
                KillingAuraLife = KillingAuraLife,
                KillingAuraSize = KillingAuraSize,
                KillingAuraExpandRate = KillingAuraExpandRate,
                KillingAuraSpawnChance = KillingAuraSpawnChance,
                KillingAuraPulseSpeed = KillingAuraPulseSpeed,
                KillingAuraSpread = KillingAuraSpread,
                KillingAuraColor = KillingAuraColor,

                MaxDeathShadows = MaxDeathShadows,
                DeathShadowLife = DeathShadowLife,
                DeathShadowSize = DeathShadowSize,
                DeathShadowSpawnChance = DeathShadowSpawnChance,
                DeathShadowFadeDelay = DeathShadowFadeDelay,
                DeathShadowShimmerSpeed = DeathShadowShimmerSpeed,
                DeathShadowSpread = DeathShadowSpread,
                DeathShadowColor = DeathShadowColor,

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