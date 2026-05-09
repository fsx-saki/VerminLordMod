using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.Players
{
    /// <summary>
    /// 护盾系统 ModPlayer，用于 DaoEffectSystem 的护盾效果。
    /// 存储护盾值和剩余时间，在 ModifyHurt 中减伤。
    /// </summary>
    public class ShieldPlayer : ModPlayer
    {
        /// <summary>当前护盾值。</summary>
        public int ShieldAmount = 0;

        /// <summary>护盾剩余时间（帧）。</summary>
        public int ShieldTimer = 0;

        public override void ResetEffects()
        {
            if (ShieldTimer > 0)
            {
                ShieldTimer--;
                if (ShieldTimer <= 0)
                {
                    ShieldAmount = 0;
                }
            }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (ShieldAmount > 0 && ShieldTimer > 0)
            {
                // 护盾吸收伤害：按比例减免
                float reduction = System.Math.Min(ShieldAmount, (int)modifiers.FinalDamage.Base) * 0.01f;
                modifiers.FinalDamage *= (1f - reduction);
                ShieldAmount -= (int)(reduction * 100);
                if (ShieldAmount <= 0)
                {
                    ShieldTimer = 0;
                }
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["ShieldAmount"] = ShieldAmount;
            tag["ShieldTimer"] = ShieldTimer;
        }

        public override void LoadData(TagCompound tag)
        {
            ShieldAmount = tag.GetInt("ShieldAmount");
            ShieldTimer = tag.GetInt("ShieldTimer");
        }
    }
}
