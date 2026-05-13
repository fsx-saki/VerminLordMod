using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class LightningTrailBehavior : IBulletBehavior
    {
        public string Name => "LightningTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public LightningTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 1;
        public float GhostWidthScale { get; set; } = 0.3f;
        public float GhostLengthScale { get; set; } = 2.0f;
        public float GhostAlpha { get; set; } = 0.6f;
        public Color GhostColor { get; set; } = new Color(200, 200, 255, 200);

        public int MaxArcs { get; set; } = 40;
        public int ArcLife { get; set; } = 8;
        public float ArcScale { get; set; } = 0.5f;
        public float ArcLength { get; set; } = 14f;
        public int ArcSpawnInterval { get; set; } = 1;
        public float ArcJitter { get; set; } = 0.8f;
        public int ArcMaxBranch { get; set; } = 2;
        public float ArcBranchChance { get; set; } = 0.3f;
        public float ArcDriftSpeed { get; set; } = 0.15f;
        public Color ArcColor { get; set; } = new Color(180, 180, 255, 240);

        public int MaxFlashes { get; set; } = 8;
        public int FlashLife { get; set; } = 4;
        public float FlashSize { get; set; } = 1.0f;
        public float FlashSpawnChance { get; set; } = 0.06f;
        public Color FlashColor { get; set; } = new Color(255, 255, 220, 255);

        public int MaxFields { get; set; } = 15;
        public int FieldLife { get; set; } = 35;
        public float FieldSize { get; set; } = 0.4f;
        public float FieldSpawnChance { get; set; } = 0.08f;
        public float FieldCurveSpeed { get; set; } = 0.06f;
        public float FieldDriftSpeed { get; set; } = 0.2f;
        public Color FieldColor { get; set; } = new Color(140, 140, 220, 160);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public LightningTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new LightningTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxArcs = MaxArcs,
                ArcLife = ArcLife,
                ArcScale = ArcScale,
                ArcLength = ArcLength,
                ArcSpawnInterval = ArcSpawnInterval,
                ArcJitter = ArcJitter,
                ArcMaxBranch = ArcMaxBranch,
                ArcBranchChance = ArcBranchChance,
                ArcDriftSpeed = ArcDriftSpeed,
                ArcColor = ArcColor,

                MaxFlashes = MaxFlashes,
                FlashLife = FlashLife,
                FlashSize = FlashSize,
                FlashSpawnChance = FlashSpawnChance,
                FlashColor = FlashColor,

                MaxFields = MaxFields,
                FieldLife = FieldLife,
                FieldSize = FieldSize,
                FieldSpawnChance = FieldSpawnChance,
                FieldCurveSpeed = FieldCurveSpeed,
                FieldDriftSpeed = FieldDriftSpeed,
                FieldColor = FieldColor,

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