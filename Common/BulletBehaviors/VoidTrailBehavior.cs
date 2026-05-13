using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class VoidTrailBehavior : IBulletBehavior
    {
        public string Name => "VoidTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public VoidTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(80, 20, 130, 160);

        public int MaxOrbs { get; set; } = 25;
        public int OrbLife { get; set; } = 40;
        public float OrbSize { get; set; } = 0.5f;
        public int OrbSpawnInterval { get; set; } = 2;
        public float OrbRotSpeed { get; set; } = 0.06f;
        public float OrbDriftSpeed { get; set; } = 0.2f;
        public float OrbSpread { get; set; } = 5f;
        public Color OrbColor { get; set; } = new Color(80, 15, 140, 220);

        public int MaxDistortions { get; set; } = 5;
        public int DistortionLife { get; set; } = 50;
        public float DistortionStartSize { get; set; } = 0.3f;
        public float DistortionEndSize { get; set; } = 2.0f;
        public float DistortionSpawnChance { get; set; } = 0.02f;
        public float DistortionRotSpeed { get; set; } = 0.05f;
        public float DistortionDriftSpeed { get; set; } = 0.08f;
        public Color DistortionColor { get; set; } = new Color(90, 20, 130, 160);

        public int MaxShards { get; set; } = 20;
        public int ShardLife { get; set; } = 25;
        public float ShardSize { get; set; } = 0.4f;
        public float ShardSpawnChance { get; set; } = 0.15f;
        public float ShardSpinSpeed { get; set; } = 0.12f;
        public float ShardDriftSpeed { get; set; } = 0.3f;
        public Color ShardColor { get; set; } = new Color(70, 15, 110, 200);

        public float TendrilMaxDistance { get; set; } = 50f;
        public float TendrilBreakDistance { get; set; } = 80f;
        public float TendrilBaseAlpha { get; set; } = 0.2f;
        public Color TendrilColor { get; set; } = new Color(70, 15, 110, 180);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public VoidTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new VoidTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxOrbs = MaxOrbs,
                OrbLife = OrbLife,
                OrbSize = OrbSize,
                OrbSpawnInterval = OrbSpawnInterval,
                OrbRotSpeed = OrbRotSpeed,
                OrbDriftSpeed = OrbDriftSpeed,
                OrbSpread = OrbSpread,
                OrbColor = OrbColor,

                MaxDistortions = MaxDistortions,
                DistortionLife = DistortionLife,
                DistortionStartSize = DistortionStartSize,
                DistortionEndSize = DistortionEndSize,
                DistortionSpawnChance = DistortionSpawnChance,
                DistortionRotSpeed = DistortionRotSpeed,
                DistortionDriftSpeed = DistortionDriftSpeed,
                DistortionColor = DistortionColor,

                MaxShards = MaxShards,
                ShardLife = ShardLife,
                ShardSize = ShardSize,
                ShardSpawnChance = ShardSpawnChance,
                ShardSpinSpeed = ShardSpinSpeed,
                ShardDriftSpeed = ShardDriftSpeed,
                ShardColor = ShardColor,

                TendrilMaxDistance = TendrilMaxDistance,
                TendrilBreakDistance = TendrilBreakDistance,
                TendrilBaseAlpha = TendrilBaseAlpha,
                TendrilColor = TendrilColor,

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
