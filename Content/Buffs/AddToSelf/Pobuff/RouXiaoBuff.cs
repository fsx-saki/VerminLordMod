using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class RouXiaoBuff : ModBuff
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
            player.GetModPlayer<RouXiaoPlayer>().HasRouXiao = true;

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, 261);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class RouXiaoPlayer : ModPlayer
    {
        public bool HasRouXiao { get; set; }
        private const int DemoralizeRange = 300;

        public override void ResetEffects()
        {
            HasRouXiao = false;
        }

        public override void PostUpdate()
        {
            if (!HasRouXiao)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist <= DemoralizeRange)
                {
                    npc.GetGlobalNPC<RouXiaoNPC>().IsDemoralized = true;
                }
            }
        }

        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (npc.GetGlobalNPC<RouXiaoNPC>().IsDemoralized)
            {
                modifiers.FinalDamage *= 0.95f;
            }
        }
    }

    public class RouXiaoNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool IsDemoralized { get; set; }

        public override void ResetEffects(NPC npc)
        {
            IsDemoralized = false;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (IsDemoralized)
            {
                modifiers.Defense *= 0.9f;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (IsDemoralized)
            {
                modifiers.Defense *= 0.9f;
            }
        }
    }
}
