using System.Timers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Utilities;
using TerrariaApi.Server;
using TShockAPI;
using static TShockAPI.GetDataHandlers;

namespace TestPlugin;

[ApiVersion(2, 1)]
public class TestPlugin : TerrariaPlugin
{
    #region 插件信息
    public override string Name => "自定义怪物血量";
    public override string Author => "GK 阁下 羽学";
    public override Version Version => new Version(1, 0, 4, 45);
    public override string Description => "自定义怪物出没时的血量,当然不止这些！";
    #endregion

    #region 全局变量
    /// <summary>
    /// 插件主配置文件实例 - 存储所有自定义怪物配置和插件设置
    /// </summary>
    public 配置文件 _配置 = new 配置文件();

    /// <summary>
    /// 主配置文件名 - 用于存储插件的主要配置
    /// </summary>
    public string _配置文件名 => "自定义怪物血量.json";

    /// <summary>
    /// 额外配置路径 - 用于存储额外的配置文件（带点前缀）
    /// </summary>

    // public string _额外配置路径 => Path.Combine(TShock.SavePath, "." + _配置文件名);
    public string _额外配置路径 => Path.Combine(TShock.SavePath, "自定义怪物血量_额外配置");

    /// <summary>
    /// 主配置路径 - 插件配置文件在服务器中的完整路径
    /// </summary>
    public string _配置路径 => Path.Combine(TShock.SavePath, _配置文件名);

    /// <summary>
    /// NPC击杀数据文件路径 - 存储NPC击杀统计数据的文件
    /// </summary>
    public string NPCKillPath => Path.Combine(TShock.SavePath, "自定义怪物血量存档.txt");

    /// <summary>
    /// 更新定时器 - 每10毫秒触发一次，用于处理NPC的实时更新逻辑
    /// </summary>
    private static readonly System.Timers.Timer Update = new System.Timers.Timer(10.0);

    /// <summary>
    /// NPC击杀数据最后保存时间 - 用于控制数据保存频率，避免频繁写入
    /// </summary>
    public DateTime NPCKillDataTime { get; set; }

    /// <summary>
    /// 服务器启动时间 - 记录服务器启动的UTC时间，用于计算开服时长
    /// </summary>
    public DateTime OServerDataTime { get; set; }

    /// <summary>
    /// NPC击杀统计列表 - 存储所有NPC的击杀计数数据
    /// 格式：NPC网络ID -> 击杀次数
    /// </summary>
    private static List<LNKC> LNkc { get; set; }

    /// <summary>
    /// 特殊NPC标记列表 - 用于管理特殊标记的NPC实例
    /// </summary>
    private static List<LSMNPC> LLSMNPCs { get; set; }

    /// <summary>
    /// 弹幕数据数组 - 存储自定义弹幕的相关数据
    /// </summary>
    public static LPrj[] LPrjs { get; set; }

    /// <summary>
    /// 自定义NPC数据数组 - 存储所有活跃NPC的自定义数据
    /// 包括血量、时间限制、事件触发等
    /// </summary>
    public static LNPC[] LNpcs { get; set; }

    /// <summary>
    /// 版本标识 - 用于标识插件版本或配置版本
    /// </summary>
    public int Beta = 3;

    /// <summary>
    /// 队友视角计数器 - 用于控制队友视角功能的更新频率
    /// </summary>
    public int TeamP = 0;

    #endregion

    #region 注册与卸载事件，并初始化静态实例
    public TestPlugin(Terraria.Main game) : base(game)
    {
        this.Order = 1;
        LNpcs = new LNPC[201];
        LPrjs = new LPrj[1001];
        LNkc = new List<LNKC>();
        LLSMNPCs = new List<LSMNPC>();
        NPCKillDataTime = DateTime.UtcNow;
        OServerDataTime = DateTime.UtcNow;
    }

