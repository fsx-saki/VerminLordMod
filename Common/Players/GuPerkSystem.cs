using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Configs;

namespace VerminLordMod.Common.Players
{
    /// <summary>
    /// 永久增益管理系统
    /// 管理所有消耗类蛊虫的永久性增益，提供上限控制和安全的添加方法。
    ///
    /// 原文设定参考：
    /// - 白豕蛊：一转珍稀豕蛊，永久加力，上限一猪之力
    /// - 黑豕蛊：一转珍稀豕蛊，永久加力，可叠加白豕蛊
    /// - 斤力蛊/钧力蛊：增长蛊师力气，无明确上限，但受修为限制
    /// - 十命蛊/百命蛊/千命蛊：增加寿命
    /// - 酒虫系列：精炼真元，提升真元品质
    ///
    /// 从 QiPlayer 迁移的方法：
    /// - ModifyWeaponDamage / ModifyWeaponKnockback
    /// - ModifyMaxStats / PostUpdateRunSpeeds / PostUpdateEquips
    /// </summary>
    public class GuPerkSystem : ModPlayer
    {
        // ===== 力量类 =====

        /// <summary>白豕蛊之力（原文上限：一猪之力）</summary>
        public int whitePigPower { get; private set; }
        /// <summary>黑豕蛊之力（原文上限：一猪之力）</summary>
        public int blackPigPower { get; private set; }
        /// <summary>斤力蛊系列叠加值</summary>
        public int jinLiPower { get; private set; }
        /// <summary>钧力蛊系列叠加值</summary>
        public int junLiPower { get; private set; }

        /// <summary>白豕蛊上限：一猪之力</summary>
        public const int MAX_WHITE_PIG_POWER = 1;
        /// <summary>黑豕蛊上限：一猪之力</summary>
        public const int MAX_BLACK_PIG_POWER = 1;

        // ===== 寿命类 =====
        /// <summary>额外寿命（十命蛊/百命蛊/千命蛊）</summary>
        public int extraAges { get; private set; }

        // ===== 速度类 =====
        /// <summary>额外移动速度（龙珠蟋蟀）</summary>
        public float extraSpeed { get; private set; }
        /// <summary>额外加速度（龙珠蟋蟀）</summary>
        public float extraAccel { get; private set; }

        // ===== 召唤类 =====
        /// <summary>一胎蛊：增加一个召唤栏</summary>
        public bool hasOneMinion { get; private set; }

        // ===== 酒虫系列 =====
        /// <summary>酒虫等级</summary>
        public WineBugLevel wineBugLevel { get; private set; }

        /// <summary>酒虫等级枚举</summary>
        public enum WineBugLevel
        {
            None = 0,       // 无酒虫
            Basic = 1,      // 一转酒虫
            FourFlavor = 2, // 二转四味酒虫
            SevenScent = 3, // 三转七香酒虫
            NineEye = 4     // 四转九眼酒虫
        }

        // ===== 安全添加方法 =====

        /// <summary>
        /// 尝试添加白豕蛊之力
        /// </summary>
        /// <returns>true=成功添加, false=已达上限</returns>
        public bool TryAddWhitePigPower(int amount)
        {
            if (whitePigPower >= MAX_WHITE_PIG_POWER)
                return false;
            whitePigPower = Utils.Clamp(whitePigPower + amount, 0, MAX_WHITE_PIG_POWER);
            return true;
        }

        /// <summary>
        /// 尝试添加黑豕蛊之力
        /// </summary>
        public bool TryAddBlackPigPower(int amount)
        {
            if (blackPigPower >= MAX_BLACK_PIG_POWER)
                return false;
            blackPigPower = Utils.Clamp(blackPigPower + amount, 0, MAX_BLACK_PIG_POWER);
            return true;
        }

        /// <summary>
        /// 添加斤力蛊之力（受修为限制）
        /// </summary>
        public void AddJinLiPower(int amount)
        {
            jinLiPower += amount;
        }

        /// <summary>
        /// 添加钧力蛊之力（受修为限制）
        /// </summary>
        public void AddJunLiPower(int amount)
        {
            junLiPower += amount;
        }

        /// <summary>
        /// 添加额外寿命
        /// </summary>
        public void AddExtraAges(int amount)
        {
            extraAges += amount;
        }

        /// <summary>
        /// 添加额外速度
        /// </summary>
        public void AddExtraSpeed(float speedAmount, float accelAmount)
        {
            extraSpeed += speedAmount;
            extraAccel += accelAmount;
        }

        /// <summary>
        /// 设置一胎蛊
        /// </summary>
        public void SetOneMinion()
        {
            hasOneMinion = true;
        }

        /// <summary>
        /// 升级酒虫
        /// </summary>
        public bool UpgradeWineBug(WineBugLevel targetLevel)
        {
            if ((int)targetLevel <= (int)wineBugLevel)
                return false; // 不能降级
            wineBugLevel = targetLevel;
            return true;
        }

        // ===== 效果应用 =====

        /// <summary>
        /// 获取力量类增益对近战伤害的加成（百分比）
        /// </summary>
        public float GetPowerDamageBonus(int qiLevel, bool limited)
        {
            float bonus = 0f;

            if (limited)
            {
                bonus += Utils.Clamp(whitePigPower, 0, qiLevel * 10) * 0.01f;
                bonus += Utils.Clamp(blackPigPower, 0, qiLevel * 10) * 0.01f;
                bonus += Utils.Clamp(jinLiPower, 0, qiLevel * 10) * 0.01f;
                bonus += Utils.Clamp(junLiPower, 0, qiLevel * 10) * 0.01f;
            }
            else
            {
                bonus += whitePigPower * 0.01f;
                bonus += blackPigPower * 0.01f;
                bonus += jinLiPower * 0.01f;
                bonus += junLiPower * 0.01f;
            }

            return bonus;
        }

