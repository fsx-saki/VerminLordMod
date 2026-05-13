using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class WisdomTrailBehavior : IBulletBehavior
    {
        public string Name => "WisdomTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public WisdomTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.16f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.25f;
        public Color GhostColor { get; set; } = new Color(200, 200, 160, 120);

        public int MaxRuneSymbols { get; set; } = 15;
        public int RuneSymbolLife { get; set; } = 30;
        public float RuneSymbolSize { get; set; } = 0.5f;
        public float RuneSymbolSpawnChance { get; set; } = 0.06f;
        public float RuneSymbolSpinSpeed { get; set; } = 0.04f;
        public float RuneSymbolGlowSpeed { get; set; } = 0.08f;
        public float RuneSymbolDriftSpeed { get; set; } = 0.3f;
        public float RuneSymbolSpread { get; set; } = 5f;
        public Color RuneSymbolColor { get; set; } = new Color(220, 210, 140, 230);

        public int MaxWisdomGlows { get; set; } = 12;
        public int WisdomGlowLife { get; set; } = 25;
        public float WisdomGlowSize { get; set; } = 0.5f;
        public float WisdomGlowExpandRate { get; set; } = 1.8f;
        public float WisdomGlowSpawnChance { get; set; } = 0.08f;
        public float WisdomGlowPulseSpeed { get; set; } = 0.07f;
        public float WisdomGlowHueShift { get; set; } = 0.02f;
        public float WisdomGlowSpread { get; set; } = 4f;
        public Color WisdomGlowColor { get; set; } = new Color(200, 200, 120, 200);

        public int MaxBookPages { get; set; } = 10;
        public int BookPageLife { get; set; } = 40;
        public float BookPageSize { get; set; } = 0.4f;
        public float BookPageSpawnChance { get; set; } = 0.04f;
        public float BookPageSpinSpeed { get; set; } = 0.06f;
        public float BookPageFlutterSpeed { get; set; } = 0.05f;
        public float BookPageFlutterAmplitude { get; set; } = 2f;
        public float BookPageDriftSpeed { get; set; } = 0.2f;
        public float BookPageSpread { get; set; } = 6f;
        public Color BookPageColor { get; set; } = new Color(180, 170, 120, 180);

        public float InertiaFactor { get; set; } = 0.1f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public WisdomTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new WisdomTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxRuneSymbols = MaxRuneSymbols,
                RuneSymbolLife = RuneSymbolLife,
                RuneSymbolSize = RuneSymbolSize,
                RuneSymbolSpawnChance = RuneSymbolSpawnChance,
                RuneSymbolSpinSpeed = RuneSymbolSpinSpeed,
                RuneSymbolGlowSpeed = RuneSymbolGlowSpeed,
                RuneSymbolDriftSpeed = RuneSymbolDriftSpeed,
                RuneSymbolSpread = RuneSymbolSpread,
                RuneSymbolColor = RuneSymbolColor,

                MaxWisdomGlows = MaxWisdomGlows,
                WisdomGlowLife = WisdomGlowLife,
                WisdomGlowSize = WisdomGlowSize,
                WisdomGlowExpandRate = WisdomGlowExpandRate,
                WisdomGlowSpawnChance = WisdomGlowSpawnChance,
                WisdomGlowPulseSpeed = WisdomGlowPulseSpeed,
                WisdomGlowHueShift = WisdomGlowHueShift,
                WisdomGlowSpread = WisdomGlowSpread,
                WisdomGlowColor = WisdomGlowColor,

                MaxBookPages = MaxBookPages,
                BookPageLife = BookPageLife,
                BookPageSize = BookPageSize,
                BookPageSpawnChance = BookPageSpawnChance,
                BookPageSpinSpeed = BookPageSpinSpeed,
                BookPageFlutterSpeed = BookPageFlutterSpeed,
                BookPageFlutterAmplitude = BookPageFlutterAmplitude,
                BookPageDriftSpeed = BookPageDriftSpeed,
                BookPageSpread = BookPageSpread,
                BookPageColor = BookPageColor,

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