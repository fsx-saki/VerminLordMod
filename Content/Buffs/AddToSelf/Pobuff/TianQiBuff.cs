using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class TianQiBuff : ModBuff
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
            player.GetModPlayer<TianQiPlayer>().HasTianQiBuff = true;

            int dustType = player.GetModPlayer<TianQiPlayer>().CurrentWeather switch
            {
                TianQiPlayer.WeatherType.Rain => DustID.RainCloud,
                TianQiPlayer.WeatherType.Day => DustID.Sunflower,
                TianQiPlayer.WeatherType.Night => DustID.MagicMirror,
                TianQiPlayer.WeatherType.BloodMoon => DustID.Blood,
                _ => DustID.RainCloud
            };

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, dustType);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class TianQiPlayer : ModPlayer
    {
        public enum WeatherType
        {
            None = 0,
            Rain = 1,
            Day = 2,
            Night = 3,
            BloodMoon = 4
        }

        public bool HasTianQiBuff { get; set; }
        public WeatherType CurrentWeather { get; set; }

        public override void ResetEffects()
        {
            HasTianQiBuff = false;
            if (!HasTianQiBuff)
                CurrentWeather = WeatherType.None;
        }

        public override void PostUpdateEquips()
        {
            if (!HasTianQiBuff)
                return;

            var qiResource = Player.GetModPlayer<QiResourcePlayer>();

            switch (CurrentWeather)
            {
                case WeatherType.Rain:
                    Player.GetDamage(DamageClass.Generic) += 0.15f;
                    qiResource.ExtraQiRegen += 0.10f * qiResource.BaseQiRegenRate;
                    break;
                case WeatherType.Day:
                    Player.GetDamage(DamageClass.Generic) += 0.12f;
                    Player.GetCritChance(DamageClass.Generic) += 8f;
                    break;
                case WeatherType.Night:
                    Player.GetCritChance(DamageClass.Generic) += 10f;
                    break;
                case WeatherType.BloodMoon:
                    Player.GetDamage(DamageClass.Generic) += 0.25f;
                    break;
            }
        }

        public override bool ConsumableDodge(Player.HurtInfo info)
        {
            if (!HasTianQiBuff || CurrentWeather != WeatherType.Night)
                return false;

            if (Main.rand.NextFloat() < 0.15f)
                return true;

            return false;
        }

        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (HasTianQiBuff && CurrentWeather == WeatherType.BloodMoon)
                modifiers.FinalDamage *= 1.10f;
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (HasTianQiBuff && CurrentWeather == WeatherType.BloodMoon)
                modifiers.FinalDamage *= 1.10f;
        }
    }
}
