using Terraria;

namespace TestPlugin;

public class LNPC
{
    public int Index { get; set; }

    public int Time { get; set; }

    public float TiemN { get; set; }

    public int PlayerCount { get; set; }

    public int MaxLife { get; set; }

    public int MaxTime { get; set; }

    public 怪物节 Config { get; set; }

    public int LifeP { get; set; }

    public int LLifeP { get; set; }

    public float LTime { get; set; }

    public long LKC { get; set; }

    public List<状态节> RBuff { get; set; }

    public int BuffR { get; set; }

    public List<状态节> ORBuff { get; set; }

    public int OBuffR { get; set; }

    public List<比例节> PLife { get; set; }

    public List<时间节> CTime { get; set; }

    public int KillPlay { get; set; }

    public int RespawnSeconds { get; set; }

    public int DefaultMaxSpawns { get; set; }

    public int DefaultSpawnRate { get; set; }

    public int BlockTeleporter { get; set; }

    public int OSTime { get; set; }

    public int Struck { get; set; }

    public List<指示物组节> Markers { get; set; }

    public LNPC(int index, int playercount, int life, 怪物节 config, int maxtime, int ostime, long kc)
    {
        Index = index;
        PlayerCount = playercount;
        KillPlay = 0;
        Time = 0;
        TiemN = 0f;
        MaxLife = life;
        MaxTime = maxtime;
        Config = config;
        LTime = 0f;
        LifeP = 100;
        LLifeP = 100;
        BuffR = 0;
        OSTime = ostime;
        LKC = kc;
        Markers = new List<指示物组节>();
        Struck = 0;
        if (Config != null)
        {
            RespawnSeconds = config.玩家复活时间;
            BlockTeleporter = config.阻止传送器;
            DefaultMaxSpawns = config.全局最大刷怪数;
            DefaultSpawnRate = config.全局刷怪速度;
            PLife = new List<比例节>();
            Config.血量事件.ForEach(delegate (比例节 i)
            {
                PLife.Add((比例节)i.Clone());
            });
            CTime = new List<时间节>();
            Config.时间事件.ForEach(delegate (时间节 i)
            {
                CTime.Add((时间节)i.Clone());
            });
        }
    }

    #region 设置或更新指示物
    /// <summary>
    /// 设置或更新指示物（标记器）的数值
    /// </summary>
    /// <param name="Name">指示物名称</param>
    /// <param name="num">要设置或增加的数值</param>
    /// <param name="reset">
    /// 重置模式：
    /// - true: 直接将指示物数量设置为指定值
    /// - false: 在现有基础上增加指定值
    /// </param>
    /// <remarks>
    /// 功能说明：
    /// 1. 如果指定名称的指示物不存在，则创建新的指示物并初始化为0
    /// 2. 根据reset参数决定是重置还是累加数值
    /// 3. 用于管理NPC的状态标记和计数器
    /// </remarks>
    public void setMarkers(string Name, int num, bool reset)
    {
        string name = Name;

        // 检查指示物是否存在，不存在则创建
        if (!Markers.Exists((指示物组节 t) => t.名称 == name))
        {
            Markers.Add(new 指示物组节(name, 0));
        }

        // 遍历所有指示物，找到目标并更新数值
        foreach (指示物组节 marker in Markers)
        {
            if (marker.名称 == name)
            {
                if (reset)
                {
                    // 重置模式：直接设置数值
                    marker.数量 = num;
                }
                else
                {
                    // 累加模式：在原有基础上增加
                    marker.数量 += num;
                }
                break;
            }
        }
    }
    #endregion

    #region 复杂指示物设置

    /// <summary>
    /// 设置或更新指示物（标记器）的数值 - 支持随机数、注入数值和运算符操作
    /// </summary>
    /// <param name="Name">指示物名称</param>
    /// <param name="num">基础数值</param>
    /// <param name="reset">重置模式：true=重置数值，false=在现有基础上操作</param>
    /// <param name="inname">注入数值的指示物名称</param>
    /// <param name="infactor">注入数值的系数</param>
    /// <param name="inop">运算符：指定如何组合现有数值和新数值</param>
    /// <param name="rmin">随机数最小值</param>
    /// <param name="rmax">随机数最大值</param>
    /// <param name="rd">随机数生成器引用</param>
    /// <param name="npc">关联的NPC实例</param>
    /// <remarks>
    /// 功能说明：
    /// 1. 支持创建新指示物或更新现有指示物
    /// 2. 可以生成随机数范围的值
    /// 3. 支持从其他指示物注入数值并应用系数
    /// 4. 支持多种运算符操作（加、减、乘、除等）
    /// 5. 用于复杂的状态管理和计数器系统
    /// 
    /// 计算流程：
    /// 最终数值 = 运算符操作(当前数值, 基础数值 + 随机数 + 注入数值×系数)
    /// </remarks>
    public void setMarkers(string Name, int num, bool reset, string inname, float infactor, string inop, int rmin, int rmax, ref Random rd, NPC npc)
    {
        string name = Name;

        // 检查指示物是否存在，不存在则创建
        if (!Markers.Exists((指示物组节 t) => t.名称 == name))
        {
            Markers.Add(new 指示物组节(name, 0));
        }

        int num2 = 0;  // 随机数值
                       // 生成随机数（如果指定了有效范围）
        if (rmax > rmin)
        {
            num2 = rd.Next(rmin, rmax);
        }

        // 计算注入数值（从其他指示物获取并应用系数）
        int num3 = addMarkersIn(inname, infactor, npc);

        // 遍历所有指示物，找到目标并应用复杂计算
        foreach (指示物组节 marker in Markers)
        {
            if (marker.名称 == name)
            {
                if (reset)
                {
                    // 重置模式：忽略当前值，直接使用新值进行运算符操作
                    marker.数量 = Sundry.intoperation(inop, 0, num + num2 + num3);
                }
                else
                {
                    // 累加模式：在当前值基础上应用运算符和新值
                    marker.数量 = Sundry.intoperation(inop, marker.数量, num + num2 + num3);
                }
                break;
            }
        }
    }

