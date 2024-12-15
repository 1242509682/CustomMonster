namespace TestPlugin;

public class 状态节
{
    public int 状态ID = 0;

    public int 状态起始范围 = 0;

    public int 状态结束范围 = 0;

    public string 头顶提示 = "";

    public 状态节(int id, int start, int tail, string tip)
    {
        状态ID = id;
        状态起始范围 = start;
        状态结束范围 = tail;
        头顶提示 = tip;
    }
}