        /// <summary>
        /// 获取寿命类增益对生命上限的加成
        /// </summary>
        public int GetAgeHealthBonus(int qiLevel, bool limited)
        {
            if (limited)
                return Utils.Clamp(extraAges, 0, qiLevel * 100);
            return extraAges;
        }

        /// <summary>
        /// 获取酒虫提供的真元恢复加成
        /// </summary>
        public int GetWineBugRegenBonus()
        {
            return wineBugLevel switch
            {
                WineBugLevel.Basic => 1,        // 酒虫：+1 真元恢复
                WineBugLevel.FourFlavor => 2,    // 四味酒虫：+2
                WineBugLevel.SevenScent => 4,    // 七香酒虫：+4
                WineBugLevel.NineEye => 8,       // 九眼酒虫：+8
                _ => 0
            };
        }

        // ===== 从 QiPlayer 迁移的战斗数值修改 =====

        /// <summary>
        /// 近战伤害加成（来自力量系蛊虫：白豕/黑豕/斤力/钧力）。
        /// 从 QiPlayer.ModifyWeaponDamage 迁移。
        /// </summary>
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            if (item.damage > 0 && (item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.MeleeNoSpeed))
            {
                bool limited = ModContent.GetInstance<VerminLordModConfig>().LimitSth;
                damage += GetPowerDamageBonus(Player.GetModPlayer<QiRealmPlayer>().GuLevel, limited);
            }
        }

        /// <summary>
        /// 近战击退加成。
        /// 从 QiPlayer.ModifyWeaponKnockback 迁移。
        /// </summary>
        public override void ModifyWeaponKnockback(Item item, ref StatModifier knockback)
        {
            if (item.damage > 0 && item.DamageType == DamageClass.Melee)
            {
                bool limited = ModContent.GetInstance<VerminLordModConfig>().LimitSth;
                knockback += GetPowerDamageBonus(Player.GetModPlayer<QiRealmPlayer>().GuLevel, limited);
            }
        }

        /// <summary>
        /// 生命上限加成（来自寿蛊）。
        /// 从 QiPlayer.ModifyMaxStats 迁移。
        /// </summary>
        public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
        {
            base.ModifyMaxStats(out health, out mana);
            bool limited = ModContent.GetInstance<VerminLordModConfig>().LimitSth;
            health.Flat += GetAgeHealthBonus(Player.GetModPlayer<QiRealmPlayer>().GuLevel, limited);
        }

        /// <summary>
        /// 移速/加速度加成（来自龙珠蟋蟀）。
        /// 从 QiPlayer.PostUpdateRunSpeeds 迁移。
        /// </summary>
        public override void PostUpdateRunSpeeds()
        {
            Player.maxRunSpeed += extraSpeed;
            Player.runAcceleration += extraAccel;
        }

        /// <summary>
        /// 召唤栏加成 + 酒虫精炼真元加成。
        /// 从 QiPlayer.PostUpdateEquips 迁移。
        /// </summary>
        public override void PostUpdateEquips()
        {
            // 一胎蛊：增加一个召唤栏
            if (hasOneMinion)
                Player.maxMinions += 1;

            // 小灵魂蛊 Buff 加成
            if (Player.HasBuff(ModContent.BuffType<Content.Buffs.AddToSelf.Pobuff.LittleSoulbuff>()))
                Player.maxMinions += 1;

            // 巨灵心蛊 Buff 加成
            if (Player.HasBuff(ModContent.BuffType<Content.Buffs.AddToSelf.Pobuff.GiantSpiritHeartbuff>()))
                Player.maxMinions *= 3;

            // 酒虫精炼真元加成：通过 ExtraQiRegen 写入 QiResourcePlayer
            float wineBonus = GetWineBugRegenBonus();
            var qiResource = Player.GetModPlayer<QiResourcePlayer>();
            qiResource.ExtraQiRegen += wineBonus;
        }

        // ===== ModPlayer 生命周期 =====

        public override void Initialize()
        {
            ResetAll();
        }

        public override void SaveData(TagCompound tag)
        {
            tag["whitePigPower"] = whitePigPower;
            tag["blackPigPower"] = blackPigPower;
            tag["jinLiPower"] = jinLiPower;
            tag["junLiPower"] = junLiPower;
            tag["extraAges"] = extraAges;
            tag["extraSpeed"] = extraSpeed;
            tag["extraAccel"] = extraAccel;
            tag["hasOneMinion"] = hasOneMinion;
            tag["wineBugLevel"] = (int)wineBugLevel;
        }

        public override void LoadData(TagCompound tag)
        {
            whitePigPower = tag.GetInt("whitePigPower");
            blackPigPower = tag.GetInt("blackPigPower");
            jinLiPower = tag.GetInt("jinLiPower");
            junLiPower = tag.GetInt("junLiPower");
            extraAges = tag.GetInt("extraAges");
            extraSpeed = tag.GetFloat("extraSpeed");
            extraAccel = tag.GetFloat("extraAccel");
            hasOneMinion = tag.GetBool("hasOneMinion");
            wineBugLevel = (WineBugLevel)tag.GetInt("wineBugLevel");
        }

        public void ResetAll()
        {
            whitePigPower = 0;
            blackPigPower = 0;
            jinLiPower = 0;
            junLiPower = 0;
            extraAges = 0;
            extraSpeed = 0f;
            extraAccel = 0f;
            hasOneMinion = false;
            wineBugLevel = WineBugLevel.None;
        }
    }
}
