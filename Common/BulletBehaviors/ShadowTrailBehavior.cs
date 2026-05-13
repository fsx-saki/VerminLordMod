using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class ShadowTrailBehavior : IBulletBehavior
    {
        public string Name => "ShadowTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public ShadowTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 12;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(30, 25, 50, 160);

        public int MaxTendrils { get; set; } = 10;
        public int TendrilLife { get; set; } = 40;
        public float TendrilWidth { get; set; } = 0.6f;
        public float TendrilSpawnChance { get; set; } = 0.04f;
        public float TendrilReachSpeed { get; set; } = 2.5f;
        public float TendrilMaxReach { get; set; } = 30f;
        public float TendrilRetractStart { get; set; } = 0.5f;
        public float TendrilDriftSpeed { get; set; } = 0.05f;
        public Color TendrilColor { get; set; } = new Color(40, 30, 70, 200);

        public int MaxPools { get; set; } = 8;
        public int PoolLife { get; set; } = 60;
        public float PoolStartSize { get; set; } = 0.2f;
        public float PoolEndSize { get; set; } = 2.0f;
        public float PoolSpawnChance { get; set; } = 0.02f;
        public float PoolSpreadSpeed { get; set; } = 2f;
        public float PoolDriftSpeed { get; set; } = 0.04f;
        public Color PoolColor { get; set; } = new Color(25, 20, 45, 180);

        public int MaxClones { get; set; } = 6;
        public int CloneLife { get; set; } = 35;
        public float CloneSize { get; set; } = 0.7f;
        public float CloneSpawnChance { get; set; } = 0.015f;
        public float CloneFollowStrength { get; set; } = 0.08f;
        public Color CloneColor { get; set; } = new Color(50, 40, 80, 180);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public ShadowTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new ShadowTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxTendrils = MaxTendrils,
                TendrilLife = TendrilLife,
                TendrilWidth = TendrilWidth,
                TendrilSpawnChance = TendrilSpawnChance,
                TendrilReachSpeed = TendrilReachSpeed,
                TendrilMaxReach = TendrilMaxReach,
                TendrilRetractStart = TendrilRetractStart,
                TendrilDriftSpeed = TendrilDriftSpeed,
                TendrilColor = TendrilColor,

                MaxPools = MaxPools,
                PoolLife = PoolLife,
                PoolStartSize = PoolStartSize,
                PoolEndSize = PoolEndSize,
                PoolSpawnChance = PoolSpawnChance,
                PoolSpreadSpeed = PoolSpreadSpeed,
                PoolDriftSpeed = PoolDriftSpeed,
                PoolColor = PoolColor,

                MaxClones = MaxClones,
                CloneLife = CloneLife,
                CloneSize = CloneSize,
                CloneSpawnChance = CloneSpawnChance,
                CloneFollowStrength = CloneFollowStrength,
                CloneColor = CloneColor,

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