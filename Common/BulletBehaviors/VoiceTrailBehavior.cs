using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class VoiceTrailBehavior : IBulletBehavior
    {
        public string Name => "VoiceTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public VoiceTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.15f;
        public float GhostLengthScale { get; set; } = 1.3f;
        public float GhostAlpha { get; set; } = 0.25f;
        public Color GhostColor { get; set; } = new Color(180, 160, 220, 120);

        public int MaxSoundWaves { get; set; } = 12;
        public int SoundWaveLife { get; set; } = 22;
        public float SoundWaveSize { get; set; } = 0.5f;
        public float SoundWaveExpandRate { get; set; } = 3f;
        public int SoundWaveSpawnInterval { get; set; } = 4;
        public float SoundWaveRingSpeed { get; set; } = 0.12f;
        public float SoundWaveThickness { get; set; } = 0.3f;
        public float SoundWaveSpread { get; set; } = 4f;
        public Color SoundWaveColor { get; set; } = new Color(200, 180, 240, 200);

        public int MaxMusicNotes { get; set; } = 10;
        public int MusicNoteLife { get; set; } = 35;
        public float MusicNoteSize { get; set; } = 0.45f;
        public float MusicNoteSpawnChance { get; set; } = 0.05f;
        public float MusicNoteFloatSpeed { get; set; } = 0.04f;
        public float MusicNoteFloatAmplitude { get; set; } = 3f;
        public float MusicNoteRiseSpeed { get; set; } = 0.3f;
        public float MusicNoteSpread { get; set; } = 5f;
        public Color MusicNoteColor { get; set; } = new Color(220, 200, 255, 220);

        public int MaxResonances { get; set; } = 15;
        public int ResonanceLife { get; set; } = 18;
        public float ResonanceSize { get; set; } = 0.35f;
        public float ResonanceSpawnChance { get; set; } = 0.1f;
        public float ResonanceVibrateSpeed { get; set; } = 0.15f;
        public float ResonanceVibrateAmplitude { get; set; } = 1.5f;
        public float ResonanceSpeed { get; set; } = 1.5f;
        public float ResonanceSpread { get; set; } = 3f;
        public Color ResonanceColor { get; set; } = new Color(160, 140, 220, 230);

        public float InertiaFactor { get; set; } = 0.12f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public VoiceTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new VoiceTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxSoundWaves = MaxSoundWaves,
                SoundWaveLife = SoundWaveLife,
                SoundWaveSize = SoundWaveSize,
                SoundWaveExpandRate = SoundWaveExpandRate,
                SoundWaveSpawnInterval = SoundWaveSpawnInterval,
                SoundWaveRingSpeed = SoundWaveRingSpeed,
                SoundWaveThickness = SoundWaveThickness,
                SoundWaveSpread = SoundWaveSpread,
                SoundWaveColor = SoundWaveColor,

                MaxMusicNotes = MaxMusicNotes,
                MusicNoteLife = MusicNoteLife,
                MusicNoteSize = MusicNoteSize,
                MusicNoteSpawnChance = MusicNoteSpawnChance,
                MusicNoteFloatSpeed = MusicNoteFloatSpeed,
                MusicNoteFloatAmplitude = MusicNoteFloatAmplitude,
                MusicNoteRiseSpeed = MusicNoteRiseSpeed,
                MusicNoteSpread = MusicNoteSpread,
                MusicNoteColor = MusicNoteColor,

                MaxResonances = MaxResonances,
                ResonanceLife = ResonanceLife,
                ResonanceSize = ResonanceSize,
                ResonanceSpawnChance = ResonanceSpawnChance,
                ResonanceVibrateSpeed = ResonanceVibrateSpeed,
                ResonanceVibrateAmplitude = ResonanceVibrateAmplitude,
                ResonanceSpeed = ResonanceSpeed,
                ResonanceSpread = ResonanceSpread,
                ResonanceColor = ResonanceColor,

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