    #endregion

    #region 注入数值计算

    /// <summary>
    /// 根据指定的标记名称从NPC或自定义数据中获取数值并应用系数
    /// </summary>
    /// <param name="inname">注入数值的标记名称</param>
    /// <param name="infactor">数值系数，用于乘算获取的数值</param>
    /// <param name="npc">关联的NPC实例</param>
    /// <returns>计算后的整数值</returns>
    /// <remarks>
    /// 支持的标记名称：
    /// - [序号]: NPC的whoAmI索引
    /// - [被击]: NPC被击次数
    /// - [击杀]: NPC击杀玩家次数
    /// - [耗时]: NPC存在时间
    /// - [X坐标]: NPC中心的X坐标
    /// - [Y坐标]: NPC中心的Y坐标
    /// - [血量]: NPC当前血量
    /// - [被杀]: 该类型NPC的总击杀数
    /// - [AI0]-[AI3]: NPC的AI参数
    /// - 其他: 从自定义指示物中获取数值
    /// </remarks>
    public int addMarkersIn(string inname, float infactor, NPC npc)
    {
        float num = 0f;

        // 安全检查：确保NPC和自定义数据有效
        if (npc == null)
        {
            return (int)num;
        }
        if (TestPlugin.LNpcs[npc.whoAmI] == null)
        {
            return (int)num;
        }

        // 根据标记名称获取对应的数值
        if (inname != "")
        {
            switch (inname)
            {
                case "[序号]":
                    num = npc.whoAmI;  // NPC的实例索引
                    break;
                case "[被击]":
                    num = TestPlugin.LNpcs[npc.whoAmI].Struck;  // 被攻击次数
                    break;
                case "[击杀]":
                    num = TestPlugin.LNpcs[npc.whoAmI].KillPlay;  // 击杀玩家次数
                    break;
                case "[耗时]":
                    num = TestPlugin.LNpcs[npc.whoAmI].Time;  // 存在时间（单位：游戏刻）
                    break;
                case "[X坐标]":
                    num = npc.Center.X;  // NPC中心的X坐标（像素）
                    break;
                case "[Y坐标]":
                    num = npc.Center.Y;  // NPC中心的Y坐标（像素）
                    break;
                case "[血量]":
                    num = npc.life;  // NPC当前血量
                    break;
                case "[被杀]":
                    {
                        // 获取该类型NPC的总击杀数
                        long num2 = TestPlugin.getLNKC(npc.netID);
                        if (num2 > int.MaxValue)
                        {
                            num2 = 2147483647L;  // 限制最大值防止溢出
                        }
                        num = (int)num2;
                        break;
                    }
                case "[AI0]":
                    num = npc.ai[0];  // NPC AI参数0
                    break;
                case "[AI1]":
                    num = npc.ai[1];  // NPC AI参数1
                    break;
                case "[AI2]":
                    num = npc.ai[2];  // NPC AI参数2
                    break;
                case "[AI3]":
                    num = npc.ai[3];  // NPC AI参数3
                    break;
                default:
                    // 从自定义指示物中获取数值
                    num = TestPlugin.LNpcs[npc.whoAmI].getMarkers(inname);
                    break;
            }
        }

        // 应用系数并返回整数值
        if (num != 0f)
        {
            num *= infactor;
        }
        return (int)num;
    }

    #endregion

    #region 获取指示物数值

