using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class WindTrailBehavior : IBulletBehavior
    {
        public string Name => "WindTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public WindTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;

        public int GhostMaxPositions { get; set; } = 10;

        public int GhostRecordInterval { get; set; } = 2;

        public float GhostWidthScale { get; set; } = 0.25f;

        public float GhostLengthScale { get; set; } = 1.8f;

        public float GhostAlpha { get; set; } = 0.5f;

        public Color GhostColor { get; set; } = new Color(160, 240, 220, 180);

        public int MaxStreaks { get; set; } = 50;

        public int StreakLife { get; set; } = 18;

        public float StreakSize { get; set; } = 0.5f;

        public float StreakStretch { get; set; } = 2.5f;

        public int StreakSpawnInterval { get; set; } = 1;

        public float StreakInertia { get; set; } = 0.15f;

        public float StreakSpread { get; set; } = 3f;

        public float StreakDrift { get; set; } = 0.3f;

        public Color StreakColor { get; set; } = new Color(180, 245, 230, 200);

        public int MaxVortex { get; set; } = 30;

        public int VortexLife { get; set; } = 35;

        public float VortexSize { get; set; } = 0.35f;

        public int VortexSpawnInterval { get; set; } = 4;

        public float VortexRotSpeed { get; set; } = 0.08f;

        public float VortexExpandRate { get; set; } = 1.5f;

        public float VortexDriftSpeed { get; set; } = 0.5f;

        public Color VortexColor { get; set; } = new Color(140, 220, 200, 180);

        public int MaxMist { get; set; } = 15;

        public int MistLife { get; set; } = 40;

        public float MistStartSize { get; set; } = 0.3f;

        public float MistEndSize { get; set; } = 1.8f;

        public float MistSpawnChance { get; set; } = 0.12f;

        public float MistDriftSpeed { get; set; } = 0.3f;

        public Color MistColor { get; set; } = new Color(180, 240, 230, 100);

        public float InertiaFactor { get; set; } = 0.2f;

        public float RandomSpread { get; set; } = 4f;

        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AdaptiveDensity { get; set; } = true;

        public float AdaptiveSpeedThreshold { get; set; } = 5f;

        public float AdaptiveDensityFactor { get; set; } = 5f;

        public bool AutoDraw { get; set; } = true;

        public bool SuppressDefaultDraw { get; set; } = false;

        public WindTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new WindTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxStreaks = MaxStreaks,
                StreakLife = StreakLife,
                StreakSize = StreakSize,
                StreakStretch = StreakStretch,
                StreakSpawnInterval = StreakSpawnInterval,
                StreakInertia = StreakInertia,
                StreakSpread = StreakSpread,
                StreakDrift = StreakDrift,
                StreakColor = StreakColor,

                MaxVortex = MaxVortex,
                VortexLife = VortexLife,
                VortexSize = VortexSize,
                VortexSpawnInterval = VortexSpawnInterval,
                VortexRotSpeed = VortexRotSpeed,
                VortexExpandRate = VortexExpandRate,
                VortexDriftSpeed = VortexDriftSpeed,
                VortexColor = VortexColor,

                MaxMist = MaxMist,
                MistLife = MistLife,
                MistStartSize = MistStartSize,
                MistEndSize = MistEndSize,
                MistSpawnChance = MistSpawnChance,
                MistDriftSpeed = MistDriftSpeed,
                MistColor = MistColor,

                InertiaFactor = InertiaFactor,
                RandomSpread = RandomSpread,
                SpawnOffset = SpawnOffset,

                AdaptiveDensity = AdaptiveDensity,
                AdaptiveSpeedThreshold = AdaptiveSpeedThreshold,
                AdaptiveDensityFactor = AdaptiveDensityFactor,
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
            {
                TrailManager.Draw(spriteBatch);
            }
            return !SuppressDefaultDraw;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
