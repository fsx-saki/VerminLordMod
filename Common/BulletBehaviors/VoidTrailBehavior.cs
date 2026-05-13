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

        public int MaxShards { get; set; } = 20;
        public int ShardLife { get; set; } = 25;
        public float ShardSize { get; set; } = 0.4f;
        public float ShardSpawnChance { get; set; } = 0.15f;
        public float ShardSpinSpeed { get; set; } = 0.12f;
        public float ShardDriftSpeed { get; set; } = 0.3f;
        public Color ShardColor { get; set; } = new Color(70, 15, 110, 200);

        public int MaxRifts { get; set; } = 6;
        public int RiftLife { get; set; } = 50;
        public float RiftStartSize { get; set; } = 0.2f;
        public float RiftEndSize { get; set; } = 1.5f;
        public float RiftSpawnChance { get; set; } = 0.025f;
        public float RiftOpenSpeed { get; set; } = 3f;
        public float RiftDriftSpeed { get; set; } = 0.06f;
        public float RiftAspectRatio { get; set; } = 2.5f;
        public Color RiftEdgeColor { get; set; } = new Color(140, 50, 200, 200);
        public Color RiftCoreColor { get; set; } = new Color(20, 5, 40, 180);

        public int MaxInflows { get; set; } = 30;
        public int InflowLife { get; set; } = 25;
        public float InflowSize { get; set; } = 0.35f;
        public float InflowSpawnChance { get; set; } = 0.2f;
        public float InflowPullStrength { get; set; } = 0.12f;
        public float InflowSpread { get; set; } = 25f;
        public Color InflowColor { get; set; } = new Color(100, 30, 160, 200);

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

                MaxShards = MaxShards,
                ShardLife = ShardLife,
                ShardSize = ShardSize,
                ShardSpawnChance = ShardSpawnChance,
                ShardSpinSpeed = ShardSpinSpeed,
                ShardDriftSpeed = ShardDriftSpeed,
                ShardColor = ShardColor,

                MaxRifts = MaxRifts,
                RiftLife = RiftLife,
                RiftStartSize = RiftStartSize,
                RiftEndSize = RiftEndSize,
                RiftSpawnChance = RiftSpawnChance,
                RiftOpenSpeed = RiftOpenSpeed,
                RiftDriftSpeed = RiftDriftSpeed,
                RiftAspectRatio = RiftAspectRatio,
                RiftEdgeColor = RiftEdgeColor,
                RiftCoreColor = RiftCoreColor,

                MaxInflows = MaxInflows,
                InflowLife = InflowLife,
                InflowSize = InflowSize,
                InflowSpawnChance = InflowSpawnChance,
                InflowPullStrength = InflowPullStrength,
                InflowSpread = InflowSpread,
                InflowColor = InflowColor,

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