    /// <summary>
    /// 根据名称获取指示物的数值
    /// </summary>
    /// <param name="name">要查找的指示物名称</param>
    /// <param name="nullDefault">未找到指示物时返回的默认值，默认为0</param>
    /// <returns>指示物的数值，如果未找到则返回指定的默认值</returns>
    /// <remarks>
    /// 功能说明：
    /// 1. 空名称检查：如果名称为空字符串，直接返回默认值
    /// 2. 线性搜索：遍历指示物列表查找匹配项
    /// 3. 安全返回值：确保总是返回有效数值
    /// 
    /// 使用场景：
    /// - 获取NPC的状态标记值
    /// - 读取计数器数值
    /// - 检查条件判断的数值依据
    /// </remarks>
    public int getMarkers(string name, int nullDefault = 0)
    {
        // 空名称检查
        if (name == "")
        {
            return nullDefault;
        }

        // 遍历指示物列表查找匹配项
        foreach (指示物组节 marker in Markers)
        {
            if (marker.名称 == name)
            {
                return marker.数量;
            }
        }

        // 未找到指示物，返回默认值
        return nullDefault;
    }

    #endregion

    #region 替换标记文本

    /// <summary>
    /// 将文本中的标记占位符替换为对应的指示物数值
    /// </summary>
    /// <param name="text">包含标记占位符的原始文本</param>
    /// <returns>替换后的文本，所有标记占位符已被替换为实际数值</returns>
    /// <remarks>
    /// 功能说明：
    /// 1. 标记格式：文本中的"[指示物名称]"会被替换为对应指示物的数值
    /// 2. 遍历替换：遍历所有指示物，逐个替换文本中的标记
    /// 3. 原地替换：在原始文本基础上直接进行替换操作
    /// 
    /// 使用场景：
    /// - 动态生成NPC对话文本
    /// - 替换命令字符串中的变量
    /// - 生成包含实时数据的广播消息
    /// 
    /// 示例：
    /// 输入："玩家已击杀[骷髅王]个骷髅王"
    /// 输出："玩家已击杀15个骷髅王"
    /// </remarks>
    public string ReplaceMarkers(string text)
    {
        string text2 = text;

        // 遍历所有指示物，替换文本中的标记占位符
        foreach (指示物组节 marker in Markers)
        {
            text2 = text2.Replace("[" + marker.名称 + "]", marker.数量.ToString());
        }

        return text2;
    }

    #endregion

    #region 检查指示物条件

    /// <summary>
    /// 检查NPC是否满足指定的指示物条件
    /// </summary>
    /// <param name="list">指示物条件列表，包含多个需要检查的条件</param>
    /// <param name="npc">关联的NPC实例</param>
    /// <returns>如果满足所有条件返回true，否则返回false</returns>
    /// <remarks>
    /// 功能说明：
    /// 1. 多条件检查：遍历所有条件，只有全部满足才返回true
    /// 2. 支持自定义比较运算符：=, !, >, <
    /// 3. 支持数值注入：可以从其他指示物或NPC属性注入数值
    /// 4. 默认条件逻辑：正数要求当前值≥条件值，负数要求当前值＜条件值的绝对值
    /// 
    /// 判断逻辑：
    /// - 使用重定义判断符号时：按指定运算符比较
    /// - 不使用判断符号时：正数条件要求≥，负数条件要求＜
    /// - 任何条件不满足立即返回false
    /// 
    /// 使用场景：
    /// - 事件触发条件检查
    /// - NPC行为条件验证
    /// - 技能释放前提判断
    /// </remarks>
    public bool haveMarkers(List<指示物组节> list, NPC npc)
    {
        bool flag = false;

        // 遍历所有条件进行检查
        foreach (指示物组节 item in list)
        {
            // 获取当前指示物数值
            int markers = getMarkers(item.名称);
            // 计算条件目标值（基础值 + 注入值）
            int num = item.数量 + addMarkersIn(item.指示物注入数量名, item.指示物注入数量系数, npc);

            // 使用自定义判断符号进行比较
            if (item.重定义判断符号 != "")
            {
                switch (item.重定义判断符号)
                {
                    case "=":  // 等于
                        if (markers != num)
                        {
                            flag = true;
                        }
                        break;
                    case "!":  // 不等于
                        if (markers == num)
                        {
                            flag = true;
                        }
                        break;
                    case ">":  // 大于
                        if (markers <= num)
                        {
                            flag = true;
                        }
                        break;
                    case "<":  // 小于
                        if (markers >= num)
                        {
                            flag = true;
                        }
                        break;
                }
            }
            else
            {
                // 使用默认判断逻辑
                if (num == 0)
                {
                    continue;  // 条件值为0，跳过检查
                }

                if (num > 0)
                {
                    // 正数条件：要求当前值 ≥ 条件值
                    if (markers < num)
                    {
                        flag = true;
                    }
                }
                else
                {
                    // 负数条件：要求当前值 < 条件值的绝对值
                    if (markers >= Math.Abs(num))
                    {
                        flag = true;
                    }
                }
            }

            // 如果任一条件不满足，立即跳出循环
            if (flag)
            {
                break;
            }
        }

        // 返回检查结果：flag为true表示有条件不满足，返回false
        if (flag)
        {
            return false;
        }
        return true;
    }

    #endregion
}
