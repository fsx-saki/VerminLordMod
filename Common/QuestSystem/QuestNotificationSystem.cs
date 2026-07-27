using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using VerminLordMod.Common.QuestSystem.Core;

namespace VerminLordMod.Common.QuestSystem
{
    public static class QuestNotificationSystem
    {
        private static readonly Queue<QuestNode> _pending = [];
        private static QuestNode _current;
        private static int _timer;

        public static void AddNotification(QuestNode node)
        {
            _pending.Enqueue(node);
        }

        public static void Update()
        {
            if (_current == null && _pending.Count > 0)
            {
                _current = _pending.Dequeue();
                _timer = 0;
                SoundEngine.PlaySound(SoundID.ResearchComplete);
            }

            if (_current != null)
            {
                _timer++;
                if (_timer == 1 && Main.LocalPlayer.active)
                {
                    Main.NewText(
                        $"[任务完成] {_current.DisplayName.Value}",
                        Color.Gold);
                }

                if (_timer > 180)
                {
                    _current = null;
                    _timer = 0;
                }
            }
        }
    }
}
