using TestPlugin.读配置文件;

namespace TestPlugin
{
    public class 怪物节
    {
        public string 标志 = "";

        public int 怪物ID = 0;

        public int[] 再匹配 = new int[0];

        public int 初始属性玩家系数 = 0;

        public float 初始属性强化系数 = 0f;

        public float 初始属性对怪物伤害修正 = 1f;

        public int 怪物血量 = 0;

        public int 玩家系数 = 0;

        public int 开服系数 = 0;

        public int 杀数系数 = 0;

        public int 开服时间型 = 0;

        public bool 覆盖原血量 = true;

        public bool 不低于正常 = true;

        public float 强化系数 = 0f;

        public bool 不小于正常 = true;

        public float 玩家强化系数 = 0f;

        public float 开服强化系数 = 0f;

        public float 杀数强化系数 = 0f;

        public bool 覆盖原强化 = true;

        public int 玩家复活时间 = -1;

        public int 阻止传送器 = 0;

        public int 出没秒数 = 0;

        public int 人秒系数 = 0;

        public int 开服系秒 = 0;

        public int 杀数系秒 = 0;

        public int 出没率子 = 1;

        public int 出没率母 = 1;

        public int 杀数条件 = 0;

        public int 数量条件 = 0;

        public int 人数条件 = 0;

        public int 昼夜条件 = 0;

        public int 肉山条件 = 0;

        public int 巨人条件 = 0;

        public int 血月条件 = 0;

        public int 月总条件 = 0;

        public int 开服条件 = 0;

        public List<怪物条件节> 怪物条件 = new List<怪物条件节>();

        public int 智慧机制 = -1;

        public int 免疫熔岩 = 0;

        public int 免疫陷阱 = 0;

        public int 能够穿墙 = 0;

        public int 无视重力 = 0;

        public int 设为老怪 = 0;

        public bool 修改防御 = false;

        public int 怪物防御 = 0;

        public int 怪物无敌 = 0;

        public string 自定缀称 = "";

        public string 出场喊话 = "";

        public string 死亡喊话 = "";

        public bool 不宣读信息 = false;

        public int 状态范围 = 0;

        public List<状态节> 周围状态 = new List<状态节>();

        public int 死状范围 = 0;

        public Dictionary<int, int> 死亡状态 = new Dictionary<int, int>();

        public List<弹幕节> 出场弹幕 = new List<弹幕节>();

        public List<弹幕节> 死亡弹幕 = new List<弹幕节>();

        public List<声音节> 出场放音 = new List<声音节>();

        public List<声音节> 死亡放音 = new List<声音节>();

        public List<怪物杀伤节> 出场伤怪 = new List<怪物杀伤节>();

        public List<怪物杀伤节> 死亡伤怪 = new List<怪物杀伤节>();

        public List<string> 出场命令 = new List<string>();

        public List<string> 死亡命令 = new List<string>();

        public int 血事件限 = 1;

        public List<比例节> 血量事件 = new List<比例节>();

        public int 时事件限 = 3;

        public List<时间节> 时间事件 = new List<时间节>();

        public Dictionary<int, int> 随从怪物 = new Dictionary<int, int>();

        public Dictionary<int, int> 遗言怪物 = new Dictionary<int, int>();

        public int 掉落组限 = 1;

        public List<掉落节> 额外掉落 = new List<掉落节>();

        public string 备注 = "";
    }
}
