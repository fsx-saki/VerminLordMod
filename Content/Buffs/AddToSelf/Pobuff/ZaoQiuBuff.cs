using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ZaoQiuBuff : ModBuff
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
            player.GetModPlayer<ZaoQiuPlayer>().HasZaoQiu = true;
        }
    }

    public class ZaoQiuPlayer : ModPlayer
    {
        public bool HasZaoQiu { get; set; }
        private const int PhaseDuration = 200;
        private const int TotalDuration = 600;
        private int _phaseTimer;

        public int CurrentPhase { get; private set; }

        public override void ResetEffects()
        {
            HasZaoQiu = false;
        }

        public override void PostUpdate()
        {
            if (!HasZaoQiu)
            {
                _phaseTimer = 0;
                CurrentPhase = 0;
                return;
            }

            _phaseTimer++;
            if (_phaseTimer >= TotalDuration)
                _phaseTimer = 0;

            CurrentPhase = _phaseTimer / PhaseDuration;

            switch (CurrentPhase)
            {
                case 0:
                    Player.GetDamage(DamageClass.Generic) += 0.10f;
                    Player.GetModPlayer<QiResourcePlayer>().ExtraQiRegen += 0.05f;

                    if (Main.rand.NextBool(8))
                    {
                        var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, ModContent.DustType<TimeDust>());
                        d.noGravity = true;
                        d.velocity *= 0.3f;
                        d.scale = Main.rand.NextFloat(0.6f, 1.0f);
                    }
                    break;

                case 1:
                    Player.GetDamage(DamageClass.Generic) += 0.15f;
                    Player.GetCritChance(DamageClass.Generic) += 10f;

                    if (Main.rand.NextBool(6))
                    {
                        var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.GoldFlame);
                        d.noGravity = true;
                        d.velocity *= 0.3f;
                        d.scale = Main.rand.NextFloat(0.7f, 1.1f);
                    }
                    break;

                case 2:
                    Player.statDefense += 10;
                    Player.lifeRegen += 5;

                    if (Main.rand.NextBool(8))
                    {
                        var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Grass);
                        d.noGravity = true;
                        d.velocity *= 0.3f;
                        d.scale = Main.rand.NextFloat(0.6f, 1.0f);
                    }
                    break;
            }
        }
    }
}
