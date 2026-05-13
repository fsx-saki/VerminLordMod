using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class GrassTrailBehavior : IBulletBehavior
    {
        public string Name => "GrassTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public GrassTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(80, 200, 60, 160);

        public int MaxLeaves { get; set; } = 30;
        public int LeafLife { get; set; } = 40;
        public float LeafSize { get; set; } = 0.5f;
        public int LeafSpawnInterval { get; set; } = 2;
        public float LeafRotSpeed { get; set; } = 0.06f;
        public float LeafDriftSpeed { get; set; } = 0.25f;
        public float LeafSpread { get; set; } = 5f;
        public Color LeafColor { get; set; } = new Color(80, 200, 60, 210);

        public int MaxPollen { get; set; } = 35;
        public int PollenLife { get; set; } = 30;
        public float PollenSize { get; set; } = 0.25f;
        public float PollenSpawnChance { get; set; } = 0.25f;
        public float PollenDriftSpeed { get; set; } = 0.35f;
        public Color PollenColor { get; set; } = new Color(200, 230, 80, 200);

        public int MaxBranches { get; set; } = 15;
        public int BranchLife { get; set; } = 40;
        public float BranchSize { get; set; } = 0.5f;
        public float BranchLength { get; set; } = 20f;
        public float BranchSpawnChance { get; set; } = 0.06f;
        public float BranchGrowSpeed { get; set; } = 3f;
        public float BranchDriftSpeed { get; set; } = 0.08f;
        public int BranchMaxDepth { get; set; } = 2;
        public float BranchSubAngle { get; set; } = 0.6f;
        public Color BranchColor { get; set; } = new Color(60, 160, 50, 200);

        public int MaxPetals { get; set; } = 12;
        public int PetalLife { get; set; } = 50;
        public float PetalSize { get; set; } = 0.45f;
        public float PetalSpawnChance { get; set; } = 0.04f;
        public float PetalSpinSpeed { get; set; } = 0.04f;
        public float PetalDriftSpeed { get; set; } = 0.15f;
        public Color PetalColor { get; set; } = new Color(255, 180, 200, 200);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public GrassTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new GrassTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxLeaves = MaxLeaves,
                LeafLife = LeafLife,
                LeafSize = LeafSize,
                LeafSpawnInterval = LeafSpawnInterval,
                LeafRotSpeed = LeafRotSpeed,
                LeafDriftSpeed = LeafDriftSpeed,
                LeafSpread = LeafSpread,
                LeafColor = LeafColor,

                MaxPollen = MaxPollen,
                PollenLife = PollenLife,
                PollenSize = PollenSize,
                PollenSpawnChance = PollenSpawnChance,
                PollenDriftSpeed = PollenDriftSpeed,
                PollenColor = PollenColor,

                MaxBranches = MaxBranches,
                BranchLife = BranchLife,
                BranchSize = BranchSize,
                BranchLength = BranchLength,
                BranchSpawnChance = BranchSpawnChance,
                BranchGrowSpeed = BranchGrowSpeed,
                BranchDriftSpeed = BranchDriftSpeed,
                BranchMaxDepth = BranchMaxDepth,
                BranchSubAngle = BranchSubAngle,
                BranchColor = BranchColor,

                MaxPetals = MaxPetals,
                PetalLife = PetalLife,
                PetalSize = PetalSize,
                PetalSpawnChance = PetalSpawnChance,
                PetalSpinSpeed = PetalSpinSpeed,
                PetalDriftSpeed = PetalDriftSpeed,
                PetalColor = PetalColor,

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
