using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class BaiHeBuff : ModBuff
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
            player.GetModPlayer<BaiHePlayer>().HasBaiHe = true;
            player.lifeRegen += 5;

            if (Main.rand.NextBool(5))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(20f, 50f);
                var pos = player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.WhiteTorch);
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.velocity.Y -= 0.5f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class BaiHePlayer : ModPlayer
    {
        public bool HasBaiHe { get; set; }
        private int _purifyTimer;

        public override void ResetEffects()
        {
            HasBaiHe = false;
        }

        public override void PostUpdate()
        {
            if (!HasBaiHe)
            {
                _purifyTimer = 0;
                return;
            }

            _purifyTimer++;
            if (_purifyTimer >= 120)
            {
                _purifyTimer = 0;
                PurifyDebuffs();
            }
        }

        private void PurifyDebuffs()
        {
            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                int buffType = Player.buffType[i];
                if (buffType <= 0)
                    continue;

                if (Main.debuff[buffType] && !BuffID.Sets.NurseCannotRemoveDebuff[buffType])
                {
                    Player.DelBuff(i);
                    i--;
                }
            }
        }
    }
}
