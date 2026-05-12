using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using VerminLordMod.Content.Projectiles.Zero;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 冰晶放置行为 — 弹幕飞行途中每隔若干帧在当前位置"放置"一枚冰晶贴图。
    /// 冰晶原地不动，短暂停留后崩裂成 IceFragmentProj 冰碎片弹幕飞散。
    ///
    /// 效果："向前凝结，后方碎裂"
    /// - 弹幕向前飞 → 身后留下一串静止冰晶
    /// - 冰晶短暂滞留后碎裂 → 冰碎片弹幕向外爆散 + 受重力下落
    /// </summary>
    public class IceCrystalPlaceBehavior : IBulletBehavior
    {
        public string Name => "IceCrystalPlace";

        private struct IceCrystal
        {
            public Vector2 Position;
            public float Rotation;
            public int Life;
            public int MaxLife;
        }

        private List<IceCrystal> _crystals = new();
        private int _placeCounter = 0;
        private int _owner;

        /// <summary>放置间隔（帧）</summary>
        public int PlaceInterval { get; set; } = 3;

        /// <summary>冰晶存活时间（帧）</summary>
        public int CrystalLife { get; set; } = 20;

        /// <summary>冰晶贴图旋转偏移（弧度），默认 0</summary>
        public float CrystalRotationOffset { get; set; } = 0f;

        /// <summary>碎裂时产生的冰碎片弹幕数量</summary>
        public int ShatterFragmentCount { get; set; } = 3;

        /// <summary>冰碎片飞散速度</summary>
        public float ShatterSpeed { get; set; } = 3f;

        /// <summary>碎裂产生碎片的概率</summary>
        public float FragmentSpawnChance { get; set; } = 0.5f;

        /// <summary>冰晶绘制缩放</summary>
        public float CrystalDrawScale { get; set; } = 0.7f;

        /// <summary>冰晶颜色</summary>
        public Color CrystalColor { get; set; } = new Color(180, 230, 255, 200);

        private Texture2D _crystalTex;

        public IceCrystalPlaceBehavior() { }

        private void EnsureTexture()
        {
            if (_crystalTex != null) return;
            _crystalTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/Zero/IceSnowBaseProj").Value;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            EnsureTexture();
            _owner = projectile.owner;
        }

        public void Update(Projectile projectile)
        {
            EnsureTexture();

            _placeCounter++;
            if (_placeCounter >= PlaceInterval)
            {
                _placeCounter = 0;
                _crystals.Add(new IceCrystal
                {
                    Position = projectile.Center,
                    Rotation = projectile.velocity.ToRotation(),
                    Life = CrystalLife,
                    MaxLife = CrystalLife,
                });
            }

            for (int i = _crystals.Count - 1; i >= 0; i--)
            {
                var c = _crystals[i];
                c.Life--;
                if (c.Life <= 0)
                {
                    ShatterCrystal(c.Position);
                    _crystals.RemoveAt(i);
                }
                else
                {
                    _crystals[i] = c;
                }
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            foreach (var c in _crystals)
            {
                ShatterCrystal(c.Position);
            }
            _crystals.Clear();
            _placeCounter = 0;
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            if (_crystalTex == null || _crystals.Count == 0) return true;

            Vector2 origin = _crystalTex.Size() * 0.5f;

            for (int i = 0; i < _crystals.Count; i++)
            {
                var c = _crystals[i];
                float progress = 1f - (float)c.Life / c.MaxLife;
                float alpha = 1f - progress * progress;
                float scale = CrystalDrawScale * (1f - progress * 0.3f);

                Color drawColor = CrystalColor * alpha;
                Vector2 pos = c.Position - Main.screenPosition;

                float finalRotation = c.Rotation + CrystalRotationOffset;
                spriteBatch.Draw(_crystalTex, pos, null, drawColor, finalRotation,
                    origin, scale, SpriteEffects.None, 0);
            }

            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;

        private void ShatterCrystal(Vector2 position)
        {
            if (Main.rand.NextFloat() >= FragmentSpawnChance)
                return;

            int fragType = ModContent.ProjectileType<IceFragmentProj>();
            IEntitySource source = Main.player[_owner]?.GetSource_FromThis();

            for (int i = 0; i < ShatterFragmentCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(ShatterSpeed, ShatterSpeed);
                vel.Y -= Main.rand.NextFloat(1f, ShatterSpeed * 0.5f);

                Projectile.NewProjectile(
                    source,
                    position,
                    vel,
                    fragType,
                    0,
                    0f,
                    _owner
                );
            }
        }
    }
}