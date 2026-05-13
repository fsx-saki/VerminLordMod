using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class DreamTrailBehavior : IBulletBehavior
    {
        public string Name => "DreamTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public DreamTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(200, 150, 220, 140);

        public int MaxBubbles { get; set; } = 10;
        public int BubbleLife { get; set; } = 55;
        public float BubbleStartSize { get; set; } = 0.3f;
        public float BubbleEndSize { get; set; } = 1.6f;
        public float BubbleSpawnChance { get; set; } = 0.025f;
        public float BubbleExpandSpeed { get; set; } = 1.5f;
        public float BubbleDriftSpeed { get; set; } = 0.08f;
        public float BubbleIridescence { get; set; } = 0.7f;
        public Color BubbleColor { get; set; } = new Color(200, 160, 240, 200);

        public int MaxRipples { get; set; } = 6;
        public int RippleLife { get; set; } = 40;
        public float RippleStartRadius { get; set; } = 2f;
        public float RippleEndRadius { get; set; } = 30f;
        public float RippleWidth { get; set; } = 0.4f;
        public float RippleSpawnChance { get; set; } = 0.02f;
        public float RippleExpandSpeed { get; set; } = 2f;
        public Color RippleColor { get; set; } = new Color(180, 140, 220, 180);

        public int MaxButterflies { get; set; } = 8;
        public int ButterflyLife { get; set; } = 50;
        public float ButterflySize { get; set; } = 0.6f;
        public float ButterflySpawnChance { get; set; } = 0.03f;
        public float ButterflyWingSpeed { get; set; } = 0.12f;
        public float ButterflyDissolveSpeed { get; set; } = 1.2f;
        public float ButterflyDriftSpeed { get; set; } = 0.15f;
        public Color ButterflyColor { get; set; } = new Color(220, 180, 255, 220);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public DreamTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new DreamTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxBubbles = MaxBubbles,
                BubbleLife = BubbleLife,
                BubbleStartSize = BubbleStartSize,
                BubbleEndSize = BubbleEndSize,
                BubbleSpawnChance = BubbleSpawnChance,
                BubbleExpandSpeed = BubbleExpandSpeed,
                BubbleDriftSpeed = BubbleDriftSpeed,
                BubbleIridescence = BubbleIridescence,
                BubbleColor = BubbleColor,

                MaxRipples = MaxRipples,
                RippleLife = RippleLife,
                RippleStartRadius = RippleStartRadius,
                RippleEndRadius = RippleEndRadius,
                RippleWidth = RippleWidth,
                RippleSpawnChance = RippleSpawnChance,
                RippleExpandSpeed = RippleExpandSpeed,
                RippleColor = RippleColor,

                MaxButterflies = MaxButterflies,
                ButterflyLife = ButterflyLife,
                ButterflySize = ButterflySize,
                ButterflySpawnChance = ButterflySpawnChance,
                ButterflyWingSpeed = ButterflyWingSpeed,
                ButterflyDissolveSpeed = ButterflyDissolveSpeed,
                ButterflyDriftSpeed = ButterflyDriftSpeed,
                ButterflyColor = ButterflyColor,

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
