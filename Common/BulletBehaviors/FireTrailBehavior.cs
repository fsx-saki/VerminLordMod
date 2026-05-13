using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class FireTrailBehavior : IBulletBehavior
    {
        public string Name => "FireTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public FireTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.25f;
        public float GhostLengthScale { get; set; } = 1.6f;
        public float GhostAlpha { get; set; } = 0.5f;
        public Color GhostColor { get; set; } = new Color(255, 150, 50, 180);

        public int MaxTongues { get; set; } = 35;
        public int TongueLife { get; set; } = 20;
        public float TongueScale { get; set; } = 0.5f;
        public float TongueLength { get; set; } = 16f;
        public int TongueSpawnInterval { get; set; } = 1;
        public float TongueSwaySpeed { get; set; } = 0.15f;
        public float TongueSwayAmp { get; set; } = 0.4f;
        public float TongueRiseSpeed { get; set; } = 0.8f;
        public float TongueSpread { get; set; } = 5f;
        public Color TongueColor { get; set; } = new Color(255, 200, 80, 240);

        public int MaxEmbers { get; set; } = 25;
        public int EmberLife { get; set; } = 30;
        public float EmberSize { get; set; } = 0.35f;
        public float EmberSpawnChance { get; set; } = 0.2f;
        public float EmberRiseSpeed { get; set; } = 1.2f;
        public float EmberDriftSpeed { get; set; } = 0.4f;
        public Color EmberColor { get; set; } = new Color(255, 180, 50, 220);

        public int MaxAshes { get; set; } = 15;
        public int AshLife { get; set; } = 40;
        public float AshSize { get; set; } = 0.3f;
        public float AshSpawnChance { get; set; } = 0.08f;
        public float AshFallSpeed { get; set; } = 0.5f;
        public float AshDriftSpeed { get; set; } = 0.2f;
        public float AshSpinSpeed { get; set; } = 0.05f;
        public Color AshColor { get; set; } = new Color(120, 100, 80, 160);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public FireTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new FireTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxTongues = MaxTongues,
                TongueLife = TongueLife,
                TongueScale = TongueScale,
                TongueLength = TongueLength,
                TongueSpawnInterval = TongueSpawnInterval,
                TongueSwaySpeed = TongueSwaySpeed,
                TongueSwayAmp = TongueSwayAmp,
                TongueRiseSpeed = TongueRiseSpeed,
                TongueSpread = TongueSpread,
                TongueColor = TongueColor,

                MaxEmbers = MaxEmbers,
                EmberLife = EmberLife,
                EmberSize = EmberSize,
                EmberSpawnChance = EmberSpawnChance,
                EmberRiseSpeed = EmberRiseSpeed,
                EmberDriftSpeed = EmberDriftSpeed,
                EmberColor = EmberColor,

                MaxAshes = MaxAshes,
                AshLife = AshLife,
                AshSize = AshSize,
                AshSpawnChance = AshSpawnChance,
                AshFallSpeed = AshFallSpeed,
                AshDriftSpeed = AshDriftSpeed,
                AshSpinSpeed = AshSpinSpeed,
                AshColor = AshColor,

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
