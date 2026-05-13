using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class BoneTrailBehavior : IBulletBehavior
    {
        public string Name => "BoneTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public BoneTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(180, 175, 160, 140);

        public int MaxRibCages { get; set; } = 8;
        public int RibCageLife { get; set; } = 50;
        public float RibCageScale { get; set; } = 0.5f;
        public float RibCageArcSpan { get; set; } = MathHelper.Pi * 0.8f;
        public float RibCageSpawnChance { get; set; } = 0.04f;
        public float RibCageGrowSpeed { get; set; } = 2.5f;
        public float RibCageDriftSpeed { get; set; } = 0.06f;
        public Color RibCageColor { get; set; } = new Color(200, 195, 180, 200);

        public int MaxMarrowGlows { get; set; } = 20;
        public int MarrowGlowLife { get; set; } = 35;
        public float MarrowGlowStartSize { get; set; } = 0.2f;
        public float MarrowGlowEndSize { get; set; } = 1.2f;
        public float MarrowGlowSpawnChance { get; set; } = 0.15f;
        public float MarrowGlowExpandSpeed { get; set; } = 2.5f;
        public float MarrowGlowDriftSpeed { get; set; } = 0.1f;
        public Color MarrowGlowColor { get; set; } = new Color(140, 200, 180, 180);

        public int MaxBoneSpikes { get; set; } = 25;
        public int BoneSpikeLife { get; set; } = 22;
        public float BoneSpikeSize { get; set; } = 0.4f;
        public float BoneSpikeLength { get; set; } = 16f;
        public float BoneSpikeSpawnChance { get; set; } = 0.2f;
        public float BoneSpikeSpinSpeed { get; set; } = 0.08f;
        public float BoneSpikeDriftSpeed { get; set; } = 0.25f;
        public Color BoneSpikeColor { get; set; } = new Color(220, 215, 200, 220);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public BoneTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new BoneTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxRibCages = MaxRibCages,
                RibCageLife = RibCageLife,
                RibCageScale = RibCageScale,
                RibCageArcSpan = RibCageArcSpan,
                RibCageSpawnChance = RibCageSpawnChance,
                RibCageGrowSpeed = RibCageGrowSpeed,
                RibCageDriftSpeed = RibCageDriftSpeed,
                RibCageColor = RibCageColor,

                MaxMarrowGlows = MaxMarrowGlows,
                MarrowGlowLife = MarrowGlowLife,
                MarrowGlowStartSize = MarrowGlowStartSize,
                MarrowGlowEndSize = MarrowGlowEndSize,
                MarrowGlowSpawnChance = MarrowGlowSpawnChance,
                MarrowGlowExpandSpeed = MarrowGlowExpandSpeed,
                MarrowGlowDriftSpeed = MarrowGlowDriftSpeed,
                MarrowGlowColor = MarrowGlowColor,

                MaxBoneSpikes = MaxBoneSpikes,
                BoneSpikeLife = BoneSpikeLife,
                BoneSpikeSize = BoneSpikeSize,
                BoneSpikeLength = BoneSpikeLength,
                BoneSpikeSpawnChance = BoneSpikeSpawnChance,
                BoneSpikeSpinSpeed = BoneSpikeSpinSpeed,
                BoneSpikeDriftSpeed = BoneSpikeDriftSpeed,
                BoneSpikeColor = BoneSpikeColor,

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
