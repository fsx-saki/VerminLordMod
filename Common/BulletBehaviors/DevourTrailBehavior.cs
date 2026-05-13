using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class DevourTrailBehavior : IBulletBehavior
    {
        public string Name => "DevourTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public DevourTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 6;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.18f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(160, 80, 180, 130);

        public int MaxVortexMaws { get; set; } = 5;
        public int VortexMawLife { get; set; } = 35;
        public float VortexMawSize { get; set; } = 0.6f;
        public float VortexMawSpawnChance { get; set; } = 0.04f;
        public float VortexMawRotSpeed { get; set; } = 0.1f;
        public float VortexMawSwirlRadius { get; set; } = 4f;
        public float VortexMawSwirlSpeed { get; set; } = 0.08f;
        public float VortexMawDriftSpeed { get; set; } = 0.2f;
        public Color VortexMawColor { get; set; } = new Color(180, 80, 200, 220);

        public int MaxAcidDrops { get; set; } = 10;
        public int AcidDropLife { get; set; } = 28;
        public float AcidDropSize { get; set; } = 0.3f;
        public float AcidDropSpawnChance { get; set; } = 0.08f;
        public float AcidDropStretch { get; set; } = 1.5f;
        public float AcidDropGravity { get; set; } = 0.15f;
        public float AcidDropDriftSpeed { get; set; } = 0.4f;
        public Color AcidDropColor { get; set; } = new Color(100, 220, 80, 200);

        public int MaxAbsorbTendrils { get; set; } = 6;
        public int AbsorbTendrilLife { get; set; } = 22;
        public float AbsorbTendrilLength { get; set; } = 30f;
        public float AbsorbTendrilWidth { get; set; } = 0.2f;
        public float AbsorbTendrilSpawnChance { get; set; } = 0.06f;
        public float AbsorbTendrilWriggleAmount { get; set; } = 5f;
        public float AbsorbTendrilPullStrength { get; set; } = 0.05f;
        public float AbsorbTendrilSpread { get; set; } = 12f;
        public Color AbsorbTendrilColor { get; set; } = new Color(200, 120, 220, 180);

        public float InertiaFactor { get; set; } = 0.15f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public DevourTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new DevourTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxVortexMaws = MaxVortexMaws,
                VortexMawLife = VortexMawLife,
                VortexMawSize = VortexMawSize,
                VortexMawSpawnChance = VortexMawSpawnChance,
                VortexMawRotSpeed = VortexMawRotSpeed,
                VortexMawSwirlRadius = VortexMawSwirlRadius,
                VortexMawSwirlSpeed = VortexMawSwirlSpeed,
                VortexMawDriftSpeed = VortexMawDriftSpeed,
                VortexMawColor = VortexMawColor,

                MaxAcidDrops = MaxAcidDrops,
                AcidDropLife = AcidDropLife,
                AcidDropSize = AcidDropSize,
                AcidDropSpawnChance = AcidDropSpawnChance,
                AcidDropStretch = AcidDropStretch,
                AcidDropGravity = AcidDropGravity,
                AcidDropDriftSpeed = AcidDropDriftSpeed,
                AcidDropColor = AcidDropColor,

                MaxAbsorbTendrils = MaxAbsorbTendrils,
                AbsorbTendrilLife = AbsorbTendrilLife,
                AbsorbTendrilLength = AbsorbTendrilLength,
                AbsorbTendrilWidth = AbsorbTendrilWidth,
                AbsorbTendrilSpawnChance = AbsorbTendrilSpawnChance,
                AbsorbTendrilWriggleAmount = AbsorbTendrilWriggleAmount,
                AbsorbTendrilPullStrength = AbsorbTendrilPullStrength,
                AbsorbTendrilSpread = AbsorbTendrilSpread,
                AbsorbTendrilColor = AbsorbTendrilColor,

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