using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class MoonTrailBehavior : IBulletBehavior
    {
        public string Name => "MoonTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public MoonTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.22f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.4f;
        public Color GhostColor { get; set; } = new Color(180, 200, 230, 180);

        public int MaxCrescents { get; set; } = 16;
        public int CrescentLife { get; set; } = 40;
        public float CrescentSize { get; set; } = 0.5f;
        public float CrescentOrbitRadius { get; set; } = 18f;
        public float CrescentAngularSpeed { get; set; } = 0.06f;
        public float CrescentSpawnChance { get; set; } = 0.12f;
        public Color CrescentColor { get; set; } = new Color(200, 215, 240, 220);

        public int MaxBeams { get; set; } = 20;
        public int BeamLife { get; set; } = 25;
        public float BeamSize { get; set; } = 0.35f;
        public float BeamLength { get; set; } = 16f;
        public int BeamSpawnInterval { get; set; } = 2;
        public float BeamSwaySpeed { get; set; } = 0.1f;
        public float BeamSwayAmp { get; set; } = 0.3f;
        public float BeamSpread { get; set; } = 5f;
        public float BeamDriftSpeed { get; set; } = 0.15f;
        public Color BeamColor { get; set; } = new Color(180, 200, 235, 210);

        public int MaxTides { get; set; } = 6;
        public int TideLife { get; set; } = 55;
        public float TideStartSize { get; set; } = 0.2f;
        public float TideEndSize { get; set; } = 2.0f;
        public float TideSpawnChance { get; set; } = 0.02f;
        public float TideExpandSpeed { get; set; } = 1.8f;
        public float TideWaveSpeed { get; set; } = 0.08f;
        public float TideWaveAmp { get; set; } = 1f;
        public float TideDriftSpeed { get; set; } = 0.05f;
        public Color TideColor { get; set; } = new Color(160, 185, 220, 170);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public MoonTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new MoonTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxCrescents = MaxCrescents,
                CrescentLife = CrescentLife,
                CrescentSize = CrescentSize,
                CrescentOrbitRadius = CrescentOrbitRadius,
                CrescentAngularSpeed = CrescentAngularSpeed,
                CrescentSpawnChance = CrescentSpawnChance,
                CrescentColor = CrescentColor,

                MaxBeams = MaxBeams,
                BeamLife = BeamLife,
                BeamSize = BeamSize,
                BeamLength = BeamLength,
                BeamSpawnInterval = BeamSpawnInterval,
                BeamSwaySpeed = BeamSwaySpeed,
                BeamSwayAmp = BeamSwayAmp,
                BeamSpread = BeamSpread,
                BeamDriftSpeed = BeamDriftSpeed,
                BeamColor = BeamColor,

                MaxTides = MaxTides,
                TideLife = TideLife,
                TideStartSize = TideStartSize,
                TideEndSize = TideEndSize,
                TideSpawnChance = TideSpawnChance,
                TideExpandSpeed = TideExpandSpeed,
                TideWaveSpeed = TideWaveSpeed,
                TideWaveAmp = TideWaveAmp,
                TideDriftSpeed = TideDriftSpeed,
                TideColor = TideColor,

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