using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class CrystalTrailBehavior : IBulletBehavior
    {
        public string Name => "CrystalTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public CrystalTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(180, 180, 255, 160);

        public int MaxResonanceRings { get; set; } = 5;
        public int ResonanceRingLife { get; set; } = 45;
        public float ResonanceRingStartRadius { get; set; } = 3f;
        public float ResonanceRingEndRadius { get; set; } = 25f;
        public float ResonanceRingWidth { get; set; } = 0.5f;
        public float ResonanceRingSpawnChance { get; set; } = 0.02f;
        public float ResonanceRingExpandSpeed { get; set; } = 1.8f;
        public int ResonanceRingSegments { get; set; } = 4;
        public float ResonanceRingGapAngle { get; set; } = 0.3f;
        public float ResonanceRingRotSpeed { get; set; } = 0.03f;
        public Color ResonanceRingColor { get; set; } = new Color(160, 180, 255, 200);

        public int MaxPrismShards { get; set; } = 15;
        public int PrismShardLife { get; set; } = 35;
        public float PrismShardSize { get; set; } = 0.5f;
        public float PrismShardSpawnChance { get; set; } = 0.07f;
        public float PrismShardDriftSpeed { get; set; } = 0.1f;
        public float PrismShardRotSpeed { get; set; } = 0.05f;
        public float PrismShardHueSpeed { get; set; } = 0.8f;
        public Color PrismShardColor { get; set; } = new Color(200, 200, 255, 220);

        public int MaxLatticeNodes { get; set; } = 12;
        public int LatticeNodeLife { get; set; } = 30;
        public float LatticeNodeSize { get; set; } = 0.4f;
        public float LatticeNodeSpawnChance { get; set; } = 0.08f;
        public float LatticeNodeDriftSpeed { get; set; } = 0.08f;
        public float LatticeNodeConnectRadius { get; set; } = 20f;
        public float LatticeNodeLineWidth { get; set; } = 0.3f;
        public Color LatticeNodeColor { get; set; } = new Color(180, 200, 255, 200);
        public Color LatticeLineColor { get; set; } = new Color(140, 160, 220, 150);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public CrystalTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new CrystalTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxResonanceRings = MaxResonanceRings,
                ResonanceRingLife = ResonanceRingLife,
                ResonanceRingStartRadius = ResonanceRingStartRadius,
                ResonanceRingEndRadius = ResonanceRingEndRadius,
                ResonanceRingWidth = ResonanceRingWidth,
                ResonanceRingSpawnChance = ResonanceRingSpawnChance,
                ResonanceRingExpandSpeed = ResonanceRingExpandSpeed,
                ResonanceRingSegments = ResonanceRingSegments,
                ResonanceRingGapAngle = ResonanceRingGapAngle,
                ResonanceRingRotSpeed = ResonanceRingRotSpeed,
                ResonanceRingColor = ResonanceRingColor,

                MaxPrismShards = MaxPrismShards,
                PrismShardLife = PrismShardLife,
                PrismShardSize = PrismShardSize,
                PrismShardSpawnChance = PrismShardSpawnChance,
                PrismShardDriftSpeed = PrismShardDriftSpeed,
                PrismShardRotSpeed = PrismShardRotSpeed,
                PrismShardHueSpeed = PrismShardHueSpeed,
                PrismShardColor = PrismShardColor,

                MaxLatticeNodes = MaxLatticeNodes,
                LatticeNodeLife = LatticeNodeLife,
                LatticeNodeSize = LatticeNodeSize,
                LatticeNodeSpawnChance = LatticeNodeSpawnChance,
                LatticeNodeDriftSpeed = LatticeNodeDriftSpeed,
                LatticeNodeConnectRadius = LatticeNodeConnectRadius,
                LatticeNodeLineWidth = LatticeNodeLineWidth,
                LatticeNodeColor = LatticeNodeColor,
                LatticeLineColor = LatticeLineColor,

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
