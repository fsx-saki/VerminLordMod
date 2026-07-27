using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Items;
using VerminLordMod.Common.PRTTypes;
using VerminLordMod.Content.DamageClasses;
using InnoVault;
using InnoVault.PRT;
using VerminLordMod.Content.Projectiles.Elements;

namespace VerminLordMod.Content.Items.QuestItems
{
    public class MoonlightGu : GuWeaponItem
    {
        public override int GuLevel => 1;
        public override int QiCost => 5;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 24; Item.height = 24; Item.value = 50000; Item.rare = ItemRarityID.White;
            Item.damage = 20;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 4f; Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<MoonBaseProj>();
            Item.shootSpeed = 7f;
            Item.useTime = 20; Item.useAnimation = 20;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var p = Projectile.NewProjectileDirect(source, position, velocity, type, damage * 2, knockback * 2, player.whoAmI);
            p.scale = 0.8f;
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tools)
        {
            tools.Add(new TooltipLine(Mod, "GuType", "[c/87ceeb:月光蛊 · 一转]"));
            tools.Add(new TooltipLine(Mod, "GuDesc", "[c/ffd700:蕴含月华之力的基础蛊虫]"));
            tools.Add(new TooltipLine(Mod, "GuQiCost", $"[c/aaaaaa:每次攻击消耗 {QiCost} 真元]"));
            tools.Add(new TooltipLine(Mod, "GuEffect", "投掷出月光凝结的月刃攻击敌人"));
            if (!IsRefined)
                tools.Add(new TooltipLine(Mod, "GuRefine", "[c/ff4444:未炼化]"));
        }
    }

    // public class MoonlightProj : ModProjectile
    // {
    //     public override void SetStaticDefaults()
    //     {
    //         ProjectileID.Sets.TrailCacheLength[Type] = 8;
    //         ProjectileID.Sets.TrailingMode[Type] = 2;
    //     }

    //     public override void SetDefaults()
    //     {
    //         Projectile.width = 24; Projectile.height = 24; Projectile.scale = 1.2f;
    //         Projectile.ignoreWater = true; Projectile.tileCollide = true;
    //         Projectile.penetrate = -1; Projectile.timeLeft = 45;
    //         Projectile.alpha = 0; Projectile.friendly = true; Projectile.hostile = false;
    //         Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
    //         Projectile.aiStyle = -1;
    //         Projectile.light = 0.6f;
    //     }

    //     public override void AI()
    //     {
    //         if (Projectile.velocity.Length() < 3f) Projectile.velocity *= 1.01f;
    //         Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

    //         // 飞行时散落星星点点
    //         if (Main.rand != null && Main.rand.NextBool(4))
    //         {
    //             var s = PRTLoader.NewParticle<PRT_MoonSparkle>(
    //                 Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 8f + Main.rand.NextVector2Circular(4f, 4f),
    //                 Projectile.velocity * -0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
    //                 new Color(200, 230, 255), Main.rand.NextFloat(0.2f, 0.35f));
    //             if (s != null) s.Lifetime = 30 + Main.rand.Next(20);
    //         }
    //     }

    //     public override bool PreDraw(ref Color lightColor)
    //     {
    //         var tex = TextureAssets.Projectile[Type].Value;
    //         Vector2 origin = tex.Size() / 2f;

    //         for (int i = 1; i < Projectile.oldPos.Length; i++)
    //         {
    //             if (Projectile.oldPos[i] == Vector2.Zero) continue;
    //             Vector2 dp = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
    //             float fade = 1f - i / (float)Projectile.oldPos.Length;
    //             Color c = new Color(180, 220, 255) * fade * 0.3f;
    //             Main.EntitySpriteDraw(tex, dp, null, c, Projectile.oldRot[i], origin, Projectile.scale * fade * 0.8f, SpriteEffects.None, 0);
    //         }

    //         Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
    //         return false;
    //     }

    //     public override void PostDraw(Color lightColor)
    //     {
    //         var glow = VaultAsset.Light?.Value;
    //         if (glow == null) return;
    //         Vector2 pos = Projectile.Center - Main.screenPosition;

    //         Main.spriteBatch.End();
    //         Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

    //         Color gc = new Color(180, 220, 255, 0) * 0.4f;
    //         Main.spriteBatch.Draw(glow, pos, null, gc, 0f, glow.Size() / 2f, 0.6f, SpriteEffects.None, 0f);
    //         Main.spriteBatch.Draw(glow, pos, null, Color.White * 0.2f, 0f, glow.Size() / 2f, 0.25f, SpriteEffects.None, 0f);

    //         for (int i = 1; i < Projectile.oldPos.Length; i++)
    //         {
    //             if (Projectile.oldPos[i] == Vector2.Zero) continue;
    //             Vector2 gp = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
    //             float fade = 1f - i / (float)Projectile.oldPos.Length;
    //             Main.spriteBatch.Draw(glow, gp, null, gc * fade * 0.5f, 0f, glow.Size() / 2f, 0.3f * fade, SpriteEffects.None, 0f);
    //         }

    //         Main.spriteBatch.End();
    //         Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    //     }

    //     public override Color? GetAlpha(Color lightColor) => Color.White * 0.95f;

    //     public override bool OnTileCollide(Vector2 oldVelocity)
    //     {
    //         Projectile.penetrate--;
    //         if (Projectile.penetrate <= 0) { Burst(); return true; }
    //         if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = -oldVelocity.X * 0.5f;
    //         if (Projectile.velocity.Y != oldVelocity.Y) Projectile.velocity.Y = -oldVelocity.Y * 0.5f;
    //         return false;
    //     }

    //     public override void OnKill(int timeLeft) { Burst(); }

    //     private void Burst()
    //     {
    //         if (Main.netMode == 2) return;
    //         var rand = Main.rand;

    //         // 星星点点 — 极小光点缓慢漂移淡出
    //         for (int i = 0; i < 24; i++)
    //         {
    //             var s = PRTLoader.NewParticle<PRT_MoonSparkle>(
    //                 Projectile.Center + rand.NextVector2Circular(8f, 8f),
    //                 rand.NextVector2Circular(3f, 3f),
    //                 new Color(200, 230, 255), rand.NextFloat(0.3f, 0.6f));
    //             if (s != null) s.Lifetime = 60 + rand.Next(30);
    //         }

    //         // 光痕 — 快速向外射出的短光带
    //         for (int i = 0; i < 10; i++)
    //         {
    //             float angle = rand.NextFloat(MathHelper.TwoPi);
    //             var t = PRTLoader.NewParticle<PRT_MoonTrail>(
    //                 Projectile.Center + angle.ToRotationVector2() * 5f,
    //                 angle.ToRotationVector2() * rand.NextFloat(4f, 8f),
    //                 new Color(180, 220, 255), rand.NextFloat(0.25f, 0.4f));
    //             if (t != null) t.Lifetime = 15 + rand.Next(8);
    //         }

    //         // 中心光爆 — 脉动星芒
    //         for (int i = 0; i < 6; i++)
    //         {
    //             var b = PRTLoader.NewParticle<PRT_MoonBurst>(
    //                 Projectile.Center + rand.NextVector2Circular(3f, 3f),
    //                 rand.NextVector2Circular(2f, 2f),
    //                 new Color(220, 240, 255), rand.NextFloat(0.3f, 0.5f));
    //             if (b != null) b.Lifetime = 25 + rand.Next(10);
    //         }
    //     }
    // }
}
