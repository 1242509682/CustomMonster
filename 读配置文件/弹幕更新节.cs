namespace TestPlugin;

public class 弹幕更新节
{
    public int 弹幕ID = 0;

    public string 标志 = "";

    public Dictionary<string, float> AI条件 = new Dictionary<string, float>();

    public float X轴速度 = 0f;

    public float Y轴速度 = 0f;

    public float 角度偏移 = 0f;

    public bool 速度注入AI0 = false;

    public float 速度注入AI0后X轴速度 = 0f;

    public float 速度注入AI0后Y轴速度 = 0f;

    public int 弹幕伤害 = 0;

    public int 弹幕击退 = 0;

    public string 指示物数量注入X轴速度名 = "";

    public float 指示物数量注入X轴速度系数 = 1f;

    public string 指示物数量注入Y轴速度名 = "";

    public float 指示物数量注入Y轴速度系数 = 1f;

    public string 指示物数量注入角度名 = "";

    public float 指示物数量注入角度系数 = 1f;

    public int 锁定范围 = 0;

    public float 锁定速度 = 0f;

    public string 指示物数量注入锁定速度名 = "";

    public float 指示物数量注入锁定速度系数 = 1f;

    public bool 计入仇恨 = false;

    public bool 锁定血少 = false;

    public bool 锁定低防 = false;

    public bool 仅攻击对象 = false;

    public bool 逆仇恨锁定 = false;

    public bool 逆血量锁定 = false;

    public bool 逆防御锁定 = false;

    public int 弹点召唤怪物 = 0;

    public Dictionary<int, float> AI赋值 = new Dictionary<int, float>();

    public Dictionary<int, string> 指示物注入AI赋值 = new Dictionary<int, string>();

    public string 弹幕X轴注入指示物名 = "";

    public string 弹幕Y轴注入指示物名 = "";

    public bool 销毁弹幕 = false;

    public 弹幕更新节(int id, string note)
    {
        弹幕ID = id;
        标志 = note;
    }
}
