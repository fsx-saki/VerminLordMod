using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class StarTrailBehavior : IBulletBehavior
    {
        public string Name => "StarTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public StarTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;

        public int GhostMaxPositions { get; set; } = 10;

        public int GhostRecordInterval { get; set; } = 2;

        public float GhostWidthScale { get; set; } = 0.2f;

        public float GhostLengthScale { get; set; } = 1.5f;

        public float GhostAlpha { get; set; } = 0.4f;

        public Color GhostColor { get; set; } = new Color(180, 170, 230, 180);

        public int MaxStarPoints { get; set; } = 35;

        public int StarLife { get; set; } = 50;

        public float StarSize { get; set; } = 0.45f;

        public int StarSpawnInterval { get; set; } = 2;

        public float StarDriftSpeed { get; set; } = 0.4f;

        public float StarSpread { get; set; } = 6f;

        public Color StarColor { get; set; } = new Color(220, 215, 255, 230);

        public float LineMaxDistance { get; set; } = 55f;

        public float LineBreakDistance { get; set; } = 80f;

        public float LineBaseAlpha { get; set; } = 0.25f;

        public Color LineColor { get; set; } = new Color(180, 175, 230, 200);

        public int MaxNebula { get; set; } = 8;

        public int NebulaLife { get; set; } = 60;

        public float NebulaStartSize { get; set; } = 0.3f;

        public float NebulaEndSize { get; set; } = 2.0f;

        public float NebulaSpawnChance { get; set; } = 0.04f;

        public float NebulaDriftSpeed { get; set; } = 0.15f;

        public Color NebulaColor { get; set; } = new Color(130, 110, 200, 120);

        public int MaxStardust { get; set; } = 40;

        public int StardustLife { get; set; } = 35;

        public float StardustSize { get; set; } = 0.2f;

        public float StardustSpawnChance { get; set; } = 0.3f;

        public float StardustDriftSpeed { get; set; } = 0.5f;

        public Color StardustColor { get; set; } = new Color(200, 195, 255, 180);

        public float InertiaFactor { get; set; } = 0.15f;

        public float RandomSpread { get; set; } = 3f;

        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;

        public bool SuppressDefaultDraw { get; set; } = false;

        public StarTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new StarTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxStarPoints = MaxStarPoints,
                StarLife = StarLife,
                StarSize = StarSize,
                StarSpawnInterval = StarSpawnInterval,
                StarDriftSpeed = StarDriftSpeed,
                StarSpread = StarSpread,
                StarColor = StarColor,

                LineMaxDistance = LineMaxDistance,
                LineBreakDistance = LineBreakDistance,
                LineBaseAlpha = LineBaseAlpha,
                LineColor = LineColor,

                MaxNebula = MaxNebula,
                NebulaLife = NebulaLife,
                NebulaStartSize = NebulaStartSize,
                NebulaEndSize = NebulaEndSize,
                NebulaSpawnChance = NebulaSpawnChance,
                NebulaDriftSpeed = NebulaDriftSpeed,
                NebulaColor = NebulaColor,

                MaxStardust = MaxStardust,
                StardustLife = StardustLife,
                StardustSize = StardustSize,
                StardustSpawnChance = StardustSpawnChance,
                StardustDriftSpeed = StardustDriftSpeed,
                StardustColor = StardustColor,

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
            {
                TrailManager.Draw(spriteBatch);
            }
            return !SuppressDefaultDraw;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
