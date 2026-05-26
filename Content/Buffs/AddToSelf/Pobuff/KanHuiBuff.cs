using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class KanHuiBuff : ModBuff
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
            player.GetModPlayer<KanHuiPlayer>().HasKanHui = true;

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.SilverFlame);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }
    }

    public class KanHuiPlayer : ModPlayer
    {
        public bool HasKanHui { get; set; }

        public override void ResetEffects()
        {
            HasKanHui = false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (!HasKanHui)
                return;

            modifiers.FinalDamage *= 0.5f;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!HasKanHui)
                return;

            int reflectedDamage = (int)(info.SourceDamage * 0.3f);
            if (reflectedDamage <= 0)
                return;

            if (info.DamageSource.TryGetCausingEntity(out Entity source) && source is NPC npc && npc.active)
            {
                npc.SimpleStrikeNPC(reflectedDamage, 0, knockBack: 0f);
                CombatText.NewText(npc.Hitbox, new Color(200, 200, 255), reflectedDamage, true);
            }

            for (int i = 0; i < 8; i++)
            {
                var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.SilverFlame);
                d.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }

            Player.ClearBuff(ModContent.BuffType<KanHuiBuff>());
        }
    }
}
