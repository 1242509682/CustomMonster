namespace TestPlugin.读配置文件;

public class 指示物组节
{
    public string 名称 = "";

    public int 数量 = 0;

    public string 重定义判断符号 = "";

    public string 指示物注入数量名 = "";

    public float 指示物注入数量系数 = 1f;

    public 指示物组节(string name, int num)
    {
        名称 = name;
        数量 = num;
    }
}
