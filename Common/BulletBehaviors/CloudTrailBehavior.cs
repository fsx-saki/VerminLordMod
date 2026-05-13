using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class CloudTrailBehavior : IBulletBehavior
    {
        public string Name => "CloudTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public CloudTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 12;
        public int GhostRecordInterval { get; set; } = 3;
        public float GhostWidthScale { get; set; } = 0.3f;
        public float GhostLengthScale { get; set; } = 1.8f;
        public float GhostAlpha { get; set; } = 0.2f;
        public Color GhostColor { get; set; } = new Color(220, 225, 240, 100);

        public int MaxCloudPuffs { get; set; } = 12;
        public int CloudPuffLife { get; set; } = 60;
        public float CloudPuffStartSize { get; set; } = 0.4f;
        public float CloudPuffEndSize { get; set; } = 2.5f;
        public float CloudPuffSpawnChance { get; set; } = 0.06f;
        public float CloudPuffExpandSpeed { get; set; } = 1.5f;
        public float CloudPuffRotSpeed { get; set; } = 0.01f;
        public float CloudPuffDriftSpeed { get; set; } = 0.06f;
        public Color CloudPuffColor { get; set; } = new Color(230, 235, 250, 120);

        public int MaxVaporWisps { get; set; } = 15;
        public int VaporWispLife { get; set; } = 40;
        public float VaporWispSize { get; set; } = 0.3f;
        public float VaporWispLength { get; set; } = 20f;
        public float VaporWispSpawnChance { get; set; } = 0.1f;
        public float VaporWispWaveSpeed { get; set; } = 0.12f;
        public float VaporWispWaveAmplitude { get; set; } = 0.3f;
        public float VaporWispDriftSpeed { get; set; } = 0.08f;
        public Color VaporWispColor { get; set; } = new Color(210, 220, 245, 140);

        public int MaxCondensations { get; set; } = 8;
        public int CondensationLife { get; set; } = 50;
        public float CondensationStartSize { get; set; } = 0.3f;
        public float CondensationEndSize { get; set; } = 1.8f;
        public float CondensationSpawnChance { get; set; } = 0.03f;
        public float CondensationExpandSpeed { get; set; } = 2f;
        public float CondensationDriftSpeed { get; set; } = 0.04f;
        public Color CondensationColor { get; set; } = new Color(200, 210, 240, 100);

        public float InertiaFactor { get; set; } = 0.12f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public CloudTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new CloudTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxCloudPuffs = MaxCloudPuffs,
                CloudPuffLife = CloudPuffLife,
                CloudPuffStartSize = CloudPuffStartSize,
                CloudPuffEndSize = CloudPuffEndSize,
                CloudPuffSpawnChance = CloudPuffSpawnChance,
                CloudPuffExpandSpeed = CloudPuffExpandSpeed,
                CloudPuffRotSpeed = CloudPuffRotSpeed,
                CloudPuffDriftSpeed = CloudPuffDriftSpeed,
                CloudPuffColor = CloudPuffColor,

                MaxVaporWisps = MaxVaporWisps,
                VaporWispLife = VaporWispLife,
                VaporWispSize = VaporWispSize,
                VaporWispLength = VaporWispLength,
                VaporWispSpawnChance = VaporWispSpawnChance,
                VaporWispWaveSpeed = VaporWispWaveSpeed,
                VaporWispWaveAmplitude = VaporWispWaveAmplitude,
                VaporWispDriftSpeed = VaporWispDriftSpeed,
                VaporWispColor = VaporWispColor,

                MaxCondensations = MaxCondensations,
                CondensationLife = CondensationLife,
                CondensationStartSize = CondensationStartSize,
                CondensationEndSize = CondensationEndSize,
                CondensationSpawnChance = CondensationSpawnChance,
                CondensationExpandSpeed = CondensationExpandSpeed,
                CondensationDriftSpeed = CondensationDriftSpeed,
                CondensationColor = CondensationColor,

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
