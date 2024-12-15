namespace TestPlugin;

public class 弹幕节
{
    public int 弹幕ID = 0;

    public string 标志 = "";

    public float X轴偏移 = 0f;

    public float Y轴偏移 = 0f;

    public float X轴速度 = 0f;

    public float Y轴速度 = 0f;

    public float 角度偏移 = 0f;

    public int 弹幕伤害 = 0;

    public int 弹幕击退 = 0;

    public string 指示物数量注入X轴偏移名 = "";

    public float 指示物数量注入X轴偏移系数 = 1f;

    public string 指示物数量注入Y轴偏移名 = "";

    public float 指示物数量注入Y轴偏移系数 = 1f;

    public string 指示物数量注入X轴速度名 = "";

    public float 指示物数量注入X轴速度系数 = 1f;

    public string 指示物数量注入Y轴速度名 = "";

    public float 指示物数量注入Y轴速度系数 = 1f;

    public string 指示物数量注入角度名 = "";

    public float 指示物数量注入角度系数 = 1f;

    public bool 初始坐标为零 = false;

    public float 弹幕Ai0 = 0f;

    public bool 速度注入AI0 = false;

    public float 速度注入AI0后X轴速度 = 0f;

    public float 速度注入AI0后Y轴速度 = 0f;

    public float 弹幕Ai1 = 0f;

    public float 弹幕Ai2 = 0f;

    public string 指示物数量注入Ai0名 = "";

    public float 指示物数量注入Ai0系数 = 1f;

    public string 指示物数量注入Ai1名 = "";

    public float 指示物数量注入Ai1系数 = 1f;

    public string 指示物数量注入Ai2名 = "";

    public float 指示物数量注入Ai2系数 = 1f;

    public float 怪面向X偏移修正 = 0f;

    public float 怪面向Y偏移修正 = 0f;

    public float 怪面向X速度修正 = 0f;

    public float 怪面向Y速度修正 = 0f;

    public bool 始弹点怪物传送 = false;

    public int 始弹点怪物传送类型 = 0;

    public int 始弹点怪物传送信息 = 0;

    public bool 不射原始 = false;

    public string 射出始弹X轴注入指示物名 = "";

    public string 射出始弹Y轴注入指示物名 = "";

    public string 锁定玩家序号注入指示物名 = "";

    public int 差度位始角 = 0;

    public int 差度位射数 = 0;

    public int 差度位射角 = 0;

    public int 差度位半径 = 0;

    public bool 不射差度位 = false;

    public string 指示物数量注入差度位始角名 = "";

    public float 指示物数量注入差度位始角系数 = 1f;

    public string 指示物数量注入差度位射数名 = "";

    public float 指示物数量注入差度位射数系数 = 1f;

    public string 指示物数量注入差度位射角名 = "";

    public float 指示物数量注入差度位射角系数 = 1f;

    public string 指示物数量注入差度位半径名 = "";

    public float 指示物数量注入差度位半径系数 = 1f;

    public int 差度射数 = 0;

    public float 差度射角 = 0f;

    public string 指示物数量注入差度射数名 = "";

    public float 指示物数量注入差度射数系数 = 1f;

    public string 指示物数量注入差度射角名 = "";

    public float 指示物数量注入差度射角系数 = 1f;

    public int 差位射数 = 0;

    public float 差位偏移X = 0f;

    public float 差位偏移Y = 0f;

    public string 指示物数量注入差位射数名 = "";

    public float 指示物数量注入差位射数系数 = 1f;

    public string 指示物数量注入差位偏移X名 = "";

    public float 指示物数量注入差位偏移X系数 = 1f;

    public string 指示物数量注入差位偏移Y名 = "";

    public float 指示物数量注入差位偏移Y系数 = 1f;

    public int 锁定范围 = 0;

    public float 锁定速度 = 0f;

    public string 指示物数量注入锁定速度名 = "";

    public float 指示物数量注入锁定速度系数 = 1f;

    public bool 以弹为位 = false;

    public int 持续时间 = -1;

    public bool 计入仇恨 = false;

    public bool 锁定血少 = false;

    public bool 锁定低防 = false;

    public bool 仅攻击对象 = false;

    public bool 逆仇恨锁定 = false;

    public bool 逆血量锁定 = false;

    public bool 逆防御锁定 = false;

    public bool 仅扇形锁定 = false;

    public int 扇形半偏角 = 60;

    public bool 以锁定为点 = false;

    public int 最大锁定数 = 1;

    public int 弹点召唤怪物 = 0;

    public bool 弹点召唤怪物无弹 = false;

    public 弹幕节(int id, int sx, int sy, int dm)
    {
        弹幕ID = id;
        X轴速度 = sx;
        Y轴速度 = sy;
        弹幕伤害 = dm;
    }
}