using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.Entities
{
    /// <summary>
    /// 通用尸体实体（使用 Projectile 而非 NPC，避免被原版 NPC 系统干扰）
    /// 
    /// 职责：
    /// - 任何 NPC/怪物死亡后生成，静态不移动
    /// - 持有额外战利品列表（被存入尸体的物品）
    /// - 玩家死亡后也生成，持有剩余物品
    /// - 5 分钟腐烂倒计时，到期后剩余物品散落为原版掉落物
    /// - 可被玩家深度搜尸（长按交互）
    /// - 可被 NPC 搜尸（标记已搜）
    /// 
    /// MVA 简化：
    /// - 无自定义贴图，使用灰色矩形 + 名称绘制
    /// - 无多人同步（P1 再实现）
    /// - 无小地图标记（P1 再实现）
    /// </summary>
    public class NpcCorpse : ModProjectile
    {
        // ===== 核心字段 =====

        /// <summary> 尸体类型 </summary>
        public CorpseType CorpseType;

        /// <summary> 尸体原主人 Player.whoAmI（玩家尸体时有效） </summary>
        public int OwnerPlayerID;

        /// <summary> 尸体原主人名称（用于显示） </summary>
        public string OwnerName = "";

        /// <summary> 原 NPC 类型（怪物尸体时有效） </summary>
        public int SourceNPCType;

        /// <summary> 原 NPC 名称（怪物尸体时有效） </summary>
        public string SourceNPCName = "";

        /// <summary> 剩余物品（未被搜走的） </summary>
        public List<Item> RemainingItems = new();

        /// <summary> 腐烂计时器（默认 18000 帧 = 5 分钟） </summary>
        public int DecayTimer = 18000;

        /// <summary> 是否被 NPC 搜过 </summary>
        public bool IsLootedByNPC;

        /// <summary> 搜尸的 NPC 类型 </summary>
        public int LootingNPCType;

        /// <summary> 搜尸的 NPC 名称（用于日志） </summary>
        public string LootingNPCName = "";

        /// <summary> 是否正在被玩家深度搜尸 </summary>
        public bool IsBeingDeepLooted;

        /// <summary> 深度搜尸的玩家 ID </summary>
        public int DeepLootingPlayerID = -1;

        /// <summary> 是否为 Boss 尸体（Boss 尸体有特殊绘制） </summary>
        public bool IsBossCorpse;

        // ===== 绘制相关 =====

        private const float CorpseWidth = 32f;
        private const float CorpseHeight = 48f;

        /// <summary>
        /// 使用原版木箭贴图（ID=0）作为占位，因为我们用 PreDraw 自定义绘制。
        /// 避免 tModLoader 要求同名 PNG 资源文件。
        /// </summary>
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = (int)CorpseWidth;
            Projectile.height = (int)CorpseHeight;
            Projectile.aiStyle = -1;        // 自定义 AI
            Projectile.timeLeft = 18000;    // 5 分钟腐烂
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;      // 无限穿透（不消失）
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.netImportant = true; // 网络同步标记

            // 确保尸体不会与玩家碰撞
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            // 静止不动
            Projectile.velocity = Vector2.Zero;

            // 腐烂倒计时由 Projectile.timeLeft 自动处理
            // timeLeft <= 0 时触发 OnKill
        }

        public override void OnKill(int timeLeft)
        {
            // 腐烂完成或尸体被销毁：剩余物品散落为原版掉落物
            foreach (var item in RemainingItems)
            {
                if (item != null && !item.IsAir)
                    Item.NewItem(Projectile.GetSource_Death(), Projectile.Center, item);
            }
            RemainingItems.Clear();
        }

        /// <summary>
        /// 从尸体中移除指定数量的随机物品（用于 NPC 搜尸或玩家深度搜尸）
        /// </summary>
        public List<Item> RemoveRandomItems(int count)
        {
            var looted = new List<Item>();
            for (int i = 0; i < count && RemainingItems.Count > 0; i++)
            {
                int index = Main.rand.Next(RemainingItems.Count);
                var item = RemainingItems[index];
                RemainingItems.RemoveAt(index);
                looted.Add(item);
            }
            return looted;
        }

        /// <summary>
        /// 从尸体中移除指定类型的物品（用于特定搜刮逻辑）
        /// </summary>
        public List<Item> RemoveItemsOfType(int itemType, int maxCount = 1)
        {
            var looted = new List<Item>();
            for (int i = RemainingItems.Count - 1; i >= 0 && looted.Count < maxCount; i--)
            {
                if (RemainingItems[i].type == itemType)
                {
                    looted.Add(RemainingItems[i]);
                    RemainingItems.RemoveAt(i);
                }
            }
            return looted;
        }

        /// <summary>
        /// 检查指定玩家是否靠近尸体（用于交互检测）
        /// </summary>
        public bool IsPlayerNearby(Player player, float range = 60f)
        {
            return Vector2.Distance(player.Center, Projectile.Center) < range;
        }

        /// <summary>
        /// 检查尸体是否还有物品
        /// </summary>
        public bool HasRemainingItems()
        {
            foreach (var item in RemainingItems)
            {
                if (item != null && !item.IsAir)
                    return true;
            }
            return false;
        }

        // ============================================================
        // 绘制
        // ============================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;

            // 绘制尸体轮廓（灰色半透明矩形）
            Rectangle rect = new Rectangle(
                (int)(Projectile.Center.X - CorpseWidth / 2 - Main.screenPosition.X),
                (int)(Projectile.Center.Y - CorpseHeight / 2 - Main.screenPosition.Y),
                (int)CorpseWidth,
                (int)CorpseHeight
            );

            Color corpseColor;
            if (IsBossCorpse)
                corpseColor = new Color(180, 50, 50, 200);   // Boss：红色
            else if (IsLootedByNPC)
                corpseColor = new Color(80, 80, 80, 150);    // 已被搜过：更暗
            else if (CorpseType == CorpseType.Player)
                corpseColor = new Color(120, 120, 180, 180); // 玩家：蓝灰色
            else
                corpseColor = new Color(120, 120, 120, 180); // 怪物：灰色半透明

            // 使用原版白色像素绘制尸体矩形
            spriteBatch.Draw(
                Terraria.GameContent.TextureAssets.MagicPixel.Value,
                rect,
                null,
                corpseColor,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0f
            );

            // 绘制名称
            if (!string.IsNullOrEmpty(OwnerName))
            {
                string displayText = OwnerName + "的尸体";
                if (IsLootedByNPC)
                    displayText += " (已搜刮)";

                Terraria.Utils.DrawBorderString(
                    spriteBatch,
                    displayText,
                    Projectile.Center - Main.screenPosition - new Vector2(0, CorpseHeight / 2 + 16),
                    IsLootedByNPC ? Color.Gray : Color.White,
                    0.8f,
                    0.5f,
                    1f
                );
            }

            return false; // 阻止默认绘制
        }

        /// <summary>
        /// 获取尸体剩余物品的摘要文本（用于 UI 显示）
        /// </summary>
        public string GetRemainingSummary()
        {
            if (!HasRemainingItems())
                return "尸体已被搜刮干净";

            int count = 0;
            foreach (var item in RemainingItems)
            {
                if (item != null && !item.IsAir)
                    count++;
            }
            return $"剩余 {count} 件物品";
        }
    }

    /// <summary>
    /// 尸体类型
    /// </summary>
    public enum CorpseType
    {
        /// <summary> 玩家尸体 </summary>
        Player,
        /// <summary> 怪物/NPC 尸体 </summary>
        Monster,
        /// <summary> Boss 尸体 </summary>
        Boss
    }
}
