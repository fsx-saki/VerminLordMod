using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class FlyingTrailBehavior : IBulletBehavior
    {
        public string Name => "FlyingTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public FlyingTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 1;
        public float GhostWidthScale { get; set; } = 0.15f;
        public float GhostLengthScale { get; set; } = 1.6f;
        public float GhostAlpha { get; set; } = 0.2f;
        public Color GhostColor { get; set; } = new Color(200, 220, 255, 100);

        public int MaxFeathers { get; set; } = 12;
        public int FeatherLife { get; set; } = 40;
        public float FeatherSize { get; set; } = 0.4f;
        public float FeatherSpawnChance { get; set; } = 0.05f;
        public float FeatherRotSpeed { get; set; } = 0.04f;
        public float FeatherSwayAmount { get; set; } = 2f;
        public float FeatherDriftSpeed { get; set; } = 0.15f;
        public Color FeatherColor { get; set; } = new Color(220, 235, 255, 200);

        public int MaxWindTails { get; set; } = 6;
        public int WindTailLife { get; set; } = 20;
        public float WindTailLength { get; set; } = 40f;
        public float WindTailWidth { get; set; } = 0.2f;
        public float WindTailSpawnChance { get; set; } = 0.08f;
        public float WindTailDriftSpeed { get; set; } = 0.1f;
        public Color WindTailColor { get; set; } = new Color(180, 210, 255, 150);

        public int MaxSpeedAfters { get; set; } = 8;
        public int SpeedAfterLife { get; set; } = 12;
        public float SpeedAfterSize { get; set; } = 0.3f;
        public float SpeedAfterSpawnChance { get; set; } = 0.2f;
        public float SpeedAfterStretch { get; set; } = 2f;
        public float SpeedAfterFadeSpeed { get; set; } = 2.5f;
        public Color SpeedAfterColor { get; set; } = new Color(160, 200, 255, 120);

        public float InertiaFactor { get; set; } = 0.08f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public FlyingTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new FlyingTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxFeathers = MaxFeathers,
                FeatherLife = FeatherLife,
                FeatherSize = FeatherSize,
                FeatherSpawnChance = FeatherSpawnChance,
                FeatherRotSpeed = FeatherRotSpeed,
                FeatherSwayAmount = FeatherSwayAmount,
                FeatherDriftSpeed = FeatherDriftSpeed,
                FeatherColor = FeatherColor,

                MaxWindTails = MaxWindTails,
                WindTailLife = WindTailLife,
                WindTailLength = WindTailLength,
                WindTailWidth = WindTailWidth,
                WindTailSpawnChance = WindTailSpawnChance,
                WindTailDriftSpeed = WindTailDriftSpeed,
                WindTailColor = WindTailColor,

                MaxSpeedAfters = MaxSpeedAfters,
                SpeedAfterLife = SpeedAfterLife,
                SpeedAfterSize = SpeedAfterSize,
                SpeedAfterSpawnChance = SpeedAfterSpawnChance,
                SpeedAfterStretch = SpeedAfterStretch,
                SpeedAfterFadeSpeed = SpeedAfterFadeSpeed,
                SpeedAfterColor = SpeedAfterColor,

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