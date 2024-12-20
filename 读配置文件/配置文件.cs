﻿using System.Configuration;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TestPlugin.读配置文件;

public class 配置文件
{
    public string 配置作者 = "GK 阁下";
    public string 配置优化 = "羽学";
    public bool 控制台广告 = false;
    public string 配置说明 = "版本①;难度直线提升,怪物最低3个人血量,几乎全Boss加强!";
    public bool 启动错误报告 = false;
    public string 配置文件插件版本号 = "1.0.4.39";
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


    [JsonProperty(PropertyName = "是否隐藏没用到配置项的指令/ggw", Order = -10)]
    public bool 是否隐藏没用到的配置项 { get; set; } = false;
    [JsonProperty(PropertyName = "自定义强制隐藏哪些配置项", Order = -9)]
    public List<string> 自定义强制隐藏哪些配置项 = new List<string>();

    #region 隐藏默认值
    public class ContractResolver : DefaultContractResolver
    {
        private readonly 读配置文件.配置文件 config;
        private readonly bool hide;
        public ContractResolver(配置文件 Config, bool hide)
        {
            config = Config;
            this.hide = hide;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            var custom = config.自定义强制隐藏哪些配置项;
            if (hide && custom.Contains(member.Name)) { property.ShouldSerialize = instance => false; }

            if (member is PropertyInfo propertyInfo)
            {
                var defaultValue = Activator.CreateInstance(propertyInfo.PropertyType);
                property.ShouldSerialize = instance =>
                {
                    var value = propertyInfo.GetValue(instance);
                    var defaultValue = Activator.CreateInstance(propertyInfo.PropertyType);
                    return !(!config.是否隐藏没用到的配置项 && !Equals(value, defaultValue));
                };
            }
            return property;
        }
    }
    #endregion

    #region 读取与写入
    public 配置文件 Write(string 路径)
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = 是否隐藏没用到的配置项 ? NullValueHandling.Ignore : NullValueHandling.Include,
            DefaultValueHandling = 是否隐藏没用到的配置项 ? DefaultValueHandling.Ignore : DefaultValueHandling.Include,
            ContractResolver = new ContractResolver(this, 是否隐藏没用到的配置项),
        };

        using (var fs = new FileStream(路径, FileMode.Create, FileAccess.Write, FileShare.Write))
        using (var sw = new StreamWriter(fs, new UTF8Encoding(false)))
        using (var jtw = new JsonTextWriter(sw))
        {
            JsonSerializer.CreateDefault(settings).Serialize(jtw, this);
        }

        return this;
    }

    public static 配置文件 Read(string 路径)
    {
        if (!File.Exists(路径))
        {
            WriteExample(路径);
        }
        return JsonConvert.DeserializeObject<配置文件>(File.ReadAllText(路径))!;
    }
    #endregion

    #region 内嵌文件方法
    public static void WriteExample(string 路径)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var FullName = $"{assembly.GetName().Name}.内嵌文件.自定义怪物血量.json";

        using (var stream = assembly.GetManifestResourceStream(FullName))
        using (var reader = new StreamReader(stream!))
        {
            var text = reader.ReadToEnd();
            var 配置文件2 = JsonConvert.DeserializeObject<配置文件>(text);
            配置文件2!.Write(路径);
        }
    } 
    #endregion
}
