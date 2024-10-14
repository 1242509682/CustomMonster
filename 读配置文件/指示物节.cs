namespace TestPlugin.读配置文件
{
    public class 指示物节
    {
        public string 名称 = "";

        public int 数量 = 0;

        public bool 清除 = false;

        public int 随机小 = 0;

        public int 随机大 = 0;

        public string 指示物注入数量名 = "";

        public float 指示物注入数量系数 = 1f;

        public string 指示物注入数量运算符 = "";

        public 指示物节(string name, int num)
        {
            名称 = name;
            数量 = num;
        }
    }
}
