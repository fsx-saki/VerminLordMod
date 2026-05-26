using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class MaiDiBuff : ModBuff
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
            player.GetModPlayer<MaiDiPlayer>().HasMaiDi = true;
        }
    }

    public class MaiDiPlayer : ModPlayer
    {
        public bool HasMaiDi { get; set; }
        private int _healTimer;

        public override void ResetEffects()
        {
            HasMaiDi = false;
        }

        public override void PostUpdate()
        {
            if (!HasMaiDi)
            {
                _healTimer = 0;
                return;
            }

            Player.invis = true;
            Player.immune = true;
            Player.immuneTime = 2;
            Player.stoned = true;

            _healTimer++;
            if (_healTimer >= 60)
            {
                _healTimer = 0;
                Player.Heal(3);
            }

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, ModContent.DustType<MudDust>());
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }
}
