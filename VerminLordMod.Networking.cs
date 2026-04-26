using VerminLordMod.Common.Players;
using VerminLordMod.Content.NPCs;
using System.IO;
using Terraria;
using Terraria.ID;

namespace VerminLordMod
{
	// This is a partial class, meaning some of its parts were split into other files. See VerminLordMod.*.cs for other portions.
	partial class VerminLordMod
	{
		internal enum MessageType : byte
		{
			ExampleStatIncreasePlayerSync,
			ExampleTeleportToStatue,
			ExampleDodge
		}

		// Override this method to handle network packets sent for this mod.
		//TODO: Introduce OOP packets into tML, to avoid this god-class level hardcode.
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			MessageType msgType = (MessageType)reader.ReadByte();

			switch (msgType) {
				// This message syncs ExampleStatIncreasePlayer.exampleLifeFruits and ExampleStatIncreasePlayer.exampleManaCrystals
				case MessageType.ExampleStatIncreasePlayerSync:
					byte playerNumber = reader.ReadByte();
					//ExampleStatIncreasePlayer examplePlayer = Main.player[playerNumber].GetModPlayer<ExampleStatIncreasePlayer>();
					//examplePlayer.ReceivePlayerSync(reader);

					if (Main.netMode == NetmodeID.Server) {
						// Forward the changes to the other clients
						//examplePlayer.SyncPlayer(-1, whoAmI, false);
					}
					break;
				case MessageType.ExampleTeleportToStatue:
					//if (Main.npc[reader.ReadByte()].ModNPC is ExamplePerson person && person.NPC.active) {
						//person.StatueTeleport();
					//}

					break;
				case MessageType.ExampleDodge:
					//ExampleDamageModificationPlayer.HandleExampleDodgeMessage(reader, whoAmI);
					break;
				default:
					Logger.WarnFormat("VerminLordMod: Unknown Message type: {0}", msgType);
					break;
			}
		}
	}
}