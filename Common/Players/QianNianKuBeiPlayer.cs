using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameContent;
using VerminLordMod.Content;

namespace VerminLordMod.Common.Players
{
    public class QianNianKuBeiPlayer : ModPlayer
    {
        public bool QianNianKuBeiActive { get; set; }

        public bool PearlShieldActive { get; set; }

        public int PearlShieldCooldown { get; set; }

        private const int CooldownFrames = 900;

        public override void ResetEffects()
        {
            QianNianKuBeiActive = false;

            if (PearlShieldCooldown > 0)
                PearlShieldCooldown--;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (PearlShieldActive)
            {
                modifiers.FinalDamage *= 0f;
                Player.immuneTime = (int)(60);
                PearlShieldActive = false;
                PearlShieldCooldown = CooldownFrames;

                for (int i = 0; i < 15; i++)
                {
                    var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Sand, 0f, 0f, 0, default, 1.2f);
                    d.noGravity = true;
                    d.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                }

                if (Main.netMode != NetmodeID.Server)
                {
                    Text.ShowTextGreen(Player, "千年苦贝珍珠护盾！完全抵挡了一次攻击！");
                }
            }
        }

        public override void PostHurt(Player.HurtInfo info)
        {
            if (QianNianKuBeiActive && !PearlShieldActive && PearlShieldCooldown <= 0 && info.Damage > 30)
            {
                PearlShieldActive = true;

                if (Main.netMode != NetmodeID.Server)
                {
                    Text.ShowTextGreen(Player, "千年苦贝感知重击，珍珠护盾已生成！");
                }
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["PearlShieldCooldown"] = PearlShieldCooldown;
            tag["PearlShieldActive"] = PearlShieldActive;
        }

        public override void LoadData(TagCompound tag)
        {
            PearlShieldCooldown = tag.GetInt("PearlShieldCooldown");
            PearlShieldActive = tag.GetBool("PearlShieldActive");
        }
    }
}
