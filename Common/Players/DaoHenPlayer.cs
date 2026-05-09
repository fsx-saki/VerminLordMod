using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;

namespace VerminLordMod.Common.Players
{
    public class DaoHenPlayer : ModPlayer
    {
        public static DaoHenPlayer Instance => Main.LocalPlayer.GetModPlayer<DaoHenPlayer>();

        public Dictionary<DaoType, float> DaoHen = new Dictionary<DaoType, float>();

        public override void Initialize()
        {
            foreach (DaoType dao in System.Enum.GetValues(typeof(DaoType)))
                DaoHen[dao] = 0f;
        }

        public float GetMultiplier(DaoType dao)
        {
            if (DaoHen.TryGetValue(dao, out float val))
            {
                if (val >= 500) return 1.5f;
                if (val >= 200) return 1.25f;
                if (val >= 50) return 1.1f;
            }
            return 1f;
        }

        public void AddDaoHen(DaoType dao, float amount)
        {
            if (DaoHen.ContainsKey(dao))
                DaoHen[dao] += amount;
        }
    }
}
