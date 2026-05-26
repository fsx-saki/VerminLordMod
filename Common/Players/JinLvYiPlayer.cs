using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.GameContent;
using VerminLordMod.Content;

namespace VerminLordMod.Common.Players
{
    public class JinLvYiPlayer : ModPlayer
    {
        public bool JinLvYiActive { get; set; }

        public int CooldownTimer { get; set; }

        private const int CooldownFrames = 3600;

        public override void ResetEffects()
        {
            JinLvYiActive = false;

            if (CooldownTimer > 0)
                CooldownTimer--;
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (JinLvYiActive && CooldownTimer <= 0)
            {
                Player.statLife = 1;
                Player.HealEffect(1);
                Player.immuneTime = (int)(60);
                CooldownTimer = CooldownFrames;

                if (Main.netMode != Terraria.ID.NetmodeID.Server)
                {
                    Text.ShowTextGreen(Player, "金缕衣蛊护主！死里逃生！");
                }

                return false;
            }

            return true;
        }

        public override void SaveData(Terraria.ModLoader.IO.TagCompound tag)
        {
            tag["JinLvYiCooldown"] = CooldownTimer;
        }

        public override void LoadData(Terraria.ModLoader.IO.TagCompound tag)
        {
            CooldownTimer = tag.GetInt("JinLvYiCooldown");
        }
    }
}
