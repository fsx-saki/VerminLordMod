using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class KnifeTrailBehavior : IBulletBehavior
    {
        public string Name => "KnifeTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public KnifeTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 6;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.12f;
        public float GhostLengthScale { get; set; } = 1.2f;
        public float GhostAlpha { get; set; } = 0.25f;
        public Color GhostColor { get; set; } = new Color(190, 200, 220, 120);

        public int MaxBladeFlashes { get; set; } = 12;
        public int BladeFlashLife { get; set; } = 18;
        public float BladeFlashSize { get; set; } = 0.5f;
        public float BladeFlashStretch { get; set; } = 4f;
        public float BladeFlashSpawnChance { get; set; } = 0.12f;
        public float BladeFlashSpinSpeed { get; set; } = 0.2f;
        public float BladeFlashDriftSpeed { get; set; } = 0.3f;
        public Color BladeFlashColor { get; set; } = new Color(200, 215, 240, 230);

        public int MaxCuttingMarks { get; set; } = 6;
        public int CuttingMarkLife { get; set; } = 30;
        public float CuttingMarkLength { get; set; } = 25f;
        public float CuttingMarkWidth { get; set; } = 0.2f;
        public float CuttingMarkSpawnChance { get; set; } = 0.04f;
        public float CuttingMarkFadeInSpeed { get; set; } = 5f;
        public float CuttingMarkDriftSpeed { get; set; } = 0.03f;
        public Color CuttingMarkColor { get; set; } = new Color(160, 180, 220, 200);

        public int MaxEdgeShards { get; set; } = 20;
        public int EdgeShardLife { get; set; } = 22;
        public float EdgeShardSize { get; set; } = 0.35f;
        public float EdgeShardSpawnChance { get; set; } = 0.18f;
        public float EdgeShardRotSpeed { get; set; } = 0.15f;
        public float EdgeShardFadeSpeed { get; set; } = 1.5f;
        public float EdgeShardDriftSpeed { get; set; } = 0.5f;
        public Color EdgeShardColor { get; set; } = new Color(180, 195, 230, 200);

        public float InertiaFactor { get; set; } = 0.12f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public KnifeTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new KnifeTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxBladeFlashes = MaxBladeFlashes,
                BladeFlashLife = BladeFlashLife,
                BladeFlashSize = BladeFlashSize,
                BladeFlashStretch = BladeFlashStretch,
                BladeFlashSpawnChance = BladeFlashSpawnChance,
                BladeFlashSpinSpeed = BladeFlashSpinSpeed,
                BladeFlashDriftSpeed = BladeFlashDriftSpeed,
                BladeFlashColor = BladeFlashColor,

                MaxCuttingMarks = MaxCuttingMarks,
                CuttingMarkLife = CuttingMarkLife,
                CuttingMarkLength = CuttingMarkLength,
                CuttingMarkWidth = CuttingMarkWidth,
                CuttingMarkSpawnChance = CuttingMarkSpawnChance,
                CuttingMarkFadeInSpeed = CuttingMarkFadeInSpeed,
                CuttingMarkDriftSpeed = CuttingMarkDriftSpeed,
                CuttingMarkColor = CuttingMarkColor,

                MaxEdgeShards = MaxEdgeShards,
                EdgeShardLife = EdgeShardLife,
                EdgeShardSize = EdgeShardSize,
                EdgeShardSpawnChance = EdgeShardSpawnChance,
                EdgeShardRotSpeed = EdgeShardRotSpeed,
                EdgeShardFadeSpeed = EdgeShardFadeSpeed,
                EdgeShardDriftSpeed = EdgeShardDriftSpeed,
                EdgeShardColor = EdgeShardColor,

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