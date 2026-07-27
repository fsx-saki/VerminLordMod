using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VerminLordMod.Common.QuestSystem.Core
{
    public interface IQuestLogStyle
    {
        void UpdateStyle();
        void DrawBackground(SpriteBatch spriteBatch, QuestLog log, Rectangle panelRect);
        void DrawNode(SpriteBatch spriteBatch, QuestNode node, Vector2 drawPos, float scale, bool isHovered, float alpha);
        void DrawConnection(SpriteBatch spriteBatch, Vector2 start, Vector2 end, bool isUnlocked, float alpha);
        Vector4 GetPadding();
        void DrawQuestDetail(SpriteBatch spriteBatch, QuestNode node, Rectangle panelRect, float alpha);
        Rectangle GetCloseButtonRect(Rectangle panelRect);
        Rectangle GetRewardButtonRect(Rectangle panelRect);
        void DrawProgressBar(SpriteBatch spriteBatch, QuestLog log, Rectangle panelRect);
        Rectangle GetClaimAllButtonRect(Rectangle panelRect);
        Rectangle GetResetViewButtonRect(Rectangle panelRect);
        Rectangle GetStyleSwitchButtonRect(Rectangle panelRect);
    }
}
