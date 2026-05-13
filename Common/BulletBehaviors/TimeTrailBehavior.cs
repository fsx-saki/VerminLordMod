using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class TimeTrailBehavior : IBulletBehavior
    {
        public string Name => "TimeTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public TimeTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 12;
        public int GhostRecordInterval { get; set; } = 3;
        public float GhostWidthScale { get; set; } = 0.15f;
        public float GhostLengthScale { get; set; } = 1.6f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(200, 180, 100, 140);

        public int MaxGrains { get; set; } = 40;
        public int GrainLife { get; set; } = 30;
        public float GrainSize { get; set; } = 0.3f;
        public float GrainSpawnChance { get; set; } = 0.15f;
        public float GrainGravity { get; set; } = 0.12f;
        public float GrainSpread { get; set; } = 4f;
        public float GrainSpeed { get; set; } = 0.3f;
        public Color GrainColor { get; set; } = new Color(220, 190, 100, 220);

        public int MaxClockHands { get; set; } = 8;
        public int ClockHandLife { get; set; } = 45;
        public float ClockHandLength { get; set; } = 14f;
        public float ClockHandWidth { get; set; } = 0.5f;
        public float ClockHandSpawnChance { get; set; } = 0.04f;
        public float ClockHandBaseSpeed { get; set; } = 0.08f;
        public int ClockHandMaxDepth { get; set; } = 2;
        public Color ClockHandColor { get; set; } = new Color(200, 170, 80, 200);

        public int MaxAfterimages { get; set; } = 8;
        public int AfterimageLife { get; set; } = 20;
        public float AfterimageSize { get; set; } = 0.8f;
        public int AfterimageRecordInterval { get; set; } = 4;
        public float AfterimageAlpha { get; set; } = 0.5f;
        public Color AfterimageColor { get; set; } = new Color(180, 160, 80, 180);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public TimeTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new TimeTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxGrains = MaxGrains,
                GrainLife = GrainLife,
                GrainSize = GrainSize,
                GrainSpawnChance = GrainSpawnChance,
                GrainGravity = GrainGravity,
                GrainSpread = GrainSpread,
                GrainSpeed = GrainSpeed,
                GrainColor = GrainColor,

                MaxClockHands = MaxClockHands,
                ClockHandLife = ClockHandLife,
                ClockHandLength = ClockHandLength,
                ClockHandWidth = ClockHandWidth,
                ClockHandSpawnChance = ClockHandSpawnChance,
                ClockHandBaseSpeed = ClockHandBaseSpeed,
                ClockHandMaxDepth = ClockHandMaxDepth,
                ClockHandColor = ClockHandColor,

                MaxAfterimages = MaxAfterimages,
                AfterimageLife = AfterimageLife,
                AfterimageSize = AfterimageSize,
                AfterimageRecordInterval = AfterimageRecordInterval,
                AfterimageAlpha = AfterimageAlpha,
                AfterimageColor = AfterimageColor,

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
