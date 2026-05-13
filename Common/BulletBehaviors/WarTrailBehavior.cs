using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class WarTrailBehavior : IBulletBehavior
    {
        public string Name => "WarTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public WarTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.25f;
        public Color GhostColor { get; set; } = new Color(180, 120, 80, 120);

        public int MaxGunSmokes { get; set; } = 20;
        public int GunSmokeLife { get; set; } = 40;
        public float GunSmokeSize { get; set; } = 0.8f;
        public float GunSmokeExpandRate { get; set; } = 2f;
        public float GunSmokeFadeDelay { get; set; } = 0.4f;
        public int GunSmokeSpawnInterval { get; set; } = 3;
        public float GunSmokeSpeed { get; set; } = 1.5f;
        public float GunSmokeSpread { get; set; } = 4f;
        public Color GunSmokeColor { get; set; } = new Color(160, 140, 120, 200);

        public int MaxShrapnels { get; set; } = 20;
        public int ShrapnelLife { get; set; } = 25;
        public float ShrapnelSize { get; set; } = 0.35f;
        public float ShrapnelSpawnChance { get; set; } = 0.1f;
        public float ShrapnelSpeed { get; set; } = 4f;
        public float ShrapnelSpinSpeed { get; set; } = 0.3f;
        public Color ShrapnelColor { get; set; } = new Color(200, 180, 140, 230);

        public int MaxWarFlames { get; set; } = 15;
        public int WarFlameLife { get; set; } = 18;
        public float WarFlameSize { get; set; } = 0.5f;
        public float WarFlameSpawnChance { get; set; } = 0.08f;
        public float WarFlameRiseSpeed { get; set; } = 0.5f;
        public float WarFlameSpread { get; set; } = 3f;
        public Color WarFlameColor { get; set; } = new Color(255, 160, 60, 255);

        public float InertiaFactor { get; set; } = 0.2f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public WarTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new WarTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxGunSmokes = MaxGunSmokes,
                GunSmokeLife = GunSmokeLife,
                GunSmokeSize = GunSmokeSize,
                GunSmokeExpandRate = GunSmokeExpandRate,
                GunSmokeFadeDelay = GunSmokeFadeDelay,
                GunSmokeSpawnInterval = GunSmokeSpawnInterval,
                GunSmokeSpeed = GunSmokeSpeed,
                GunSmokeSpread = GunSmokeSpread,
                GunSmokeColor = GunSmokeColor,

                MaxShrapnels = MaxShrapnels,
                ShrapnelLife = ShrapnelLife,
                ShrapnelSize = ShrapnelSize,
                ShrapnelSpawnChance = ShrapnelSpawnChance,
                ShrapnelSpeed = ShrapnelSpeed,
                ShrapnelSpinSpeed = ShrapnelSpinSpeed,
                ShrapnelColor = ShrapnelColor,

                MaxWarFlames = MaxWarFlames,
                WarFlameLife = WarFlameLife,
                WarFlameSize = WarFlameSize,
                WarFlameSpawnChance = WarFlameSpawnChance,
                WarFlameRiseSpeed = WarFlameRiseSpeed,
                WarFlameSpread = WarFlameSpread,
                WarFlameColor = WarFlameColor,

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