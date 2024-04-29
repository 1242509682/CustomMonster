using Newtonsoft.Json;
using System.Text;

namespace TestPlugin.读配置文件
{
    public class 配置文件
    {
        public bool 启动错误报告 = false;

        public bool 启动死亡队友视角 = false;

        public bool 队友视角仅BOSS时 = true;

        public int 队友视角流畅度 = -1;

        public int 队友视角等待范围 = 18;

        public float 统一对怪物伤害修正 = 1f;

        public float 统一怪物弹幕伤害修正 = 1f;

        public int 统一初始怪物玩家系数 = 0;

        public bool 统一初始玩家系数不低于人数 = true;

        public float 统一初始怪物强化系数 = 0f;

        public double 统一怪物血量倍数 = 1.0;

        public bool 统一血量不低于正常 = true;

        public double 统一怪物强化倍数 = 1.0;

        public bool 统一强化不低于正常 = true;

        public int 统一免疫熔岩类型 = 1;

        public int 统一免疫陷阱类型 = 0;

        public List<int> 统一设置例外怪物 = new List<int>();

        public bool 启动动态血量上限 = true;

        public bool 启动怪物时间限制 = true;

        public bool 启动动态时间限制 = true;

        public 怪物节[] 怪物节集 = new 怪物节[0];

        public void Write(string 路径)
        {
            using (var fs = new FileStream(路径, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var sw = new StreamWriter(fs, new UTF8Encoding(false)))
            {
                var str = JsonConvert.SerializeObject(this, Formatting.Indented);
                sw.Write(str);
                return;
            }
        }

        internal static 配置文件 Read(string 路径)
        {
            if (!File.Exists(路径))
            {
                var defaultConfig = new 配置文件
                {
                    启动错误报告 = false,
                    启动死亡队友视角 = false,
                    队友视角仅BOSS时 = true,
                    队友视角流畅度 = -1,
                    队友视角等待范围 = 18,
                    统一对怪物伤害修正 = 1f,
                    统一怪物弹幕伤害修正 = 1f,
                    统一初始怪物玩家系数 = 0,
                    统一初始玩家系数不低于人数 = true,
                    统一初始怪物强化系数 = 0f,
                    统一怪物血量倍数 = 1.0,
                    统一血量不低于正常 = true,
                    统一怪物强化倍数 = 1.0,
                    统一免疫熔岩类型 = 1,
                    统一免疫陷阱类型 = 0,
                    统一设置例外怪物 = new List<int>(),
                    启动动态血量上限 = true,
                    启动怪物时间限制 = true,
                    启动动态时间限制 = true,
                    怪物节集 = new 怪物节[0]
                };
                defaultConfig.Write(路径);
                return defaultConfig;
            }
            else
            {
                using (var fs = new FileStream(路径, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var sr = new StreamReader(fs))
                {
                    var json = sr.ReadToEnd();
                    var cf = JsonConvert.DeserializeObject<配置文件>(json);
                    return cf;
                }
            }
        }
    }
}
