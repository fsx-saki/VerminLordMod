using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class SwordTrailBehavior : IBulletBehavior
    {
        public string Name => "SwordTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public SwordTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.15f;
        public float GhostLengthScale { get; set; } = 1.3f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(180, 200, 255, 140);

        public int MaxSwordGlows { get; set; } = 15;
        public int SwordGlowLife { get; set; } = 20;
        public float SwordGlowSize { get; set; } = 0.6f;
        public float SwordGlowStretch { get; set; } = 3f;
        public float SwordGlowSpawnChance { get; set; } = 0.15f;
        public float SwordGlowSpinSpeed { get; set; } = 0.15f;
        public float SwordGlowDriftSpeed { get; set; } = 0.4f;
        public Color SwordGlowColor { get; set; } = new Color(200, 220, 255, 230);

        public int MaxSwordQis { get; set; } = 25;
        public int SwordQiLife { get; set; } = 35;
        public float SwordQiSize { get; set; } = 0.3f;
        public int SwordQiSpawnInterval { get; set; } = 2;
        public float SwordQiDriftSpeed { get; set; } = 0.6f;
        public float SwordQiSpread { get; set; } = 5f;
        public Color SwordQiColor { get; set; } = new Color(160, 200, 255, 180);

        public int MaxSwordScars { get; set; } = 8;
        public int SwordScarLife { get; set; } = 25;
        public float SwordScarLength { get; set; } = 30f;
        public float SwordScarWidth { get; set; } = 0.25f;
        public float SwordScarSpawnChance { get; set; } = 0.05f;
        public float SwordScarCurlAmount { get; set; } = 3f;
        public float SwordScarDriftSpeed { get; set; } = 0.05f;
        public Color SwordScarColor { get; set; } = new Color(140, 180, 255, 200);

        public float InertiaFactor { get; set; } = 0.15f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public SwordTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new SwordTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxSwordGlows = MaxSwordGlows,
                SwordGlowLife = SwordGlowLife,
                SwordGlowSize = SwordGlowSize,
                SwordGlowStretch = SwordGlowStretch,
                SwordGlowSpawnChance = SwordGlowSpawnChance,
                SwordGlowSpinSpeed = SwordGlowSpinSpeed,
                SwordGlowDriftSpeed = SwordGlowDriftSpeed,
                SwordGlowColor = SwordGlowColor,

                MaxSwordQis = MaxSwordQis,
                SwordQiLife = SwordQiLife,
                SwordQiSize = SwordQiSize,
                SwordQiSpawnInterval = SwordQiSpawnInterval,
                SwordQiDriftSpeed = SwordQiDriftSpeed,
                SwordQiSpread = SwordQiSpread,
                SwordQiColor = SwordQiColor,

                MaxSwordScars = MaxSwordScars,
                SwordScarLife = SwordScarLife,
                SwordScarLength = SwordScarLength,
                SwordScarWidth = SwordScarWidth,
                SwordScarSpawnChance = SwordScarSpawnChance,
                SwordScarCurlAmount = SwordScarCurlAmount,
                SwordScarDriftSpeed = SwordScarDriftSpeed,
                SwordScarColor = SwordScarColor,

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