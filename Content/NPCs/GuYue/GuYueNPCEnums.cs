using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuYue
{
    /// <summary>
    /// 古月家族NPC角色类型层级
    /// 用于区分不同身份、不同功能的NPC
    /// 后续扩展只需在此枚举添加新类型，并在对应工厂/配置中注册
    /// </summary>
    public enum GuYueNPCType
    {
        /// <summary> 族长 - 古月博，四转蛊师，家族最高领袖 </summary>
        Chief,

        /// <summary> 学堂家老 - 负责教学，三转蛊师 </summary>
        SchoolElder,

        /// <summary> 药堂家老 - 负责治疗和丹药，三转蛊师 </summary>
        MedicineElder,

        /// <summary> 御堂家老 - 负责防御和装备，三转蛊师 </summary>
        DefenseElder,

        /// <summary> 赤脉家老 - 当权家老，赤脉首领 </summary>
        ChiElder,

        /// <summary> 漠脉家老 - 当权家老，漠脉首领 </summary>
        MoElder,

        /// <summary> 药脉家老 - 药堂负责人 </summary>
        MedicinePulseElder,

        /// <summary> 一转蛊师 - 普通家族蛊师学员/成员 </summary>
        FirstTurnGuMaster,

        /// <summary> 二转蛊师 - 资深家族蛊师 </summary>
        SecondTurnGuMaster,

        /// <summary> 巡逻蛊师 - 野外巡逻（已存在，但纳入体系） </summary>
        PatrolGuMaster,

        /// <summary> 拳脚教头 - 负责教导拳脚功夫 </summary>
        FistInstructor,

        /// <summary> 杂役 - 负责杂务的凡人 </summary>
        Servant,

        /// <summary> 凡人 - 普通家族凡人成员 </summary>
        Commoner,
    }

    /// <summary>
    /// 古月NPC角色配置数据
    /// 每个NPC类型对应的默认属性配置
    /// 后续可从JSON/数据库加载，实现数据驱动
    /// </summary>
    public class GuYueNPCConfig
    {
        public GuYueNPCType NPCType { get; set; }
        public string DisplayName { get; set; }
        public GuRank Rank { get; set; }
        public GuPersonality Personality { get; set; }
        public int BaseDamage { get; set; }
        public int BaseLife { get; set; }
        public int BaseDefense { get; set; }
        public bool IsTownNPC { get; set; }
        public bool IsFriendly { get; set; }
        public string Description { get; set; }

        public static GuYueNPCConfig GetDefaultConfig(GuYueNPCType type) => type switch
        {
            GuYueNPCType.Chief => new GuYueNPCConfig
            {
                NPCType = type,
                DisplayName = "古月族长",
                Rank = GuRank.Zhuan4_Chu,
                Personality = GuPersonality.Benevolent,
                BaseDamage = 120,
                BaseLife = 500,
                BaseDefense = 25,
                IsTownNPC = true,
                IsFriendly = true,
                Description = "古月一族当代族长，四转蛊师，统领整个古月山寨。"
            },
            GuYueNPCType.SchoolElder => new GuYueNPCConfig
            {
                NPCType = type,
                DisplayName = "学堂家老",
                Rank = GuRank.Zhuan3_Chu,
                Personality = GuPersonality.Benevolent,
                BaseDamage = 100,
                BaseLife = 250,
                BaseDefense = 15,
                IsTownNPC = true,
                IsFriendly = true,
                Description = "古月家族学堂家老，负责教导家族后辈蛊师知识。"
            },
            GuYueNPCType.MedicineElder => new GuYueNPCConfig
            {
                NPCType = type,
                DisplayName = "药堂家老",
                Rank = GuRank.Zhuan3_Chu,
                Personality = GuPersonality.Benevolent,
                BaseDamage = 80,
                BaseLife = 220,
                BaseDefense = 12,
                IsTownNPC = true,
                IsFriendly = true,
                Description = "古月家族药堂家老，精通治疗和炼丹。"
            },
            GuYueNPCType.DefenseElder => new GuYueNPCConfig
            {
                NPCType = type,
                DisplayName = "御堂家老",
                Rank = GuRank.Zhuan3_Chu,
                Personality = GuPersonality.Cautious,
                BaseDamage = 90,
                BaseLife = 300,
                BaseDefense = 20,
                IsTownNPC = true,
                IsFriendly = true,
                Description = "古月家族御堂家老，负责家族防御和装备。"
            },
            GuYueNPCType.ChiElder => new GuYueNPCConfig
            {
                NPCType = type,
                DisplayName = "赤脉家老",
                Rank = GuRank.Zhuan3_Zhong,
                Personality = GuPersonality.Proud,
                BaseDamage = 110,
                BaseLife = 280,
                BaseDefense = 18,
                IsTownNPC = true,
                IsFriendly = true,
                Description = "古月家族赤脉当权家老，性格高傲。"
            },
            GuYueNPCType.MoElder => new GuYueNPCConfig
            {
                NPCType = type,
                DisplayName = "漠脉家老",
                Rank = GuRank.Zhuan3_Zhong,
                Personality = GuPersonality.Cautious,
                BaseDamage = 105,
                BaseLife = 270,
                BaseDefense = 17,
                IsTownNPC = true,
                IsFriendly = true,
                Description = "古月家族漠脉当权家老，老成持重。"
            },
            GuYueNPCType.MedicinePulseElder => new GuYueNPCConfig
            {
                NPCType = type,
                DisplayName = "药脉家老",
                Rank = GuRank.Zhuan3_Chu,
                Personality = GuPersonality.Benevolent,
                BaseDamage = 75,
                BaseLife = 200,
                BaseDefense = 10,
                IsTownNPC = true,
                IsFriendly = true,
                Description = "古月家族药脉家主，精通治疗之术。"
            },
            GuYueNPCType.FirstTurnGuMaster => new GuYueNPCConfig
            {
                NPCType = type,
                DisplayName = "古月蛊师",
                Rank = GuRank.Zhuan1_Zhong,
                Personality = GuPersonality.Neutral,
                BaseDamage = 25,
                BaseLife = 120,
                BaseDefense = 8,
                IsTownNPC = true,
                IsFriendly = true,
                Description = "古月家族的一转蛊师，正在努力修行。"
            },
            GuYueNPCType.SecondTurnGuMaster => new GuYueNPCConfig
            {
                NPCType = type,
                DisplayName = "古月资深蛊师",
                Rank = GuRank.Zhuan2_Chu,
                Personality = GuPersonality.Neutral,
                BaseDamage = 50,
                BaseLife = 180,
                BaseDefense = 12,
                IsTownNPC = true,
                IsFriendly = true,
                Description = "古月家族的二转蛊师，已具备一定实力。"
            },
            GuYueNPCType.PatrolGuMaster => new GuYueNPCConfig
            {
                NPCType = type,
                DisplayName = "古月巡逻蛊师",
                Rank = GuRank.Zhuan1_Gao,
                Personality = GuPersonality.Cautious,
                BaseDamage = 18,
                BaseLife = 180,
                BaseDefense = 12,
                IsTownNPC = false,
                IsFriendly = false,
                Description = "古月家族的巡逻蛊师，负责山寨周边的安全。"
            },
            GuYueNPCType.FistInstructor => new GuYueNPCConfig
            {
                NPCType = type,
                DisplayName = "拳脚教头",
                Rank = GuRank.Zhuan1_Gao,
                Personality = GuPersonality.Aggressive,
                BaseDamage = 35,
                BaseLife = 160,
                BaseDefense = 10,
                IsTownNPC = true,
                IsFriendly = true,
                Description = "古月家族的拳脚教头，负责教导族人拳脚功夫。"
            },
            GuYueNPCType.Servant => new GuYueNPCConfig
            {
                NPCType = type,
                DisplayName = "杂役",
                Rank = GuRank.Zhuan1_Chu,
                Personality = GuPersonality.Benevolent,
                BaseDamage = 5,
                BaseLife = 60,
                BaseDefense = 3,
                IsTownNPC = true,
                IsFriendly = true,
                Description = "古月家族的杂役，负责日常杂务。"
            },
            GuYueNPCType.Commoner => new GuYueNPCConfig
            {
                NPCType = type,
                DisplayName = "古月凡人",
                Rank = GuRank.Zhuan1_Chu,
                Personality = GuPersonality.Neutral,
                BaseDamage = 1,
                BaseLife = 40,
                BaseDefense = 1,
                IsTownNPC = true,
                IsFriendly = true,
                Description = "古月家族的凡人成员，尚未开辟空窍。"
            },
            _ => new GuYueNPCConfig
            {
                NPCType = type,
                DisplayName = "古月族人",
                Rank = GuRank.Zhuan1_Chu,
                Personality = GuPersonality.Neutral,
                BaseDamage = 10,
                BaseLife = 100,
                BaseDefense = 5,
                IsTownNPC = true,
                IsFriendly = true,
                Description = "古月家族成员。"
            }
        };
    }
}
