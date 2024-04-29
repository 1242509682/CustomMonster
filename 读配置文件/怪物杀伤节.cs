namespace TestPlugin
{
    public class 怪物杀伤节
    {
        public int 怪物ID = 0;

        public int 范围内 = 0;

        public int 造成伤害 = 0;

        public bool 直接伤害 = false;

        public bool 直接清除 = false;

        public 怪物杀伤节(int id, int range, int num)
        {
            怪物ID = id;
            范围内 = range;
            造成伤害 = num;
        }
    }
}
