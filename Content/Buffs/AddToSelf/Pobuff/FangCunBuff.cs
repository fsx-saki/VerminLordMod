using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class FangCunBuff : ModBuff
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
            player.GetModPlayer<FangCunPlayer>().HasFangCun = true;
        }
    }

    public class FangCunPlayer : ModPlayer
    {
        public bool HasFangCun { get; set; }
        private int _healTimer;

        public override void ResetEffects()
        {
            HasFangCun = false;
        }

        public override void PostUpdateEquips()
        {
            if (!HasFangCun)
            {
                _healTimer = 0;
                return;
            }

            Player.invis = true;
            Player.immune = true;
            Player.immuneTime = 2;
            Player.dead = false;
            Player.velocity.X = 0f;
            Player.velocity.Y = 0f;
            Player.gravDir = 1f;
            Player.controlLeft = false;
            Player.controlRight = false;
            Player.controlUp = false;
            Player.controlDown = false;
            Player.controlJump = false;
            Player.jump = 0;
            Player.noKnockback = true;

            _healTimer++;
            if (_healTimer >= 12)
            {
                _healTimer = 0;
                Player.statLife += 1;
                if (Player.statLife > Player.statLifeMax2)
                    Player.statLife = Player.statLifeMax2;
            }

            var qiPlayer = Player.GetModPlayer<QiResourcePlayer>();
            qiPlayer.ExtraQiRegen += qiPlayer.BaseQiRegenRate * 2f;

            if (Main.rand.NextBool(5))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(20f, 50f);
                var pos = Player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.Teleporter);
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.8f, 1.4f);
            }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (HasFangCun)
            {
                modifiers.FinalDamage *= 0f;
            }
        }
    }
}
