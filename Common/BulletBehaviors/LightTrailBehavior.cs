using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class LightTrailBehavior : IBulletBehavior
    {
        public string Name => "LightTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public LightTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 1;
        public float GhostWidthScale { get; set; } = 0.3f;
        public float GhostLengthScale { get; set; } = 2.0f;
        public float GhostAlpha { get; set; } = 0.55f;
        public Color GhostColor { get; set; } = new Color(255, 255, 200, 200);

        public int MaxRays { get; set; } = 35;
        public int RayLife { get; set; } = 10;
        public float RayScale { get; set; } = 0.4f;
        public float RayLength { get; set; } = 22f;
        public int RaySpawnInterval { get; set; } = 1;
        public float RaySpread { get; set; } = 6f;
        public float RayDriftSpeed { get; set; } = 0.15f;
        public Color RayColor { get; set; } = new Color(255, 255, 220, 240);

        public int MaxPrisms { get; set; } = 15;
        public int PrismLife { get; set; } = 30;
        public float PrismSize { get; set; } = 0.4f;
        public float PrismSpawnChance { get; set; } = 0.12f;
        public float PrismHueSpeed { get; set; } = 3.0f;
        public float PrismSpinSpeed { get; set; } = 0.08f;
        public float PrismDriftSpeed { get; set; } = 0.12f;

        public int MaxHalos { get; set; } = 5;
        public int HaloLife { get; set; } = 45;
        public float HaloStartSize { get; set; } = 0.2f;
        public float HaloEndSize { get; set; } = 1.8f;
        public float HaloSpawnChance { get; set; } = 0.02f;
        public float HaloExpandSpeed { get; set; } = 2.5f;
        public float HaloRotSpeed { get; set; } = 0.04f;
        public Color HaloInnerColor { get; set; } = new Color(255, 255, 230, 220);
        public Color HaloOuterColor { get; set; } = new Color(255, 240, 180, 160);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public LightTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new LightTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxRays = MaxRays,
                RayLife = RayLife,
                RayScale = RayScale,
                RayLength = RayLength,
                RaySpawnInterval = RaySpawnInterval,
                RaySpread = RaySpread,
                RayDriftSpeed = RayDriftSpeed,
                RayColor = RayColor,

                MaxPrisms = MaxPrisms,
                PrismLife = PrismLife,
                PrismSize = PrismSize,
                PrismSpawnChance = PrismSpawnChance,
                PrismHueSpeed = PrismHueSpeed,
                PrismSpinSpeed = PrismSpinSpeed,
                PrismDriftSpeed = PrismDriftSpeed,

                MaxHalos = MaxHalos,
                HaloLife = HaloLife,
                HaloStartSize = HaloStartSize,
                HaloEndSize = HaloEndSize,
                HaloSpawnChance = HaloSpawnChance,
                HaloExpandSpeed = HaloExpandSpeed,
                HaloRotSpeed = HaloRotSpeed,
                HaloInnerColor = HaloInnerColor,
                HaloOuterColor = HaloOuterColor,

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