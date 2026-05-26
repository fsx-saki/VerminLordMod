using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class YiSiYiShengBuff : ModBuff
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
            var modPlayer = player.GetModPlayer<YiSiYiShengPlayer>();

            if (!modPlayer.HasTriggeredDeath)
            {
                modPlayer.HasTriggeredDeath = true;
                modPlayer.DeathTimer = 30;
                player.statLife = 1;
                player.AddBuff(BuffID.Frozen, 30);

                for (int i = 0; i < 20; i++)
                {
                    var d = Dust.NewDustDirect(player.position, player.width, player.height,
                        ModContent.DustType<LifeDeathDust>());
                    d.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-4f, -1f));
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(1.0f, 1.8f);
                }
            }

            if (modPlayer.DeathTimer > 0)
            {
                modPlayer.DeathTimer--;
                player.statLife = 1;
                player.moveSpeed = 0f;
                player.maxRunSpeed = 0f;

                if (Main.rand.NextBool(3))
                {
                    var d = Dust.NewDustDirect(player.position, player.width, player.height,
                        ModContent.DustType<LifeDeathDust>());
                    d.velocity *= 0.3f;
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(0.6f, 1.2f);
                }
            }
            else
            {
                if (!modPlayer.HasRevived)
                {
                    modPlayer.HasRevived = true;
                    int healAmount = player.statLifeMax2 / 2;
                    player.statLife = healAmount;
                    player.HealEffect(healAmount);

                    for (int i = 0; i < 30; i++)
                    {
                        var d = Dust.NewDustDirect(player.position, player.width, player.height,
                            ModContent.DustType<LifeDeathDust>());
                        d.velocity = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-5f, -1f));
                        d.noGravity = true;
                        d.scale = Main.rand.NextFloat(1.2f, 2.0f);
                    }
                }

                player.GetDamage(DamageClass.Generic) += 0.25f;

                if (Main.rand.NextBool(4))
                {
                    var d = Dust.NewDustDirect(player.position, player.width, player.height,
                        ModContent.DustType<LifeDeathDust>());
                    d.velocity = new Vector2(0, -1.5f);
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(0.5f, 0.9f);
                }
            }

            modPlayer.HasYiSiYiSheng = true;
        }
    }

    public class YiSiYiShengPlayer : ModPlayer
    {
        public bool HasYiSiYiSheng { get; set; }
        public bool HasTriggeredDeath { get; set; }
        public bool HasRevived { get; set; }
        public int DeathTimer { get; set; }

        public override void ResetEffects()
        {
            HasYiSiYiSheng = false;
            if (!HasYiSiYiSheng)
            {
                HasTriggeredDeath = false;
                HasRevived = false;
                DeathTimer = 0;
            }
        }
    }
}
