using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class MeleeSwingTrailBehavior : IBulletBehavior
    {
        public string Name => "MeleeSwingTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public MeleeSwingTrail Trail { get; private set; }

        public int MaxSwingArcs { get; set; } = 6;
        public int SwingArcLife { get; set; } = 15;
        public float SwingArcLength { get; set; } = 35f;
        public float SwingArcWidth { get; set; } = 0.3f;
        public float SwingArcSpawnChance { get; set; } = 0.15f;
        public float SwingArcCurlAmount { get; set; } = 3f;
        public Color SwingArcColor { get; set; } = new Color(200, 200, 220, 200);

        public int MaxStabImpacts { get; set; } = 8;
        public int StabImpactLife { get; set; } = 12;
        public float StabImpactSize { get; set; } = 0.5f;
        public float StabImpactSpawnChance { get; set; } = 0.2f;
        public float StabImpactSpread { get; set; } = 0.4f;
        public float StabImpactStretch { get; set; } = 2.5f;
        public Color StabImpactColor { get; set; } = new Color(220, 210, 200, 220);

        public int MaxSmashRings { get; set; } = 3;
        public int SmashRingLife { get; set; } = 30;
        public float SmashRingStartRadius { get; set; } = 2f;
        public float SmashRingEndRadius { get; set; } = 40f;
        public float SmashRingWidth { get; set; } = 0.4f;
        public float SmashRingSpawnChance { get; set; } = 0.02f;
        public float SmashRingExpandSpeed { get; set; } = 1.2f;
        public int SmashRingCrackCount { get; set; } = 6;
        public Color SmashRingColor { get; set; } = new Color(180, 180, 200, 180);

        public float InertiaFactor { get; set; } = 0.1f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public MeleeSwingTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new MeleeSwingTrail
            {
                MaxSwingArcs = MaxSwingArcs,
                SwingArcLife = SwingArcLife,
                SwingArcLength = SwingArcLength,
                SwingArcWidth = SwingArcWidth,
                SwingArcSpawnChance = SwingArcSpawnChance,
                SwingArcCurlAmount = SwingArcCurlAmount,
                SwingArcColor = SwingArcColor,

                MaxStabImpacts = MaxStabImpacts,
                StabImpactLife = StabImpactLife,
                StabImpactSize = StabImpactSize,
                StabImpactSpawnChance = StabImpactSpawnChance,
                StabImpactSpread = StabImpactSpread,
                StabImpactStretch = StabImpactStretch,
                StabImpactColor = StabImpactColor,

                MaxSmashRings = MaxSmashRings,
                SmashRingLife = SmashRingLife,
                SmashRingStartRadius = SmashRingStartRadius,
                SmashRingEndRadius = SmashRingEndRadius,
                SmashRingWidth = SmashRingWidth,
                SmashRingSpawnChance = SmashRingSpawnChance,
                SmashRingExpandSpeed = SmashRingExpandSpeed,
                SmashRingCrackCount = SmashRingCrackCount,
                SmashRingColor = SmashRingColor,

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