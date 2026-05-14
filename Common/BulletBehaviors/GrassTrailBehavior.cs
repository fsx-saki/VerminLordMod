using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class GrassTrailBehavior : IBulletBehavior
    {
        public string Name => "GrassTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public GrassTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(80, 200, 60, 160);

        public int MaxLeaves { get; set; } = 30;
        public int LeafLife { get; set; } = 40;
        public float LeafSize { get; set; } = 0.5f;
        public int LeafSpawnInterval { get; set; } = 2;
        public float LeafRotSpeed { get; set; } = 0.06f;
        public float LeafDriftSpeed { get; set; } = 0.25f;
        public float LeafSpread { get; set; } = 5f;
        public Color LeafColor { get; set; } = new Color(80, 200, 60, 210);

        public int MaxPollen { get; set; } = 35;
        public int PollenLife { get; set; } = 30;
        public float PollenSize { get; set; } = 0.25f;
        public float PollenSpawnChance { get; set; } = 0.25f;
        public float PollenDriftSpeed { get; set; } = 0.35f;
        public Color PollenColor { get; set; } = new Color(200, 230, 80, 200);

        public int MaxBlooms { get; set; } = 5;
        public int BloomLife { get; set; } = 50;
        public float BloomStartSize { get; set; } = 0.3f;
        public float BloomEndSize { get; set; } = 1.5f;
        public float BloomSpawnChance { get; set; } = 0.02f;
        public float BloomDriftSpeed { get; set; } = 0.08f;
        public Color BloomColor { get; set; } = new Color(150, 230, 120, 140);

        public float VineMaxDistance { get; set; } = 45f;
        public float VineBreakDistance { get; set; } = 70f;
        public float VineBaseAlpha { get; set; } = 0.25f;
        public Color VineColor { get; set; } = new Color(60, 160, 50, 180);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public GrassTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new GrassTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxLeaves = MaxLeaves,
                LeafLife = LeafLife,
                LeafSize = LeafSize,
                LeafSpawnInterval = LeafSpawnInterval,
                LeafRotSpeed = LeafRotSpeed,
                LeafDriftSpeed = LeafDriftSpeed,
                LeafSpread = LeafSpread,
                LeafColor = LeafColor,

                MaxPollen = MaxPollen,
                PollenLife = PollenLife,
                PollenSize = PollenSize,
                PollenSpawnChance = PollenSpawnChance,
                PollenDriftSpeed = PollenDriftSpeed,
                PollenColor = PollenColor,

                MaxBlooms = MaxBlooms,
                BloomLife = BloomLife,
                BloomStartSize = BloomStartSize,
                BloomEndSize = BloomEndSize,
                BloomSpawnChance = BloomSpawnChance,
                BloomDriftSpeed = BloomDriftSpeed,
                BloomColor = BloomColor,

                VineMaxDistance = VineMaxDistance,
                VineBreakDistance = VineBreakDistance,
                VineBaseAlpha = VineBaseAlpha,
                VineColor = VineColor,

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
