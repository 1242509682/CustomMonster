namespace TestPlugin.读配置文件;

public class 玩家条件节
{
    public int 范围起 = 0;

    public int 范围内 = 0;

    public int 生命值 = 0;

    public int 符合数 = 0;

    public 玩家条件节(int rangeB, int rangeE, int num)
    {
        范围起 = rangeB;
        范围内 = rangeE;
        符合数 = num;
    }
}
