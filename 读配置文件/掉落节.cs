using TestPlugin.读配置文件;

namespace TestPlugin;

public class 掉落节
{
    public int 掉落率子 = 1;

    public int 掉落率母 = 1;

    public string[] 种子条件 = new string[0];

    public int[] 难度条件 = new int[0];

    public int 杀数条件 = 0;

    public int 人数条件 = 0;

    public int 杀死条件 = 0;

    public int 被击条件 = 0;

    public int 昼夜条件 = 0;

    public int 耗时条件 = 0;

    public int 肉山条件 = 0;

    public int 巨人条件 = 0;

    public int 降雨条件 = 0;

    public int 血月条件 = 0;

    public int 日食条件 = 0;

    public int 月总条件 = 0;

    public int 开服条件 = 0;

    public Dictionary<string, float> AI条件 = new Dictionary<string, float>();

    public bool 跳出掉落 = false;

    public List<怪物条件节> 怪物条件 = new List<怪物条件节>();

    public Dictionary<int, long> 杀怪条件 = new Dictionary<int, long>();

    public List<指示物组节> 指示物条件 = new List<指示物组节>();

    public List<物品节> 掉落物品 = new List<物品节>();

    public string 喊话 = "";

    public bool 喊话无头 = false;

    public string 备注 = "";

    public 掉落节(int id, int stack)
    {
        掉落物品.Add(new 物品节(id, stack));
    }
}
