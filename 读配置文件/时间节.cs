﻿namespace TestPlugin.读配置文件
{
    public class 时间节 : ICloneable
    {
        public float 消耗时间 = 0f;

        public bool 循环执行 = false;

        public float 延迟秒数 = 0f;

        public int 可触发次 = 1;

        public int 触发率子 = 1;

        public int 触发率母 = 1;

        public int 杀数条件 = 0;

        public int 人数条件 = 0;

        public int 昼夜条件 = 0;

        public int 血比条件 = 0;

        public int 杀死条件 = 0;

        public int 怪数条件 = 0;

        public int 耗时条件 = 0;

        public int ID条件 = 0;

        public Dictionary<string, float> AI条件 = new Dictionary<string, float>();

        public int 肉山条件 = 0;

        public int 巨人条件 = 0;

        public int 血月条件 = 0;

        public int 月总条件 = 0;

        public int 开服条件 = 0;

        public int X轴条件 = 0;

        public int Y轴条件 = 0;

        public int 面向条件 = 0;

        public bool 跳出事件 = false;

        public List<怪物条件节> 怪物条件 = new List<怪物条件节>();

        public List<玩家条件节> 玩家条件 = new List<玩家条件节>();

        public List<指示物组节> 指示物条件 = new List<指示物组节>();

        public List<指示物节> 指示物修改 = new List<指示物节>();

        public bool 直接撤退 = false;

        public int 玩家复活时间 = -2;

        public int 阻止传送器 = 0;

        public int 切换智慧 = -1;

        public int 能够穿墙 = 0;

        public int 无视重力 = 0;

        public bool 修改防御 = false;

        public int 怪物防御 = 0;

        public Dictionary<int, float> AI赋值 = new Dictionary<int, float>();

        public int 恢复血量 = 0;

        public int 比例回血 = 0;

        public int 怪物无敌 = 0;

        public int 拉取起始 = 0;

        public int 拉取范围 = 0;

        public int 拉取止点 = 0;

        public float 拉取点X轴偏移 = 0f;

        public float 拉取点Y轴偏移 = 0f;

        public int 击退范围 = 0;

        public int 击退力度 = 0;

        public int 杀伤范围 = 0;

        public int 杀伤伤害 = 0;

        public int 反射范围 = 0;

        public List<弹幕节> 释放弹幕 = new List<弹幕节>();

        public int 状态范围 = 0;

        public List<状态节> 周围状态 = new List<状态节>();

        public List<怪物杀伤节> 杀伤怪物 = new List<怪物杀伤节>();

        public Dictionary<int, int> 召唤怪物 = new Dictionary<int, int>();

        public List<声音节> 播放声音 = new List<声音节>();

        public List<string> 释放命令 = new List<string>();

        public string 喊话 = "";

        public 时间节(int p, Dictionary<int, int> summon, string shout, int heal, int num, List<玩家条件节> playR, bool R, int killp, List<弹幕节> proj)
        {
            消耗时间 = p;
            循环执行 = R;
            召唤怪物 = summon;
            恢复血量 = heal;
            喊话 = shout;
            可触发次 = num;
            玩家条件 = playR;
            杀死条件 = killp;
            释放弹幕 = proj;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
