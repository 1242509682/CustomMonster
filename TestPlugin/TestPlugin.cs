using Microsoft.Xna.Framework;
using System.ComponentModel;
using System.Timers;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Utilities;
using TerrariaApi.Server;
using TestPlugin.读配置文件;
using TShockAPI;
using TShockAPI.Configuration;
using static TShockAPI.GetDataHandlers;
using Main = Terraria.Main;
using NPC = Terraria.NPC;


namespace TestPlugin
{
    [ApiVersion(2, 1)]
    public class TestPlugin : TerrariaPlugin
    {
        #region 插件信息
        public override string Author => "GK 阁下 羽学";

        public override string Description => "自定义怪物出没时的血量,当然不止这些！";

        public override string Name => "自定义怪物血量";

        public override Version Version => new Version(1, 0, 4, 36);
        #endregion

        #region 实例变量
        public 配置文件 _配置 = new 配置文件();

        private static readonly System.Timers.Timer Update = new System.Timers.Timer(10.0);

        public static bool ULock = false;

        public int LRespawnSeconds = -1;

        public int LRespawnBossSeconds = -1;

        public int TeamP = 0;

        public string _配置文件名 => "自定义怪物血量.json";

        public string _额外配置路径 => Path.Combine(TShock.SavePath, "." + _配置文件名);

        public string _配置路径 => Path.Combine(TShock.SavePath, _配置文件名);

        public string NPCKillPath => Path.Combine(TShock.SavePath, "自定义怪物血量存档.txt");

        public DateTime NPCKillDataTime { get; set; }

        public DateTime OServerDataTime { get; set; }

        private static List<LNKC> LNkc { get; set; }

        public static LNPC[] LNpcs { get; set; }
        #endregion

        #region 注册卸载

        public TestPlugin(Main game)
            : base(game)
        {
            this.Order = 1;
            LNpcs = new LNPC[201];
            LNkc = new List<LNKC>();
            NPCKillDataTime = DateTime.UtcNow;
            OServerDataTime = DateTime.UtcNow;
        }

        public override void Initialize()
        {
            RC();
            RD();
            GetDataHandlers.KillMe += OnKillMe;
            ServerApi.Hooks.GamePostInitialize.Register((TerrariaPlugin)(object)this, PostInitialize);
            ServerApi.Hooks.GameInitialize.Register((TerrariaPlugin)(object)this, OnInitialize);
            ServerApi.Hooks.NpcSpawn.Register((TerrariaPlugin)(object)this, NpcSpawn);
            ServerApi.Hooks.NpcKilled.Register((TerrariaPlugin)(object)this, NpcKilled);
            ServerApi.Hooks.NpcStrike.Register((TerrariaPlugin)(object)this, NpcStrike);
            ServerApi.Hooks.NetSendData.Register((TerrariaPlugin)(object)this, SendData);
            On.Terraria.NPC.SetDefaults += OnSetDefaults;
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
                Update.Elapsed -= OnUpdate;
                Update.Stop();
                GetDataHandlers.KillMe -= OnKillMe;
                ServerApi.Hooks.GameInitialize.Deregister((TerrariaPlugin)(object)this, OnInitialize);
                ServerApi.Hooks.NpcSpawn.Deregister((TerrariaPlugin)(object)this, NpcSpawn);
                ServerApi.Hooks.NpcKilled.Deregister((TerrariaPlugin)(object)this, NpcKilled);
                ServerApi.Hooks.GamePostInitialize.Deregister((TerrariaPlugin)(object)this, PostInitialize);
                ServerApi.Hooks.NpcStrike.Deregister((TerrariaPlugin)(object)this, NpcStrike);
                ServerApi.Hooks.NetSendData.Deregister((TerrariaPlugin)(object)this, SendData);
                On.Terraria.NPC.SetDefaults -= OnSetDefaults;
                On.Terraria.Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += Projectile_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region 广告！
        public void PostInitialize(EventArgs e)
        {
            Update.Elapsed += OnUpdate;
            Update.Start();
            if (_配置.控制台广告)
            {
                Console.WriteLine(" -------------- " + this.Name + " 版本:" + this.Version?.ToString() + " 已启动! -------------");
                Console.WriteLine(" >>> 如果使用过程中出现什么问题可前往QQ群232109072交流反馈.");
                Console.WriteLine(" >>> 本插件免费请勿上当受骗,您可在QQ群232109072中获取最新插件.");
                Console.WriteLine(" ----------------------------------------------------------------");
            }
        }
        #endregion

        #region 配置文件的读取方法
        private void RC()
        {
            try
            {
                if (!File.Exists(_配置路径))
                {
                    TShock.Log.ConsoleError("未找到自定义怪物血量配置，已为您创建！修改配置后重启即可重新载入数据。");
                }
                _配置 = 配置文件.Read(_配置路径);
                Version version = new Version(_配置.配置文件插件版本号);
                if (version <= this.Version)
                {
                    _配置.配置文件插件版本号 = this.Version.ToString();
                    _配置.Write(_配置路径);
                }
                else
                {
                    TShock.Log.ConsoleError("[自定义怪物血量]您载入的配置文件插件版本高于本插件版本,配置可能无法正常使用,请升级插件后使用！");
                }
                if (!Directory.Exists(_额外配置路径))
                {
                    Directory.CreateDirectory(_额外配置路径);
                }
                DirectoryInfo directoryInfo = new DirectoryInfo(_额外配置路径);
                FileInfo[] files = directoryInfo.GetFiles("*." + _配置文件名);
                for (int i = 0; i < files.Length; i++)
                {
                    string fullName = files[i].FullName;
                    Console.WriteLine("[自定义怪物血量] 读入额外配置:" + files[i].Name);
                    try
                    {
                        配置文件 配置文件 = 配置文件.Read(fullName);
                        version = new Version(配置文件.配置文件插件版本号);
                        if (version <= this.Version)
                        {
                            配置文件.配置文件插件版本号 = this.Version.ToString();
                            配置文件.Write(fullName);
                        }
                        else
                        {
                            TShock.Log.ConsoleError("[自定义怪物血量]您读入的额外配置 " + files[i].Name + " 插件版本高于本插件版本,配置可能无法正常使用,请升级插件后使用！");
                        }
                        int num = 配置文件.怪物节集.Length;
                        int num2 = _配置.怪物节集.Length;
                        if (num > 0)
                        {
                            怪物节[] array = new 怪物节[num2 + num];
                            Array.Copy(_配置.怪物节集, 0, array, 0, num2);
                            Array.Copy(配置文件.怪物节集, 0, array, num2, num);
                            _配置.怪物节集 = array;
                        }
                        if (配置文件.统一设置例外怪物.Count > 0)
                        {
                            _配置.统一设置例外怪物 = _配置.统一设置例外怪物.Union(配置文件.统一设置例外怪物).ToList();
                        }
                        Console.WriteLine("[自定义怪物血量] 额外配置[" + files[i].Name + "]增添了" + num + "条配置");
                    }
                    catch (Exception ex)
                    {
                        TShock.Log.ConsoleError("[自定义怪物血量] 额外配置[" + files[i].Name + "]错误:\n" + ex.ToString() + "\n");
                    }
                }
            }
            catch (Exception ex2)
            {
                TShock.Log.ConsoleError("[自定义怪物血量] 配置错误:\n" + ex2.ToString() + "\n");
            }
        }

        private void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("重读自定义怪物血量权限", new CommandDelegate(CRC), new string[1] { "重读自定义怪物血量" })
            {
                HelpText = "输入 /重读自定义怪物血量 会重新读取重读自定义怪物血量表"
            });

            Commands.ChatCommands.Add(new Command("重读自定义怪物血量权限", CRS, "改怪物", "ggw"));
        }

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

        private void CRC(CommandArgs args)
        {
            RC();
            args.Player.SendSuccessMessage(args.Player.Name + "[自定义怪物血量]配置重读完毕。");
        }
        #endregion