    public override void Initialize()
    {
        RC();
        RD();
        GetDataHandlers.KillMe += OnKillMe!;
        ServerApi.Hooks.GamePostInitialize.Register(this, PostInitialize);
        ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
        ServerApi.Hooks.NpcSpawn.Register(this, NpcSpawn);
        ServerApi.Hooks.NpcKilled.Register(this, NpcKilled);
        ServerApi.Hooks.NpcStrike.Register(this, NpcStrike);
        ServerApi.Hooks.NetSendData.Register(this, SendData);
        On.Terraria.NPC.SetDefaults += NPC_SetDefaults;
        On.Terraria.Projectile.Kill += Projectile_Kill;
        On.Terraria.Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += Projectile_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (LNkc)
            {
                SD(notime: true);
            }
            Update.Elapsed -= OnUpdate!;
            Update.Stop();
            GetDataHandlers.KillMe -= OnKillMe!;
            ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
            ServerApi.Hooks.NpcSpawn.Deregister(this, NpcSpawn);
            ServerApi.Hooks.NpcKilled.Deregister(this, NpcKilled);
            ServerApi.Hooks.GamePostInitialize.Deregister(this, PostInitialize);
            ServerApi.Hooks.NpcStrike.Deregister(this, NpcStrike);
            ServerApi.Hooks.NetSendData.Deregister(this, SendData);
            On.Terraria.NPC.SetDefaults -= NPC_SetDefaults;
            On.Terraria.Projectile.Kill -= Projectile_Kill;
            On.Terraria.Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float -= Projectile_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float;
        }
        base.Dispose(disposing);
    }
    #endregion

    #region 插件指令注册方法
    private void OnInitialize(EventArgs args)
    {
        Commands.ChatCommands.Add(new Command("重读自定义怪物血量权限", new CommandDelegate(CRC), new string[2] { "重读自定义怪物血量", "reload" })
        {
            HelpText = "输入 /重读自定义怪物血量 会重新读取重读自定义怪物血量表"
        });
        Commands.ChatCommands.Add(new Command("召唤自定义怪物血量怪物", new CommandDelegate(CMD), new string[1] { "召唤自定义怪物血量怪物" })
        {
            HelpText = "输入 /召唤自定义怪物血量怪物 怪物标志 会召唤指定标记的怪物(无视召唤条件)"
        });
        Commands.ChatCommands.Add(new Command("重置自定义怪物血量存档", new CommandDelegate(CME), new string[1] { "重置自定义怪物血量存档" })
        {
            HelpText = "输入 /召唤自定义怪物血量怪物 怪物标志 会召唤指定标记的怪物(无视召唤条件)"
        });
        Commands.ChatCommands.Add(new Command("重读自定义怪物血量权限", CRS, "改怪物", "ggw"));
    }
    #endregion

    #region 插件启动后输出信息方法并启动定时器
    public void PostInitialize(EventArgs e)
    {
        Update.Elapsed += OnUpdate!;
        Update.Start();
        if (_配置.控制台广告)
        {
            Console.WriteLine(" ------------ " + (this.Name + " 版本:" + (this.Version?.ToString() + ((Beta > 0) ? (" Beta" + Beta + " 已启动! --------") : " 已启动! --------------"))));
            Console.WriteLine(" >>> 如果使用过程中出现什么问题可前往QQ群232109072交流反馈.");
            Console.WriteLine(" >>> 本插件免费请勿上当受骗,您可在QQ群232109072中获取最新插件.");
            if (Beta > 0)
            {
                Console.WriteLine(" >>> 抢先测试版,因配置项待定后续可能会修改,所以仅仅测试使用!!!!!!");
            }
            Console.WriteLine(" ----------------------------------------------------------------");
        }
    }
    #endregion

    #region 隐藏多余配置项的指令方法 羽学加
    private void CRS(CommandArgs args)
    {
        CRC(args);
        _配置 = 配置文件.Read(_配置路径);

        _配置.是否隐藏没用到的配置项 = !_配置.是否隐藏没用到的配置项;
        _配置.Write(_配置路径);

        args.Player.SendSuccessMessage("[自定义怪物血量] 隐藏配置项:" +
            (_配置.是否隐藏没用到的配置项 ? "已隐藏" : "已显示"));
    }
    #endregion

    #region 弹幕伤害统一修正钩子
    /// <summary>
    /// 弹幕创建时的伤害修正钩子方法
    /// </summary>
    /// <param name="orig">原始弹幕创建方法</param>
    /// <param name="spawnSource">弹幕生成源</param>
    /// <param name="X">生成位置X坐标</param>
    /// <param name="Y">生成位置Y坐标</param>
    /// <param name="SpeedX">X轴速度</param>
    /// <param name="SpeedY">Y轴速度</param>
    /// <param name="Type">弹幕类型ID</param>
    /// <param name="Damage">弹幕伤害值</param>
    /// <param name="KnockBack">弹幕击退力</param>
    /// <param name="Owner">弹幕所有者</param>
    /// <param name="ai0">AI参数0</param>
    /// <param name="ai1">AI参数1</param>
    /// <param name="ai2">AI参数2</param>
    /// <returns>创建的弹幕索引</returns>
    /// <remarks>
    /// 此钩子方法在弹幕创建时拦截并统一修正怪物弹幕的伤害值
    /// 只对怪物发射的弹幕生效（Owner == Main.myPlayer）
    /// </remarks>
    private int Projectile_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float(
        On.Terraria.Projectile.orig_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float orig,
        IEntitySource spawnSource,
        float X,
        float Y,
        float SpeedX,
        float SpeedY,
        int Type,
        int Damage,
        float KnockBack,
        int Owner,
        float ai0,
        float ai1,
        float ai2)
    {
        // 检查是否为怪物弹幕且需要伤害修正
        if (Owner == Terraria.Main.myPlayer && _配置.统一怪物弹幕伤害修正 != 1f)
        {
            // 应用统一的伤害修正系数
            Damage = (int)((float)Damage * _配置.统一怪物弹幕伤害修正);
        }

        // 调用原始方法创建弹幕
        return orig.Invoke(spawnSource, X, Y, SpeedX, SpeedY, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
    }

    #endregion

    #region 弹幕销毁时的清理钩子方法
    /// <summary>
    /// 弹幕销毁时的清理钩子方法
    /// </summary>
    /// <param name="orig">原始方法</param>
    /// <param name="self">要销毁的弹幕实例</param>
    /// <remarks>
    /// 当弹幕被销毁时，自动清理自定义弹幕管理系统中的对应数据
    /// 使用锁机制确保线程安全
    /// </remarks>
    private void Projectile_Kill(On.Terraria.Projectile.orig_Kill orig, Terraria.Projectile self)
    {
        // 使用锁确保线程安全地访问弹幕数据
        lock (LPrjs)
        {
            // 检查并清理自定义弹幕数据
            if (LPrjs[self.identity] != null)
            {
                LPrjs[self.identity] = null;
            }
        }

        // 调用原始方法执行实际的弹幕销毁逻辑
        orig.Invoke(self);
    }
    #endregion

    #region NPC默认设置钩子
    /// <summary>
    /// NPC默认设置钩子方法，用于统一调整怪物生成参数
    /// </summary>
    /// <param name="orig">原始方法</param>
    /// <param name="self">NPC实例</param>
    /// <param name="Type">NPC类型ID</param>
    /// <param name="spawnparams">NPC生成参数</param>
    /// <remarks>
    /// 此钩子方法在NPC设置默认值时调用，用于统一调整怪物的难度系数
    /// 支持基于玩家数量和固定系数的难度调整
    /// </remarks>
    private void NPC_SetDefaults(On.Terraria.NPC.orig_SetDefaults orig, Terraria.NPC self, int Type, Terraria.NPCSpawnParams spawnparams)
    {
        // 检查是否需要应用统一难度调整
        if (_配置.统一初始怪物玩家系数 > 0 || _配置.统一初始怪物玩家单体系数 > 0 || _配置.统一初始怪物强化系数 > 0f)
        {
            // 玩家数量系数调整
            if (_配置.统一初始怪物玩家系数 > 0 || _配置.统一初始怪物玩家单体系数 > 0)
            {
                // 获取当前活跃玩家数量
                int activePlayerCount = TShock.Utils.GetActivePlayerCount();

                // 计算玩家数量难度系数
                if (_配置.统一初始玩家系数额外增加)
                {
                    // 模式1：基础系数 + 玩家数量 + (玩家数量 × 单体系数)
                    spawnparams.playerCountForMultiplayerDifficultyOverride = activePlayerCount + _配置.统一初始怪物玩家系数 + activePlayerCount * _配置.统一初始怪物玩家单体系数;
                }
                else
                {
                    // 模式2：基础系数 + (玩家数量 × 单体系数)
                    spawnparams.playerCountForMultiplayerDifficultyOverride = _配置.统一初始怪物玩家系数 + activePlayerCount * _配置.统一初始怪物玩家单体系数;
                }

                // 限制最大难度系数（防止数值过大）
                if (spawnparams.playerCountForMultiplayerDifficultyOverride > 1000)
                {
                    spawnparams.playerCountForMultiplayerDifficultyOverride = 1000;
                }

                // 确保难度系数不低于实际玩家数量
                if (_配置.统一初始玩家系数不低于人数 && spawnparams.playerCountForMultiplayerDifficultyOverride < activePlayerCount)
                {
                    spawnparams.playerCountForMultiplayerDifficultyOverride = activePlayerCount;
                }
            }

            // 统一强化系数调整
            if (_配置.统一初始怪物强化系数 > 0f)
            {
                // 设置统一的怪物强度乘数
                spawnparams.strengthMultiplierOverride = _配置.统一初始怪物强化系数;

                // 限制最大强化系数
                if (spawnparams.strengthMultiplierOverride > 1000f)
                {
                    spawnparams.strengthMultiplierOverride = 1000f;
                }
            }
        }

        // 调用原始方法完成NPC的默认设置
        orig.Invoke(self, Type, spawnparams);
    }

    #endregion

    #region 配置文件读取与合并
    /// <summary>
    /// 读取并合并配置文件（主配置 + 额外配置）
    /// </summary>
    /// <remarks>
    /// 此方法负责：
    /// 1. 读取主配置文件
    /// 2. 检查配置文件版本兼容性
    /// 3. 读取并合并额外配置文件
    /// 4. 处理配置合并过程中的异常
    /// </remarks>
    private void RC()
    {
        try
        {
            // ===== 主配置文件处理 =====
            // 检查主配置文件是否存在
            if (!File.Exists(_配置路径))
            {
                TShock.Log.ConsoleError("未找到自定义怪物血量配置，已为您创建！修改配置后重启即可重新载入数据。");
            }

            // 读取主配置文件
            _配置 = 配置文件.Read(_配置路径);

            // 检查配置文件版本兼容性
            Version version = new Version(_配置.配置文件插件版本号);
            if (version <= this.Version)
            {
                // 版本兼容，更新配置文件版本号
                _配置.配置文件插件版本号 = this.Version.ToString();
                _配置.Write(_配置路径);
            }
            else
            {
                // 配置文件版本高于插件版本，提示用户升级插件
                TShock.Log.ConsoleError("[自定义怪物血量]您载入的配置文件插件版本高于本插件版本,配置可能无法正常使用,请升级插件后使用！");
            }

            // ===== 额外配置文件处理 =====
            // 确保额外配置目录存在
            if (!Directory.Exists(_额外配置路径))
            {
                Directory.CreateDirectory(_额外配置路径);
            }

            // 获取额外配置文件列表
            DirectoryInfo directoryInfo = new DirectoryInfo(_额外配置路径);
            FileInfo[] files = directoryInfo.GetFiles("*." + _配置文件名);
            int num = 0; // 统计额外配置添加的怪物配置数量

            // 遍历所有额外配置文件
            for (int i = 0; i < files.Length; i++)
            {
                string fullName = files[i].FullName;
                try
                {
                    // 读取单个额外配置文件
                    配置文件 配置文件 = 配置文件.Read(fullName);

                    // 检查额外配置文件版本兼容性
                    version = new Version(配置文件.配置文件插件版本号);
                    if (version <= this.Version)
                    {
                        // 版本兼容，更新配置文件版本号
                        配置文件.配置文件插件版本号 = this.Version.ToString();
                        配置文件.Write(fullName);
                    }
                    else
                    {
                        // 配置文件版本高于插件版本，提示用户
                        TShock.Log.ConsoleError("[自定义怪物血量]您读入的额外配置 " + files[i].Name + " 插件版本高于本插件版本,配置可能无法正常使用,请升级插件后使用！");
                    }

                    // ===== 配置合并逻辑 =====
                    int num2 = 配置文件.怪物节集.Length;     // 额外配置中的怪物配置数量
                    int num3 = _配置.怪物节集.Length;         // 主配置中的怪物配置数量

                    // 合并怪物配置数组
                    if (num2 > 0)
                    {
                        怪物节[] array = new 怪物节[num3 + num2];
                        Array.Copy(_配置.怪物节集, 0, array, 0, num3);        // 复制主配置
                        Array.Copy(配置文件.怪物节集, 0, array, num3, num2);  // 复制额外配置
                        _配置.怪物节集 = array;                              // 更新合并后的配置
                    }

                    // 合并统一设置例外怪物列表
                    if (配置文件.统一设置例外怪物.Count > 0)
                    {
                        _配置.统一设置例外怪物 = _配置.统一设置例外怪物.Union(配置文件.统一设置例外怪物).ToList();
                    }

                    // 统计添加的配置数量
                    num += num2;
                }
                catch (Exception ex)
                {
                    // 处理单个额外配置文件的读取错误
                    TShock.Log.ConsoleError("[自定义怪物血量] 额外配置[" + files[i].Name + "]错误:\n" + ex.ToString() + "\n");
                }
            }

            // 输出合并统计信息
            if (num > 0)
            {
                Console.WriteLine("[自定义怪物血量] 通过额外配置增添了" + num + "条配置");
            }
        }
        catch (Exception ex2)
        {
            // 处理整体配置读取过程中的错误
            TShock.Log.ConsoleError("[自定义怪物血量] 配置错误:\n" + ex2.ToString() + "\n");
        }
    }

    #endregion

    #region 命令处理方法
    /// <summary>
    /// 重置自定义怪物血量存档数据
    /// </summary>
    /// <param name="args">命令参数</param>
    /// <remarks>
    /// 此命令用于重置所有怪物的击杀计数和存档数据
    /// 使用锁机制确保线程安全
    /// </remarks>
    private void CME(CommandArgs args)
    {
        // 使用锁确保线程安全地访问存档数据
        lock (LNkc)
        {
            // 重置击杀计数数据
            ReD();

            // 保存重置后的数据（不记录时间）
            SD(notime: true);
        }

        // 向玩家发送成功消息
        args.Player.SendSuccessMessage(args.Player.Name + "[自定义怪物血量]存档重置完毕。");
    }

    /// <summary>
    /// 重新加载配置文件
    /// </summary>
    /// <param name="args">命令参数</param>
    /// <remarks>
    /// 此命令用于重新读取配置文件并清空已加载的NPC缓存
    /// 适用于配置修改后的热重载
    /// </remarks>
    private void CRC(CommandArgs args)
    {
        // 重新读取配置文件
        RC();

        // 清空已加载的NPC缓存
        LLSMNPCs.Clear();

        // 向玩家发送成功消息
        args.Player.SendSuccessMessage(args.Player.Name + "[自定义怪物血量]配置重读完毕。");
    }
    #endregion

    #region 自定义怪物召唤命令
    /// <summary>
    /// 处理自定义怪物召唤命令
    /// </summary>
    /// <param name="args">命令参数</param>
    /// <remarks>
    /// 此命令允许玩家通过怪物标志召唤自定义配置的怪物
    /// 支持三种调用格式：
    /// 1. 指定怪物标志（在玩家位置召唤1个）
    /// 2. 指定怪物标志和数量
    /// 3. 指定怪物标志和精确坐标
    /// </remarks>
    private void CMD(CommandArgs args)
    {
        // 参数数量验证
        if (args.Parameters.Count < 1 || args.Parameters.Count > 3)
        {
            args.Player.SendErrorMessage("格式错误,正确格式是:");
            args.Player.SendErrorMessage(" /召唤自定义怪物血量怪物 怪物标志 ");
            args.Player.SendErrorMessage(" /召唤自定义怪物血量怪物 怪物标志 数量");
            args.Player.SendErrorMessage(" /召唤自定义怪物血量怪物 怪物标志 X像素坐标 Y像素坐标");
            return;
        }

        // 怪物标志验证
        if (args.Parameters[0].Length == 0)
        {
            args.Player.SendErrorMessage("无效标志");
            return;
        }

        // 解析召唤数量参数
        int result = 1; // 默认召唤数量为1
        if (args.Parameters.Count == 2 && !int.TryParse(args.Parameters[1], out result))
        {
            args.Player.SendErrorMessage("数量错误,正确格式是: /召唤自定义怪物血量怪物 怪物标志 召唤数量");
            return;
        }

        // 解析坐标参数
        int result2 = -1; // X坐标
        int result3 = -1; // Y坐标
        if (args.Parameters.Count == 3 && !int.TryParse(args.Parameters[1], out result2) && !int.TryParse(args.Parameters[2], out result3))
        {
            args.Player.SendErrorMessage("坐标错误,正确格式是: /召唤自定义怪物血量怪物 怪物标志 X像素坐标 Y像素坐标(无法指定肉山)");
            return;
        }

        // 限制最大召唤数量
        result = Math.Min(result, 200);

        // 遍历配置查找匹配的怪物
        怪物节[] 怪物节集 = _配置.怪物节集;
        foreach (怪物节 怪物节 in 怪物节集)
        {
            // 检查标志匹配和怪物ID有效性
            if (!(怪物节.标志 == args.Parameters[0]) || 怪物节.怪物ID == 0)
            {
                continue;
            }

            // 获取NPC模板数据
            NPC nPCById = TShock.Utils.GetNPCById(怪物节.怪物ID);
            if (nPCById == null)
            {
                args.Player.SendErrorMessage("无效的空NPCID!");
            }
            // 普通怪物召唤逻辑
            else if (nPCById.netID != 0 && nPCById.type < NPCID.Count && nPCById.netID != 113 && nPCById.netID != 488)
            {
                // 添加到已加载NPC列表
                LLSMNPCs.Add(new LSMNPC(nPCById.netID, 怪物节.标志));

                // 根据参数选择召唤方式
                if (result2 < 0 || result3 < 0)
                {
                    // 在玩家位置召唤
                    TSPlayer.Server.SpawnNPC(nPCById.netID, nPCById.FullName, result, args.Player.TileX, args.Player.TileY, 50, 20);
                }
                else
                {
                    // 在指定坐标召唤
                    Sundry.LaunchProjectileSpawnNPC(nPCById.netID, result2, result3);
                }

                // 发送召唤成功消息
                if (args.Silent)
                {
                    args.Player.SendSuccessMessage("召唤 {0} {1} 次.", new object[2] { nPCById.FullName, result });
                }
                else
                {
                    TSPlayer.All.SendSuccessMessage("{0} 召唤 {1} {2} 次.", new object[3]
                    {
                    args.Player.Name,
                    nPCById.FullName,
                    result
                    });
                }
            }
            // 特殊处理：肉墙召唤
            else if (nPCById.netID == 113)
            {
                // 检查肉墙召唤条件
                if (Main.wofNPCIndex != -1 || args.Player.Y / 16f < (float)(Main.maxTilesY - 205))
                {
                    args.Player.SendErrorMessage("无法根据肉墙的当前状态或您的当前位置生成肉墙。");
                    return;
                }

                // 添加到已加载NPC列表并召唤肉墙
                LLSMNPCs.Add(new LSMNPC(nPCById.netID, 怪物节.标志));
                NPC.SpawnWOF(new Vector2(args.Player.X, args.Player.Y));

                // 发送召唤成功消息
                if (args.Silent)
                {
                    args.Player.SendSuccessMessage("肉墙召唤成功.");
                    return;
                }
                TSPlayer.All.SendSuccessMessage("{0} 召唤了肉墙.", new object[1] { args.Player.Name });
            }
            else
            {
                args.Player.SendErrorMessage("错误的召唤过程.");
            }
            return; // 找到匹配怪物后退出循环
        }

        // 没有找到匹配的怪物标志
        args.Player.SendErrorMessage("没有这个标记的怪物");
    }
    #endregion

    #region 怪物生成事件
    /// <summary>
    /// NPC生成事件处理方法 - 处理自定义NPC的生成逻辑和属性配置
    /// </summary>
    /// <param name="args">NPC生成事件参数</param>
    /// <remarks>
    /// 功能：
    /// - 检查NPC生成条件
    /// - 应用自定义NPC配置
    /// - 动态调整NPC属性
    /// - 初始化自定义NPC数据
    /// </remarks>
    private void NpcSpawn(NpcSpawnEventArgs args)
    {
        // 检查事件是否已处理或NPC无效
        if (args.Handled || Main.npc[args.NpcId] == null || Main.npc[args.NpcId].netID == 0 ||
            Main.npc[args.NpcId].netID == 488 || !Main.npc[args.NpcId].active)
        {
            return;
        }

        bool flag = false;                           // 标记是否需要更新NPC血量
        int activePlayerCount = TShock.Utils.GetActivePlayerCount();  // 活跃玩家数量
        int lifeMax = Main.npc[args.NpcId].lifeMax;  // 原始最大血量
        怪物节? config = null;                       // 匹配的怪物配置
        int maxtime = 0;                             // 最大时间限制
        long lNKC = getLNKC(Main.npc[args.NpcId].netID);  // 该类型NPC的击杀数
        int num = 0;                                 // 同类型NPC数量
        int num2 = 0;                                // 开服时间（按类型计算）
        float strengthMultiplier = Main.npc[args.NpcId].strengthMultiplier;  // 原始强化系数

        // 统计同类型NPC数量
        NPC[] npc = Main.npc;
        foreach (NPC val in npc)
        {
            if (val.netID == Main.npc[args.NpcId].netID)
            {
                num++;
            }
        }

        // 配置匹配和条件检查
        Random random = new Random();
        怪物节[] 怪物节集 = _配置.怪物节集;

        // 遍历所有怪物配置，寻找匹配的配置
        foreach (怪物节 怪物节 in 怪物节集)
        {
            // 检查NPC ID匹配
            if (Main.npc[args.NpcId].netID != 怪物节.怪物ID && 怪物节.怪物ID != 0)
            {
                continue;
            }

            // 检查NPC ID匹配
            int num3 = -1;
            for (int k = 0; k < LLSMNPCs.Count; k++)
            {
                if (LLSMNPCs[k].ID == 怪物节.怪物ID)
                {
                    if (LLSMNPCs[k].M == 怪物节.标志)
                    {
                        num3 = k;
                        break;
                    }
                    num3 = -2;
                }
            }


            if (num3 != -1)
            {
                if (num3 == -2)
                {
                    continue;
                }
                LLSMNPCs.RemoveAt(num3);
            }
            else
            {
                // 检查再匹配条件
                if ((怪物节.怪物ID == 0 && 怪物节.再匹配.Count() > 0 && !怪物节.再匹配.Contains(Main.npc[args.NpcId].netID)) || (怪物节.怪物ID == 0 && 怪物节.再匹配例外.Count() > 0 && 怪物节.再匹配例外.Contains(Main.npc[args.NpcId].netID)))
                {
                    continue;
                }

                // 计算开服时间
                num2 = ((怪物节.开服时间型 == 1) ? ((int)(DateTime.UtcNow - OServerDataTime).TotalDays) : ((怪物节.开服时间型 != 2) ? ((int)(DateTime.UtcNow - OServerDataTime).TotalHours) : ((int)(DateTime.UtcNow.Date - OServerDataTime.Date).TotalDays)));

                // 检查各种生成条件
                if ((怪物节.难度条件.Length != 0 && !怪物节.难度条件.Contains(Main.GameMode)) || Sundry.SeedRequirement(怪物节.种子条件))
                {
                    continue;
                }

                if (怪物节.杀数条件 != 0)
                {
                    if (怪物节.杀数条件 > 0)
                    {
                        if (lNKC < 怪物节.杀数条件)
                        {
                            continue;
                        }
                    }
                    else if (lNKC >= Math.Abs(怪物节.杀数条件))
                    {
                        continue;
                    }
                }
                if (怪物节.数量条件 != 0)
                {
                    if (怪物节.数量条件 > 0)
                    {
                        if (num < 怪物节.数量条件)
                        {
                            continue;
                        }
                    }
                    else if (num >= Math.Abs(怪物节.数量条件))
                    {
                        continue;
                    }
                }
                if (怪物节.人数条件 != 0)
                {
                    if (怪物节.人数条件 > 0)
                    {
                        if (activePlayerCount < 怪物节.人数条件)
                        {
                            continue;
                        }
                    }
                    else if (activePlayerCount >= Math.Abs(怪物节.人数条件))
                    {
                        continue;
                    }
                }
                if (怪物节.开服条件 != 0)
                {
                    if (怪物节.开服条件 > 0)
                    {
                        if (num2 < 怪物节.开服条件)
                        {
                            continue;
                        }
                    }
                    else if (num2 >= Math.Abs(怪物节.开服条件))
                    {
                        continue;
                    }
                }

                // 游戏状态条件检查（昼夜、降雨、血月、日食、肉山后等）
                if ((怪物节.昼夜条件 == -1 && Main.dayTime) || (怪物节.昼夜条件 == 1 && !Main.dayTime) || (怪物节.降雨条件 == -1 && Main.raining) || (怪物节.降雨条件 == 1 && !Main.raining) || (怪物节.血月条件 == -1 && Main.bloodMoon) || (怪物节.血月条件 == 1 && !Main.bloodMoon) || (怪物节.日食条件 == -1 && Main.eclipse) || (怪物节.日食条件 == 1 && !Main.eclipse) || (怪物节.肉山条件 == -1 && Main.hardMode) || (怪物节.肉山条件 == 1 && !Main.hardMode) || (怪物节.巨人条件 == -1 && NPC.downedGolemBoss) || (怪物节.巨人条件 == 1 && !NPC.downedGolemBoss) || (怪物节.月总条件 == -1 && NPC.downedMoonlord) || (怪物节.月总条件 == 1 && !NPC.downedMoonlord) || 怪物节.出没率子 <= 0 || 怪物节.出没率母 <= 0 || (怪物节.出没率子 < 怪物节.出没率母 && random.Next(1, 怪物节.出没率母 + 1) > 怪物节.出没率子) || Sundry.NPCKillRequirement(怪物节.杀怪条件) || Sundry.MonsterRequirement(怪物节.怪物条件, Main.npc[args.NpcId]))
                {
                    continue;
                }
            }

            // 检查并计算时间限制
            if (_配置.启动怪物时间限制 && (怪物节.人秒系数 != 0 || 怪物节.出没秒数 != 0 || 怪物节.开服系秒 != 0 || 怪物节.杀数系秒 != 0))
            {
                int num4 = activePlayerCount * 怪物节.人秒系数;
                num4 += 怪物节.出没秒数;
                num4 += num2 * 怪物节.开服系秒;
                num4 += (int)lNKC * 怪物节.杀数系秒;
                if (num4 < 1)
                {
                    args.Handled = true;
                    Main.npc[args.NpcId].active = false;
                    Console.WriteLine(Main.npc[args.NpcId].FullName + "定义时间过小被阻止生成");
                    return;
                }
                maxtime = num4;
            }

            //  NPC属性配置
            if (怪物节.初始属性玩家系数 > 0 || 怪物节.初始属性强化系数 > 0f)
            {
                NPCSpawnParams val2 = new NPCSpawnParams();
                if (怪物节.初始属性玩家系数 > 0)
                {
                    val2.playerCountForMultiplayerDifficultyOverride = 怪物节.初始属性玩家系数;
                    if (val2.playerCountForMultiplayerDifficultyOverride > 1000)
                    {
                        val2.playerCountForMultiplayerDifficultyOverride = 1000;
                    }
                }
                if (怪物节.初始属性强化系数 > 0f)
                {
                    val2.strengthMultiplierOverride = 怪物节.初始属性强化系数;
                    if (val2.strengthMultiplierOverride > 1000f)
                    {
                        val2.strengthMultiplierOverride = 1000f;
                    }
                }
                Main.npc[args.NpcId].SetDefaults(Main.npc[args.NpcId].netID, val2);
                lifeMax = Main.npc[args.NpcId].lifeMax;
                strengthMultiplier = Main.npc[args.NpcId].strengthMultiplier;
            }
            if (怪物节.初始属性对怪物伤害修正 == 1f)
            {
                NPC obj = Main.npc[args.NpcId];
                obj.takenDamageMultiplier *= 怪物节.初始属性对怪物伤害修正;
            }
            if (怪物节.设为老怪 == -1)
            {
                Main.npc[args.NpcId].boss = false;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (怪物节.设为老怪 == 1)
            {
                Main.npc[args.NpcId].boss = true;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (怪物节.免疫熔岩 == -1)
            {
                Main.npc[args.NpcId].lavaImmune = false;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (怪物节.免疫熔岩 == 1)
            {
                Main.npc[args.NpcId].lavaImmune = true;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (怪物节.免疫陷阱 == -1)
            {
                Main.npc[args.NpcId].trapImmune = false;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (怪物节.免疫陷阱 == 1)
            {
                Main.npc[args.NpcId].trapImmune = true;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (怪物节.能够穿墙 == -1)
            {
                Main.npc[args.NpcId].noTileCollide = false;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (怪物节.能够穿墙 == 1)
            {
                Main.npc[args.NpcId].noTileCollide = true;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (怪物节.无视重力 == -1)
            {
                Main.npc[args.NpcId].noGravity = false;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (怪物节.无视重力 == 1)
            {
                Main.npc[args.NpcId].noGravity = true;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (怪物节.怪物无敌 == -1)
            {
                Main.npc[args.NpcId].immortal = false;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (怪物节.怪物无敌 == 1)
            {
                Main.npc[args.NpcId].immortal = true;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (怪物节.智慧机制 >= 0 && 怪物节.智慧机制 != 27)
            {
                Main.npc[args.NpcId].aiStyle = 怪物节.智慧机制;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (怪物节.修改防御)
            {
                Main.npc[args.NpcId].defDefense = 怪物节.怪物防御;
                Main.npc[args.NpcId].defense = 怪物节.怪物防御;
                Main.npc[args.NpcId].netUpdate = true;
            }

            // 血量动态调整
            if (怪物节.玩家系数 != 0 || 怪物节.怪物血量 != 0 || 怪物节.开服系数 != 0 || 怪物节.杀数系数 != 0)
            {
                int num5 = activePlayerCount * 怪物节.玩家系数;
                num5 += 怪物节.怪物血量;
                num5 += num2 * 怪物节.开服系数;
                num5 += (int)lNKC * 怪物节.杀数系数;
                if (!怪物节.覆盖原血量)
                {
                    num5 += Main.npc[args.NpcId].lifeMax;
                }
                if (num5 < 1)
                {
                    num5 = 1;
                }
                if (Main.npc[args.NpcId].lifeMax < num5 || !怪物节.不低于正常)
                {
                    Main.npc[args.NpcId].lifeMax = num5;
                    flag = true;
                }
            }

            // 强化系数动态调整
            if (怪物节.玩家强化系数 != 0f || 怪物节.强化系数 != 0f || 怪物节.开服强化系数 != 0f || 怪物节.杀数强化系数 != 0f)
            {
                float num6 = (float)activePlayerCount * 怪物节.玩家强化系数;
                num6 += 怪物节.强化系数;
                num6 += (float)num2 * 怪物节.开服强化系数;
                num6 += (float)(int)lNKC * 怪物节.杀数强化系数;
                if (!怪物节.覆盖原强化)
                {
                    num6 += Main.npc[args.NpcId].strengthMultiplier;
                }
                if (num6 > 1000f)
                {
                    num6 = 1000f;
                }
                if (num6 < 0f)
                {
                    num6 = 0f;
                }
                if ((!(Main.npc[args.NpcId].strengthMultiplier >= num6) || !怪物节.不小于正常) && num6 > 0f)
                {
                    Main.npc[args.NpcId].strengthMultiplier = num6;
                    flag = true;
                }
            }

            // 设置自定义名称
            if (怪物节.自定缀称 != "")
            {
                Main.npc[args.NpcId]._givenName = 怪物节.自定缀称;
                Main.npc[args.NpcId].netUpdate = true;
            }
            config = 怪物节;
            break;
        }

        // 应用统一设置（如果NPC不在例外列表中）
        if (!_配置.统一设置例外怪物.Contains(Main.npc[args.NpcId].netID))
        {
            // 统一血量倍数
            if (_配置.统一怪物血量倍数 != 1.0 && _配置.统一怪物血量倍数 > 0.0)
            {
                Main.npc[args.NpcId].lifeMax = (int)((double)Main.npc[args.NpcId].lifeMax * _配置.统一怪物血量倍数);
                if (Main.npc[args.NpcId].lifeMax < 1)
                {
                    Main.npc[args.NpcId].lifeMax = 1;
                }
                flag = true;
            }

            // 统一血量保护
            if (Main.npc[args.NpcId].lifeMax < lifeMax && _配置.统一血量不低于正常)
            {
                Main.npc[args.NpcId].lifeMax = lifeMax;
                flag = false;
            }

            // 统一强化倍数
            if (_配置.统一怪物强化倍数 != 1.0 && _配置.统一怪物强化倍数 > 0.0)
            {
                float num7 = (float)((double)Main.npc[args.NpcId].strengthMultiplier * _配置.统一怪物强化倍数);
                if (num7 > 1000f)
                {
                    num7 = 1000f;
                }
                if (num7 > 0f)
                {
                    Main.npc[args.NpcId].strengthMultiplier = num7;
                    flag = false;
                }
            }

            // 统一强化保护
            if (Main.npc[args.NpcId].strengthMultiplier < strengthMultiplier && _配置.统一强化不低于正常)
            {
                Main.npc[args.NpcId].strengthMultiplier = strengthMultiplier;
                flag = false;
            }

            // 统一免疫设置
            if (_配置.统一免疫熔岩类型 == 1)
            {
                Main.npc[args.NpcId].lavaImmune = true;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (_配置.统一免疫熔岩类型 == -1)
            {
                Main.npc[args.NpcId].lavaImmune = false;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (_配置.统一免疫陷阱类型 == 1)
            {
                Main.npc[args.NpcId].trapImmune = true;
                Main.npc[args.NpcId].netUpdate = true;
            }
            if (_配置.统一免疫陷阱类型 == -1)
            {
                Main.npc[args.NpcId].trapImmune = false;
                Main.npc[args.NpcId].netUpdate = true;
            }

            // 统一伤害修正
            if (_配置.统一对怪物伤害修正 != 1f)
            {
                NPC obj2 = Main.npc[args.NpcId];
                obj2.takenDamageMultiplier *= _配置.统一对怪物伤害修正;
            }
        }

        // 更新当前血量
        if (flag)
        {
            Main.npc[args.NpcId].life = Main.npc[args.NpcId].lifeMax;
            Main.npc[args.NpcId].netUpdate = true;
        }

        // 初始化自定义NPC数据
        lock (LNpcs)
        {
            LNpcs[args.NpcId] = new LNPC(args.NpcId, activePlayerCount, lifeMax, config!, maxtime, num2, lNKC);
        }
    }
    #endregion

    #region 传送数据包拦截处理
    /// <summary>
    /// 拦截并处理传送相关的网络数据包
    /// </summary>
    /// <param name="args">网络数据发送事件参数</param>
    /// <remarks>
    /// 此方法用于拦截特定的传送数据包（MsgId = 65），
    /// 当存在阻止传送的NPC时，阻止该数据包的发送
    /// </remarks>
    private void SendData(SendDataEventArgs args)
    {
        // 检查是否需要处理此数据包：
        // 1. 数据包已被其他插件处理
        // 2. 不是目标消息类型（65）
        // 3. 特定的数字参数不为0
        if (args.Handled || (int)args.MsgId != 65 || args.number5 != 0 || args.number != 0)
        {
            return;
        }

        bool flag = false; // 标记是否发现阻止传送的NPC

        // 遍历所有NPC
        NPC[] npc = Main.npc;
        foreach (NPC val in npc)
        {
            // 跳过无效或非活跃的NPC
            if (val == null || !val.active)
            {
                continue;
            }

            // 使用锁确保线程安全地访问NPC数据
            lock (LNpcs)
            {
                LNPC lNPC = LNpcs[val.whoAmI];

                // 检查NPC是否配置了阻止传送
                if (lNPC == null || lNPC.Config == null || lNPC.BlockTeleporter != 1)
                {
                    continue;
                }

                // 发现阻止传送的NPC，设置标记并退出循环
                flag = true;
                break;
            }
        }

        // 如果发现阻止传送的NPC，拦截此数据包
        if (flag)
        {
            args.Handled = true;
        }
    }
    #endregion

    #region NPC受击事件处理
    /// <summary>
    /// 处理NPC受到攻击的事件
    /// </summary>
    /// <param name="args">NPC受击事件参数</param>
    /// <remarks>
    /// 此方法在NPC受到攻击时被调用，用于记录NPC被击中的次数
    /// 主要用于统计和触发基于受击次数的特殊效果
    /// </remarks>
    private void NpcStrike(NpcStrikeEventArgs args)
    {
        // 检查事件是否应该被处理：
        // 1. 事件已被其他插件处理
        // 2. 伤害值为负数（治疗效果）
        // 3. NPC网络ID为0（无效NPC）
        // 4. NPC不处于活跃状态
        if (args.Handled || args.Damage < 0 || args.Npc.netID == 0 || !args.Npc.active)
        {
            return;
        }

        // 使用锁确保线程安全地访问NPC数据
        lock (LNpcs)
        {
            // 检查NPC是否存在于自定义管理系统中且有有效配置
            if (LNpcs[args.Npc.whoAmI] != null && LNpcs[args.Npc.whoAmI].Config != null)
            {
                // 增加该NPC的受击计数
                LNpcs[args.Npc.whoAmI].Struck++;
            }
        }
    }

    #endregion

    #region NPC死亡事件处理
    /// <summary>
    /// 处理NPC死亡时的复杂逻辑
    /// </summary>
    /// <param name="args">NPC死亡事件参数</param>
    /// <remarks>
    /// 这是NPC死亡事件的核心处理方法，负责处理：
    /// - 死亡广播和音效
    /// - 死亡命令执行
    /// - 复杂掉落系统
    /// - 死亡触发效果
    /// - 数据清理和统计
    /// </remarks>
    private void NpcKilled(NpcKilledEventArgs args)
    {
        // 跳过无效NPC
        if (args.npc.netID == 0)
        {
            return;
        }

        // 使用锁确保线程安全地访问NPC数据
        lock (LNpcs)
        {
            // 检查NPC是否存在于自定义管理系统中
            if (LNpcs[args.npc.whoAmI] != null)
            {
                // 检查NPC是否有有效配置
                if (LNpcs[args.npc.whoAmI].Config != null)
                {
                    // ===== 死亡信息广播 =====
                    if (!LNpcs[args.npc.whoAmI].Config.不宣读信息)
                    {
                        TShock.Utils.Broadcast("攻略成功: " + args.npc.FullName + " 已被击败.", Convert.ToByte(130), Convert.ToByte(50), Convert.ToByte(230));
                    }

                    // ===== 死亡音效播放 =====
                    foreach (声音节 item in LNpcs[args.npc.whoAmI].Config.死亡放音)
                    {
                        if (item.声音ID >= 0)
                        {
                            NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(args.npc.Center, (ushort)1, item.声音ID, item.声音规模, item.高音补偿), -1, -1);
                        }
                    }

                    // ===== 死亡命令执行 =====
                    foreach (string item2 in LNpcs[args.npc.whoAmI].Config.死亡命令)
                    {
                        Commands.HandleCommand(TSPlayer.Server, TShock.Config.Settings.CommandSilentSpecifier + LNpcs[args.npc.whoAmI].ReplaceMarkers(item2));
                    }

                    // ===== 死亡喊话 =====
                    if (LNpcs[args.npc.whoAmI].Config.死亡喊话 != "")
                    {
                        TShock.Utils.Broadcast((LNpcs[args.npc.whoAmI].Config.死亡喊话无头 ? "" : (args.npc.FullName + ": ")) + LNpcs[args.npc.whoAmI].Config.死亡喊话, Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255));
                    }

                    // ===== 复杂掉落系统 =====
                    long lNKC = getLNKC(args.npc.netID); // 获取该类型NPC的总击杀数
                    int num = LNpcs[args.npc.whoAmI].Config.掉落组限; // 掉落组数量限制
                    Random rd = new Random(); // 随机数生成器

                    // 遍历所有额外掉落配置
                    foreach (掉落节 item3 in LNpcs[args.npc.whoAmI].Config.额外掉落)
                    {
                        // 检查掉落组限制
                        if (num <= 0)
                        {
                            break;
                        }

                        // ===== 复杂条件检查 =====
                        // 难度条件检查
                        if ((item3.难度条件.Length != 0 && !item3.难度条件.Contains(Main.GameMode)) || Sundry.SeedRequirement(item3.种子条件))
                        {
                            continue;
                        }

                        // 杀数条件检查
                        if (item3.杀数条件 != 0)
                        {
                            if (item3.杀数条件 > 0)
                            {
                                if (lNKC < item3.杀数条件) continue;
                            }
                            else if (lNKC >= Math.Abs(item3.杀数条件)) continue;
                        }

                        // 被击条件检查
                        if (item3.被击条件 != 0)
                        {
                            if (item3.被击条件 > 0)
                            {
                                if (LNpcs[args.npc.whoAmI].Struck < item3.被击条件) continue;
                            }
                            else if (LNpcs[args.npc.whoAmI].Struck >= Math.Abs(item3.被击条件)) continue;
                        }

                        // 人数条件检查
                        if (item3.人数条件 != 0)
                        {
                            if (item3.人数条件 > 0)
                            {
                                if (LNpcs[args.npc.whoAmI].PlayerCount < item3.人数条件) continue;
                            }
                            else if (LNpcs[args.npc.whoAmI].PlayerCount >= Math.Abs(item3.人数条件)) continue;
                        }

                        // 耗时条件检查
                        if (item3.耗时条件 != 0)
                        {
                            if (item3.耗时条件 > 0)
                            {
                                if (LNpcs[args.npc.whoAmI].TiemN < (float)item3.耗时条件) continue;
                            }
                            else if (LNpcs[args.npc.whoAmI].TiemN >= (float)Math.Abs(item3.耗时条件)) continue;
                        }

                        // 杀死条件检查
                        if (item3.杀死条件 != 0)
                        {
                            if (item3.杀死条件 > 0)
                            {
                                if (LNpcs[args.npc.whoAmI].KillPlay < item3.杀死条件) continue;
                            }
                            else if (LNpcs[args.npc.whoAmI].KillPlay >= Math.Abs(item3.杀死条件)) continue;
                        }

                        // 开服条件检查
                        if (item3.开服条件 != 0)
                        {
                            if (item3.开服条件 > 0)
                            {
                                if (LNpcs[args.npc.whoAmI].OSTime < item3.开服条件) continue;
                            }
                            else if (LNpcs[args.npc.whoAmI].OSTime >= Math.Abs(item3.开服条件)) continue;
                        }

                        // 环境条件检查（昼夜、血月、日食等）
                        if ((item3.昼夜条件 == -1 && Main.dayTime) ||
                            (item3.昼夜条件 == 1 && !Main.dayTime) ||
                            (item3.血月条件 == -1 && Main.bloodMoon) ||
                            (item3.血月条件 == 1 && !Main.bloodMoon) ||
                            (item3.日食条件 == -1 && Main.eclipse) ||
                            (item3.日食条件 == 1 && !Main.eclipse) ||
                            (item3.降雨条件 == -1 && Main.raining) ||
                            (item3.降雨条件 == 1 && !Main.raining) ||
                            (item3.肉山条件 == -1 && Main.hardMode) ||
                            (item3.肉山条件 == 1 && !Main.hardMode) ||
                            (item3.巨人条件 == -1 && NPC.downedGolemBoss) ||
                            (item3.巨人条件 == 1 && !NPC.downedGolemBoss) ||
                            (item3.月总条件 == -1 && NPC.downedMoonlord) ||
                            (item3.月总条件 == 1 && !NPC.downedMoonlord) ||
                            item3.掉落率子 <= 0 ||
                            item3.掉落率母 <= 0 ||
                            (item3.掉落率子 < item3.掉落率母 && rd.Next(1, item3.掉落率母 + 1) > item3.掉落率子) ||
                            Sundry.NPCKillRequirement(item3.杀怪条件) ||
                            !LNpcs[args.npc.whoAmI].haveMarkers(item3.指示物条件, args.npc) ||
                            Sundry.AIRequirement(item3.AI条件, args.npc) ||
                            Sundry.MonsterRequirement(item3.怪物条件, args.npc))
                        {
                            continue;
                        }

                        // ===== 物品掉落执行 =====
                        foreach (物品节 item4 in item3.掉落物品)
                        {
                            // 计算实际掉落数量（基础数量 + 指示物注入）
                            int num2 = item4.物品数量 + LNpcs[args.npc.whoAmI].getMarkers(item4.指示物数量注入物品数量名);
                            if (num2 <= 0 || item4.物品ID <= 0)
                            {
                                continue;
                            }

                            // 根据前缀和独立掉落设置选择掉落方式
                            if (item4.物品前缀 >= 0)
                            {
                                if (item4.独立掉落)
                                {
                                    args.npc.DropItemInstanced(args.npc.Center, args.npc.Size, item4.物品ID, num2, true);
                                    continue;
                                }
                                int num3 = Item.NewItem((IEntitySource)new EntitySource_DebugCommand(), args.npc.Center, args.npc.Size, item4.物品ID, num2, false, item4.物品前缀, false, false);
                                NetMessage.TrySendData(21, -1, -1, null, num3, 0f, 0f, 0f, 0, 0, 0);
                            }
                            else if (item4.独立掉落)
                            {
                                args.npc.DropItemInstanced(args.npc.Center, args.npc.Size, item4.物品ID, num2, true);
                            }
                            else
                            {
                                int num4 = Item.NewItem((IEntitySource)new EntitySource_DebugCommand(), args.npc.Center, args.npc.Size, item4.物品ID, num2, false, 0, false, false);
                                NetMessage.TrySendData(21, -1, -1, null, num4, 0f, 0f, 0f, 0, 0, 0);
                            }
                        }

                        // ===== 掉落喊话 =====
                        if (item3.喊话 != "")
                        {
                            TShock.Utils.Broadcast((item3.喊话无头 ? "" : (args.npc.FullName + ": ")) + item3.喊话, Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255));
                        }

                        // ===== 跳出掉落组 =====
                        if (item3.跳出掉落)
                        {
                            break;
                        }

                        // 减少可用掉落组数量
                        num--;
                    }

                    // ===== 死亡触发效果 =====
                    // 设置怪物指示物
                    Sundry.SetMonsterMarkers(LNpcs[args.npc.whoAmI].Config.死亡怪物指示物修改, args.npc, ref rd);

                    // 死亡伤怪效果
                    Sundry.HurtMonster(LNpcs[args.npc.whoAmI].Config.死亡伤怪, args.npc);

                    // 死亡弹幕发射
                    Sundry.LaunchProjectile(LNpcs[args.npc.whoAmI].Config.死亡弹幕, args.npc, LNpcs[args.npc.whoAmI]);

                    // 死亡弹幕更新
                    Sundry.updataProjectile(LNpcs[args.npc.whoAmI].Config.死亡更新弹幕, args.npc, LNpcs[args.npc.whoAmI]);

                    // ===== 遗言怪物召唤 =====
                    foreach (KeyValuePair<int, int> item5 in LNpcs[args.npc.whoAmI].Config.遗言怪物)
                    {
                        if (item5.Value >= 1 && item5.Key != 0)
                        {
                            int num5 = Math.Min(item5.Value, 200); // 限制最大召唤数量
                            NPC nPCById = TShock.Utils.GetNPCById(item5.Key);
                            if (nPCById != null && nPCById.netID != 488 && nPCById.netID != 113 && nPCById.netID != 0 && nPCById.type < NPCID.Count)
                            {
                                TSPlayer.Server.SpawnNPC(nPCById.netID, nPCById.FullName, num5, Terraria.Utils.ToTileCoordinates(args.npc.Center).X, Terraria.Utils.ToTileCoordinates(args.npc.Center).Y, 3, 3);
                            }
                        }
                    }

                    // ===== 死亡状态效果 =====
                    Sundry.regionBuff(args.npc.Center, LNpcs[args.npc.whoAmI].Config.死状范围, LNpcs[args.npc.whoAmI].Config.死亡状态);
                }

                // ===== 数据清理 =====
                // 清空该NPC的自定义数据
                LNpcs[args.npc.whoAmI] = null;
            }
        }

        // ===== 击杀统计 =====
        // 增加该类型NPC的总击杀数
        addLNKC(args.npc.netID);
    }

    #endregion

    #region 定时更新处理方法（自怪核心方法）
    public static bool ULock = false;
    /// <summary>
    /// 定时更新处理方法 - 负责管理自定义NPC的完整生命周期和战斗逻辑
    /// </summary>
    /// <remarks>
    /// 主要功能：
    /// 1. 动态难度调整 - 根据玩家数量、开服时间、击杀数实时调整BOSS血量和时间限制
    /// 2. 事件触发系统 - 基于时间、血量比例等条件触发复杂事件链
    /// 3. NPC行为管理 - 处理出场、战斗、撤退等完整生命周期
    /// 4. 玩家交互系统 - 管理击退、伤害、状态效果、传送等玩家交互
    /// 5. 全局系统控制 - 调整刷怪设置、复活时间等服务器配置
    /// 6. 队友视角功能 - 死亡玩家自动传送到活着的队友位置
    /// 
    /// 执行流程：
    /// 加锁保护 → 数据统计 → NPC排序处理 → 动态调整 → 事件触发 → 效果应用 → 系统更新
    /// </remarks>
    public void OnUpdate(object sender, ElapsedEventArgs e)
    {
        // 检查插件是否启用
        if (!_配置.启用插件) return;

        // 使用锁机制防止重复执行
        if (ULock)
        {
            return;
        }
        ULock = true;
        DateTime now = DateTime.Now;
        Random rd = new Random();

        // 初始化随机数生成器
        if (Main.rand == null)
        {
            Main.rand = new UnifiedRandom();
        }
        try
        {
            // 获取活跃玩家数量
            int activePlayerCount = TShock.Utils.GetActivePlayerCount();

            // 统计活跃NPC数量
            int num = 0;
            NPC[] npc = Main.npc;
            foreach (NPC val in npc)
            {
                if (val != null && val.active)
                {
                    num++;
                }
            }
            bool flag = false;
            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            Dictionary<int, int> dictionary2 = dictionary;

            // 遍历所有NPC，收集BOSS信息和事件权重
            NPC[] npc2 = Main.npc;
            foreach (NPC val2 in npc2)
            {
                if (activePlayerCount <= 0 || val2 == null || !val2.active)
                {
                    continue;
                }
                if (val2.boss)
                {
                    flag = true;// 标记存在BOSS
                }

                // 从自定义NPC数据中获取事件权重
                lock (LNpcs)
                {
                    LNPC lNPC = LNpcs[val2.whoAmI];
                    if (lNPC != null && lNPC.Config != null)
                    {
                        dictionary2.Add(val2.whoAmI, lNPC.Config.事件权重);
                    }
                }
            }

            // 按事件权重降序排序NPC
            IOrderedEnumerable<KeyValuePair<int, int>> orderedEnumerable = dictionary2.OrderByDescending(delegate (KeyValuePair<int, int> objDic)
            {
                KeyValuePair<int, int> keyValuePair = objDic;
                return keyValuePair.Value;
            });

            // 处理每个NPC的自定义行为
            foreach (KeyValuePair<int, int> item in orderedEnumerable)
            {
                if (activePlayerCount <= 0)
                {
                    continue;
                }
                NPC val3 = Main.npc[item.Key];
                if (val3 == null || !val3.active)
                {
                    continue;
                }


                lock (LNpcs)
                {
                    LNPC lNPC2 = LNpcs[val3.whoAmI];
                    if (lNPC2 == null || lNPC2.Config == null)
                    {
                        continue;
                    }

                    // 动态调整时间限制（基于玩家数量、开服时间、击杀数等）
                    if (lNPC2.PlayerCount < activePlayerCount)
                    {
                        lNPC2.PlayerCount = activePlayerCount;
                        if (_配置.启动动态时间限制 && (lNPC2.Config.人秒系数 != 0 || lNPC2.Config.出没秒数 != 0 || lNPC2.Config.开服系秒 != 0 || lNPC2.Config.杀数系秒 != 0))
                        {
                            // 计算基于各种系数的时间限制
                            int num4 = lNPC2.PlayerCount * lNPC2.Config.人秒系数;
                            num4 += lNPC2.Config.出没秒数;
                            int num5 = lNPC2.PlayerCount * lNPC2.Config.人秒系数;
                            num5 += lNPC2.Config.出没秒数;
                            num5 += lNPC2.OSTime * lNPC2.Config.开服系秒;
                            num5 += (int)lNPC2.LKC * lNPC2.Config.杀数系秒;

                            if (num5 < 1) num5 = -1;

                            // 更新时间限制并广播通知
                            if (lNPC2.MaxTime != num5)
                            {
                                lNPC2.MaxTime = num5;
                                int value = num4 - num5;
                                if (num4 > num5)
                                {
                                    if (!lNPC2.Config.不宣读信息)
                                    {
                                        // 广播时间变化信息
                                        TShock.Utils.Broadcast("注意: " + val3.FullName + " 受服务器人数增多影响攻略时间减少 " + value + " 秒剩" + (int)((float)num5 - lNPC2.TiemN) + "秒.", Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(100));
                                    }
                                }
                                else if (num4 < num5 && !lNPC2.Config.不宣读信息)
                                {
                                    TShock.Utils.Broadcast("注意: " + val3.FullName + " 受服务器人数增多影响攻略时间增加 " + Math.Abs(value) + "秒剩" + (int)((float)num5 - lNPC2.TiemN) + "秒.", Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(100));
                                }
                            }
                        }

                        // 动态调整血量上限
                        if (_配置.启动动态血量上限 && (lNPC2.Config.玩家系数 != 0 || lNPC2.Config.怪物血量 != 0 || lNPC2.Config.开服系数 != 0 || lNPC2.Config.杀数系数 != 0))
                        {
                            // 计算基于各种系数的血量
                            int num6 = activePlayerCount * lNPC2.Config.玩家系数;
                            num6 += lNPC2.Config.怪物血量;
                            num6 += lNPC2.OSTime * lNPC2.Config.开服系数;
                            num6 += (int)lNPC2.LKC * lNPC2.Config.杀数系数;
                            if (!lNPC2.Config.覆盖原血量)
                            {
                                num6 += val3.lifeMax;
                            }

                            if (num6 < 1) num6 = 1;

                            // 应用血量调整
                            if (lNPC2.MaxLife < num6 || !lNPC2.Config.不低于正常)
                            {
                                int lifeMax = val3.lifeMax;
                                int num7 = num6;

                                // 应用统一血量倍数设置
                                if (!_配置.统一设置例外怪物.Contains(val3.netID))
                                {
                                    if (_配置.统一怪物血量倍数 != 1.0 && _配置.统一怪物血量倍数 > 0.0)
                                    {
                                        num7 = (int)((double)num7 * _配置.统一怪物血量倍数);
                                        if (num7 < 1)
                                        {
                                            num7 = 1;
                                        }
                                    }
                                    if (_配置.统一血量不低于正常 && num7 < lNPC2.MaxLife)
                                    {
                                        num7 = lNPC2.MaxLife;
                                    }
                                }

                                // 更新NPC血量并广播通知
                                if (lifeMax != num7)
                                {
                                    int life = val3.life;
                                    int num8 = (int)((double)life * ((double)num7 / (double)lifeMax));
                                    if (num8 < 1)
                                    {
                                        num8 = 1;
                                    }
                                    val3.lifeMax = num7;
                                    val3.life = num8;
                                    int value2 = life - num8;
                                    if (life > num8)
                                    {
                                        if (!lNPC2.Config.不宣读信息)
                                        {
                                            TShock.Utils.Broadcast("注意: " + val3.FullName + " 受服务器人数增多影响怪物血量减少 " + value2 + " 剩" + val3.life + ".", Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(100));
                                        }
                                    }
                                    else if (life < num8 && !lNPC2.Config.不宣读信息)
                                    {
                                        TShock.Utils.Broadcast("注意: " + val3.FullName + " 受服务器人数增多影响怪物血量增加 " + Math.Abs(value2) + "剩" + val3.life + ".", Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(100));
                                    }
                                }
                            }
                        }
                    }

                    if (lNPC2.Config == null)
                    {
                        continue;
                    }

                    // NPC出场处理
                    if (lNPC2.TiemN == 0f)
                    {
                        if (lNPC2.MaxTime != 0)
                        {
                            // NPC出场处理
                            if (!lNPC2.Config.不宣读信息)
                            {
                                TShock.Utils.Broadcast("注意: " + val3.FullName + " 现身,攻略时间为 " + lNPC2.MaxTime + " 秒,血量为 " + val3.lifeMax + ",快快加入战斗吧!", Convert.ToByte(130), Convert.ToByte(255), Convert.ToByte(170));
                            }
                        }
                        else if (!lNPC2.Config.不宣读信息)
                        {
                            TShock.Utils.Broadcast("注意: " + val3.FullName + " 现身,血量为 " + val3.lifeMax + ",快快加入战斗吧!", Convert.ToByte(130), Convert.ToByte(255), Convert.ToByte(170));
                        }

                        // 播放出场音效
                        foreach (声音节 item2 in lNPC2.Config.出场放音)
                        {
                            if (item2.声音ID >= 0)
                            {
                                NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(val3.Center, (ushort)1, item2.声音ID, item2.声音规模, item2.高音补偿), -1, -1);
                            }
                        }

                        // 执行出场命令
                        foreach (string item3 in lNPC2.Config.出场命令)
                        {
                            Commands.HandleCommand((TSPlayer)(object)TSPlayer.Server, TShock.Config.Settings.CommandSilentSpecifier + item3);
                        }

                        // 出场喊话
                        if (lNPC2.Config.出场喊话 != "")
                        {
                            TShock.Utils.Broadcast((lNPC2.Config.出场喊话无头 ? "" : (val3.FullName + ": ")) + lNPC2.Config.出场喊话, Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255));
                        }

                        // 设置怪物指示物、伤害怪物、发射弹幕等
                        Sundry.SetMonsterMarkers(lNPC2.Config.出场怪物指示物修改, val3, ref rd);
                        Sundry.HurtMonster(lNPC2.Config.出场伤怪, val3);
                        Sundry.LaunchProjectile(lNPC2.Config.出场弹幕, val3, lNPC2);

                        // 生成随从怪物
                        foreach (KeyValuePair<int, int> item4 in lNPC2.Config.随从怪物)
                        {
                            if (item4.Value >= 1 && item4.Key != 0)
                            {
                                int num9 = Math.Min(item4.Value, 200);
                                NPC nPCById = TShock.Utils.GetNPCById(item4.Key);
                                if (nPCById != null && nPCById.netID != 488 && nPCById.netID != 113 && nPCById.netID != 0 && nPCById.type < NPCID.Count)
                                {
                                    TSPlayer.Server.SpawnNPC(nPCById.netID, nPCById.FullName, num9, Terraria.Utils.ToTileCoordinates(val3.Center).X, Terraria.Utils.ToTileCoordinates(val3.Center).Y, 30, 15);
                                }
                            }
                        }

                        // 初始化状态范围设置
                        lNPC2.BuffR = lNPC2.Config.状态范围;
                        lNPC2.RBuff = lNPC2.Config.周围状态;
                        lNPC2.OBuffR = lNPC2.Config.反状态范围;
                        lNPC2.ORBuff = lNPC2.Config.反周围状态;
                    }

                    // 更新时间计数
                    lNPC2.Time++;
                    lNPC2.TiemN = (float)lNPC2.Time / 100f;

                    // 检查时间限制
                    if (lNPC2.MaxTime == 0)
                    {
                        goto IL_0e4d;
                    }
                    int maxTime = lNPC2.MaxTime;

                    // 在特定时间点广播剩余时间
                    if ((double)lNPC2.TiemN == Math.Round((double)maxTime * 0.5) || (double)lNPC2.TiemN == Math.Round((double)maxTime * 0.7) || (double)lNPC2.TiemN == Math.Round((double)maxTime * 0.9))
                    {
                        maxTime -= (int)lNPC2.TiemN;
                        if (!lNPC2.Config.不宣读信息)
                        {
                            TShock.Utils.Broadcast("注意: " + val3.FullName + " 剩余攻略时间 " + maxTime + " 秒.", Convert.ToByte(130), Convert.ToByte(255), Convert.ToByte(170));
                        }
                        goto IL_0e4d;
                    }

                    // 超时处理 - NPC撤离
                    if (!(lNPC2.TiemN >= (float)maxTime))
                    {
                        goto IL_0e4d;
                    }

                    if (!lNPC2.Config.不宣读信息)
                    {
                        TShock.Utils.Broadcast("攻略失败: " + val3.FullName + " 已撤离.", Convert.ToByte(190), Convert.ToByte(150), Convert.ToByte(150));
                    }

                    int whoAmI = val3.whoAmI;
                    Main.npc[whoAmI] = new NPC();
                    NetMessage.SendData(23, -1, -1, NetworkText.Empty, whoAmI, 0f, 0f, 0f, 0, 0, 0);
                    goto end_IL_0211;

                // 标签：继续处理NPC事件
                IL_0e4d:
                    int num10 = 0;           // 击退范围
                    float num11 = 0f;        // 击退力度
                    bool s = false;          // 柔和击退标志
                    int num12 = 0;           // 杀伤范围
                    int num13 = 0;           // 杀伤伤害
                    bool flag2 = false;      // 直接杀伤标志
                    int num14 = 0;           // 拉取起始范围
                    int num15 = 0;           // 拉取范围
                    int num16 = 0;           // 拉取止点
                    float num17 = 0f;        // 拉取点X轴偏移
                    float num18 = 0f;        // 拉取点Y轴偏移
                    bool flag3 = false;      // 初始拉取点坐标为零标志
                    bool flag4 = false;      // 拉取范围为矩形标志
                    int num19 = 0;           // 清弹起始范围
                    int num20 = 0;           // 清弹结束范围
                    int num21 = 0;           // 反射范围
                    long lNKC = getLNKC(val3.netID);  // 获取该类型NPC的击杀数
                    bool flag5 = false;      // 直接撤退标志
                    int num22 = 0;           // 跳过事件计数
                    int num23 = 0;           // 当前血量百分比s

                    // 计算NPC当前血量百分比
                    if (val3.lifeMax > 0)
                    {
                        num23 = val3.life * 100 / val3.lifeMax;
                    }

                    // 时间触发事件处理
                    if (lNPC2.TiemN != lNPC2.LTime)
                    {
                        int 时事件限 = lNPC2.Config.时事件限;  // 时间事件处理限制

                        // 遍历所有时间触发事件
                        foreach (时间节 item5 in lNPC2.CTime)
                        {
                            if (时事件限 <= 0) break;  // 达到处理限制，跳出循环

                            // 跳过事件计数处理
                            if (num22 > 0)
                            {
                                num22--;
                            }
                            else
                            {
                                // 检查事件是否可触发
                                if (item5.可触发次 <= 0 && item5.可触发次 != -1)
                                {
                                    continue;
                                }

                                // 计算延迟时间和消耗时间
                                int num24 = (int)(item5.延迟秒数 * 100f);
                                int num25 = (int)(item5.消耗时间 * 100f);
                                if (num25 <= 0)
                                {
                                    continue;
                                }

                                // 检查时间触发条件
                                if (item5.循环执行)
                                {
                                    // 循环执行：检查是否在循环时间点上
                                    if ((lNPC2.Time - num24) % num25 != 0)
                                    {
                                        continue;
                                    }
                                }
                                // 单次执行：检查是否精确匹配时间
                                else if (num25 != lNPC2.Time - num24)
                                {
                                    continue;
                                }

                                // ========== 复杂条件检查 ==========
                                // 检查难度条件
                                if ((item5.难度条件.Length != 0 && !item5.难度条件.Contains(Main.GameMode)) || Sundry.SeedRequirement(item5.种子条件))
                                {
                                    continue;
                                }

                                if (item5.杀数条件 != 0)
                                {
                                    if (item5.杀数条件 > 0)
                                    {
                                        if (lNKC < item5.杀数条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNKC >= Math.Abs(item5.杀数条件))
                                    {
                                        continue;
                                    }
                                }

                                if (item5.怪数条件 != 0)
                                {
                                    if (item5.怪数条件 > 0)
                                    {
                                        if (num < item5.怪数条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (num >= Math.Abs(item5.怪数条件))
                                    {
                                        continue;
                                    }
                                }

                                if (item5.血比条件 != 0)
                                {
                                    if (item5.血比条件 > 0)
                                    {
                                        if (num23 < item5.血比条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (num23 >= Math.Abs(item5.血比条件))
                                    {
                                        continue;
                                    }
                                }

                                if (item5.血量条件 != 0)
                                {
                                    if (item5.血量条件 > 0)
                                    {
                                        if (val3.life < item5.血量条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (val3.life >= Math.Abs(item5.血量条件))
                                    {
                                        continue;
                                    }
                                }

                                if (item5.被击条件 != 0)
                                {
                                    if (item5.被击条件 > 0)
                                    {
                                        if (lNPC2.Struck < item5.被击条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNPC2.Struck >= Math.Abs(item5.被击条件))
                                    {
                                        continue;
                                    }
                                }

                                if (item5.人数条件 != 0)
                                {
                                    if (item5.人数条件 > 0)
                                    {
                                        if (lNPC2.PlayerCount < item5.人数条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNPC2.PlayerCount >= Math.Abs(item5.人数条件))
                                    {
                                        continue;
                                    }
                                }

                                if (item5.耗时条件 != 0)
                                {
                                    if (item5.耗时条件 > 0)
                                    {
                                        if (lNPC2.TiemN < (float)item5.耗时条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNPC2.TiemN >= (float)Math.Abs(item5.耗时条件))
                                    {
                                        continue;
                                    }
                                }


                                if (item5.ID条件 != 0 && item5.ID条件 != val3.netID)
                                {
                                    continue;
                                }

                                if (item5.杀死条件 != 0)
                                {
                                    if (item5.杀死条件 > 0)
                                    {
                                        if (lNPC2.KillPlay < item5.杀死条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNPC2.KillPlay >= Math.Abs(item5.杀死条件))
                                    {
                                        continue;
                                    }
                                }

                                // 检查开服时间条件
                                if (item5.开服条件 != 0)
                                {
                                    if (item5.开服条件 > 0)
                                    {
                                        if (lNPC2.OSTime < item5.开服条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNPC2.OSTime >= Math.Abs(item5.开服条件))
                                    {
                                        continue;
                                    }
                                }

                                // 检查游戏状态条件（昼夜、血月、日食、降雨、肉山后、巨人后、月总后等）
                                if ((item5.昼夜条件 == -1 && Main.dayTime) || (item5.昼夜条件 == 1 && !Main.dayTime) || (item5.血月条件 == -1 && Main.bloodMoon) || (item5.血月条件 == 1 && !Main.bloodMoon) || (item5.日食条件 == -1 && Main.eclipse) || (item5.日食条件 == 1 && !Main.eclipse) || (item5.降雨条件 == -1 && Main.raining) || (item5.降雨条件 == 1 && !Main.raining) || (item5.肉山条件 == -1 && Main.hardMode) || (item5.肉山条件 == 1 && !Main.hardMode) || (item5.巨人条件 == -1 && NPC.downedGolemBoss) || (item5.巨人条件 == 1 && !NPC.downedGolemBoss) || (item5.月总条件 == -1 && NPC.downedMoonlord) || (item5.月总条件 == 1 && !NPC.downedMoonlord))
                                {
                                    continue;
                                }

                                // 检查X轴位置条件
                                if (item5.X轴条件 != 0)
                                {
                                    if (item5.X轴条件 > 0)
                                    {
                                        if (val3.Center.X < (float)(item5.X轴条件 << 4))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (val3.Center.X >= (float)Math.Abs(item5.X轴条件 << 4))
                                    {
                                        continue;
                                    }
                                }

                                // 检查Y轴位置条件
                                if (item5.Y轴条件 != 0)
                                {
                                    if (item5.Y轴条件 > 0)
                                    {
                                        if (val3.Center.Y < (float)(item5.Y轴条件 << 4))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (val3.Center.Y >= (float)Math.Abs(item5.Y轴条件 << 4))
                                    {
                                        continue;
                                    }
                                }

                                // 检查面向方向条件（8个方向+4个单向）
                                if ((item5.面向条件 == 1 && (val3.direction != 1 || val3.directionY != 0)) || (item5.面向条件 == 2 && (val3.direction != 1 || val3.directionY != 1)) || (item5.面向条件 == 3 && (val3.direction != 0 || val3.directionY != 1)) || (item5.面向条件 == 4 && (val3.direction != -1 || val3.directionY != 1)) || (item5.面向条件 == 5 && (val3.direction != -1 || val3.directionY != 0)) || (item5.面向条件 == 6 && (val3.direction != -1 || val3.directionY != -1)) || (item5.面向条件 == 7 && (val3.direction != 0 || val3.directionY != -1)) || (item5.面向条件 == 8 && (val3.direction != 1 || val3.directionY != -1)) || (item5.面向条件 == 9 && val3.direction != 1) || (item5.面向条件 == 10 && val3.directionY != 1) || (item5.面向条件 == 11 && val3.direction != -1) || (item5.面向条件 == 12 && val3.directionY != -1) || item5.触发率子 <= 0 || item5.触发率母 <= 0 || (item5.触发率子 < item5.触发率母 && rd.Next(1, item5.触发率母 + 1) > item5.触发率子) || Sundry.NPCKillRequirement(item5.杀怪条件) || !lNPC2.haveMarkers(item5.指示物条件, val3) || Sundry.AIRequirement(item5.AI条件, val3) || Sundry.MonsterRequirement(item5.怪物条件, val3) || Sundry.PlayerRequirement(item5.玩家条件, val3) || Sundry.ProjectileRequirement(item5.弹幕条件, val3))
                                {
                                    continue;
                                }


                                // ========== 执行事件动作 ==========

                                // 更新指示物
                                foreach (指示物节 item6 in item5.指示物修改)
                                {
                                    lNPC2.setMarkers(item6.名称, item6.数量, item6.清除, item6.指示物注入数量名, item6.指示物注入数量系数, item6.指示物注入数量运算符, item6.随机小, item6.随机大, ref rd, val3);
                                }
                                Sundry.SetMonsterMarkers(item5.怪物指示物修改, val3, ref rd);

                                // 修改NPC物理属性
                                if (item5.能够穿墙 == -1)
                                {
                                    val3.noTileCollide = false;
                                    val3.netUpdate = true;
                                }

                                if (item5.能够穿墙 == 1)
                                {
                                    val3.noTileCollide = true;
                                    val3.netUpdate = true;
                                }

                                if (item5.无视重力 == -1)
                                {
                                    val3.noGravity = false;
                                    val3.netUpdate = true;
                                }

                                if (item5.无视重力 == 1)
                                {
                                    val3.noGravity = true;
                                    val3.netUpdate = true;
                                }

                                if (item5.怪物无敌 == -1)
                                {
                                    val3.immortal = false;
                                    val3.netUpdate = true;
                                }

                                if (item5.怪物无敌 == 1)
                                {
                                    val3.immortal = true;
                                    val3.netUpdate = true;
                                }

                                if (item5.速度乘数 != 1f)
                                {
                                    NPC val4 = val3;
                                    val4.velocity = val4.velocity * item5.速度乘数;
                                    val3.netUpdate = true;
                                }

                                if (item5.切换智慧 >= 0 && item5.切换智慧 != 27)
                                {
                                    val3.aiStyle = item5.切换智慧;
                                    val3.netUpdate = true;
                                }

                                if (item5.修改防御)
                                {
                                    val3.defDefense = item5.怪物防御;
                                    val3.defense = item5.怪物防御;
                                    val3.netUpdate = true;
                                }

                                if (item5.AI赋值.Count > 0)
                                {
                                    for (int k = 0; k < val3.ai.Count(); k++)
                                    {
                                        if (item5.AI赋值.ContainsKey(k) && item5.AI赋值.TryGetValue(k, out var value3))
                                        {
                                            val3.ai[k] = value3;
                                            val3.netUpdate = true;
                                        }
                                    }
                                }

                                // 基于指示物的AI参数赋值
                                if (item5.指示物注入AI赋值.Count > 0)
                                {
                                    for (int l = 0; l < val3.ai.Count(); l++)
                                    {
                                        if (item5.指示物注入AI赋值.ContainsKey(l) && item5.指示物注入AI赋值.TryGetValue(l, out var value4))
                                        {
                                            string[] array = value4.Split('*');
                                            float result = 1f;
                                            if (array.Length == 2 && array[1] != "" && float.TryParse(array[1], out result))
                                            {
                                                value4 = array[0];
                                            }
                                            val3.ai[l] = (float)lNPC2.getMarkers(value4) * result;
                                            val3.netUpdate = true;
                                        }
                                    }
                                }

                                // 播放音效
                                foreach (声音节 item7 in item5.播放声音)
                                {
                                    if (item7.声音ID >= 0)
                                    {
                                        NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(val3.Center, (ushort)1, item7.声音ID, item7.声音规模, item7.高音补偿), -1, -1);
                                    }
                                }

                                // 执行命令
                                foreach (string item8 in item5.释放命令)
                                {
                                    Commands.HandleCommand((TSPlayer)(object)TSPlayer.Server, TShock.Config.Settings.CommandSilentSpecifier + lNPC2.ReplaceMarkers(item8));
                                }

                                // NPC喊话
                                if (item5.喊话 != "")
                                {
                                    TShock.Utils.Broadcast((item5.喊话无头 ? "" : (val3.FullName + ": ")) + lNPC2.ReplaceMarkers(item5.喊话), Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255));
                                }

                                // 修改玩家复活时间
                                if (item5.玩家复活时间 >= -1)
                                {
                                    lNPC2.RespawnSeconds = item5.玩家复活时间;
                                }

                                // 修改全局刷怪设置
                                if (item5.全局最大刷怪数 >= -1)
                                {
                                    lNPC2.DefaultMaxSpawns = item5.全局最大刷怪数;
                                }
                                if (item5.全局刷怪速度 >= -1)
                                {
                                    lNPC2.DefaultSpawnRate = item5.全局刷怪速度;
                                }

                                // 阻止传送机
                                if (item5.阻止传送器 != 0)
                                {
                                    lNPC2.BlockTeleporter = item5.阻止传送器;
                                }

                                // 修改游戏时间
                                Sundry.changeTime(item5.时间修改);

                                // 对NPC造成伤害
                                Sundry.HurtMonster(item5.杀伤怪物, val3);

                                // 发射弹幕
                                Sundry.LaunchProjectile(item5.释放弹幕, val3, lNPC2);

                                // 更新弹幕
                                Sundry.updataProjectile(item5.更新弹幕, val3, lNPC2);

                                // 召唤怪物
                                foreach (KeyValuePair<int, int> item9 in item5.召唤怪物)
                                {
                                    if (item9.Value >= 1 && item9.Key != 0)
                                    {
                                        int num26 = Math.Min(item9.Value, 200);
                                        NPC nPCById2 = TShock.Utils.GetNPCById(item9.Key);
                                        if (nPCById2 != null && nPCById2.netID != 488 && nPCById2.netID != 113 && nPCById2.netID != 0 && nPCById2.type < NPCID.Count)
                                        {
                                            TSPlayer.Server.SpawnNPC(nPCById2.netID, nPCById2.FullName, num26, Terraria.Utils.ToTileCoordinates(val3.Center).X, Terraria.Utils.ToTileCoordinates(val3.Center).Y, 5, 5);
                                        }
                                    }
                                }

                                // 更新Buff范围
                                if (item5.状态范围 > 0)
                                {
                                    lNPC2.BuffR = item5.状态范围;
                                    lNPC2.RBuff = item5.周围状态;
                                }
                                if (item5.反状态范围 > 0)
                                {
                                    lNPC2.OBuffR = item5.反状态范围;
                                    lNPC2.ORBuff = item5.反周围状态;
                                }

                                // NPC回血处理
                                if (item5.恢复血量 > 0)
                                {
                                    NPC val4 = val3;
                                    val4.life += item5.恢复血量;
                                    if (val3.life > val3.lifeMax)
                                    {
                                        val3.life = val3.lifeMax;
                                    }
                                    if (val3.life < 1)
                                    {
                                        val3.life = 1;
                                    }
                                    val3.HealEffect(item5.恢复血量, true);
                                    val3.netUpdate = true;
                                }

                                // 百分比回血
                                if (item5.比例回血 > 0)
                                {
                                    if (item5.比例回血 > 100)
                                    {
                                        item5.比例回血 = 100;
                                    }
                                    int num27 = val3.lifeMax * item5.比例回血 / 100;
                                    NPC val4 = val3;
                                    val4.life += num27;
                                    if (val3.life > val3.lifeMax)
                                    {
                                        val3.life = val3.lifeMax;
                                    }
                                    if (val3.life < 1)
                                    {
                                        val3.life = 1;
                                    }
                                    val3.HealEffect(num27, true);
                                    val3.netUpdate = true;
                                }

                                if (item5.怪物变形 >= 0 && (item5.怪物变形 != 113 || item5.怪物变形 != 114))
                                {
                                    val3.Transform(item5.怪物变形);
                                    val3.netUpdate = true;
                                }

                                // 减少可触发次数
                                if (item5.可触发次 != -1)
                                {
                                    时间节 时间节 = item5;
                                    时间节.可触发次--;
                                }

                                if (item5.击退范围 > 0 && item5.击退力度 != 0f)
                                {
                                    num10 = item5.击退范围;
                                    num11 = item5.击退力度;
                                    s = item5.柔和击退;
                                }

                                if (item5.杀伤范围 > 0 && item5.杀伤伤害 != 0)
                                {
                                    num12 = item5.杀伤范围;
                                    num13 = item5.杀伤伤害;
                                    flag2 = item5.直接杀伤;
                                }


                                if (item5.拉取范围 > 0)
                                {
                                    num14 = item5.拉取起始;
                                    num15 = item5.拉取范围;
                                    num16 = item5.拉取止点;
                                    num17 = item5.拉取点X轴偏移 + (float)lNPC2.getMarkers(item5.指示物数量注入拉取点X轴偏移名) * item5.指示物数量注入拉取点X轴偏移系数;
                                    num18 = item5.拉取点Y轴偏移 + (float)lNPC2.getMarkers(item5.指示物数量注入拉取点Y轴偏移名) * item5.指示物数量注入拉取点Y轴偏移系数;
                                    flag3 = item5.初始拉取点坐标为零;
                                    flag4 = item5.拉取范围为矩形;
                                }

                                if (item5.清弹结束范围 > 0)
                                {
                                    num19 = item5.清弹起始范围;
                                    num20 = item5.清弹结束范围;
                                }

                                if (item5.反射范围 > 0)
                                {
                                    num21 = item5.反射范围;
                                }

                                if (item5.直接撤退)
                                {
                                    flag5 = true;
                                }

                                if (item5.跳过事件 > 0)
                                {
                                    num22 = item5.跳过事件;
                                }

                                if (item5.跳出事件)
                                {
                                    break;
                                }
                                时事件限--;  // 减少时间事件处理限制计数
                            }
                        }
                        lNPC2.LTime = lNPC2.TiemN; // 更新最后处理时间
                    }


                    // 血量比例触发事件处理
                    // 检查血量百分比是否发生变化
                    if (lNPC2.LifeP != num23)
                    {
                        lNPC2.LifeP = num23;  // 更新当前血量百分比

                        // 检查是否到达新的血量百分比阈值
                        if (lNPC2.LifeP != lNPC2.LLifeP)
                        {
                            int 血事件限 = lNPC2.Config.血事件限;  // 血量事件处理限制s

                            // 遍历所有血量比例触发事件
                            foreach (比例节 item10 in lNPC2.PLife)
                            {
                                if (血事件限 <= 0) break;

                                // 检查事件是否可触发
                                if (item10.血量剩余比例 <= 0 || (item10.可触发次 <= 0 && item10.可触发次 != -1) || item10.血量剩余比例 < lNPC2.LifeP || item10.血量剩余比例 >= lNPC2.LLifeP)
                                {
                                    continue;
                                }

                                // ========== 血量事件条件检查 ==========
                                // 检查击杀数条件
                                if (item10.杀数条件 != 0)
                                {
                                    if (item10.杀数条件 > 0)
                                    {
                                        if (lNKC < item10.杀数条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNKC >= Math.Abs(item10.杀数条件))
                                    {
                                        continue;
                                    }
                                }

                                // 检查玩家数量条件
                                if (item10.人数条件 != 0)
                                {
                                    if (item10.人数条件 > 0)
                                    {
                                        if (lNPC2.PlayerCount < item10.人数条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNPC2.PlayerCount >= Math.Abs(item10.人数条件))
                                    {
                                        continue;
                                    }
                                }


                                if (item10.耗时条件 != 0)
                                {
                                    if (item10.耗时条件 > 0)
                                    {
                                        if (lNPC2.TiemN < (float)item10.耗时条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNPC2.TiemN >= (float)Math.Abs(item10.耗时条件))
                                    {
                                        continue;
                                    }
                                }

                                // 检查NPC ID条件
                                if (item10.ID条件 != 0 && item10.ID条件 != val3.netID)
                                {
                                    continue;
                                }

                                // 检查玩家击杀条件
                                if (item10.杀死条件 != 0)
                                {
                                    if (item10.杀死条件 > 0)
                                    {
                                        if (lNPC2.KillPlay < item10.杀死条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNPC2.KillPlay >= Math.Abs(item10.杀死条件))
                                    {
                                        continue;
                                    }
                                }

                                // 检查开服时间条件
                                if (item10.开服条件 != 0)
                                {
                                    if (item10.开服条件 > 0)
                                    {
                                        if (lNPC2.OSTime < item10.开服条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNPC2.OSTime >= Math.Abs(item10.开服条件))
                                    {
                                        continue;
                                    }
                                }

                                // 检查游戏状态条件
                                if ((item10.昼夜条件 == -1 && Main.dayTime) || (item10.昼夜条件 == 1 && !Main.dayTime) || (item10.血月条件 == -1 && Main.bloodMoon) || (item10.血月条件 == 1 && !Main.bloodMoon) || (item10.肉山条件 == -1 && Main.hardMode) || (item10.肉山条件 == 1 && !Main.hardMode) || (item10.巨人条件 == -1 && NPC.downedGolemBoss) || (item10.巨人条件 == 1 && !NPC.downedGolemBoss) || (item10.月总条件 == -1 && NPC.downedMoonlord) || (item10.月总条件 == 1 && !NPC.downedMoonlord))
                                {
                                    continue;
                                }

                                // 检查Y轴位置条件
                                if (item10.Y轴条件 != 0)
                                {
                                    if (item10.Y轴条件 > 0)
                                    {
                                        if (val3.Center.Y < (float)(item10.Y轴条件 << 4))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (val3.Center.Y >= (float)Math.Abs(item10.Y轴条件 << 4))
                                    {
                                        continue;
                                    }
                                }

                                // 检查面向方向条件
                                if ((item10.面向条件 == 1 && (val3.direction != 1 || val3.directionY != 0)) || (item10.面向条件 == 2 && (val3.direction != 1 || val3.directionY != 1)) || (item10.面向条件 == 3 && (val3.direction != 0 || val3.directionY != 1)) || (item10.面向条件 == 4 && (val3.direction != -1 || val3.directionY != 1)) || (item10.面向条件 == 5 && (val3.direction != -1 || val3.directionY != 0)) || (item10.面向条件 == 6 && (val3.direction != -1 || val3.directionY != -1)) || (item10.面向条件 == 7 && (val3.direction != 0 || val3.directionY != -1)) || (item10.面向条件 == 8 && (val3.direction != 1 || val3.directionY != -1)) || (item10.面向条件 == 9 && val3.direction != 1) || (item10.面向条件 == 10 && val3.directionY != 1) || (item10.面向条件 == 11 && val3.direction != -1) || (item10.面向条件 == 12 && val3.directionY != -1) || item10.触发率子 <= 0 || item10.触发率母 <= 0 || (item10.触发率子 < item10.触发率母 && rd.Next(1, item10.触发率母 + 1) > item10.触发率子) || Sundry.MonsterRequirement(item10.怪物条件, val3) || Sundry.PlayerRequirement(item10.玩家条件, val3))
                                {
                                    continue;
                                }

                                // ========== 执行血量事件动作 ==========
                                // 修改NPC物理属性
                                if (item10.能够穿墙 == -1)
                                {
                                    val3.noTileCollide = false;
                                    val3.netUpdate = true;
                                }
                                if (item10.能够穿墙 == 1)
                                {
                                    val3.noTileCollide = true;
                                    val3.netUpdate = true;
                                }
                                if (item10.无视重力 == -1)
                                {
                                    val3.noGravity = false;
                                    val3.netUpdate = true;
                                }
                                if (item10.无视重力 == 1)
                                {
                                    val3.noGravity = true;
                                    val3.netUpdate = true;
                                }
                                if (item10.怪物无敌 == -1)
                                {
                                    val3.immortal = false;
                                    val3.netUpdate = true;
                                }
                                if (item10.怪物无敌 == 1)
                                {
                                    val3.immortal = true;
                                    val3.netUpdate = true;
                                }
                                if (item10.切换智慧 >= 0 && item10.切换智慧 != 27)
                                {
                                    val3.aiStyle = item10.切换智慧;
                                    val3.netUpdate = true;
                                }
                                if (item10.修改防御)
                                {
                                    val3.defDefense = item10.怪物防御;
                                    val3.defense = item10.怪物防御;
                                    val3.netUpdate = true;
                                }
                                if (item10.喊话 != "")
                                {
                                    TShock.Utils.Broadcast((item10.喊话无头 ? "" : (val3.FullName + ": ")) + item10.喊话, Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255));
                                }
                                if (item10.玩家复活时间 >= -1)
                                {
                                    lNPC2.RespawnSeconds = item10.玩家复活时间;
                                }

                                // 对NPC造成伤害
                                Sundry.HurtMonster(item10.杀伤怪物, val3);

                                // 发射弹幕
                                Sundry.LaunchProjectile(item10.释放弹幕, val3, lNPC2);

                                // 召唤怪物
                                foreach (KeyValuePair<int, int> item11 in item10.召唤怪物)
                                {
                                    if (item11.Value >= 1 && item11.Key != 0)
                                    {
                                        int num28 = Math.Min(item11.Value, 200);
                                        NPC nPCById3 = TShock.Utils.GetNPCById(item11.Key);
                                        if (nPCById3 != null && nPCById3.netID != 488 && nPCById3.netID != 113 && nPCById3.netID != 0 && nPCById3.type < NPCID.Count)
                                        {
                                            TSPlayer.Server.SpawnNPC(nPCById3.netID, nPCById3.FullName, num28, Terraria.Utils.ToTileCoordinates(val3.Center).X, Terraria.Utils.ToTileCoordinates(val3.Center).Y, 15, 15);
                                        }
                                    }
                                }

                                // 更新BUFF
                                if (item10.状态范围 > 0)
                                {
                                    lNPC2.BuffR = item10.状态范围;
                                    lNPC2.RBuff = item10.周围状态;
                                }

                                // NPC回血处理
                                if (item10.恢复血量 > 0)
                                {
                                    NPC val4 = val3;
                                    val4.life += item10.恢复血量;
                                    if (val3.life > val3.lifeMax)
                                    {
                                        val3.life = val3.lifeMax;
                                    }
                                    if (val3.life < 1)
                                    {
                                        val3.life = 1;
                                    }
                                    val3.HealEffect(item10.恢复血量, true);
                                    val3.netUpdate = true;
                                }

                                // 百分比回血
                                if (item10.比例回血 > 0)
                                {
                                    if (item10.比例回血 > 100)
                                    {
                                        item10.比例回血 = 100;
                                    }
                                    NPC val4 = val3;
                                    val4.life += (int)((double)val3.lifeMax * ((double)item10.比例回血 / 100.0));
                                    if (val3.life > val3.lifeMax)
                                    {
                                        val3.life = val3.lifeMax;
                                    }
                                    if (val3.life < 1)
                                    {
                                        val3.life = 1;
                                    }
                                    val3.HealEffect(item10.恢复血量, true);
                                    val3.netUpdate = true;
                                }

                                // 减少可触发次数
                                if (item10.可触发次 != -1)
                                {
                                    比例节 比例节 = item10;
                                    比例节.可触发次--;
                                }

                                if (item10.击退范围 > 0 && item10.击退力度 != 0f)
                                {
                                    num10 = item10.击退范围;
                                    num11 = item10.击退力度;
                                    s = false;
                                }

                                if (item10.杀伤范围 > 0 && item10.杀伤伤害 != 0)
                                {
                                    num12 = item10.杀伤范围;
                                    num13 = item10.杀伤伤害;
                                    flag2 = false;
                                }

                                if (item10.拉取范围 > 0)
                                {
                                    num14 = item10.拉取起始;
                                    num15 = item10.拉取范围;
                                    num16 = item10.拉取止点;
                                    num17 = item10.拉取点X轴偏移;
                                    num18 = item10.拉取点Y轴偏移;
                                    flag3 = false;
                                    flag4 = false;
                                }

                                if (item10.直接撤退)
                                {
                                    flag5 = true;
                                }

                                // 跳出事件循环
                                if (item10.跳出事件)
                                {
                                    break;
                                }

                                血事件限--;  // 减少血量事件处理限制计数
                            }
                            lNPC2.LLifeP = lNPC2.LifeP; // 更新最后处理的血量百分比
                        }
                    }

                    // 检查弹幕清理范围设置
                    if (num19 > num20) num20 = 0;

                    // 处理弹幕反射和清理
                    if (num21 > 0 || num20 > 0)
                    {
                        for (int m = 0; m < 1000; m++)
                        {
                            if (Main.projectile[m].active)
                            {
                                // 反射弹幕
                                if (num21 > 0 && Main.projectile[m].CanBeReflected() && val3.WithinRange(Main.projectile[m].Center, (float)(num21 << 4)))
                                {
                                    val3.ReflectProjectile(Main.projectile[m]);
                                    NetMessage.SendData(27, -1, -1, null, m, 0f, 0f, 0f, 0, 0, 0);
                                }

                                // 清理弹幕
                                if (num20 > 0 && val3.WithinRange(Main.projectile[m].Center, (float)(num20 << 4)) && (num19 <= 0 || !val3.WithinRange(Main.projectile[m].Center, (float)(num19 << 4))))
                                {
                                    Main.projectile[m].active = false;
                                    Main.projectile[m].type = 0;
                                    TSPlayer.All.SendData((PacketTypes)27, "", m, 0f, 0f, 0f, 0);
                                }
                            }
                        }
                    }

                    // 调整拉取距离
                    if (num16 > num14)
                    {
                        num16 = num14;
                    }

                    num16 *= 16; // 转换为像素单位

                    // 计算拉取点坐标
                    float num29 = num17;
                    float num30 = num18;
                    if (!flag3)  // 如果不是初始坐标为零，则基于NPC位置计算
                    {
                        num29 += val3.Center.X;
                        num30 += val3.Center.Y;
                    }

                    // 检查是否有需要应用的玩家效果
                    if (lNPC2.BuffR > 0 || lNPC2.OBuffR > 0 || (num10 > 0 && num11 != 0f) || (num12 > 0 && num13 != 0) || num15 > 0)
                    {
                        TSPlayer[] players = TShock.Players;

                        // 遍历所有玩家应用效果
                        foreach (TSPlayer plr5 in players)
                        {
                            if (plr5 == null || !plr5.Active || plr5.Dead || plr5.TPlayer.statLife < 1)
                            {
                                continue;
                            }

                            // 拉取玩家效果
                            if (num15 > 0 && (flag4 ? Sundry.WithinRange2(plr5.TPlayer.Center, (int)num29, (int)num30, num15 * 16) : Sundry.WithinRange(plr5.TPlayer.Center, (int)num29, (int)num30, num15 * 16)))
                            {
                                if (num14 > 0)
                                {
                                    // 在拉取范围内但不在起始范围内的玩家才被拉取 
                                    if (!(flag4 ? Sundry.WithinRange2(plr5.TPlayer.Center, (int)num29, (int)num30, num15 * 16) : Sundry.WithinRange(plr5.TPlayer.Center, (int)num29, (int)num30, num14 * 16)))
                                    {
                                        Sundry.PullTP(plr5, num29, num30, num16, flag4);
                                    }
                                }
                                else
                                {
                                    // 无起始范围限制，直接拉取
                                    Sundry.PullTP(plr5, num29, num30, num16, flag4);
                                }
                            }

                            // 玩家伤害效果
                            if (num12 > 0 && num13 != 0 && Sundry.WithinRange(plr5.TPlayer.Center, val3.Center, num12 * 16))
                            {
                                if (num13 < 0)
                                {
                                    // 负伤害表示治疗
                                    if (num13 > plr5.TPlayer.statLifeMax2)
                                    {
                                        num13 = plr5.TPlayer.statLifeMax2;
                                    }
                                    plr5.Heal(Math.Abs(num13));
                                }
                                else if (num13 > plr5.TPlayer.statLifeMax2 + plr5.TPlayer.statDefense || num13 == 999999999)
                                {
                                    // 超高伤害直接杀死玩家
                                    plr5.KillPlayer();
                                }
                                else if (flag2)
                                {
                                    // 直接杀伤（无视防御）
                                    Player tPlayer = plr5.TPlayer;
                                    tPlayer.statLife -= num13;
                                    if (plr5.TPlayer.statLife < 1)
                                    {
                                        plr5.TPlayer.statLife = 1;
                                    }
                                    NetMessage.SendData(16, -1, -1, NetworkText.Empty, plr5.Index, 0f, 0f, 0f, 0, 0, 0);
                                }
                                else
                                {
                                    // 正常伤害计算
                                    plr5.DamagePlayer(num13);
                                }
                            }

                            // 玩家击退效果
                            if (num10 > 0 && num11 != 0f && Sundry.WithinRange(plr5.TPlayer.Center, val3.Center, num10 * 16))
                            {
                                Sundry.UserRepel(plr5, val3.Center.X, val3.Center.Y, num11, s);
                            }

                            Color val6;
                            // 施加buff效果
                            if (lNPC2.BuffR > 0 && lNPC2.Time % 100 == 0 && Sundry.WithinRange(plr5.TPlayer.Center, val3.Center, lNPC2.BuffR * 16))
                            {
                                foreach (状态节 item12 in lNPC2.RBuff)
                                {
                                    if ((item12.状态起始范围 <= 0 || !Sundry.WithinRange(plr5.TPlayer.Center, val3.Center, item12.状态起始范围 * 16)) && (item12.状态结束范围 <= 0 || Sundry.WithinRange(plr5.TPlayer.Center, val3.Center, item12.状态结束范围 * 16)))
                                    {
                                        plr5.SetBuff(item12.状态ID, 100, false);
                                        if (item12.头顶提示 != "")
                                        {
                                            string 头顶提示 = item12.头顶提示;
                                            val6 = new Color(255, 255, 255);
                                            plr5.SendData((PacketTypes)119, 头顶提示, (int)val6.PackedValue, plr5.X, plr5.Y, 0f, 0);
                                        }
                                    }
                                }
                            }

                            // 清除buff效果
                            if (lNPC2.OBuffR <= 0 || lNPC2.Time % 100 != 0 || !Sundry.WithinRange(plr5.TPlayer.Center, val3.Center, lNPC2.OBuffR * 16))
                            {
                                continue;
                            }

                            foreach (状态节 item13 in lNPC2.ORBuff)
                            {
                                if ((item13.状态起始范围 <= 0 || !Sundry.WithinRange(plr5.TPlayer.Center, val3.Center, item13.状态起始范围 * 16)) && (item13.状态结束范围 <= 0 || Sundry.WithinRange(plr5.TPlayer.Center, val3.Center, item13.状态结束范围 * 16)) && plr5.TPlayer.buffType.Contains(item13.状态ID))
                                {
                                    plr5.TPlayer.ClearBuff(item13.状态ID);
                                    if (item13.头顶提示 != "")
                                    {
                                        string 头顶提示2 = item13.头顶提示;
                                        val6 = new Color(255, 255, 255);
                                        plr5.SendData((PacketTypes)119, 头顶提示2, (int)val6.PackedValue, plr5.X, plr5.Y, 0f, 0);
                                    }
                                }
                            }
                            plr5.SendData((PacketTypes)50, "", plr5.Index, 0f, 0f, 0f, 0);
                        }
                    }

                    // 处理直接撤退
                    if (flag5)
                    {
                        int whoAmI2 = val3.whoAmI;
                        Main.npc[whoAmI2] = new NPC();
                        NetMessage.SendData(23, -1, -1, NetworkText.Empty, whoAmI2, 0f, 0f, 0f, 0, 0, 0);
                        if (!lNPC2.Config.不宣读信息)
                        {
                            TShock.Utils.Broadcast("攻略失败: " + val3.FullName + " 已撤离.", Convert.ToByte(190), Convert.ToByte(150), Convert.ToByte(150));
                        }
                    }
                end_IL_0211:;
                }
            }

            // 更新全局刷怪设置
            int num31 = -1;
            int num32 = -1;
            NPC[] npc3 = Main.npc;
            foreach (NPC val7 in npc3)
            {
                if (val7 == null || !val7.active)
                {
                    continue;
                }
                lock (LNpcs)
                {
                    LNPC lNPC3 = LNpcs[val7.whoAmI];
                    if (lNPC3 != null && lNPC3.Config != null)
                    {
                        if (lNPC3.DefaultMaxSpawns > num31)
                        {
                            num31 = lNPC3.DefaultMaxSpawns;
                        }
                        if (lNPC3.DefaultSpawnRate > num32)
                        {
                            num32 = lNPC3.DefaultSpawnRate;
                        }
                    }
                }
            }

            // 应用刷怪设置
            if (num31 >= 0 && NPC.defaultMaxSpawns != num31)
            {
                NPC.defaultMaxSpawns = num31;
            }
            else if (num31 < 0)
            {
                NPC.defaultMaxSpawns = TShock.Config.Settings.DefaultMaximumSpawns;
            }

            if (num32 >= 0 && NPC.defaultSpawnRate != num32)
            {
                NPC.defaultSpawnRate = num32;
            }
            else if (num32 < 0)
            {
                NPC.defaultSpawnRate = TShock.Config.Settings.DefaultSpawnRate;
            }

            // 处理队友视角功能
            if (_配置.启动死亡队友视角 && (!_配置.队友视角仅BOSS时 || flag))
            {
                if (TeamP <= _配置.队友视角流畅度)
                {
                    TeamP++;
                }
                else
                {
                    TeamP = 0;
                    // 将死亡玩家传送到活着的队友位置
                    TSPlayer[] players2 = TShock.Players;
                    foreach (TSPlayer val8 in players2)
                    {
                        if (val8 == null)
                        {
                            continue;
                        }
                        if (Timeout(now))
                        {
                            ULock = false;
                            return;
                        }
                        if (!val8.Active || !val8.Dead || val8.TPlayer.statLife >= 1 || val8.Team == 0)
                        {
                            continue;
                        }

                        // 查找最近的活着的队友
                        int num35 = -1;
                        float num36 = -1f;
                        for (int num37 = 0; num37 < 255; num37++)
                        {
                            if (Main.player[num37] != null && val8.Team == Main.player[num37].team && Main.player[num37].active && !Main.player[num37].dead)
                            {
                                float num38 = Math.Abs(Main.player[num37].position.X + (float)(Main.player[num37].width / 2) - (val8.TPlayer.position.X + (float)(val8.TPlayer.width / 2))) + Math.Abs(Main.player[num37].position.Y + (float)(Main.player[num37].height / 2) - (val8.TPlayer.position.Y + (float)(val8.TPlayer.height / 2)));
                                if (num36 == -1f || num38 < num36)
                                {
                                    num36 = num38;
                                    num35 = num37;
                                }
                            }
                        }

                        // 传送玩家到队友位置
                        if (num35 != -1 && !Sundry.WithinRange(val8.TPlayer.Center, Main.player[num35].Center, _配置.队友视角等待范围 * 16))
                        {
                            val8.TPlayer.position = Main.player[num35].position;
                            NetMessage.SendData(13, -1, -1, NetworkText.Empty, val8.Index, 0f, 0f, 0f, 0, 0, 0);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (_配置.启动错误报告)
            {
                TShock.Log.ConsoleError("自定义怪物血量时钟:" + ex.ToString());
            }
        }
        ULock = false;
    }
    #endregion

    #region 超时检查方法
    /// <summary>
    /// 超时检查方法
    /// </summary>
    /// <param name="Start">开始时间</param>
    /// <param name="warn">是否警告</param>
    /// <param name="ms">超时毫秒数</param>
    /// <returns>是否超时</returns>
    public static bool Timeout(DateTime Start, bool warn = true, int ms = 500)
    {
        bool flag = (DateTime.Now - Start).TotalMilliseconds >= ms;
        if (flag)
        {
            ULock = false;
        }
        if (warn && flag)
        {
            TShock.Log.Error("自定义怪物血量插件处理超时,已抛弃部分处理!");
        }
        return flag;
    }
    #endregion

    #region 玩家死亡事件处理

    // 保存原始复活时间配置的静态变量
    private static int LRespawnSeconds = -1;
    private static int LRespawnBossSeconds = -1;

    /// <summary>
    /// 玩家死亡事件处理方法
    /// 处理自定义NPC相关的复活时间调整
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="args">死亡事件参数</param>
    private void OnKillMe(object sender, KillMeEventArgs args)
    {
        // 检查事件是否已处理或无效参数
        if (args.Handled || args.Player == null || args.Pvp)
        {
            return;
        }

        int maxRespawnSeconds = -1;
        NPC[] npc = Main.npc;

        // 遍历所有NPC，更新击杀计数并找出最大的复活时间设置
        foreach (NPC val in npc)
        {
            if (val == null || !val.active)
            {
                continue;
            }

            lock (LNpcs)
            {
                LNPC lNPC = LNpcs[val.whoAmI];
                if (lNPC != null && lNPC.Config != null)
                {
                    // 增加该NPC的玩家击杀计数
                    lNPC.KillPlay++;

                    // 更新最大复活时间
                    if (lNPC.RespawnSeconds > maxRespawnSeconds)
                    {
                        maxRespawnSeconds = lNPC.RespawnSeconds;
                    }
                }
            }
        }

        // 检查服务器配置是否允许修改复活时间
        if (TShock.Config.Settings.RespawnSeconds < 0 || TShock.Config.Settings.RespawnBossSeconds < 0)
        {
            return;
        }

        // 应用自定义复活时间设置
        if (maxRespawnSeconds >= 0)
        {
            // 如果是第一次修改，保存原始配置
            if (LRespawnSeconds == -1 && LRespawnBossSeconds == -1)
            {
                LRespawnSeconds = TShock.Config.Settings.RespawnSeconds;
                LRespawnBossSeconds = TShock.Config.Settings.RespawnBossSeconds;
            }

            // 应用自定义复活时间
            TShock.Config.Settings.RespawnSeconds = maxRespawnSeconds;
            TShock.Config.Settings.RespawnBossSeconds = maxRespawnSeconds;
        }
        else if (LRespawnSeconds >= 0 && LRespawnBossSeconds >= 0)
        {
            // 没有自定义NPC要求复活时间时，恢复原始配置
            TShock.Config.Settings.RespawnSeconds = LRespawnSeconds;
            TShock.Config.Settings.RespawnBossSeconds = LRespawnBossSeconds;
            LRespawnSeconds = -1;
            LRespawnBossSeconds = -1;
        }
    }

    #endregion

    #region NPC击杀数据管理

    /// <summary>
    /// 获取指定NPC ID的击杀计数
    /// </summary>
    /// <param name="id">NPC的网络ID</param>
    /// <returns>该NPC的累计击杀次数，如果未找到或ID为0则返回0</returns>
    public static long getLNKC(int id)
    {
        if (id == 0)
        {
            return 0L;
        }
        lock (LNkc)
        {
            for (int i = 0; i < LNkc.Count; i++)
            {
                if (LNkc[i].ID == id)
                {
                    return LNkc[i].KC;
                }
            }
        }
        return 0L;
    }

    #endregion

    #region NPC击杀计数增加

    /// <summary>
    /// 增加指定NPC ID的击杀计数
    /// </summary>
    /// <param name="id">NPC的网络ID</param>
    /// <remarks>
    /// 如果该NPC ID已存在则计数+1，不存在则创建新的击杀记录
    /// 操作完成后会自动保存数据到文件
    /// </remarks>
    private void addLNKC(int id)
    {
        lock (LNkc)
        {
            for (int i = 0; i < LNkc.Count; i++)
            {
                if (LNkc[i].ID == id)
                {
                    LNkc[i].KC++;
                    SD(notime: false);
                    return;
                }
            }
            LNkc.Add(new LNKC(id));
            SD(notime: false);
        }
    }

    #endregion

    #region 数据保存到文件

    /// <summary>
    /// 将NPC击杀数据保存到文件
    /// </summary>
    /// <param name="notime">是否忽略时间间隔检查强制保存</param>
    /// <remarks>
    /// 数据格式：
    /// 第一行：服务器启动时间
    /// 后续行：NPC网络ID|击杀计数
    /// 包含15秒的保存间隔限制，避免频繁写入
    /// </remarks>
    public void SD(bool notime)
    {
        if (!notime)
        {
            if ((DateTime.UtcNow - NPCKillDataTime).TotalMilliseconds < 15000.0)
            {
                return;
            }
            NPCKillDataTime = DateTime.UtcNow;
        }
        using StreamWriter streamWriter = new StreamWriter(NPCKillPath);
        string text = OServerDataTime.ToString();
        for (int i = 0; i < LNkc.Count; i++)
        {
            text += Environment.NewLine;
            text = text + LNkc[i].ID + "|" + LNkc[i].KC;
        }
        streamWriter.Write(text);
    }

    #endregion

    #region 重置服务器数据
    /// <summary>
    /// 重置服务器数据 - 清空NPC击杀记录并重置服务器启动时间
    /// </summary>
    /// <remarks>
    /// 功能：
    /// - 将服务器启动时间设为当前UTC时间
    /// - 清空所有NPC的击杀计数记录
    /// 使用场景：服务器重启或需要重置统计数据时调用
    /// </remarks>
    public void ReD()
    {
        OServerDataTime = DateTime.UtcNow;
        LNkc.Clear();
    }
    #endregion

    #region 从文件加载NPC击杀数据
    /// <summary>
    /// 从文件加载NPC击杀数据 - 读取持久化存储的NPC击杀统计信息
    /// </summary>
    /// <remarks>
    /// 功能：
    /// - 检查数据文件是否存在，不存在则创建
    /// - 读取文件内容并解析服务器启动时间
    /// - 逐行解析NPC击杀数据并加载到内存
    /// 数据格式：
    /// 第一行：服务器启动时间(DateTime)
    /// 后续行：NPC网络ID|击杀计数 (如: "123|50")
    /// </remarks>
    public void RD()
    {
        // 确保数据文件存在
        if (!File.Exists(NPCKillPath))
        {
            File.Create(NPCKillPath).Close();
        }

        // 读取文件内容
        using StreamReader streamReader = new StreamReader(NPCKillPath);
        string text = streamReader.ReadToEnd();
        string[] array = text.Split(Environment.NewLine.ToCharArray());

        // 解析服务器启动时间
        if (array.Count() < 1 || !DateTime.TryParse(array[0], out var result))
        {
            return;
        }
        OServerDataTime = result;

        // 解析NPC击杀数据
        for (int i = 1; i < array.Count(); i++)
        {
            string[] array2 = array[i].Split('|');
            if (array2.Length == 2)
            {
                LNkc.Add(new LNKC(int.Parse(array2[0]), long.Parse(array2[1])));
            }
        }
    }
    #endregion
}