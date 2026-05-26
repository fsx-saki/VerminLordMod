using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class YuQingBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.persistentBuff[Type] = false;
            Main.vanityPet[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<YuQingPlayer>().HasYuQing = true;
            player.moveSpeed += 0.10f;
            player.wingTimeMax = 600;
            if (player.wings <= 0)
                player.wings = 1;

            if (Main.rand.NextBool(4))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(20f, 50f);
                var pos = player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.Cloud);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.velocity.Y -= 0.5f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class YuQingPlayer : ModPlayer
    {
        public bool HasYuQing { get; set; }
        private const float WindRange = 300f;
        private int _windTimer;

        public override void ResetEffects()
        {
            HasYuQing = false;
        }

        public override void PostUpdate()
        {
            if (!HasYuQing)
            {
                _windTimer = 0;
                return;
            }

            if (Player.whoAmI != Main.myPlayer)
                return;

            _windTimer++;
            if (_windTimer >= 10)
            {
                _windTimer = 0;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (!proj.active || proj.friendly || proj.owner == Player.whoAmI)
                        continue;

                    float dist = Vector2.Distance(Player.Center, proj.Center);
                    if (dist <= WindRange)
                    {
                        Vector2 away = (proj.Center - Player.Center).SafeNormalize(Vector2.Zero) * 3f;
                        proj.velocity += away;

                        if (Main.rand.NextBool(3))
                        {
                            var d = Dust.NewDustDirect(proj.position, proj.width, proj.height, DustID.Cloud);
                            d.noGravity = true;
                            d.velocity *= 0.3f;
                            d.scale = Main.rand.NextFloat(0.5f, 0.8f);
                        }
                    }
                }
            }
        }
    }
}
