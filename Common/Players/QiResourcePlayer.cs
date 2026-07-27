using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Content.Items.QuestItems;

namespace VerminLordMod.Common.Players
{
    public class QiResourcePlayer : ModPlayer
    {
        public float QiMaxBase = 80;
        public float QiMaxCurrent = 80;
        public float QiCurrent = 80;
        public float QiOccupied;
        public float QiAvailable => QiMaxCurrent - QiOccupied;
        public float BaseQiRegenRate = 1f;
        public float QiCostReduction;

        private int _regenTimer;

        public bool ConsumeQi(float amount)
        {
            float effectiveCost = amount * (1f - MathHelper.Min(QiCostReduction, 0.75f));
            if (QiCurrent < effectiveCost) return false;
            QiCurrent -= effectiveCost;
            return true;
        }

        public void RefundQi(float amount)
        {
            QiCurrent = MathHelper.Min(QiMaxCurrent, QiCurrent + amount);
        }

        public override void ResetEffects()
        {
            QiMaxCurrent = QiMaxBase;
            QiCostReduction = 0;
        }

        public override void PostUpdate()
        {
            var flags = Main.LocalPlayer.GetModPlayer<YuanHaiFlags>();
            if (flags == null || !flags.Activated) return;

            float effectiveRegen = BaseQiRegenRate;
            _regenTimer++;
            int regenInterval = effectiveRegen >= 60f ? 1 : (int)(60f / effectiveRegen);
            if (_regenTimer >= regenInterval)
            {
                _regenTimer = 0;
                if (QiCurrent < QiMaxCurrent)
                    QiCurrent = MathHelper.Min(QiMaxCurrent, QiCurrent + 1);
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["QR_QiCur"] = QiCurrent;
            tag["QR_QiMaxBase"] = QiMaxBase;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.TryGet("QR_QiCur", out float qc)) QiCurrent = qc;
            if (tag.TryGet("QR_QiMaxBase", out float qb)) QiMaxBase = qb;
            QiMaxCurrent = QiMaxBase;
        }
    }
}
