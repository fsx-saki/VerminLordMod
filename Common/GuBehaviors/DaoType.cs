/// <summary>
/// 道途类型枚举 — 蛊师修炼的法则方向。
/// 每种道途对应不同的战斗风格和道痕加成。
/// 道痕积累到阈值后提供对应道途的倍率加成（见 DaoHenPlayer）。
/// </summary>
namespace VerminLordMod.Common.GuBehaviors
{
    public enum DaoType
    {
        Ban,            // 搬道 — 搬运/位移
        Blood,          // 血道 — 吸血/生命操控
        Bone,           // 骨道 — 防御/骨骼强化
        Charm,          // 魅道 — 魅惑/精神控制
        Cloud,          // 云道 — 飞行/云雾
        Dark,           // 暗道 — 潜行/暗影
        Draw,           // 御道 — 御物/远程操控
        Dream,          // 梦道 — 梦境/幻术
        Eating,         // 食道 — 吞噬/消化
        Fire,           // 火道 — 火焰/燃烧
        Flying,         // 飞道 — 飞行/空中作战
        Gold,           // 金道 — 金属/财富
        IceSnow,        // 冰雪道 — 冰冻/减速
        Info,           // 情报道 — 侦查/信息
        Killing,        // 杀道 — 纯粹杀戮
        Knife,          // 刀道 — 刀法/近战
        LifeDeath,      // 生死道 — 生死操控
        Light,          // 光道 — 治愈/光明
        Lightning,      // 雷道 — 雷电/连锁
        Love,           // 情道 — 情感操控
        Luck,           // 运道 — 幸运/概率
        Moon,           // 月道 — 月光/标记
        Mud,            // 泥道 — 泥土/束缚
        Pellet,         // 丹道 — 炼丹/丹药
        Person,         // 人道 — 人际/社交
        Poison,         // 毒道 — 毒素/腐蚀
        Power,          // 力道 — 纯粹力量
        Practise,       // 修道 — 修炼加速
        Qi,             // 气道 — 真元/灵气
        Rule,           // 规则道 — 法则/规则操控
        Shadow,         // 影道 — 影子/分身
        Sky,            // 天道 — 天象/天气
        Slave,          // 奴道 — 奴役/控制
        Soul,           // 魂道 — 灵魂/精神
        Space,          // 空间道 — 空间/传送
        Star,           // 星道 — 星辰/命运
        Stealing,       // 盗道 — 偷窃/隐匿
        SuccessFailure, // 成败道 — 赌博/风险
        Sword,          // 剑道 — 剑法/剑气
        Tactical,       // 战术道 — 策略/指挥
        Time,           // 时间道 — 时间操控
        Unreal,         // 虚道 — 虚幻/假象
        Variation,      // 变道 — 变化/变形
        Voice,          // 音道 — 声音/音波
        Void,           // 虚空道 — 虚空/湮灭
        War,            // 战道 — 战争/统帅
        Water,          // 水道 — 水流/牵引
        Wind,           // 风道 — 风/推力
        Wisdom,         // 智道 — 智慧/洞察
        Wood,           // 木道 — 植物/生长
        YinYang         // 阴阳道 — 平衡/转化
    }
}
