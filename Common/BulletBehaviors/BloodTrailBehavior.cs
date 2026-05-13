using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class BloodTrailBehavior : IBulletBehavior
    {
        public string Name => "BloodTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public BloodTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.25f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.4f;
        public Color GhostColor { get; set; } = new Color(180, 30, 30, 180);

        public int MaxDrops { get; set; } = 30;
        public int DropLife { get; set; } = 35;
        public float DropSize { get; set; } = 0.5f;
        public int DropSpawnInterval { get; set; } = 1;
        public float DropGravity { get; set; } = 0.15f;
        public float DropStretch { get; set; } = 2.5f;
        public float DropSpread { get; set; } = 5f;
        public float DropSpeed { get; set; } = 0.4f;
        public Color DropColor { get; set; } = new Color(200, 30, 30, 230);

        public int MaxVeins { get; set; } = 12;
        public int VeinLife { get; set; } = 45;
        public float VeinSize { get; set; } = 0.5f;
        public float VeinLength { get; set; } = 18f;
        public float VeinSpawnChance { get; set; } = 0.05f;
        public float VeinGrowSpeed { get; set; } = 3f;
        public float VeinDriftSpeed { get; set; } = 0.06f;
        public int VeinMaxDepth { get; set; } = 2;
        public float VeinSubAngle { get; set; } = 0.7f;
        public Color VeinColor { get; set; } = new Color(160, 20, 40, 210);

        public int MaxMists { get; set; } = 15;
        public int MistLife { get; set; } = 50;
        public float MistStartSize { get; set; } = 0.3f;
        public float MistEndSize { get; set; } = 1.8f;
        public float MistSpawnChance { get; set; } = 0.03f;
        public float MistExpandSpeed { get; set; } = 2f;
        public float MistRotSpeed { get; set; } = 0.02f;
        public float MistDriftSpeed { get; set; } = 0.08f;
        public Color MistColor { get; set; } = new Color(140, 20, 30, 160);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public BloodTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new BloodTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxDrops = MaxDrops,
                DropLife = DropLife,
                DropSize = DropSize,
                DropSpawnInterval = DropSpawnInterval,
                DropGravity = DropGravity,
                DropStretch = DropStretch,
                DropSpread = DropSpread,
                DropSpeed = DropSpeed,
                DropColor = DropColor,

                MaxVeins = MaxVeins,
                VeinLife = VeinLife,
                VeinSize = VeinSize,
                VeinLength = VeinLength,
                VeinSpawnChance = VeinSpawnChance,
                VeinGrowSpeed = VeinGrowSpeed,
                VeinDriftSpeed = VeinDriftSpeed,
                VeinMaxDepth = VeinMaxDepth,
                VeinSubAngle = VeinSubAngle,
                VeinColor = VeinColor,

                MaxMists = MaxMists,
                MistLife = MistLife,
                MistStartSize = MistStartSize,
                MistEndSize = MistEndSize,
                MistSpawnChance = MistSpawnChance,
                MistExpandSpeed = MistExpandSpeed,
                MistRotSpeed = MistRotSpeed,
                MistDriftSpeed = MistDriftSpeed,
                MistColor = MistColor,

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