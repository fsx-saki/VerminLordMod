using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class MudTrailBehavior : IBulletBehavior
    {
        public string Name => "MudTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public MudTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.25f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.25f;
        public Color GhostColor { get; set; } = new Color(120, 80, 40, 100);

        public int MaxMudClods { get; set; } = 18;
        public int MudClodLife { get; set; } = 40;
        public float MudClodSize { get; set; } = 0.5f;
        public float MudClodSpawnChance { get; set; } = 0.15f;
        public float MudClodGravity { get; set; } = 0.18f;
        public float MudClodSpinSpeed { get; set; } = 0.05f;
        public float MudClodSquash { get; set; } = 0.4f;
        public float MudClodDriftSpeed { get; set; } = 0.3f;
        public Color MudClodColor { get; set; } = new Color(140, 95, 45, 220);

        public int MaxGroundCracks { get; set; } = 8;
        public int GroundCrackLife { get; set; } = 50;
        public float GroundCrackSize { get; set; } = 0.5f;
        public float GroundCrackLength { get; set; } = 16f;
        public float GroundCrackSpawnChance { get; set; } = 0.04f;
        public float GroundCrackGrowSpeed { get; set; } = 2.5f;
        public int GroundCrackBranchCount { get; set; } = 2;
        public float GroundCrackBranchAngle { get; set; } = 0.6f;
        public int GroundCrackMaxDepth { get; set; } = 2;
        public float GroundCrackDriftSpeed { get; set; } = 0.04f;
        public Color GroundCrackColor { get; set; } = new Color(100, 65, 30, 200);

        public int MaxSludgeDrips { get; set; } = 15;
        public int SludgeDripLife { get; set; } = 35;
        public float SludgeDripSize { get; set; } = 0.4f;
        public float SludgeDripStretch { get; set; } = 2f;
        public float SludgeDripSpawnChance { get; set; } = 0.12f;
        public float SludgeDripGravity { get; set; } = 0.12f;
        public float SludgeDripDripSpeed { get; set; } = 0.08f;
        public float SludgeDripDriftSpeed { get; set; } = 0.15f;
        public Color SludgeDripColor { get; set; } = new Color(110, 75, 35, 210);

        public float InertiaFactor { get; set; } = 0.18f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public MudTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new MudTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxMudClods = MaxMudClods,
                MudClodLife = MudClodLife,
                MudClodSize = MudClodSize,
                MudClodSpawnChance = MudClodSpawnChance,
                MudClodGravity = MudClodGravity,
                MudClodSpinSpeed = MudClodSpinSpeed,
                MudClodSquash = MudClodSquash,
                MudClodDriftSpeed = MudClodDriftSpeed,
                MudClodColor = MudClodColor,

                MaxGroundCracks = MaxGroundCracks,
                GroundCrackLife = GroundCrackLife,
                GroundCrackSize = GroundCrackSize,
                GroundCrackLength = GroundCrackLength,
                GroundCrackSpawnChance = GroundCrackSpawnChance,
                GroundCrackGrowSpeed = GroundCrackGrowSpeed,
                GroundCrackBranchCount = GroundCrackBranchCount,
                GroundCrackBranchAngle = GroundCrackBranchAngle,
                GroundCrackMaxDepth = GroundCrackMaxDepth,
                GroundCrackDriftSpeed = GroundCrackDriftSpeed,
                GroundCrackColor = GroundCrackColor,

                MaxSludgeDrips = MaxSludgeDrips,
                SludgeDripLife = SludgeDripLife,
                SludgeDripSize = SludgeDripSize,
                SludgeDripStretch = SludgeDripStretch,
                SludgeDripSpawnChance = SludgeDripSpawnChance,
                SludgeDripGravity = SludgeDripGravity,
                SludgeDripDripSpeed = SludgeDripDripSpeed,
                SludgeDripDriftSpeed = SludgeDripDriftSpeed,
                SludgeDripColor = SludgeDripColor,

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
