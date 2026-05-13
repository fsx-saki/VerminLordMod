using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class YinYangTrailBehavior : IBulletBehavior
    {
        public string Name => "YinYangTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public YinYangTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.4f;
        public Color GhostColor { get; set; } = new Color(200, 195, 240, 180);

        public int MaxOrbs { get; set; } = 20;
        public int OrbLife { get; set; } = 35;
        public float OrbSize { get; set; } = 0.45f;
        public int OrbSpawnInterval { get; set; } = 2;
        public float OrbRotSpeed { get; set; } = 0.08f;
        public float OrbDriftSpeed { get; set; } = 0.2f;
        public float OrbSpread { get; set; } = 5f;
        public Color OrbYinColor { get; set; } = new Color(60, 50, 100, 220);
        public Color OrbYangColor { get; set; } = new Color(230, 225, 255, 220);

        public int MaxFish { get; set; } = 8;
        public int FishLife { get; set; } = 50;
        public float FishSize { get; set; } = 0.5f;
        public float FishSpawnChance { get; set; } = 0.04f;
        public float FishRotSpeed { get; set; } = 0.12f;
        public float FishDriftSpeed { get; set; } = 0.15f;
        public Color FishYinColor { get; set; } = new Color(50, 40, 90, 200);
        public Color FishYangColor { get; set; } = new Color(220, 215, 250, 200);

        public int MaxSCurves { get; set; } = 12;
        public int SCurveLife { get; set; } = 45;
        public float SCurveSize { get; set; } = 0.5f;
        public float SCurveAmplitude { get; set; } = 12f;
        public float SCurveSpawnChance { get; set; } = 0.06f;
        public float SCurveRotSpeed { get; set; } = 0.05f;
        public float SCurveDriftSpeed { get; set; } = 0.1f;
        public Color SCurveYinColor { get; set; } = new Color(70, 55, 120, 180);
        public Color SCurveYangColor { get; set; } = new Color(210, 200, 245, 180);

        public int MaxOrbitDots { get; set; } = 24;
        public int OrbitDotLife { get; set; } = 30;
        public float OrbitDotSize { get; set; } = 0.3f;
        public float OrbitDotRadius { get; set; } = 20f;
        public float OrbitDotAngularSpeed { get; set; } = 0.08f;
        public float OrbitDotSpawnChance { get; set; } = 0.15f;
        public Color OrbitDotYinColor { get; set; } = new Color(80, 65, 140, 220);
        public Color OrbitDotYangColor { get; set; } = new Color(240, 235, 255, 220);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public YinYangTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new YinYangTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxOrbs = MaxOrbs,
                OrbLife = OrbLife,
                OrbSize = OrbSize,
                OrbSpawnInterval = OrbSpawnInterval,
                OrbRotSpeed = OrbRotSpeed,
                OrbDriftSpeed = OrbDriftSpeed,
                OrbSpread = OrbSpread,
                OrbYinColor = OrbYinColor,
                OrbYangColor = OrbYangColor,

                MaxFish = MaxFish,
                FishLife = FishLife,
                FishSize = FishSize,
                FishSpawnChance = FishSpawnChance,
                FishRotSpeed = FishRotSpeed,
                FishDriftSpeed = FishDriftSpeed,
                FishYinColor = FishYinColor,
                FishYangColor = FishYangColor,

                MaxSCurves = MaxSCurves,
                SCurveLife = SCurveLife,
                SCurveSize = SCurveSize,
                SCurveAmplitude = SCurveAmplitude,
                SCurveSpawnChance = SCurveSpawnChance,
                SCurveRotSpeed = SCurveRotSpeed,
                SCurveDriftSpeed = SCurveDriftSpeed,
                SCurveYinColor = SCurveYinColor,
                SCurveYangColor = SCurveYangColor,

                MaxOrbitDots = MaxOrbitDots,
                OrbitDotLife = OrbitDotLife,
                OrbitDotSize = OrbitDotSize,
                OrbitDotRadius = OrbitDotRadius,
                OrbitDotAngularSpeed = OrbitDotAngularSpeed,
                OrbitDotSpawnChance = OrbitDotSpawnChance,
                OrbitDotYinColor = OrbitDotYinColor,
                OrbitDotYangColor = OrbitDotYangColor,

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
