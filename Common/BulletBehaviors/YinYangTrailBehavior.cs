using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class YinYangTrailBehavior : IBulletBehavior
    {
        public string Name => "YinYangTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public YinYangTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.4f;
        public Color GhostColor { get; set; } = new Color(200, 195, 240, 180);

        public int MaxOrbs { get; set; } = 20;
        public int OrbLife { get; set; } = 35;
        public float OrbSize { get; set; } = 0.45f;
        public int OrbSpawnInterval { get; set; } = 2;
        public float OrbRotSpeed { get; set; } = 0.08f;
        public float OrbDriftSpeed { get; set; } = 0.2f;
        public float OrbSpread { get; set; } = 5f;
        public Color OrbYinColor { get; set; } = new Color(60, 50, 100, 220);
        public Color OrbYangColor { get; set; } = new Color(230, 225, 255, 220);

        public int MaxFish { get; set; } = 8;
        public int FishLife { get; set; } = 50;
        public float FishSize { get; set; } = 0.5f;
        public float FishSpawnChance { get; set; } = 0.04f;
        public float FishRotSpeed { get; set; } = 0.12f;
        public float FishDriftSpeed { get; set; } = 0.15f;
        public Color FishYinColor { get; set; } = new Color(50, 40, 90, 200);
        public Color FishYangColor { get; set; } = new Color(220, 215, 250, 200);

        public int MaxRipples { get; set; } = 6;
        public int RippleLife { get; set; } = 45;
        public float RippleStartSize { get; set; } = 0.3f;
        public float RippleEndSize { get; set; } = 1.8f;
        public float RippleSpawnChance { get; set; } = 0.025f;
        public float RippleRotSpeed { get; set; } = 0.04f;
        public float RippleDriftSpeed { get; set; } = 0.08f;
        public Color RippleColor { get; set; } = new Color(160, 150, 220, 150);

        public int MaxSparks { get; set; } = 30;
        public int SparkLife { get; set; } = 25;
        public float SparkSize { get; set; } = 0.25f;
        public float SparkSpawnChance { get; set; } = 0.25f;
        public float SparkDriftSpeed { get; set; } = 0.35f;
        public Color SparkYinColor { get; set; } = new Color(80, 70, 140, 200);
        public Color SparkYangColor { get; set; } = new Color(220, 215, 255, 200);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public YinYangTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new YinYangTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxOrbs = MaxOrbs,
                OrbLife = OrbLife,
                OrbSize = OrbSize,
                OrbSpawnInterval = OrbSpawnInterval,
                OrbRotSpeed = OrbRotSpeed,
                OrbDriftSpeed = OrbDriftSpeed,
                OrbSpread = OrbSpread,
                OrbYinColor = OrbYinColor,
                OrbYangColor = OrbYangColor,

                MaxFish = MaxFish,
                FishLife = FishLife,
                FishSize = FishSize,
                FishSpawnChance = FishSpawnChance,
                FishRotSpeed = FishRotSpeed,
                FishDriftSpeed = FishDriftSpeed,
                FishYinColor = FishYinColor,
                FishYangColor = FishYangColor,

                MaxRipples = MaxRipples,
                RippleLife = RippleLife,
                RippleStartSize = RippleStartSize,
                RippleEndSize = RippleEndSize,
                RippleSpawnChance = RippleSpawnChance,
                RippleRotSpeed = RippleRotSpeed,
                RippleDriftSpeed = RippleDriftSpeed,
                RippleColor = RippleColor,

                MaxSparks = MaxSparks,
                SparkLife = SparkLife,
                SparkSize = SparkSize,
                SparkSpawnChance = SparkSpawnChance,
                SparkDriftSpeed = SparkDriftSpeed,
                SparkYinColor = SparkYinColor,
                SparkYangColor = SparkYangColor,

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
