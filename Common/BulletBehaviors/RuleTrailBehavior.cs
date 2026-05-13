using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class RuleTrailBehavior : IBulletBehavior
    {
        public string Name => "RuleTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public RuleTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 6;
        public int GhostRecordInterval { get; set; } = 3;
        public float GhostWidthScale { get; set; } = 0.12f;
        public float GhostLengthScale { get; set; } = 1.2f;
        public float GhostAlpha { get; set; } = 0.2f;
        public Color GhostColor { get; set; } = new Color(180, 200, 230, 100);

        public int MaxGridNodes { get; set; } = 12;
        public int GridNodeLife { get; set; } = 30;
        public float GridNodeSize { get; set; } = 0.3f;
        public float GridNodeSpawnChance { get; set; } = 0.08f;
        public float GridNodePulseSpeed { get; set; } = 0.12f;
        public float GridNodeDriftSpeed { get; set; } = 0.15f;
        public float GridNodeSpread { get; set; } = 8f;
        public Color GridNodeColor { get; set; } = new Color(150, 200, 255, 200);

        public int MaxRulerMarks { get; set; } = 8;
        public int RulerMarkLife { get; set; } = 25;
        public float RulerMarkLength { get; set; } = 20f;
        public float RulerMarkWidth { get; set; } = 0.15f;
        public float RulerMarkSpawnChance { get; set; } = 0.05f;
        public float RulerMarkTickSpacing { get; set; } = 4f;
        public float RulerMarkDriftSpeed { get; set; } = 0.08f;
        public Color RulerMarkColor { get; set; } = new Color(180, 210, 240, 180);

        public int MaxOrderRings { get; set; } = 3;
        public int OrderRingLife { get; set; } = 45;
        public float OrderRingStartRadius { get; set; } = 2f;
        public float OrderRingEndRadius { get; set; } = 30f;
        public float OrderRingWidth { get; set; } = 0.3f;
        public float OrderRingSpawnChance { get; set; } = 0.02f;
        public float OrderRingExpandSpeed { get; set; } = 0.8f;
        public float OrderRingRotSpeed { get; set; } = 0.04f;
        public int OrderRingSegmentCount { get; set; } = 8;
        public Color OrderRingColor { get; set; } = new Color(120, 180, 255, 160);

        public float InertiaFactor { get; set; } = 0.12f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public RuleTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new RuleTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxGridNodes = MaxGridNodes,
                GridNodeLife = GridNodeLife,
                GridNodeSize = GridNodeSize,
                GridNodeSpawnChance = GridNodeSpawnChance,
                GridNodePulseSpeed = GridNodePulseSpeed,
                GridNodeDriftSpeed = GridNodeDriftSpeed,
                GridNodeSpread = GridNodeSpread,
                GridNodeColor = GridNodeColor,

                MaxRulerMarks = MaxRulerMarks,
                RulerMarkLife = RulerMarkLife,
                RulerMarkLength = RulerMarkLength,
                RulerMarkWidth = RulerMarkWidth,
                RulerMarkSpawnChance = RulerMarkSpawnChance,
                RulerMarkTickSpacing = RulerMarkTickSpacing,
                RulerMarkDriftSpeed = RulerMarkDriftSpeed,
                RulerMarkColor = RulerMarkColor,

                MaxOrderRings = MaxOrderRings,
                OrderRingLife = OrderRingLife,
                OrderRingStartRadius = OrderRingStartRadius,
                OrderRingEndRadius = OrderRingEndRadius,
                OrderRingWidth = OrderRingWidth,
                OrderRingSpawnChance = OrderRingSpawnChance,
                OrderRingExpandSpeed = OrderRingExpandSpeed,
                OrderRingRotSpeed = OrderRingRotSpeed,
                OrderRingSegmentCount = OrderRingSegmentCount,
                OrderRingColor = OrderRingColor,

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