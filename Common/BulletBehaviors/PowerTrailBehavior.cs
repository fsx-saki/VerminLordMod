using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class PowerTrailBehavior : IBulletBehavior
    {
        public string Name => "PowerTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public PowerTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(255, 180, 80, 140);

        public int MaxShockWaves { get; set; } = 4;
        public int ShockWaveLife { get; set; } = 40;
        public float ShockWaveStartRadius { get; set; } = 3f;
        public float ShockWaveEndRadius { get; set; } = 35f;
        public float ShockWaveWidth { get; set; } = 0.3f;
        public float ShockWaveSpawnChance { get; set; } = 0.03f;
        public float ShockWaveExpandSpeed { get; set; } = 0.9f;
        public Color ShockWaveColor { get; set; } = new Color(255, 200, 100, 200);

        public int MaxAuras { get; set; } = 8;
        public int AuraLife { get; set; } = 25;
        public float AuraSize { get; set; } = 0.6f;
        public float AuraSpawnChance { get; set; } = 0.08f;
        public float AuraRotSpeed { get; set; } = 0.1f;
        public float AuraBurstPhase { get; set; } = 0.5f;
        public float AuraDriftSpeed { get; set; } = 0.3f;
        public Color AuraColor { get; set; } = new Color(255, 180, 60, 220);

        public int MaxBurstLines { get; set; } = 10;
        public int BurstLineLife { get; set; } = 15;
        public float BurstLineLength { get; set; } = 35f;
        public float BurstLineWidth { get; set; } = 0.2f;
        public float BurstLineSpawnChance { get; set; } = 0.1f;
        public float BurstLineFadeSpeed { get; set; } = 2f;
        public float BurstLineDriftSpeed { get; set; } = 0.8f;
        public Color BurstLineColor { get; set; } = new Color(255, 180, 80, 200);

        public float InertiaFactor { get; set; } = 0.15f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public PowerTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new PowerTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxShockWaves = MaxShockWaves,
                ShockWaveLife = ShockWaveLife,
                ShockWaveStartRadius = ShockWaveStartRadius,
                ShockWaveEndRadius = ShockWaveEndRadius,
                ShockWaveWidth = ShockWaveWidth,
                ShockWaveSpawnChance = ShockWaveSpawnChance,
                ShockWaveExpandSpeed = ShockWaveExpandSpeed,
                ShockWaveColor = ShockWaveColor,

                MaxAuras = MaxAuras,
                AuraLife = AuraLife,
                AuraSize = AuraSize,
                AuraSpawnChance = AuraSpawnChance,
                AuraRotSpeed = AuraRotSpeed,
                AuraBurstPhase = AuraBurstPhase,
                AuraDriftSpeed = AuraDriftSpeed,
                AuraColor = AuraColor,

                MaxBurstLines = MaxBurstLines,
                BurstLineLife = BurstLineLife,
                BurstLineLength = BurstLineLength,
                BurstLineWidth = BurstLineWidth,
                BurstLineSpawnChance = BurstLineSpawnChance,
                BurstLineFadeSpeed = BurstLineFadeSpeed,
                BurstLineDriftSpeed = BurstLineDriftSpeed,
                BurstLineColor = BurstLineColor,

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