using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class SoulTrailBehavior : IBulletBehavior
    {
        public string Name => "SoulTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public SoulTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(120, 180, 255, 160);

        public int MaxFlames { get; set; } = 18;
        public int FlameLife { get; set; } = 40;
        public float FlameSize { get; set; } = 0.6f;
        public float FlameSpawnChance { get; set; } = 0.08f;
        public float FlameWanderAmp { get; set; } = 0.5f;
        public float FlameDriftSpeed { get; set; } = 0.12f;
        public Color FlameColor { get; set; } = new Color(140, 200, 255, 230);

        public int MaxChains { get; set; } = 6;
        public int ChainLife { get; set; } = 35;
        public float ChainWidth { get; set; } = 0.4f;
        public float ChainSpawnChance { get; set; } = 0.03f;
        public int ChainSegmentCount { get; set; } = 6;
        public float ChainDragStrength { get; set; } = 0.05f;
        public float ChainReach { get; set; } = 22f;
        public Color ChainColor { get; set; } = new Color(100, 160, 240, 200);

        public int MaxWisps { get; set; } = 5;
        public int WispLife { get; set; } = 50;
        public float WispStartSize { get; set; } = 0.3f;
        public float WispEndSize { get; set; } = 0.8f;
        public float WispSpawnChance { get; set; } = 0.015f;
        public float WispOrbitRadius { get; set; } = 20f;
        public float WispOrbitSpeed { get; set; } = 0.06f;
        public float WispLeadSpeed { get; set; } = 2f;
        public Color WispColor { get; set; } = new Color(180, 220, 255, 240);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public SoulTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new SoulTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxFlames = MaxFlames,
                FlameLife = FlameLife,
                FlameSize = FlameSize,
                FlameSpawnChance = FlameSpawnChance,
                FlameWanderAmp = FlameWanderAmp,
                FlameDriftSpeed = FlameDriftSpeed,
                FlameColor = FlameColor,

                MaxChains = MaxChains,
                ChainLife = ChainLife,
                ChainWidth = ChainWidth,
                ChainSpawnChance = ChainSpawnChance,
                ChainSegmentCount = ChainSegmentCount,
                ChainDragStrength = ChainDragStrength,
                ChainReach = ChainReach,
                ChainColor = ChainColor,

                MaxWisps = MaxWisps,
                WispLife = WispLife,
                WispStartSize = WispStartSize,
                WispEndSize = WispEndSize,
                WispSpawnChance = WispSpawnChance,
                WispOrbitRadius = WispOrbitRadius,
                WispOrbitSpeed = WispOrbitSpeed,
                WispLeadSpeed = WispLeadSpeed,
                WispColor = WispColor,

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
