using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class LuckTrailBehavior : IBulletBehavior
    {
        public string Name => "LuckTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public LuckTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.15f;
        public float GhostLengthScale { get; set; } = 1.3f;
        public float GhostAlpha { get; set; } = 0.25f;
        public Color GhostColor { get; set; } = new Color(180, 230, 180, 120);

        public int MaxLuckyClovers { get; set; } = 8;
        public int LuckyCloverLife { get; set; } = 40;
        public float LuckyCloverSize { get; set; } = 0.5f;
        public float LuckyCloverSpawnChance { get; set; } = 0.04f;
        public float LuckyCloverRotSpeed { get; set; } = 0.03f;
        public float LuckyCloverFloatAmplitude { get; set; } = 1.5f;
        public float LuckyCloverDriftSpeed { get; set; } = 0.2f;
        public Color LuckyCloverColor { get; set; } = new Color(100, 220, 100, 220);

        public int MaxFortuneStars { get; set; } = 15;
        public int FortuneStarLife { get; set; } = 25;
        public float FortuneStarSize { get; set; } = 0.35f;
        public float FortuneStarSpawnChance { get; set; } = 0.12f;
        public float FortuneStarTwinkleSpeed { get; set; } = 0.15f;
        public float FortuneStarDriftSpeed { get; set; } = 0.3f;
        public Color FortuneStarColor { get; set; } = new Color(255, 220, 80, 230);

        public int MaxFateThreads { get; set; } = 5;
        public int FateThreadLife { get; set; } = 30;
        public float FateThreadWidth { get; set; } = 0.15f;
        public float FateThreadSpawnChance { get; set; } = 0.03f;
        public float FateThreadCurlAmount { get; set; } = 4f;
        public float FateThreadSpread { get; set; } = 20f;
        public Color FateThreadColor { get; set; } = new Color(220, 200, 100, 180);

        public float InertiaFactor { get; set; } = 0.1f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public LuckTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new LuckTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxLuckyClovers = MaxLuckyClovers,
                LuckyCloverLife = LuckyCloverLife,
                LuckyCloverSize = LuckyCloverSize,
                LuckyCloverSpawnChance = LuckyCloverSpawnChance,
                LuckyCloverRotSpeed = LuckyCloverRotSpeed,
                LuckyCloverFloatAmplitude = LuckyCloverFloatAmplitude,
                LuckyCloverDriftSpeed = LuckyCloverDriftSpeed,
                LuckyCloverColor = LuckyCloverColor,

                MaxFortuneStars = MaxFortuneStars,
                FortuneStarLife = FortuneStarLife,
                FortuneStarSize = FortuneStarSize,
                FortuneStarSpawnChance = FortuneStarSpawnChance,
                FortuneStarTwinkleSpeed = FortuneStarTwinkleSpeed,
                FortuneStarDriftSpeed = FortuneStarDriftSpeed,
                FortuneStarColor = FortuneStarColor,

                MaxFateThreads = MaxFateThreads,
                FateThreadLife = FateThreadLife,
                FateThreadWidth = FateThreadWidth,
                FateThreadSpawnChance = FateThreadSpawnChance,
                FateThreadCurlAmount = FateThreadCurlAmount,
                FateThreadSpread = FateThreadSpread,
                FateThreadColor = FateThreadColor,

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