namespace TestPlugin.读配置文件
{
    public class 物品节
    {
        public int 物品ID = 0;

        public int 物品数量 = 0;

        public int 物品前缀 = -1;

        public bool 独立掉落 = false;

        public 物品节(int id, int stack)
        {
            物品ID = id;
            物品数量 = stack;
        }
    }
}
