namespace TestPlugin.读配置文件;

public class 怪物条件节
{
    public int 怪物ID = 0;

    public string 查标志 = "";

    public List<指示物组节> 指示物 = new List<指示物组节>();

    public int 范围内 = 0;

    public int 血量比 = 0;

    public int 符合数 = 0;

    public 怪物条件节(int id, int range, int num)
    {
        怪物ID = id;
        范围内 = range;
        符合数 = num;
        指示物 = new List<指示物组节>();
        查标志 = "";
    }
}
