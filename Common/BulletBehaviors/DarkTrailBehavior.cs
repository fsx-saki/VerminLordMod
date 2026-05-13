using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class DarkTrailBehavior : IBulletBehavior
    {
        public string Name => "DarkTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public DarkTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(60, 15, 90, 160);

        public int MaxPatches { get; set; } = 10;
        public int PatchLife { get; set; } = 55;
        public float PatchStartSize { get; set; } = 0.2f;
        public float PatchEndSize { get; set; } = 2.2f;
        public float PatchSpawnChance { get; set; } = 0.025f;
        public float PatchSpreadSpeed { get; set; } = 1.8f;
        public float PatchDriftSpeed { get; set; } = 0.03f;
        public Color PatchColor { get; set; } = new Color(50, 10, 80, 180);

        public int MaxChains { get; set; } = 8;
        public int ChainLife { get; set; } = 35;
        public float ChainWidth { get; set; } = 0.5f;
        public float ChainSpawnChance { get; set; } = 0.04f;
        public int ChainLinkCount { get; set; } = 5;
        public float ChainDragStrength { get; set; } = 0.06f;
        public float ChainReach { get; set; } = 25f;
        public Color ChainColor { get; set; } = new Color(80, 25, 120, 200);

        public int MaxCurseMarks { get; set; } = 12;
        public int CurseMarkLife { get; set; } = 40;
        public float CurseMarkSize { get; set; } = 0.5f;
        public float CurseMarkSpawnChance { get; set; } = 0.06f;
        public float CurseMarkSpinSpeed { get; set; } = 0.03f;
        public float CurseMarkRevealSpeed { get; set; } = 3f;
        public float CurseMarkDriftSpeed { get; set; } = 0.08f;
        public Color CurseMarkColor { get; set; } = new Color(100, 30, 150, 220);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public DarkTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new DarkTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxPatches = MaxPatches,
                PatchLife = PatchLife,
                PatchStartSize = PatchStartSize,
                PatchEndSize = PatchEndSize,
                PatchSpawnChance = PatchSpawnChance,
                PatchSpreadSpeed = PatchSpreadSpeed,
                PatchDriftSpeed = PatchDriftSpeed,
                PatchColor = PatchColor,

                MaxChains = MaxChains,
                ChainLife = ChainLife,
                ChainWidth = ChainWidth,
                ChainSpawnChance = ChainSpawnChance,
                ChainLinkCount = ChainLinkCount,
                ChainDragStrength = ChainDragStrength,
                ChainReach = ChainReach,
                ChainColor = ChainColor,

                MaxCurseMarks = MaxCurseMarks,
                CurseMarkLife = CurseMarkLife,
                CurseMarkSize = CurseMarkSize,
                CurseMarkSpawnChance = CurseMarkSpawnChance,
                CurseMarkSpinSpeed = CurseMarkSpinSpeed,
                CurseMarkRevealSpeed = CurseMarkRevealSpeed,
                CurseMarkDriftSpeed = CurseMarkDriftSpeed,
                CurseMarkColor = CurseMarkColor,

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
