using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class GoldTrailBehavior : IBulletBehavior
    {
        public string Name => "GoldTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public GoldTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.22f;
        public float GhostLengthScale { get; set; } = 1.6f;
        public float GhostAlpha { get; set; } = 0.45f;
        public Color GhostColor { get; set; } = new Color(255, 215, 80, 180);

        public int MaxBlades { get; set; } = 40;
        public int BladeLife { get; set; } = 12;
        public float BladeScale { get; set; } = 0.6f;
        public float BladeLength { get; set; } = 18f;
        public int BladeSpawnInterval { get; set; } = 1;
        public float BladeDriftSpeed { get; set; } = 0.2f;
        public float BladeSpread { get; set; } = 5f;
        public Color BladeColor { get; set; } = new Color(255, 230, 140, 240);

        public int MaxPrisms { get; set; } = 20;
        public int PrismLife { get; set; } = 28;
        public float PrismSize { get; set; } = 0.45f;
        public float PrismSpawnChance { get; set; } = 0.18f;
        public float PrismDriftSpeed { get; set; } = 0.15f;
        public float PrismHueSpeed { get; set; } = 2.0f;

        public int MaxShards { get; set; } = 15;
        public int ShardLife { get; set; } = 25;
        public float ShardSize { get; set; } = 0.6f;
        public float ShardStretch { get; set; } = 2.2f;
        public float ShardSpawnChance { get; set; } = 0.08f;
        public float ShardSpinSpeed { get; set; } = 0.1f;
        public float ShardDriftSpeed { get; set; } = 0.25f;
        public Color ShardColor { get; set; } = new Color(255, 220, 100, 220);

        public int MaxFlashes { get; set; } = 8;
        public int FlashLife { get; set; } = 6;
        public float FlashSize { get; set; } = 0.8f;
        public float FlashSpawnChance { get; set; } = 0.04f;
        public Color FlashColor { get; set; } = new Color(255, 250, 220, 255);

        public float InertiaFactor { get; set; } = 0.18f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public GoldTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new GoldTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxBlades = MaxBlades,
                BladeLife = BladeLife,
                BladeScale = BladeScale,
                BladeLength = BladeLength,
                BladeSpawnInterval = BladeSpawnInterval,
                BladeDriftSpeed = BladeDriftSpeed,
                BladeSpread = BladeSpread,
                BladeColor = BladeColor,

                MaxPrisms = MaxPrisms,
                PrismLife = PrismLife,
                PrismSize = PrismSize,
                PrismSpawnChance = PrismSpawnChance,
                PrismDriftSpeed = PrismDriftSpeed,
                PrismHueSpeed = PrismHueSpeed,

                MaxShards = MaxShards,
                ShardLife = ShardLife,
                ShardSize = ShardSize,
                ShardStretch = ShardStretch,
                ShardSpawnChance = ShardSpawnChance,
                ShardSpinSpeed = ShardSpinSpeed,
                ShardDriftSpeed = ShardDriftSpeed,
                ShardColor = ShardColor,

                MaxFlashes = MaxFlashes,
                FlashLife = FlashLife,
                FlashSize = FlashSize,
                FlashSpawnChance = FlashSpawnChance,
                FlashColor = FlashColor,

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