        private int Projectile_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float(On.Terraria.Projectile.orig_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float orig, IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner, float ai0, float ai1, float ai2)
        {
            if (Owner == Main.myPlayer && _配置.统一怪物弹幕伤害修正 != 1f)
            {
                Damage = (int)(Damage * _配置.统一怪物弹幕伤害修正);
            }
            return orig.Invoke(spawnSource, X, Y, SpeedX, SpeedY, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
        }

        private void OnSetDefaults(On.Terraria.NPC.orig_SetDefaults orig, NPC self, int Type, Terraria.NPCSpawnParams spawnparams)
        {
            if (_配置.统一初始怪物玩家系数 > 0 || _配置.统一初始怪物强化系数 > 0f)
            {
                if (_配置.统一初始怪物玩家系数 > 0)
                {
                    spawnparams.playerCountForMultiplayerDifficultyOverride = _配置.统一初始怪物玩家系数;
                    if (spawnparams.playerCountForMultiplayerDifficultyOverride > 1000)
                    {
                        spawnparams.playerCountForMultiplayerDifficultyOverride = 1000;
                    }
                    if (_配置.统一初始玩家系数不低于人数)
                    {
                        int activePlayerCount = TShock.Utils.GetActivePlayerCount();
                        if (spawnparams.playerCountForMultiplayerDifficultyOverride < activePlayerCount)
                        {
                            spawnparams.playerCountForMultiplayerDifficultyOverride = activePlayerCount;
                        }
                    }
                }
                if (_配置.统一初始怪物强化系数 > 0f)
                {
                    spawnparams.strengthMultiplierOverride = _配置.统一初始怪物强化系数;
                    if (spawnparams.strengthMultiplierOverride > 1000f)
                    {
                        spawnparams.strengthMultiplierOverride = 1000f;
                    }
                }
            }
            orig.Invoke(self, Type, spawnparams);
        }

        private void NpcSpawn(NpcSpawnEventArgs args)
        {
            if (((HandledEventArgs)(object)args).Handled || Main.npc[args.NpcId] == null || Main.npc[args.NpcId].netID == 0 || !Main.npc[args.NpcId].active)
            {
                return;
            }
            bool flag = false;
            int activePlayerCount = TShock.Utils.GetActivePlayerCount();
            int lifeMax = Main.npc[args.NpcId].lifeMax;
            怪物节 config = null;
            int maxtime = 0;
            long lNKC = getLNKC(Main.npc[args.NpcId].netID);
            int num = 0;
            int num2 = 0;
            float strengthMultiplier = Main.npc[args.NpcId].strengthMultiplier;
            NPC[] npc = Main.npc;
            foreach (NPC val in npc)
            {
                if (val.netID == Main.npc[args.NpcId].netID)
                {
                    num++;
                }
            }
            Random random = new Random();
            怪物节[] 怪物节集 = _配置.怪物节集;
            foreach (怪物节 怪物节 in 怪物节集)
            {
                if ((Main.npc[args.NpcId].netID != 怪物节.怪物ID && 怪物节.怪物ID != 0) || (怪物节.怪物ID == 0 && 怪物节.再匹配.Count() > 0 && !怪物节.再匹配.Contains(Main.npc[args.NpcId].netID)) || (怪物节.怪物ID == 0 && 怪物节.再匹配例外.Count() > 0 && 怪物节.再匹配例外.Contains(Main.npc[args.NpcId].netID)))
                {
                    continue;
                }
                num2 = ((怪物节.开服时间型 == 1) ? ((int)(DateTime.UtcNow - OServerDataTime).TotalDays) : ((怪物节.开服时间型 != 2) ? ((int)(DateTime.UtcNow - OServerDataTime).TotalHours) : ((int)(DateTime.UtcNow.Date - OServerDataTime.Date).TotalDays)));
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
                if ((怪物节.昼夜条件 == -1 && Main.dayTime) || (怪物节.昼夜条件 == 1 && !Main.dayTime) || (怪物节.降雨条件 == -1 && Main.raining) || (怪物节.降雨条件 == 1 && !Main.raining) || (怪物节.血月条件 == -1 && Main.bloodMoon) || (怪物节.血月条件 == 1 && !Main.bloodMoon) || (怪物节.日食条件 == -1 && Main.eclipse) || (怪物节.日食条件 == 1 && !Main.eclipse) || (怪物节.肉山条件 == -1 && Main.hardMode) || (怪物节.肉山条件 == 1 && !Main.hardMode) || (怪物节.巨人条件 == -1 && NPC.downedGolemBoss) || (怪物节.巨人条件 == 1 && !NPC.downedGolemBoss) || (怪物节.月总条件 == -1 && NPC.downedMoonlord) || (怪物节.月总条件 == 1 && !NPC.downedMoonlord) || 怪物节.出没率子 <= 0 || 怪物节.出没率母 <= 0 || (怪物节.出没率子 < 怪物节.出没率母 && random.Next(1, 怪物节.出没率母 + 1) > 怪物节.出没率子) || Sundry.NPCKillRequirement(怪物节.杀怪条件) || Sundry.MonsterRequirement(怪物节.怪物条件, Main.npc[args.NpcId]))
                {
                    continue;
                }
                if (_配置.启动怪物时间限制 && (怪物节.人秒系数 != 0 || 怪物节.出没秒数 != 0 || 怪物节.开服系秒 != 0 || 怪物节.杀数系秒 != 0))
                {
                    int num3 = activePlayerCount * 怪物节.人秒系数;
                    num3 += 怪物节.出没秒数;
                    num3 += num2 * 怪物节.开服系秒;
                    num3 += (int)lNKC * 怪物节.杀数系秒;
                    if (num3 < 1)
                    {
                        ((HandledEventArgs)(object)args).Handled = true;
                        Main.npc[args.NpcId].active = false;
                        Console.WriteLine(Main.npc[args.NpcId].FullName + "定义时间过小被阻止生成");
                        return;
                    }
                    maxtime = num3;
                }
                if (怪物节.初始属性玩家系数 > 0 || 怪物节.初始属性强化系数 > 0f)
                {
                    Terraria.NPCSpawnParams val2 = default;
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
                    Main.npc[args.NpcId].SetDefaults(Main.npc[args.NpcId].type, val2);
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
                if (怪物节.玩家系数 != 0 || 怪物节.怪物血量 != 0 || 怪物节.开服系数 != 0 || 怪物节.杀数系数 != 0)
                {
                    int num4 = activePlayerCount * 怪物节.玩家系数;
                    num4 += 怪物节.怪物血量;
                    num4 += num2 * 怪物节.开服系数;
                    num4 += (int)lNKC * 怪物节.杀数系数;
                    if (!怪物节.覆盖原血量)
                    {
                        num4 += Main.npc[args.NpcId].lifeMax;
                    }
                    if (num4 < 1)
                    {
                        num4 = 1;
                    }
                    if (Main.npc[args.NpcId].lifeMax < num4 || !怪物节.不低于正常)
                    {
                        Main.npc[args.NpcId].lifeMax = num4;
                        flag = true;
                    }
                }
                if (怪物节.玩家强化系数 != 0f || 怪物节.强化系数 != 0f || 怪物节.开服强化系数 != 0f || 怪物节.杀数强化系数 != 0f)
                {
                    float num5 = activePlayerCount * 怪物节.玩家强化系数;
                    num5 += 怪物节.强化系数;
                    num5 += num2 * 怪物节.开服强化系数;
                    num5 += (int)lNKC * 怪物节.杀数强化系数;
                    if (!怪物节.覆盖原强化)
                    {
                        num5 += Main.npc[args.NpcId].strengthMultiplier;
                    }
                    if (num5 > 1000f)
                    {
                        num5 = 1000f;
                    }
                    if (num5 < 0f)
                    {
                        num5 = 0f;
                    }
                    if ((!(Main.npc[args.NpcId].strengthMultiplier >= num5) || !怪物节.不小于正常) && num5 > 0f)
                    {
                        Main.npc[args.NpcId].strengthMultiplier = num5;
                        flag = true;
                    }
                }
                if (怪物节.自定缀称 != "")
                {
                    Main.npc[args.NpcId]._givenName = 怪物节.自定缀称;
                    Main.npc[args.NpcId].netUpdate = true;
                }
                config = 怪物节;
                break;
            }
            if (!_配置.统一设置例外怪物.Contains(Main.npc[args.NpcId].netID))
            {
                if (_配置.统一怪物血量倍数 != 1.0 && _配置.统一怪物血量倍数 > 0.0)
                {
                    Main.npc[args.NpcId].lifeMax = (int)(Main.npc[args.NpcId].lifeMax * _配置.统一怪物血量倍数);
                    if (Main.npc[args.NpcId].lifeMax < 1)
                    {
                        Main.npc[args.NpcId].lifeMax = 1;
                    }
                    flag = true;
                }
                if (Main.npc[args.NpcId].lifeMax < lifeMax && _配置.统一血量不低于正常)
                {
                    Main.npc[args.NpcId].lifeMax = lifeMax;
                    flag = false;
                }
                if (_配置.统一怪物强化倍数 != 1.0 && _配置.统一怪物强化倍数 > 0.0)
                {
                    float num6 = (float)(Main.npc[args.NpcId].strengthMultiplier * _配置.统一怪物强化倍数);
                    if (num6 > 1000f)
                    {
                        num6 = 1000f;
                    }
                    if (num6 > 0f)
                    {
                        Main.npc[args.NpcId].strengthMultiplier = num6;
                        flag = false;
                    }
                }
                if (Main.npc[args.NpcId].strengthMultiplier < strengthMultiplier && _配置.统一强化不低于正常)
                {
                    Main.npc[args.NpcId].strengthMultiplier = strengthMultiplier;
                    flag = false;
                }
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
                if (_配置.统一对怪物伤害修正 != 1f)
                {
                    NPC obj2 = Main.npc[args.NpcId];
                    obj2.takenDamageMultiplier *= _配置.统一对怪物伤害修正;
                }
            }
            if (flag)
            {
                Main.npc[args.NpcId].life = Main.npc[args.NpcId].lifeMax;
                Main.npc[args.NpcId].netUpdate = true;
            }
            lock (LNpcs)
            {
                LNpcs[args.NpcId] = new LNPC(args.NpcId, activePlayerCount, lifeMax, config, maxtime, num2, lNKC);
            }
        }

        private void SendData(SendDataEventArgs args)
        {
            if (((HandledEventArgs)(object)args).Handled || (int)args.MsgId != 65 || args.number5 != 0 || args.number != 0)
            {
                return;
            }
            bool flag = false;
            NPC[] npc = Main.npc;
            foreach (NPC val in npc)
            {
                if (val == null || !val.active)
                {
                    continue;
                }
                lock (LNpcs)
                {
                    LNPC lNPC = LNpcs[val.whoAmI];
                    if (lNPC == null || lNPC.Config == null || lNPC.BlockTeleporter != 1)
                    {
                        continue;
                    }
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                ((HandledEventArgs)(object)args).Handled = true;
            }
        }

        private void NpcStrike(NpcStrikeEventArgs args)
        {
            if (((HandledEventArgs)(object)args).Handled || args.Damage < 0 || args.Npc.netID == 0 || !args.Npc.active)
            {
                return;
            }
            lock (LNpcs)
            {
                if (LNpcs[args.Npc.whoAmI] != null && LNpcs[args.Npc.whoAmI].Config != null)
                {
                    LNpcs[args.Npc.whoAmI].Struck++;
                }
            }
        }

        private void NpcKilled(NpcKilledEventArgs args)
        {
            if (args.npc.netID == 0)
            {
                return;
            }
            lock (LNpcs)
            {
                if (LNpcs[args.npc.whoAmI] != null)
                {
                    if (LNpcs[args.npc.whoAmI].Config != null)
                    {
                        if (!LNpcs[args.npc.whoAmI].Config.不宣读信息)
                        {
                            TShock.Utils.Broadcast("攻略成功: " + args.npc.FullName + " 已被击败.", Convert.ToByte(130), Convert.ToByte(50), Convert.ToByte(230));
                        }
                        foreach (声音节 item in LNpcs[args.npc.whoAmI].Config.死亡放音)
                        {
                            if (item.声音ID >= 0)
                            {
                                Terraria.NetMessage.PlayNetSound(new Terraria.NetMessage.NetSoundInfo(args.npc.Center, (ushort)1, item.声音ID, item.声音规模, item.高音补偿), -1, -1);
                            }
                        }
                        foreach (string item2 in LNpcs[args.npc.whoAmI].Config.死亡命令)
                        {
                            Commands.HandleCommand((TSPlayer)(object)TSPlayer.Server, ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.CommandSilentSpecifier + LNpcs[args.npc.whoAmI].ReplaceMarkers(item2));
                        }
                        if (LNpcs[args.npc.whoAmI].Config.死亡喊话 != "")
                        {
                            TShock.Utils.Broadcast((LNpcs[args.npc.whoAmI].Config.死亡喊话无头 ? "" : (args.npc.FullName + ": ")) + LNpcs[args.npc.whoAmI].Config.死亡喊话, Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255));
                        }
                        long lNKC = getLNKC(args.npc.netID);
                        int num = LNpcs[args.npc.whoAmI].Config.掉落组限;
                        Random rd = new Random();
                        foreach (掉落节 item3 in LNpcs[args.npc.whoAmI].Config.额外掉落)
                        {
                            if (num <= 0)
                            {
                                break;
                            }
                            if ((item3.难度条件.Length != 0 && !item3.难度条件.Contains(Main.GameMode)) || Sundry.SeedRequirement(item3.种子条件))
                            {
                                continue;
                            }
                            if (item3.杀数条件 != 0)
                            {
                                if (item3.杀数条件 > 0)
                                {
                                    if (lNKC < item3.杀数条件)
                                    {
                                        continue;
                                    }
                                }
                                else if (lNKC >= Math.Abs(item3.杀数条件))
                                {
                                    continue;
                                }
                            }
                            if (item3.被击条件 != 0)
                            {
                                if (item3.被击条件 > 0)
                                {
                                    if (LNpcs[args.npc.whoAmI].Struck < item3.被击条件)
                                    {
                                        continue;
                                    }
                                }
                                else if (LNpcs[args.npc.whoAmI].Struck >= Math.Abs(item3.被击条件))
                                {
                                    continue;
                                }
                            }
                            if (item3.人数条件 != 0)
                            {
                                if (item3.人数条件 > 0)
                                {
                                    if (LNpcs[args.npc.whoAmI].PlayerCount < item3.人数条件)
                                    {
                                        continue;
                                    }
                                }
                                else if (LNpcs[args.npc.whoAmI].PlayerCount >= Math.Abs(item3.人数条件))
                                {
                                    continue;
                                }
                            }
                            if (item3.耗时条件 != 0)
                            {
                                if (item3.耗时条件 > 0)
                                {
                                    if (LNpcs[args.npc.whoAmI].TiemN < item3.耗时条件)
                                    {
                                        continue;
                                    }
                                }
                                else if (LNpcs[args.npc.whoAmI].TiemN >= Math.Abs(item3.耗时条件))
                                {
                                    continue;
                                }
                            }
                            if (item3.杀死条件 != 0)
                            {
                                if (item3.杀死条件 > 0)
                                {
                                    if (LNpcs[args.npc.whoAmI].KillPlay < item3.杀死条件)
                                    {
                                        continue;
                                    }
                                }
                                else if (LNpcs[args.npc.whoAmI].KillPlay >= Math.Abs(item3.杀死条件))
                                {
                                    continue;
                                }
                            }
                            if (item3.开服条件 != 0)
                            {
                                if (item3.开服条件 > 0)
                                {
                                    if (LNpcs[args.npc.whoAmI].OSTime < item3.开服条件)
                                    {
                                        continue;
                                    }
                                }
                                else if (LNpcs[args.npc.whoAmI].OSTime >= Math.Abs(item3.开服条件))
                                {
                                    continue;
                                }
                            }
                            if ((item3.昼夜条件 == -1 && Main.dayTime) || (item3.昼夜条件 == 1 && !Main.dayTime) || (item3.血月条件 == -1 && Main.bloodMoon) || (item3.血月条件 == 1 && !Main.bloodMoon) || (item3.日食条件 == -1 && Main.eclipse) || (item3.日食条件 == 1 && !Main.eclipse) || (item3.降雨条件 == -1 && Main.raining) || (item3.降雨条件 == 1 && !Main.raining) || (item3.肉山条件 == -1 && Main.hardMode) || (item3.肉山条件 == 1 && !Main.hardMode) || (item3.巨人条件 == -1 && NPC.downedGolemBoss) || (item3.巨人条件 == 1 && !NPC.downedGolemBoss) || (item3.月总条件 == -1 && NPC.downedMoonlord) || (item3.月总条件 == 1 && !NPC.downedMoonlord) || item3.掉落率子 <= 0 || item3.掉落率母 <= 0 || (item3.掉落率子 < item3.掉落率母 && rd.Next(1, item3.掉落率母 + 1) > item3.掉落率子) || Sundry.NPCKillRequirement(item3.杀怪条件) || !LNpcs[args.npc.whoAmI].haveMarkers(item3.指示物条件, args.npc) || Sundry.AIRequirement(item3.AI条件, args.npc) || Sundry.MonsterRequirement(item3.怪物条件, args.npc))
                            {
                                continue;
                            }
                            foreach (物品节 item4 in item3.掉落物品)
                            {
                                if (item4.物品数量 <= 0 || item4.物品ID <= 0)
                                {
                                    continue;
                                }
                                if (item4.物品前缀 >= 0)
                                {
                                    if (item4.独立掉落)
                                    {
                                        args.npc.DropItemInstanced(args.npc.Center, args.npc.Size, item4.物品ID, item4.物品数量, true);
                                        continue;
                                    }
                                    int num2 = Terraria.Item.NewItem(new EntitySource_DebugCommand(), args.npc.Center, args.npc.Size, item4.物品ID, item4.物品数量, false, item4.物品前缀, false, false);
                                    Terraria.NetMessage.TrySendData(21, -1, -1, null, num2, 0f, 0f, 0f, 0, 0, 0);
                                }
                                else if (item4.独立掉落)
                                {
                                    args.npc.DropItemInstanced(args.npc.Center, args.npc.Size, item4.物品ID, item4.物品数量, true);
                                }
                                else
                                {
                                    int num3 = Terraria.Item.NewItem(new EntitySource_DebugCommand(), args.npc.Center, args.npc.Size, item4.物品ID, item4.物品数量, false, 0, false, false);
                                    Terraria.NetMessage.TrySendData(21, -1, -1, null, num3, 0f, 0f, 0f, 0, 0, 0);
                                }
                            }
                            if (item3.喊话 != "")
                            {
                                TShock.Utils.Broadcast((item3.喊话无头 ? "" : (args.npc.FullName + ": ")) + item3.喊话, Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255));
                            }
                            if (item3.跳出掉落)
                            {
                                break;
                            }
                            num--;
                        }
                        Sundry.SetMonsterMarkers(LNpcs[args.npc.whoAmI].Config.死亡怪物指示物修改, args.npc, ref rd);
                        Sundry.HurtMonster(LNpcs[args.npc.whoAmI].Config.死亡伤怪, args.npc);
                        Sundry.LaunchProjectile(LNpcs[args.npc.whoAmI].Config.死亡弹幕, args.npc, LNpcs[args.npc.whoAmI]);
                        foreach (KeyValuePair<int, int> item5 in LNpcs[args.npc.whoAmI].Config.遗言怪物)
                        {
                            if (item5.Value >= 1 && item5.Key != 0)
                            {
                                int num4 = Math.Min(item5.Value, 200);
                                NPC nPCById = TShock.Utils.GetNPCById(item5.Key);
                                if (nPCById != null && nPCById.type != 113 && nPCById.type != 0 && nPCById.type < NPCID.Count)
                                {
                                    TSPlayer.Server.SpawnNPC(nPCById.type, nPCById.FullName, num4, Terraria.Utils.ToTileCoordinates(args.npc.Center).X, Terraria.Utils.ToTileCoordinates(args.npc.Center).Y, 3, 3);
                                }
                            }
                        }
                        if (LNpcs[args.npc.whoAmI].Config.死状范围 > 0)
                        {
                            TSPlayer[] players = TShock.Players;
                            foreach (TSPlayer val in players)
                            {
                                if (val == null || val.Dead || val.TPlayer.statLife < 1 || !Sundry.WithinRange(val.TPlayer.Center, args.npc.Center, LNpcs[args.npc.whoAmI].Config.死状范围 * 16))
                                {
                                    continue;
                                }
                                foreach (KeyValuePair<int, int> item6 in LNpcs[args.npc.whoAmI].Config.死亡状态)
                                {
                                    if (item6.Value > 0)
                                    {
                                        val.SetBuff(item6.Key, item6.Value * 60, false);
                                    }
                                }
                            }
                        }
                    }
                    LNpcs[args.npc.whoAmI] = null;
                }
            }
            addLNKC(args.npc.netID);
        }

        public void OnUpdate(object sender, ElapsedEventArgs e)
        {
            if (ULock)
            {
                return;
            }
            ULock = true;
            DateTime now = DateTime.Now;
            Random rd = new Random();
            if (Main.rand == null)
            {
                Main.rand = new UnifiedRandom();
            }
            try
            {
                int activePlayerCount = TShock.Utils.GetActivePlayerCount();
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
                NPC[] npc2 = Main.npc;
                foreach (NPC val2 in npc2)
                {
                    if (activePlayerCount <= 0 || val2 == null || !val2.active)
                    {
                        continue;
                    }
                    if (val2.boss)
                    {
                        flag = true;
                    }
                    lock (LNpcs)
                    {
                        LNPC lNPC = LNpcs[val2.whoAmI];
                        if (lNPC != null && lNPC.Config != null)
                        {
                            dictionary2.Add(val2.whoAmI, lNPC.Config.事件权重);
                        }
                    }
                }
                IOrderedEnumerable<KeyValuePair<int, int>> orderedEnumerable = dictionary2.OrderByDescending(delegate (KeyValuePair<int, int> objDic)
                {
                    KeyValuePair<int, int> keyValuePair = objDic;
                    return keyValuePair.Value;
                });
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
                    if (Timeout(now))
                    {
                        ULock = false;
                        return;
                    }
                    lock (LNpcs)
                    {
                        LNPC lNPC2 = LNpcs[val3.whoAmI];
                        if (lNPC2 == null || lNPC2.Config == null)
                        {
                            continue;
                        }
                        if (lNPC2.PlayerCount < activePlayerCount)
                        {
                            lNPC2.PlayerCount = activePlayerCount;
                            if (_配置.启动动态时间限制 && (lNPC2.Config.人秒系数 != 0 || lNPC2.Config.出没秒数 != 0 || lNPC2.Config.开服系秒 != 0 || lNPC2.Config.杀数系秒 != 0))
                            {
                                int num4 = lNPC2.PlayerCount * lNPC2.Config.人秒系数;
                                num4 += lNPC2.Config.出没秒数;
                                int num5 = lNPC2.PlayerCount * lNPC2.Config.人秒系数;
                                num5 += lNPC2.Config.出没秒数;
                                num5 += lNPC2.OSTime * lNPC2.Config.开服系秒;
                                num5 += (int)lNPC2.LKC * lNPC2.Config.杀数系秒;
                                if (num5 < 1)
                                {
                                    num5 = -1;
                                }
                                if (lNPC2.MaxTime != num5)
                                {
                                    lNPC2.MaxTime = num5;
                                    int value = num4 - num5;
                                    if (num4 > num5)
                                    {
                                        if (!lNPC2.Config.不宣读信息)
                                        {
                                            TShock.Utils.Broadcast("注意: " + val3.FullName + " 受服务器人数增多影响攻略时间减少 " + value + " 秒剩" + (int)(num5 - lNPC2.TiemN) + "秒.", Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(100));
                                        }
                                    }
                                    else if (num4 < num5 && !lNPC2.Config.不宣读信息)
                                    {
                                        TShock.Utils.Broadcast("注意: " + val3.FullName + " 受服务器人数增多影响攻略时间增加 " + Math.Abs(value) + "秒剩" + (int)(num5 - lNPC2.TiemN) + "秒.", Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(100));
                                    }
                                }
                            }
                            if (_配置.启动动态血量上限 && (lNPC2.Config.玩家系数 != 0 || lNPC2.Config.怪物血量 != 0 || lNPC2.Config.开服系数 != 0 || lNPC2.Config.杀数系数 != 0))
                            {
                                int num6 = activePlayerCount * lNPC2.Config.玩家系数;
                                num6 += lNPC2.Config.怪物血量;
                                num6 += lNPC2.OSTime * lNPC2.Config.开服系数;
                                num6 += (int)lNPC2.LKC * lNPC2.Config.杀数系数;
                                if (!lNPC2.Config.覆盖原血量)
                                {
                                    num6 += val3.lifeMax;
                                }
                                if (num6 < 1)
                                {
                                    num6 = 1;
                                }
                                if (lNPC2.MaxLife < num6 || !lNPC2.Config.不低于正常)
                                {
                                    int lifeMax = val3.lifeMax;
                                    int num7 = num6;
                                    if (!_配置.统一设置例外怪物.Contains(val3.netID))
                                    {
                                        if (_配置.统一怪物血量倍数 != 1.0 && _配置.统一怪物血量倍数 > 0.0)
                                        {
                                            num7 = (int)(num7 * _配置.统一怪物血量倍数);
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
                                    if (lifeMax != num7)
                                    {
                                        int life = val3.life;
                                        int num8 = (int)(life * (num7 / (double)lifeMax));
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
                        if (lNPC2.TiemN == 0f)
                        {
                            if (lNPC2.MaxTime != 0)
                            {
                                if (!lNPC2.Config.不宣读信息)
                                {
                                    TShock.Utils.Broadcast("注意: " + val3.FullName + " 现身,攻略时间为 " + lNPC2.MaxTime + " 秒,血量为 " + val3.lifeMax + ",快快加入战斗吧!", Convert.ToByte(130), Convert.ToByte(255), Convert.ToByte(170));
                                }
                            }
                            else if (!lNPC2.Config.不宣读信息)
                            {
                                TShock.Utils.Broadcast("注意: " + val3.FullName + " 现身,血量为 " + val3.lifeMax + ",快快加入战斗吧!", Convert.ToByte(130), Convert.ToByte(255), Convert.ToByte(170));
                            }
                            foreach (声音节 item2 in lNPC2.Config.出场放音)
                            {
                                if (item2.声音ID >= 0)
                                {
                                    Terraria.NetMessage.PlayNetSound(new Terraria.NetMessage.NetSoundInfo(val3.Center, (ushort)1, item2.声音ID, item2.声音规模, item2.高音补偿), -1, -1);
                                }
                            }
                            foreach (string item3 in lNPC2.Config.出场命令)
                            {
                                Commands.HandleCommand((TSPlayer)(object)TSPlayer.Server, ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.CommandSilentSpecifier + item3);
                            }
                            if (lNPC2.Config.出场喊话 != "")
                            {
                                TShock.Utils.Broadcast((lNPC2.Config.出场喊话无头 ? "" : (val3.FullName + ": ")) + lNPC2.Config.出场喊话, Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255));
                            }
                            Sundry.SetMonsterMarkers(lNPC2.Config.出场怪物指示物修改, val3, ref rd);
                            Sundry.HurtMonster(lNPC2.Config.出场伤怪, val3);
                            Sundry.LaunchProjectile(lNPC2.Config.出场弹幕, val3, lNPC2);
                            foreach (KeyValuePair<int, int> item4 in lNPC2.Config.随从怪物)
                            {
                                if (item4.Value >= 1 && item4.Key != 0)
                                {
                                    int num9 = Math.Min(item4.Value, 200);
                                    NPC nPCById = TShock.Utils.GetNPCById(item4.Key);
                                    if (nPCById != null && nPCById.type != 113 && nPCById.type != 0 && nPCById.type < NPCID.Count)
                                    {
                                        TSPlayer.Server.SpawnNPC(nPCById.type, nPCById.FullName, num9, Terraria.Utils.ToTileCoordinates(val3.Center).X, Terraria.Utils.ToTileCoordinates(val3.Center).Y, 30, 15);
                                    }
                                }
                            }
                            lNPC2.BuffR = lNPC2.Config.状态范围;
                            lNPC2.RBuff = lNPC2.Config.周围状态;
                        }
                        lNPC2.Time++;
                        lNPC2.TiemN = lNPC2.Time / 100f;
                        if (lNPC2.MaxTime == 0)
                        {
                            goto IL_0e35;
                        }
                        int maxTime = lNPC2.MaxTime;
                        if ((double)lNPC2.TiemN == Math.Round(maxTime * 0.5) || (double)lNPC2.TiemN == Math.Round(maxTime * 0.7) || (double)lNPC2.TiemN == Math.Round(maxTime * 0.9))
                        {
                            maxTime -= (int)lNPC2.TiemN;
                            if (!lNPC2.Config.不宣读信息)
                            {
                                TShock.Utils.Broadcast("注意: " + val3.FullName + " 剩余攻略时间 " + maxTime + " 秒.", Convert.ToByte(130), Convert.ToByte(255), Convert.ToByte(170));
                            }
                            goto IL_0e35;
                        }
                        if (!(lNPC2.TiemN >= maxTime))
                        {
                            goto IL_0e35;
                        }
                        if (!lNPC2.Config.不宣读信息)
                        {
                            TShock.Utils.Broadcast("攻略失败: " + val3.FullName + " 已撤离.", Convert.ToByte(190), Convert.ToByte(150), Convert.ToByte(150));
                        }
                        int whoAmI = val3.whoAmI;
                        Main.npc[whoAmI] = new NPC();
                        Terraria.NetMessage.SendData(23, -1, -1, NetworkText.Empty, whoAmI, 0f, 0f, 0f, 0, 0, 0);
                        goto end_IL_022f;
                    IL_0e35:
                        int num10 = 0;
                        int num11 = 0;
                        int num12 = 0;
                        int num13 = 0;
                        int num14 = 0;
                        int num15 = 0;
                        int num16 = 0;
                        float num17 = 0f;
                        float num18 = 0f;
                        bool flag2 = false;
                        int num19 = 0;
                        long lNKC = getLNKC(val3.netID);
                        bool flag3 = false;
                        int num20 = 0;
                        if (val3.lifeMax > 0)
                        {
                            num20 = val3.life * 100 / val3.lifeMax;
                        }
                        if (lNPC2.TiemN != lNPC2.LTime)
                        {
                            int 时事件限 = lNPC2.Config.时事件限;
                            foreach (时间节 item5 in lNPC2.CTime)
                            {
                                if (时事件限 <= 0)
                                {
                                    break;
                                }
                                if (item5.可触发次 <= 0 && item5.可触发次 != -1)
                                {
                                    continue;
                                }
                                int num21 = (int)(item5.延迟秒数 * 100f);
                                int num22 = (int)(item5.消耗时间 * 100f);
                                if (num22 <= 0)
                                {
                                    continue;
                                }
                                if (item5.循环执行)
                                {
                                    if ((lNPC2.Time - num21) % num22 != 0)
                                    {
                                        continue;
                                    }
                                }
                                else if (num22 != lNPC2.Time - num21)
                                {
                                    continue;
                                }
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
                                        if (num20 < item5.血比条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (num20 >= Math.Abs(item5.血比条件))
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
                                        if (lNPC2.TiemN < item5.耗时条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNPC2.TiemN >= Math.Abs(item5.耗时条件))
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
                                if ((item5.昼夜条件 == -1 && Main.dayTime) || (item5.昼夜条件 == 1 && !Main.dayTime) || (item5.血月条件 == -1 && Main.bloodMoon) || (item5.血月条件 == 1 && !Main.bloodMoon) || (item5.日食条件 == -1 && Main.eclipse) || (item5.日食条件 == 1 && !Main.eclipse) || (item5.降雨条件 == -1 && Main.raining) || (item5.降雨条件 == 1 && !Main.raining) || (item5.肉山条件 == -1 && Main.hardMode) || (item5.肉山条件 == 1 && !Main.hardMode) || (item5.巨人条件 == -1 && NPC.downedGolemBoss) || (item5.巨人条件 == 1 && !NPC.downedGolemBoss) || (item5.月总条件 == -1 && NPC.downedMoonlord) || (item5.月总条件 == 1 && !NPC.downedMoonlord))
                                {
                                    continue;
                                }
                                if (item5.X轴条件 != 0)
                                {
                                    if (item5.X轴条件 > 0)
                                    {
                                        if (val3.Center.X < item5.X轴条件 << 4)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (val3.Center.X >= Math.Abs(item5.X轴条件 << 4))
                                    {
                                        continue;
                                    }
                                }
                                if (item5.Y轴条件 != 0)
                                {
                                    if (item5.Y轴条件 > 0)
                                    {
                                        if (val3.Center.Y < item5.Y轴条件 << 4)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (val3.Center.Y >= Math.Abs(item5.Y轴条件 << 4))
                                    {
                                        continue;
                                    }
                                }
                                if ((item5.面向条件 == 1 && (val3.direction != 1 || val3.directionY != 0)) || (item5.面向条件 == 2 && (val3.direction != 1 || val3.directionY != 1)) || (item5.面向条件 == 3 && (val3.direction != 0 || val3.directionY != 1)) || (item5.面向条件 == 4 && (val3.direction != -1 || val3.directionY != 1)) || (item5.面向条件 == 5 && (val3.direction != -1 || val3.directionY != 0)) || (item5.面向条件 == 6 && (val3.direction != -1 || val3.directionY != -1)) || (item5.面向条件 == 7 && (val3.direction != 0 || val3.directionY != -1)) || (item5.面向条件 == 8 && (val3.direction != 1 || val3.directionY != -1)) || (item5.面向条件 == 9 && val3.direction != 1) || (item5.面向条件 == 10 && val3.directionY != 1) || (item5.面向条件 == 11 && val3.direction != -1) || (item5.面向条件 == 12 && val3.directionY != -1) || item5.触发率子 <= 0 || item5.触发率母 <= 0 || (item5.触发率子 < item5.触发率母 && rd.Next(1, item5.触发率母 + 1) > item5.触发率子) || Sundry.NPCKillRequirement(item5.杀怪条件) || !lNPC2.haveMarkers(item5.指示物条件, val3) || Sundry.AIRequirement(item5.AI条件, val3) || Sundry.MonsterRequirement(item5.怪物条件, val3) || Sundry.PlayerRequirement(item5.玩家条件, val3))
                                {
                                    continue;
                                }
                                foreach (指示物节 item6 in item5.指示物修改)
                                {
                                    lNPC2.setMarkers(item6.名称, item6.数量, item6.清除, item6.指示物注入数量名, item6.指示物注入数量系数, item6.指示物注入数量运算符, item6.随机小, item6.随机大, ref rd, val3);
                                }
                                Sundry.SetMonsterMarkers(item5.怪物指示物修改, val3, ref rd);
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
                                if (item5.指示物注入AI赋值.Count > 0)
                                {
                                    for (int l = 0; l < val3.ai.Count(); l++)
                                    {
                                        if (item5.指示物注入AI赋值.ContainsKey(l) && item5.指示物注入AI赋值.TryGetValue(l, out string value4))
                                        {
                                            string[] array = value4.Split('*');
                                            float result = 1f;
                                            if (array.Length == 2 && array[1] != "")
                                            {
                                                float.TryParse(array[1], out result);
                                            }
                                            val3.ai[l] = lNPC2.getMarkers(value4) * result;
                                            val3.netUpdate = true;
                                        }
                                    }
                                }
                                foreach (声音节 item7 in item5.播放声音)
                                {
                                    if (item7.声音ID >= 0)
                                    {
                                        Terraria.NetMessage.PlayNetSound(new Terraria.NetMessage.NetSoundInfo(val3.Center, (ushort)1, item7.声音ID, item7.声音规模, item7.高音补偿), -1, -1);
                                    }
                                }
                                foreach (string item8 in item5.释放命令)
                                {
                                    Commands.HandleCommand((TSPlayer)(object)TSPlayer.Server, ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.CommandSilentSpecifier + lNPC2.ReplaceMarkers(item8));
                                }
                                if (item5.喊话 != "")
                                {
                                    TShock.Utils.Broadcast((item5.喊话无头 ? "" : (val3.FullName + ": ")) + lNPC2.ReplaceMarkers(item5.喊话), Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255));
                                }
                                if (item5.玩家复活时间 >= -1)
                                {
                                    lNPC2.RespawnSeconds = item5.玩家复活时间;
                                }
                                if (item5.全局最大刷怪数 >= -1)
                                {
                                    lNPC2.DefaultMaxSpawns = item5.全局最大刷怪数;
                                }
                                if (item5.全局刷怪速度 >= -1)
                                {
                                    lNPC2.DefaultSpawnRate = item5.全局刷怪速度;
                                }
                                if (item5.阻止传送器 != 0)
                                {
                                    lNPC2.BlockTeleporter = item5.阻止传送器;
                                }
                                Sundry.HurtMonster(item5.杀伤怪物, val3);
                                Sundry.LaunchProjectile(item5.释放弹幕, val3, lNPC2);
                                foreach (KeyValuePair<int, int> item9 in item5.召唤怪物)
                                {
                                    if (item9.Value >= 1 && item9.Key != 0)
                                    {
                                        int num23 = Math.Min(item9.Value, 200);
                                        NPC nPCById2 = TShock.Utils.GetNPCById(item9.Key);
                                        if (nPCById2 != null && nPCById2.type != 113 && nPCById2.type != 0 && nPCById2.type < NPCID.Count)
                                        {
                                            TSPlayer.Server.SpawnNPC(nPCById2.type, nPCById2.FullName, num23, Terraria.Utils.ToTileCoordinates(val3.Center).X, Terraria.Utils.ToTileCoordinates(val3.Center).Y, 5, 5);
                                        }
                                    }
                                }
                                if (item5.状态范围 > 0)
                                {
                                    lNPC2.BuffR = item5.状态范围;
                                    lNPC2.RBuff = item5.周围状态;
                                }
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
                                if (item5.比例回血 > 0)
                                {
                                    if (item5.比例回血 > 100)
                                    {
                                        item5.比例回血 = 100;
                                    }
                                    int num24 = val3.lifeMax * item5.比例回血 / 100;
                                    NPC val4 = val3;
                                    val4.life += num24;
                                    if (val3.life > val3.lifeMax)
                                    {
                                        val3.life = val3.lifeMax;
                                    }
                                    if (val3.life < 1)
                                    {
                                        val3.life = 1;
                                    }
                                    val3.HealEffect(num24, true);
                                    val3.netUpdate = true;
                                }
                                if (item5.怪物变形 >= 0 && (item5.怪物变形 != 113 || item5.怪物变形 != 114))
                                {
                                    val3.Transform(item5.怪物变形);
                                    val3.netUpdate = true;
                                }
                                if (item5.可触发次 != -1)
                                {
                                    时间节 时间节 = item5;
                                    时间节.可触发次--;
                                }
                                if (item5.击退范围 > 0 && item5.击退力度 != 0)
                                {
                                    num10 = item5.击退范围;
                                    num11 = item5.击退力度;
                                }
                                if (item5.杀伤范围 > 0 && item5.杀伤伤害 != 0)
                                {
                                    num12 = item5.杀伤范围;
                                    num13 = item5.杀伤伤害;
                                }
                                if (item5.拉取范围 > 0)
                                {
                                    num14 = item5.拉取起始;
                                    num15 = item5.拉取范围;
                                    num16 = item5.拉取止点;
                                    num17 = item5.拉取点X轴偏移 + lNPC2.getMarkers(item5.指示物数量注入拉取点X轴偏移名) * item5.指示物数量注入拉取点X轴偏移系数;
                                    num18 = item5.拉取点Y轴偏移 + lNPC2.getMarkers(item5.指示物数量注入拉取点Y轴偏移名) * item5.指示物数量注入拉取点Y轴偏移系数;
                                    flag2 = item5.初始拉取点坐标为零;
                                }
                                if (item5.反射范围 > 0)
                                {
                                    num19 = item5.反射范围;
                                }
                                if (item5.直接撤退)
                                {
                                    flag3 = true;
                                }
                                if (item5.跳出事件)
                                {
                                    break;
                                }
                                时事件限--;
                            }
                            lNPC2.LTime = lNPC2.TiemN;
                        }
                        if (lNPC2.LifeP != num20)
                        {
                            lNPC2.LifeP = num20;
                            if (lNPC2.LifeP != lNPC2.LLifeP)
                            {
                                int 血事件限 = lNPC2.Config.血事件限;
                                foreach (比例节 item10 in lNPC2.PLife)
                                {
                                    if (血事件限 <= 0)
                                    {
                                        break;
                                    }
                                    if (item10.血量剩余比例 <= 0 || (item10.可触发次 <= 0 && item10.可触发次 != -1) || item10.血量剩余比例 < lNPC2.LifeP || item10.血量剩余比例 >= lNPC2.LLifeP)
                                    {
                                        continue;
                                    }
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
                                            if (lNPC2.TiemN < item10.耗时条件)
                                            {
                                                continue;
                                            }
                                        }
                                        else if (lNPC2.TiemN >= Math.Abs(item10.耗时条件))
                                        {
                                            continue;
                                        }
                                    }
                                    if (item10.ID条件 != 0 && item10.ID条件 != val3.netID)
                                    {
                                        continue;
                                    }
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
                                    if ((item10.昼夜条件 == -1 && Main.dayTime) || (item10.昼夜条件 == 1 && !Main.dayTime) || (item10.血月条件 == -1 && Main.bloodMoon) || (item10.血月条件 == 1 && !Main.bloodMoon) || (item10.肉山条件 == -1 && Main.hardMode) || (item10.肉山条件 == 1 && !Main.hardMode) || (item10.巨人条件 == -1 && NPC.downedGolemBoss) || (item10.巨人条件 == 1 && !NPC.downedGolemBoss) || (item10.月总条件 == -1 && NPC.downedMoonlord) || (item10.月总条件 == 1 && !NPC.downedMoonlord))
                                    {
                                        continue;
                                    }
                                    if (item10.Y轴条件 != 0)
                                    {
                                        if (item10.Y轴条件 > 0)
                                        {
                                            if (val3.Center.Y < item10.Y轴条件 << 4)
                                            {
                                                continue;
                                            }
                                        }
                                        else if (val3.Center.Y >= Math.Abs(item10.Y轴条件 << 4))
                                        {
                                            continue;
                                        }
                                    }
                                    if ((item10.面向条件 == 1 && (val3.direction != 1 || val3.directionY != 0)) || (item10.面向条件 == 2 && (val3.direction != 1 || val3.directionY != 1)) || (item10.面向条件 == 3 && (val3.direction != 0 || val3.directionY != 1)) || (item10.面向条件 == 4 && (val3.direction != -1 || val3.directionY != 1)) || (item10.面向条件 == 5 && (val3.direction != -1 || val3.directionY != 0)) || (item10.面向条件 == 6 && (val3.direction != -1 || val3.directionY != -1)) || (item10.面向条件 == 7 && (val3.direction != 0 || val3.directionY != -1)) || (item10.面向条件 == 8 && (val3.direction != 1 || val3.directionY != -1)) || (item10.面向条件 == 9 && val3.direction != 1) || (item10.面向条件 == 10 && val3.directionY != 1) || (item10.面向条件 == 11 && val3.direction != -1) || (item10.面向条件 == 12 && val3.directionY != -1) || item10.触发率子 <= 0 || item10.触发率母 <= 0 || (item10.触发率子 < item10.触发率母 && rd.Next(1, item10.触发率母 + 1) > item10.触发率子) || Sundry.MonsterRequirement(item10.怪物条件, val3) || Sundry.PlayerRequirement(item10.玩家条件, val3))
                                    {
                                        continue;
                                    }
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
                                    Sundry.HurtMonster(item10.杀伤怪物, val3);
                                    Sundry.LaunchProjectile(item10.释放弹幕, val3, lNPC2);
                                    foreach (KeyValuePair<int, int> item11 in item10.召唤怪物)
                                    {
                                        if (item11.Value >= 1 && item11.Key != 0)
                                        {
                                            int num25 = Math.Min(item11.Value, 200);
                                            NPC nPCById3 = TShock.Utils.GetNPCById(item11.Key);
                                            if (nPCById3 != null && nPCById3.type != 113 && nPCById3.type != 0 && nPCById3.type < NPCID.Count)
                                            {
                                                TSPlayer.Server.SpawnNPC(nPCById3.type, nPCById3.FullName, num25, Terraria.Utils.ToTileCoordinates(val3.Center).X, Terraria.Utils.ToTileCoordinates(val3.Center).Y, 15, 15);
                                            }
                                        }
                                    }
                                    if (item10.状态范围 > 0)
                                    {
                                        lNPC2.BuffR = item10.状态范围;
                                        lNPC2.RBuff = item10.周围状态;
                                    }
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
                                    if (item10.比例回血 > 0)
                                    {
                                        if (item10.比例回血 > 100)
                                        {
                                            item10.比例回血 = 100;
                                        }
                                        NPC val4 = val3;
                                        val4.life += (int)(val3.lifeMax * (item10.比例回血 / 100.0));
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
                                    if (item10.可触发次 != -1)
                                    {
                                        比例节 比例节 = item10;
                                        比例节.可触发次--;
                                    }
                                    if (item10.击退范围 > 0 && item10.击退力度 != 0)
                                    {
                                        num10 = item10.击退范围;
                                        num11 = item10.击退力度;
                                    }
                                    if (item10.杀伤范围 > 0 && item10.杀伤伤害 != 0)
                                    {
                                        num12 = item10.杀伤范围;
                                        num13 = item10.杀伤伤害;
                                    }
                                    if (item10.拉取范围 > 0)
                                    {
                                        num14 = item10.拉取起始;
                                        num15 = item10.拉取范围;
                                        num16 = item10.拉取止点;
                                        num17 = item10.拉取点X轴偏移;
                                        num18 = item10.拉取点Y轴偏移;
                                        flag2 = false;
                                    }
                                    if (item10.直接撤退)
                                    {
                                        flag3 = true;
                                    }
                                    if (item10.跳出事件)
                                    {
                                        break;
                                    }
                                    血事件限--;
                                }
                                lNPC2.LLifeP = lNPC2.LifeP;
                            }
                        }
                        if (num19 > 0)
                        {
                            for (int m = 0; m < 1000; m++)
                            {
                                if (Main.projectile[m].active && Main.projectile[m].CanBeReflected() && val3.WithinRange(Main.projectile[m].Center, num19 << 4))
                                {
                                    val3.ReflectProjectile(Main.projectile[m]);
                                    Terraria.NetMessage.SendData(27, -1, -1, null, m, 0f, 0f, 0f, 0, 0, 0);
                                }
                            }
                        }
                        if (num16 > num14)
                        {
                            num16 = num14;
                        }
                        num16 *= 16;
                        float num26 = num17;
                        float num27 = num18;
                        if (!flag2)
                        {
                            num26 += val3.Center.X;
                            num27 += val3.Center.Y;
                        }
                        if (lNPC2.BuffR > 0 || (num10 > 0 && num11 != 0) || (num12 > 0 && num13 != 0) || num15 > 0)
                        {
                            TSPlayer[] players = TShock.Players;
                            foreach (TSPlayer val5 in players)
                            {
                                if (val5 == null)
                                {
                                    continue;
                                }
                                if (Timeout(now))
                                {
                                    ULock = false;
                                    return;
                                }
                                if (!val5.Active || val5.Dead || val5.TPlayer.statLife < 1)
                                {
                                    continue;
                                }
                                if (num15 > 0 && Sundry.WithinRange(val5.TPlayer.Center, (int)num26, (int)num27, num15 * 16))
                                {
                                    if (num14 > 0)
                                    {
                                        if (!Sundry.WithinRange(val5.TPlayer.Center, (int)num26, (int)num27, num14 * 16))
                                        {
                                            Sundry.PullTP(val5, num26, num27, num16);
                                        }
                                    }
                                    else
                                    {
                                        Sundry.PullTP(val5, num26, num27, num16);
                                    }
                                }
                                if (num12 > 0 && num13 != 0 && Sundry.WithinRange(val5.TPlayer.Center, val3.Center, num12 * 16))
                                {
                                    if (num13 < 0)
                                    {
                                        if (num13 > val5.TPlayer.statLifeMax2)
                                        {
                                            num13 = val5.TPlayer.statLifeMax2;
                                        }
                                        val5.Heal(Math.Abs(num13));
                                    }
                                    else if (num13 > val5.TPlayer.statLifeMax2 + val5.TPlayer.statDefense)
                                    {
                                        val5.KillPlayer();
                                    }
                                    else
                                    {
                                        val5.DamagePlayer(num13);
                                    }
                                }
                                if (num10 > 0 && num11 != 0 && Sundry.WithinRange(val5.TPlayer.Center, val3.Center, num10 * 16))
                                {
                                    Sundry.UserRepel(val5, val3.Center.X, val3.Center.Y, num11);
                                }
                                if (lNPC2.BuffR <= 0 || lNPC2.Time % 100 != 0 || !Sundry.WithinRange(val5.TPlayer.Center, val3.Center, lNPC2.BuffR * 16))
                                {
                                    continue;
                                }
                                foreach (状态节 item12 in lNPC2.RBuff)
                                {
                                    if ((item12.状态起始范围 <= 0 || !Sundry.WithinRange(val5.TPlayer.Center, val3.Center, item12.状态起始范围 * 16)) && (item12.状态结束范围 <= 0 || Sundry.WithinRange(val5.TPlayer.Center, val3.Center, item12.状态结束范围 * 16)))
                                    {
                                        val5.SetBuff(item12.状态ID, 100, false);
                                        if (item12.头顶提示 != "")
                                        {
                                            string 头顶提示 = item12.头顶提示;
                                            Color val6 = new Color(255, 255, 255);
                                            val5.SendData((PacketTypes)119, 头顶提示, (int)val6.PackedValue, val5.X, val5.Y, 0f, 0);
                                        }
                                    }
                                }
                            }
                        }
                        if (flag3)
                        {
                            int whoAmI2 = val3.whoAmI;
                            Main.npc[whoAmI2] = new NPC();
                            Terraria.NetMessage.SendData(23, -1, -1, NetworkText.Empty, whoAmI2, 0f, 0f, 0f, 0, 0, 0);
                            if (!lNPC2.Config.不宣读信息)
                            {
                                TShock.Utils.Broadcast("攻略失败: " + val3.FullName + " 已撤离.", Convert.ToByte(190), Convert.ToByte(150), Convert.ToByte(150));
                            }
                        }
                    end_IL_022f:;
                    }
                }
                int num28 = -1;
                int num29 = -1;
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
                            if (lNPC3.DefaultMaxSpawns > num28)
                            {
                                num28 = lNPC3.DefaultMaxSpawns;
                            }
                            if (lNPC3.DefaultSpawnRate > num29)
                            {
                                num29 = lNPC3.DefaultSpawnRate;
                            }
                        }
                    }
                }
                if (num28 >= 0 && NPC.defaultMaxSpawns != num28)
                {
                    NPC.defaultMaxSpawns = num28;
                }
                else if (num28 < 0)
                {
                    NPC.defaultMaxSpawns = ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.DefaultMaximumSpawns;
                }
                if (num29 >= 0 && NPC.defaultSpawnRate != num29)
                {
                    NPC.defaultSpawnRate = num29;
                }
                else if (num29 < 0)
                {
                    NPC.defaultSpawnRate = ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.DefaultSpawnRate;
                }
                if (_配置.启动死亡队友视角 && (!_配置.队友视角仅BOSS时 || flag))
                {
                    if (TeamP <= _配置.队友视角流畅度)
                    {
                        TeamP++;
                    }
                    else
                    {
                        TeamP = 0;
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
                            int num32 = -1;
                            float num33 = -1f;
                            for (int num34 = 0; num34 < 255; num34++)
                            {
                                if (Main.player[num34] != null && val8.Team == Main.player[num34].team && Main.player[num34].active && !Main.player[num34].dead)
                                {
                                    float num35 = Math.Abs(Main.player[num34].position.X + Main.player[num34].width / 2 - (val8.TPlayer.position.X + val8.TPlayer.width / 2)) + Math.Abs(Main.player[num34].position.Y + Main.player[num34].height / 2 - (val8.TPlayer.position.Y + val8.TPlayer.height / 2));
                                    if (num33 == -1f || num35 < num33)
                                    {
                                        num33 = num35;
                                        num32 = num34;
                                    }
                                }
                            }
                            if (num32 != -1 && !Sundry.WithinRange(val8.TPlayer.Center, Main.player[num32].Center, _配置.队友视角等待范围 * 16))
                            {
                                val8.TPlayer.position = Main.player[num32].position;
                                Terraria.NetMessage.SendData(13, -1, -1, NetworkText.Empty, val8.Index, 0f, 0f, 0f, 0, 0, 0);
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

        private void OnKillMe(object sender, KillMeEventArgs args)
        {
            if (((HandledEventArgs)(object)args).Handled || args.Player == null || args.Pvp)
            {
                return;
            }
            int num = -1;
            NPC[] npc = Main.npc;
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
                        lNPC.KillPlay++;
                        if (lNPC.RespawnSeconds > num)
                        {
                            num = lNPC.RespawnSeconds;
                        }
                    }
                }
            }
            if (((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.RespawnSeconds < 0 || ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.RespawnBossSeconds < 0)
            {
                return;
            }
            if (num >= 0)
            {
                if (LRespawnSeconds == -1 && LRespawnBossSeconds == -1)
                {
                    LRespawnSeconds = ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.RespawnSeconds;
                    LRespawnBossSeconds = ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.RespawnBossSeconds;
                }
                ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.RespawnSeconds = num;
                ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.RespawnBossSeconds = num;
            }
            else if (LRespawnSeconds >= 0 && LRespawnBossSeconds >= 0)
            {
                ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.RespawnSeconds = LRespawnSeconds;
                ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.RespawnBossSeconds = LRespawnBossSeconds;
                LRespawnSeconds = -1;
                LRespawnBossSeconds = -1;
            }
        }

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

        public void RD()
        {
            if (!File.Exists(NPCKillPath))
            {
                File.Create(NPCKillPath).Close();
            }
            using StreamReader streamReader = new StreamReader(NPCKillPath);
            string text = streamReader.ReadToEnd();
            string[] array = text.Split(Environment.NewLine.ToCharArray());
            if (array.Count() < 1 || !DateTime.TryParse(array[0], out var result))
            {
                return;
            }
            OServerDataTime = result;
            for (int i = 1; i < array.Count(); i++)
            {
                string[] array2 = array[i].Split('|');
                if (array2.Length == 2)
                {
                    LNkc.Add(new LNKC(int.Parse(array2[0]), long.Parse(array2[1])));
                }
            }
        }
    }
}