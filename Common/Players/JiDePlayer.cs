using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Content.Items.Special;
using VerminLordMod.Common.Players;
using Terraria.ID;

namespace VerminLordMod.Common.Players
{
    public class JiDePlayer : ModPlayer
    {
        public int Virtue { get; set; }

        public int QiMaxBonus { get; set; }

        public float QiRegenBonus { get; set; }

        public bool HasJiDeGuInHand { get; set; }

        public override void Initialize()
        {
            Virtue = 0;
            QiMaxBonus = 0;
            QiRegenBonus = 0;
            HasJiDeGuInHand = false;
        }

        public override void ResetEffects()
        {
            HasJiDeGuInHand = false;
        }

        public override void PostUpdate()
        {
            var qiResource = Player.GetModPlayer<QiResourcePlayer>();
            qiResource.QiMaxBase += QiMaxBonus;
            qiResource.BaseQiRegenRate += QiRegenBonus;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HasJiDeGuInHand && target.life <= 0 && target.type != NPCID.TargetDummy)
            {
                Virtue++;
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["Virtue"] = Virtue;
            tag["QiMaxBonus"] = QiMaxBonus;
            tag["QiRegenBonus"] = QiRegenBonus;
        }

        public override void LoadData(TagCompound tag)
        {
            Virtue = tag.GetInt("Virtue");
            QiMaxBonus = tag.GetInt("QiMaxBonus");
            QiRegenBonus = tag.GetFloat("QiRegenBonus");
        }
    }
}
