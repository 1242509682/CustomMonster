namespace TestPlugin;

public class 弹幕条件节
{
    public int 弹幕ID = 0;

    public string 查标志 = "";

    public int 范围内 = 0;

    public int 符合数 = 0;

    public bool 全局弹幕 = false;

    public 弹幕条件节(int id, int range, int num)
    {
        弹幕ID = id;
        范围内 = range;
        符合数 = num;
        查标志 = "";
    }
}

