using TestPlugin.读配置文件;

namespace TestPlugin
{
    public class 怪物杀伤节
    {
        public int 怪物ID = 0;

        public string 查标志 = "";

        public List<指示物组节> 指示物 = new List<指示物组节>();

        public int 范围内 = 0;

        public int 造成伤害 = 0;

        public string 指示物数量注入造成伤害名 = "";

        public float 指示物数量注入造成伤害系数 = 1f;

        public bool 直接伤害 = false;

        public bool 直接清除 = false;

        public 怪物杀伤节(int id, int range, int num)
        {
            怪物ID = id;
            范围内 = range;
            造成伤害 = num;
            指示物 = new List<指示物组节>();
            查标志 = "";
        }
    }
}
