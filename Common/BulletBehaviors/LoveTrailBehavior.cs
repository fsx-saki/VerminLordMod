using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class LoveTrailBehavior : IBulletBehavior
    {
        public string Name => "LoveTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public LoveTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.16f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(255, 180, 200, 140);

        public int MaxRedThreads { get; set; } = 12;
        public int RedThreadLife { get; set; } = 30;
        public float RedThreadLength { get; set; } = 35f;
        public float RedThreadWidth { get; set; } = 0.2f;
        public float RedThreadSpawnChance { get; set; } = 0.08f;
        public float RedThreadWaveSpeed { get; set; } = 0.06f;
        public float RedThreadWaveAmplitude { get; set; } = 2f;
        public float RedThreadDriftSpeed { get; set; } = 0.1f;
        public Color RedThreadColor { get; set; } = new Color(255, 120, 150, 220);

        public int MaxHeartGlows { get; set; } = 12;
        public int HeartGlowLife { get; set; } = 35;
        public float HeartGlowSize { get; set; } = 0.5f;
        public float HeartGlowSpawnChance { get; set; } = 0.06f;
        public float HeartGlowFloatSpeed { get; set; } = 0.3f;
        public float HeartGlowSpread { get; set; } = 5f;
        public Color HeartGlowColor { get; set; } = new Color(255, 100, 150, 200);

        public int MaxLoveMists { get; set; } = 15;
        public int LoveMistLife { get; set; } = 50;
        public float LoveMistSize { get; set; } = 0.6f;
        public float LoveMistExpandRate { get; set; } = 1.5f;
        public int LoveMistSpawnInterval { get; set; } = 4;
        public float LoveMistDriftSpeed { get; set; } = 0.4f;
        public float LoveMistSpread { get; set; } = 6f;
        public Color LoveMistColor { get; set; } = new Color(255, 150, 180, 180);

        public float InertiaFactor { get; set; } = 0.12f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public LoveTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new LoveTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxRedThreads = MaxRedThreads,
                RedThreadLife = RedThreadLife,
                RedThreadLength = RedThreadLength,
                RedThreadWidth = RedThreadWidth,
                RedThreadSpawnChance = RedThreadSpawnChance,
                RedThreadWaveSpeed = RedThreadWaveSpeed,
                RedThreadWaveAmplitude = RedThreadWaveAmplitude,
                RedThreadDriftSpeed = RedThreadDriftSpeed,
                RedThreadColor = RedThreadColor,

                MaxHeartGlows = MaxHeartGlows,
                HeartGlowLife = HeartGlowLife,
                HeartGlowSize = HeartGlowSize,
                HeartGlowSpawnChance = HeartGlowSpawnChance,
                HeartGlowFloatSpeed = HeartGlowFloatSpeed,
                HeartGlowSpread = HeartGlowSpread,
                HeartGlowColor = HeartGlowColor,

                MaxLoveMists = MaxLoveMists,
                LoveMistLife = LoveMistLife,
                LoveMistSize = LoveMistSize,
                LoveMistExpandRate = LoveMistExpandRate,
                LoveMistSpawnInterval = LoveMistSpawnInterval,
                LoveMistDriftSpeed = LoveMistDriftSpeed,
                LoveMistSpread = LoveMistSpread,
                LoveMistColor = LoveMistColor,

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