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
using TShockAPI.Hooks;
using static TShockAPI.GetDataHandlers;
using Main = Terraria.Main;
using NPC = Terraria.NPC;


namespace TestPlugin
{
    [ApiVersion(2, 1)]
    public class TestPlugin : TerrariaPlugin
    {
        public 读配置文件.配置文件 config = new 读配置文件.配置文件();

        private static readonly System.Timers.Timer Update = new System.Timers.Timer(100.0);

        public static bool ULock = false;

        public int LRespawnSeconds = -1;



        public int LRespawnBossSeconds = -1;

        public int TeamP = 0;

        public override string Author => "GK 羽学";

        public override string Description => "自定义怪物出没时的血量,当然不止这些！";

        public override string Name => "自定义怪物血量";

        public override Version Version => new Version(1, 0, 4, 9);

        public string _配置路径 => Path.Combine(TShock.SavePath, "自定义怪物血量.json");

        public string NPCKillPath => Path.Combine(TShock.SavePath, "自定义怪物血量存档.txt");

        public DateTime NPCKillDataTime { get; set; }

        public DateTime OServerDataTime { get; set; }

        private static List<LNKC>? LNkc { get; set; }

        public static LNPC[]? LNpcs { get; set; }

        public TestPlugin(Main game)
            : base(game)
        {
            base.Order = 1;
            LNpcs = new LNPC[201];
            LNkc = new List<LNKC>();
            NPCKillDataTime = DateTime.UtcNow;
            OServerDataTime = DateTime.UtcNow;
        }

        public override void Initialize()
        {
            RD();
            LoadConfig();
            GeneralHooks.ReloadEvent += LoadConfig;
            GetDataHandlers.KillMe += (EventHandler<KillMeEventArgs>)OnKillMe!;
            ServerApi.Hooks.GamePostInitialize.Register((TerrariaPlugin)(object)this, PostInitialize);
            ServerApi.Hooks.NpcSpawn.Register((TerrariaPlugin)(object)this, NpcSpawn);
            ServerApi.Hooks.NpcKilled.Register((TerrariaPlugin)(object)this, NpcKilled);
            ServerApi.Hooks.NetSendData.Register((TerrariaPlugin)(object)this, SendData);
            On.Terraria.NPC.SetDefaults += OnSetDefaults;
            On.Terraria.Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += Projectile_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (LNkc!)
                {
                    SD(notime: true);
                }
                Update.Elapsed -= OnUpdate!;
                KillMe -= (EventHandler<KillMeEventArgs>)OnKillMe!;
                ServerApi.Hooks.NpcSpawn.Deregister((TerrariaPlugin)(object)this, NpcSpawn);
                ServerApi.Hooks.NpcKilled.Deregister((TerrariaPlugin)(object)this, NpcKilled);
                ServerApi.Hooks.NetSendData.Deregister((TerrariaPlugin)(object)this, SendData);
                ServerApi.Hooks.GamePostInitialize.Deregister((TerrariaPlugin)(object)this, PostInitialize);
            }
            base.Dispose(disposing);
        }

        public void PostInitialize(EventArgs e)
        {
            Update.Elapsed += OnUpdate!;
            Update.Start();
        }

        #region 配置文件创建与重读加载方法
        private void LoadConfig(ReloadEventArgs args = null!)
        {
            RD();
            config = 配置文件.Read(_配置路径);
            config.Write(_配置路径);
            if (args != null && args.Player != null)
            {
                args.Player.SendSuccessMessage("[自定义怪物血量]重新加载配置完毕。");
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

        #endregion


        private void OnSetDefaults(On.Terraria.NPC.orig_SetDefaults orig, NPC self, int Type, Terraria.NPCSpawnParams spawnparams)
        {
            if (config.统一初始怪物玩家系数 > 0 || config.统一初始怪物强化系数 > 0f)
            {
                if (config.统一初始怪物玩家系数 > 0)
                {
                    spawnparams.playerCountForMultiplayerDifficultyOverride = config.统一初始怪物玩家系数;
                    if (spawnparams.playerCountForMultiplayerDifficultyOverride > 1000)
                    {
                        spawnparams.playerCountForMultiplayerDifficultyOverride = 1000;
                    }
                    if (config.统一初始玩家系数不低于人数)
                    {
                        int activePlayerCount = TShock.Utils.GetActivePlayerCount();
                        if (spawnparams.playerCountForMultiplayerDifficultyOverride < activePlayerCount)
                        {
                            spawnparams.playerCountForMultiplayerDifficultyOverride = activePlayerCount;
                        }
                    }
                }
                if (config.统一初始怪物强化系数 > 0f)
                {
                    spawnparams.strengthMultiplierOverride = config.统一初始怪物强化系数;
                    if (spawnparams.strengthMultiplierOverride > 1000f)
                    {
                        spawnparams.strengthMultiplierOverride = 1000f;
                    }
                }
            }
            orig.Invoke(self, Type, spawnparams);
        }

        private int Projectile_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float(On.Terraria.Projectile.orig_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float orig, IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner, float ai0, float ai1, float ai2)
        {
            if (Owner == Main.myPlayer && config.统一怪物弹幕伤害修正 != 1f)
            {
                Damage = (int)(Damage * config.统一怪物弹幕伤害修正);
            }
            return orig.Invoke(spawnSource, X, Y, SpeedX, SpeedY, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
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
            怪物节 config = null!;
            int maxtime = 0;
            long lNKC = getLNKC(Main.npc[args.NpcId].netID);
            int num = 0;
            int num2 = 0;
            float strengthMultiplier = Terraria.Main.npc[args.NpcId].strengthMultiplier;
            Terraria.NPC[] npc = Main.npc;
            foreach (Terraria.NPC val in npc)
            {
                if (val.netID == Terraria.Main.npc[args.NpcId].netID)
                {
                    num++;
                }
            }
            Random random = new Random();
            怪物节[] 怪物节集 = this.config.怪物节集;
            foreach (怪物节 怪物节 in 怪物节集)
            {
                if (Main.npc[args.NpcId].netID != 怪物节.怪物ID && 怪物节.怪物ID != 0 || 怪物节.怪物ID == 0 && 怪物节.再匹配.Count() > 0 && !怪物节.再匹配.Contains(Main.npc[args.NpcId].netID))
                {
                    continue;
                }
                num2 = 怪物节.开服时间型 == 1 ? (int)(DateTime.UtcNow - OServerDataTime).TotalDays : 怪物节.开服时间型 != 2 ? (int)(DateTime.UtcNow - OServerDataTime).TotalHours : (int)(DateTime.UtcNow.Date - OServerDataTime.Date).TotalDays;
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
                if (怪物节.昼夜条件 == -1 && Terraria.Main.dayTime || 怪物节.昼夜条件 == 1 && !Terraria.Main.dayTime || 怪物节.血月条件 == -1 && Main.bloodMoon || 怪物节.血月条件 == 1 && !Main.bloodMoon || 怪物节.肉山条件 == -1 && Main.hardMode || 怪物节.肉山条件 == 1 && !Main.hardMode || 怪物节.巨人条件 == -1 && Terraria.NPC.downedGolemBoss || 怪物节.巨人条件 == 1 && !Terraria.NPC.downedGolemBoss || 怪物节.月总条件 == -1 && NPC.downedMoonlord || 怪物节.月总条件 == 1 && !NPC.downedMoonlord || 怪物节.出没率子 <= 0 || 怪物节.出没率母 <= 0 || 怪物节.出没率子 < 怪物节.出没率母 && random.Next(1, 怪物节.出没率母 + 1) > 怪物节.出没率子 || Sundry.MonsterRequirement(怪物节.怪物条件, Main.npc[args.NpcId]))
                {
                    continue;
                }
                if (this.config.启动怪物时间限制 && (怪物节.人秒系数 != 0 || 怪物节.出没秒数 != 0 || 怪物节.开服系秒 != 0 || 怪物节.杀数系秒 != 0))
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
                    Terraria.NPC obj = Main.npc[args.NpcId];
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
            if (!this.config.统一设置例外怪物.Contains(Main.npc[args.NpcId].netID))
            {
                if (this.config.统一怪物血量倍数 != 1.0 && this.config.统一怪物血量倍数 > 0.0)
                {
                    Main.npc[args.NpcId].lifeMax = (int)(Main.npc[args.NpcId].lifeMax * this.config.统一怪物血量倍数);
                    if (Main.npc[args.NpcId].lifeMax < 1)
                    {
                        Main.npc[args.NpcId].lifeMax = 1;
                    }
                    flag = true;
                }
                if (Main.npc[args.NpcId].lifeMax < lifeMax && this.config.统一血量不低于正常)
                {
                    Main.npc[args.NpcId].lifeMax = lifeMax;
                    flag = false;
                }
                if (this.config.统一怪物强化倍数 != 1.0 && this.config.统一怪物强化倍数 > 0.0)
                {
                    float num6 = (float)(Main.npc[args.NpcId].strengthMultiplier * this.config.统一怪物强化倍数);
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
                if (Main.npc[args.NpcId].strengthMultiplier < strengthMultiplier && this.config.统一强化不低于正常)
                {
                    Main.npc[args.NpcId].strengthMultiplier = strengthMultiplier;
                    flag = false;
                }
                if (this.config.统一免疫熔岩类型 == 1)
                {
                    Main.npc[args.NpcId].lavaImmune = true;
                    Main.npc[args.NpcId].netUpdate = true;
                }
                if (this.config.统一免疫熔岩类型 == -1)
                {
                    Main.npc[args.NpcId].lavaImmune = false;
                    Main.npc[args.NpcId].netUpdate = true;
                }
                if (this.config.统一免疫陷阱类型 == 1)
                {
                    Main.npc[args.NpcId].trapImmune = true;
                    Main.npc[args.NpcId].netUpdate = true;
                }
                if (this.config.统一免疫陷阱类型 == -1)
                {
                    Main.npc[args.NpcId].trapImmune = false;
                    Main.npc[args.NpcId].netUpdate = true;
                }
                if (this.config.统一对怪物伤害修正 != 1f)
                {
                    Terraria.NPC obj2 = Terraria.Main.npc[args.NpcId];
                    obj2.takenDamageMultiplier *= this.config.统一对怪物伤害修正;
                }
            }
            if (flag)
            {
                Terraria.Main.npc[args.NpcId].life = Terraria.Main.npc[args.NpcId].lifeMax;
                Terraria.Main.npc[args.NpcId].netUpdate = true;
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
                        foreach (读配置文件.声音节 item in LNpcs[args.npc.whoAmI].Config.死亡放音)
                        {
                            if (item.声音ID >= 0)
                            {
                                Terraria.NetMessage.PlayNetSound(new Terraria.NetMessage.NetSoundInfo(args.npc.Center, 1, item.声音ID, item.声音规模, item.高音补偿), -1, -1);
                            }
                        }
                        foreach (string item2 in LNpcs[args.npc.whoAmI].Config.死亡命令)
                        {
                            Commands.HandleCommand((TSPlayer)(object)TSPlayer.Server, ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.CommandSilentSpecifier + item2);
                        }
                        if (LNpcs[args.npc.whoAmI].Config.死亡喊话 != "")
                        {
                            TShock.Utils.Broadcast(args.npc.FullName + ": " + LNpcs[args.npc.whoAmI].Config.死亡喊话, Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255));
                        }
                        long lNKC = getLNKC(args.npc.netID);
                        int num = LNpcs[args.npc.whoAmI].Config.掉落组限;
                        Random random = new Random();
                        foreach (掉落节 item3 in LNpcs[args.npc.whoAmI].Config.额外掉落)
                        {
                            if (num <= 0)
                            {
                                break;
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
                            if (item3.昼夜条件 == -1 && Main.dayTime || item3.昼夜条件 == 1 && !Main.dayTime || item3.血月条件 == -1 && Main.bloodMoon || item3.血月条件 == 1 && !Main.bloodMoon || item3.肉山条件 == -1 && Main.hardMode || item3.肉山条件 == 1 && !Main.hardMode || item3.巨人条件 == -1 && NPC.downedGolemBoss || item3.巨人条件 == 1 && !NPC.downedGolemBoss || item3.月总条件 == -1 && NPC.downedMoonlord || item3.月总条件 == 1 && !NPC.downedMoonlord || item3.掉落率子 <= 0 || item3.掉落率母 <= 0 || item3.掉落率子 < item3.掉落率母 && random.Next(1, item3.掉落率母 + 1) > item3.掉落率子 || Sundry.MonsterRequirement(item3.怪物条件, args.npc))
                            {
                                continue;
                            }
                            foreach (读配置文件.物品节 item4 in item3.掉落物品)
                            {
                                if (item4.物品数量 <= 0 || item4.物品ID <= 0)
                                {
                                    continue;
                                }
                                if (item4.物品前缀 >= 0)
                                {
                                    if (item4.独立掉落)
                                    {
                                        args.npc.DropItemInstanced(args.npc.position, args.npc.Size, item4.物品ID, item4.物品数量, true);
                                        continue;
                                    }
                                    int num2 = Terraria.Item.NewItem(new EntitySource_DebugCommand(), args.npc.position, args.npc.Size, item4.物品ID, item4.物品数量, false, item4.物品前缀, false, false);
                                    Terraria.NetMessage.TrySendData(21, -1, -1, null, num2, 0f, 0f, 0f, 0, 0, 0);
                                }
                                else if (item4.独立掉落)
                                {
                                    args.npc.DropItemInstanced(args.npc.position, args.npc.Size, item4.物品ID, item4.物品数量, true);
                                }
                                else
                                {
                                    int num3 = Terraria.Item.NewItem(new EntitySource_DebugCommand(), args.npc.position, args.npc.Size, item4.物品ID, item4.物品数量, false, 0, false, false);
                                    Terraria.NetMessage.TrySendData(21, -1, -1, null, num3, 0f, 0f, 0f, 0, 0, 0);
                                }
                            }
                            if (item3.跳出掉落)
                            {
                                break;
                            }
                            num--;
                        }
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
                                    TSPlayer.Server.SpawnNPC(nPCById.type, nPCById.FullName, num4, Terraria.Utils.ToTileCoordinates(args.npc.position).X, Terraria.Utils.ToTileCoordinates(args.npc.position).Y, 3, 3);
                                }
                            }
                        }
                        if (LNpcs[args.npc.whoAmI].Config.死状范围 > 0)
                        {
                            TSPlayer[] players = TShock.Players;
                            foreach (TSPlayer val in players)
                            {
                                if (val == null || val.Dead || val.TPlayer.statLife < 1 || !val.IsInRange(Terraria.Utils.ToTileCoordinates(args.npc.position).X, Terraria.Utils.ToTileCoordinates(args.npc.position).Y, LNpcs[args.npc.whoAmI].Config.死状范围))
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
            Random random = new Random();
            if (Main.rand == null)
            {
                Main.rand = new UnifiedRandom();
            }
            try
            {
                int activePlayerCount = TShock.Utils.GetActivePlayerCount();
                int num = 0;
                NPC[] npc2 = Main.npc;
                foreach (NPC val in npc2)
                {
                    if (val != null && val.active)
                    {
                        num++;
                    }
                }
                bool flag = false;
                NPC[] npc3 = Main.npc;
                foreach (NPC npc in npc3)
                {
                    if (activePlayerCount <= 0 || npc == null || !npc.active)
                    {
                        continue;
                    }
                    if (Timeout(now))
                    {
                        ULock = false;
                        return;
                    }
                    if (npc.boss)
                    {
                        flag = true;
                    }
                    lock (LNpcs)
                    {
                        LNPC lNPC = LNpcs[npc.whoAmI];
                        if (lNPC == null || lNPC.Config == null)
                        {
                            continue;
                        }
                        if (lNPC.PlayerCount < activePlayerCount)
                        {
                            lNPC.PlayerCount = activePlayerCount;
                            if (config.启动动态时间限制 && (lNPC.Config.人秒系数 != 0 || lNPC.Config.出没秒数 != 0 || lNPC.Config.开服系秒 != 0 || lNPC.Config.杀数系秒 != 0))
                            {
                                int num2 = lNPC.PlayerCount * lNPC.Config.人秒系数;
                                num2 += lNPC.Config.出没秒数;
                                int num3 = lNPC.PlayerCount * lNPC.Config.人秒系数;
                                num3 += lNPC.Config.出没秒数;
                                num3 += lNPC.OSTime * lNPC.Config.开服系秒;
                                num3 += (int)lNPC.LKC * lNPC.Config.杀数系秒;
                                if (num3 < 1)
                                {
                                    num3 = -1;
                                }
                                if (lNPC.MaxTime != num3)
                                {
                                    lNPC.MaxTime = num3;
                                    int value = num2 - num3;
                                    if (num2 > num3)
                                    {
                                        if (!lNPC.Config.不宣读信息)
                                        {
                                            TShock.Utils.Broadcast("注意: " + npc.FullName + " 受服务器人数增多影响攻略时间减少 " + value + " 秒剩" + (int)(num3 - lNPC.TiemN) + "秒.", Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(100));
                                        }
                                    }
                                    else if (num2 < num3 && !lNPC.Config.不宣读信息)
                                    {
                                        TShock.Utils.Broadcast("注意: " + npc.FullName + " 受服务器人数增多影响攻略时间增加 " + Math.Abs(value) + "秒剩" + (int)(num3 - lNPC.TiemN) + "秒.", Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(100));
                                    }
                                }
                            }
                            if (config.启动动态血量上限 && (lNPC.Config.玩家系数 != 0 || lNPC.Config.怪物血量 != 0 || lNPC.Config.开服系数 != 0 || lNPC.Config.杀数系数 != 0))
                            {
                                int num4 = activePlayerCount * lNPC.Config.玩家系数;
                                num4 += lNPC.Config.怪物血量;
                                num4 += lNPC.OSTime * lNPC.Config.开服系数;
                                num4 += (int)lNPC.LKC * lNPC.Config.杀数系数;
                                if (!lNPC.Config.覆盖原血量)
                                {
                                    num4 += npc.lifeMax;
                                }
                                if (num4 < 1)
                                {
                                    num4 = 1;
                                }
                                if (lNPC.MaxLife < num4 || !lNPC.Config.不低于正常)
                                {
                                    int lifeMax = npc.lifeMax;
                                    int num5 = num4;
                                    if (!config.统一设置例外怪物.Contains(npc.netID))
                                    {
                                        if (config.统一怪物血量倍数 != 1.0 && config.统一怪物血量倍数 > 0.0)
                                        {
                                            num5 = (int)(num5 * config.统一怪物血量倍数);
                                            if (num5 < 1)
                                            {
                                                num5 = 1;
                                            }
                                        }
                                        if (config.统一血量不低于正常 && num5 < lNPC.MaxLife)
                                        {
                                            num5 = lNPC.MaxLife;
                                        }
                                    }
                                    if (lifeMax != num5)
                                    {
                                        int life = npc.life;
                                        int num6 = (int)(life * (num5 / (double)lifeMax));
                                        if (num6 < 1)
                                        {
                                            num6 = 1;
                                        }
                                        npc.lifeMax = num5;
                                        npc.life = num6;
                                        int value2 = life - num6;
                                        if (life > num6)
                                        {
                                            if (!lNPC.Config.不宣读信息)
                                            {
                                                TShock.Utils.Broadcast("注意: " + npc.FullName + " 受服务器人数增多影响怪物血量减少 " + value2 + " 剩" + npc.life + ".", Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(100));
                                            }
                                        }
                                        else if (life < num6 && !lNPC.Config.不宣读信息)
                                        {
                                            TShock.Utils.Broadcast("注意: " + npc.FullName + " 受服务器人数增多影响怪物血量增加 " + Math.Abs(value2) + "剩" + npc.life + ".", Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(100));
                                        }
                                    }
                                }
                            }
                        }
                        if (lNPC.Config == null)
                        {
                            continue;
                        }
                        if (lNPC.TiemN == 0f)
                        {
                            if (lNPC.MaxTime != 0)
                            {
                                if (!lNPC.Config.不宣读信息)
                                {
                                    TShock.Utils.Broadcast("注意: " + npc.FullName + " 现身,攻略时间为 " + lNPC.MaxTime + " 秒,血量为 " + npc.lifeMax + ",快快加入战斗吧!", Convert.ToByte(130), Convert.ToByte(255), Convert.ToByte(170));
                                }
                            }
                            else if (!lNPC.Config.不宣读信息)
                            {
                                TShock.Utils.Broadcast("注意: " + npc.FullName + " 现身,血量为 " + npc.lifeMax + ",快快加入战斗吧!", Convert.ToByte(130), Convert.ToByte(255), Convert.ToByte(170));
                            }
                            foreach (读配置文件.声音节 item in lNPC.Config.出场放音)
                            {
                                if (item.声音ID >= 0)
                                {
                                    Terraria.NetMessage.PlayNetSound(new Terraria.NetMessage.NetSoundInfo(npc.Center, 1, item.声音ID, item.声音规模, item.高音补偿), -1, -1);
                                }
                            }
                            foreach (string item2 in lNPC.Config.出场命令)
                            {
                                Commands.HandleCommand((TSPlayer)(object)TSPlayer.Server, ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.CommandSilentSpecifier + item2);
                            }
                            if (lNPC.Config.出场喊话 != "")
                            {
                                TShock.Utils.Broadcast(npc.FullName + ": " + lNPC.Config.出场喊话, Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255));
                            }
                            Sundry.HurtMonster(lNPC.Config.出场伤怪, npc);
                            Sundry.LaunchProjectile(lNPC.Config.出场弹幕, npc, lNPC);
                            foreach (KeyValuePair<int, int> item3 in lNPC.Config.随从怪物)
                            {
                                if (item3.Value >= 1 && item3.Key != 0)
                                {
                                    int num7 = Math.Min(item3.Value, 200);
                                    NPC nPCById = TShock.Utils.GetNPCById(item3.Key);
                                    if (nPCById != null && nPCById.type != 113 && nPCById.type != 0 && nPCById.type < NPCID.Count)
                                    {
                                        TSPlayer.Server.SpawnNPC(nPCById.type, nPCById.FullName, num7, Terraria.Utils.ToTileCoordinates(npc.position).X, Terraria.Utils.ToTileCoordinates(npc.position).Y, 30, 15);
                                    }
                                }
                            }
                            lNPC.BuffR = lNPC.Config.状态范围;
                            lNPC.RBuff = lNPC.Config.周围状态;
                        }
                        lNPC.Time++;
                        lNPC.TiemN = lNPC.Time / 10f;
                        if (lNPC.MaxTime == 0)
                        {
                            goto IL_0d95;
                        }
                        int maxTime = lNPC.MaxTime;
                        if ((double)lNPC.TiemN == Math.Round(maxTime * 0.5) || (double)lNPC.TiemN == Math.Round(maxTime * 0.7) || (double)lNPC.TiemN == Math.Round(maxTime * 0.9))
                        {
                            maxTime -= (int)lNPC.TiemN;
                            if (!lNPC.Config.不宣读信息)
                            {
                                TShock.Utils.Broadcast("注意: " + npc.FullName + " 剩余攻略时间 " + maxTime + " 秒.", Convert.ToByte(130), Convert.ToByte(255), Convert.ToByte(170));
                            }
                            goto IL_0d95;
                        }
                        if (!(lNPC.TiemN >= maxTime))
                        {
                            goto IL_0d95;
                        }
                        if (!lNPC.Config.不宣读信息)
                        {
                            TShock.Utils.Broadcast("攻略失败: " + npc.FullName + " 已撤离.", Convert.ToByte(190), Convert.ToByte(150), Convert.ToByte(150));
                        }
                        int whoAmI = npc.whoAmI;
                        Main.npc[whoAmI] = new NPC();
                        Terraria.NetMessage.SendData(23, -1, -1, NetworkText.Empty, whoAmI, 0f, 0f, 0f, 0, 0, 0);
                        goto end_IL_0134;
                    IL_0d95:
                        int num8 = 0;
                        int num9 = 0;
                        int num10 = 0;
                        int num11 = 0;
                        int num12 = 0;
                        int num13 = 0;
                        int num14 = 0;
                        float num15 = 0f;
                        float num16 = 0f;
                        int num17 = 0;
                        long lNKC = getLNKC(npc.netID);
                        bool flag2 = false;
                        int num18 = 0;
                        if (npc.lifeMax > 0)
                        {
                            num18 = npc.life * 100 / npc.lifeMax;
                        }
                        if (lNPC.TiemN != lNPC.LTime)
                        {
                            int 时事件限 = lNPC.Config.时事件限;
                            foreach (读配置文件.时间节 item4 in lNPC.CTime)
                            {
                                if (时事件限 <= 0)
                                {
                                    break;
                                }
                                if (item4.可触发次 <= 0 && item4.可触发次 != -1)
                                {
                                    continue;
                                }
                                int num19 = (int)(item4.延迟秒数 * 10f);
                                int num20 = (int)(item4.消耗时间 * 10f);
                                if (num20 <= 0)
                                {
                                    continue;
                                }
                                if (item4.循环执行)
                                {
                                    if ((lNPC.Time - num19) % num20 != 0)
                                    {
                                        continue;
                                    }
                                }
                                else if (num20 != lNPC.Time - num19)
                                {
                                    continue;
                                }
                                if (item4.杀数条件 != 0)
                                {
                                    if (item4.杀数条件 > 0)
                                    {
                                        if (lNKC < item4.杀数条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNKC >= Math.Abs(item4.杀数条件))
                                    {
                                        continue;
                                    }
                                }
                                if (item4.怪数条件 != 0)
                                {
                                    if (item4.怪数条件 > 0)
                                    {
                                        if (num < item4.怪数条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (num >= Math.Abs(item4.怪数条件))
                                    {
                                        continue;
                                    }
                                }
                                if (item4.血比条件 != 0)
                                {
                                    if (item4.血比条件 > 0)
                                    {
                                        if (num18 < item4.血比条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (num18 >= Math.Abs(item4.血比条件))
                                    {
                                        continue;
                                    }
                                }
                                if (item4.人数条件 != 0)
                                {
                                    if (item4.人数条件 > 0)
                                    {
                                        if (lNPC.PlayerCount < item4.人数条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNPC.PlayerCount >= Math.Abs(item4.人数条件))
                                    {
                                        continue;
                                    }
                                }
                                if (item4.耗时条件 != 0)
                                {
                                    if (item4.耗时条件 > 0)
                                    {
                                        if (lNPC.TiemN < item4.耗时条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNPC.TiemN >= Math.Abs(item4.耗时条件))
                                    {
                                        continue;
                                    }
                                }
                                if (item4.ID条件 != 0 && item4.ID条件 != npc.netID)
                                {
                                    continue;
                                }
                                if (item4.杀死条件 != 0)
                                {
                                    if (item4.杀死条件 > 0)
                                    {
                                        if (lNPC.KillPlay < item4.杀死条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNPC.KillPlay >= Math.Abs(item4.杀死条件))
                                    {
                                        continue;
                                    }
                                }
                                if (item4.开服条件 != 0)
                                {
                                    if (item4.开服条件 > 0)
                                    {
                                        if (lNPC.OSTime < item4.开服条件)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (lNPC.OSTime >= Math.Abs(item4.开服条件))
                                    {
                                        continue;
                                    }
                                }
                                if (item4.昼夜条件 == -1 && Main.dayTime || item4.昼夜条件 == 1 && !Main.dayTime || item4.血月条件 == -1 && Main.bloodMoon || item4.血月条件 == 1 && !Main.bloodMoon || item4.肉山条件 == -1 && Main.hardMode || item4.肉山条件 == 1 && !Main.hardMode || item4.巨人条件 == -1 && NPC.downedGolemBoss || item4.巨人条件 == 1 && !NPC.downedGolemBoss || item4.月总条件 == -1 && NPC.downedMoonlord || item4.月总条件 == 1 && !NPC.downedMoonlord)
                                {
                                    continue;
                                }
                                if (item4.X轴条件 != 0)
                                {
                                    if (item4.X轴条件 > 0)
                                    {
                                        if (npc.Center.X < item4.X轴条件 << 4)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (npc.Center.X >= Math.Abs(item4.X轴条件 << 4))
                                    {
                                        continue;
                                    }
                                }
                                if (item4.Y轴条件 != 0)
                                {
                                    if (item4.Y轴条件 > 0)
                                    {
                                        if (npc.Center.Y < item4.Y轴条件 << 4)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (npc.Center.Y >= Math.Abs(item4.Y轴条件 << 4))
                                    {
                                        continue;
                                    }
                                }
                                if (item4.面向条件 == 1 && (npc.direction != 1 || npc.directionY != 0) || item4.面向条件 == 2 && (npc.direction != 1 || npc.directionY != 1) || item4.面向条件 == 3 && (npc.direction != 0 || npc.directionY != 1) || item4.面向条件 == 4 && (npc.direction != -1 || npc.directionY != 1) || item4.面向条件 == 5 && (npc.direction != -1 || npc.directionY != 0) || item4.面向条件 == 6 && (npc.direction != -1 || npc.directionY != -1) || item4.面向条件 == 7 && (npc.direction != 0 || npc.directionY != -1) || item4.面向条件 == 8 && (npc.direction != 1 || npc.directionY != -1) || item4.面向条件 == 9 && npc.direction != 1 || item4.面向条件 == 10 && npc.directionY != 1 || item4.面向条件 == 11 && npc.direction != -1 || item4.面向条件 == 12 && npc.directionY != -1 || item4.触发率子 <= 0 || item4.触发率母 <= 0 || item4.触发率子 < item4.触发率母 && random.Next(1, item4.触发率母 + 1) > item4.触发率子)
                                {
                                    continue;
                                }
                                bool flag3 = false;
                                foreach (读配置文件.指示物组节 item5 in item4.指示物条件)
                                {
                                    int markers = lNPC.getMarkers(item5.名称);
                                    if (item5.数量 == 0)
                                    {
                                        continue;
                                    }
                                    if (item5.数量 > 0)
                                    {
                                        if (markers < item5.数量)
                                        {
                                            flag3 = true;
                                            break;
                                        }
                                    }
                                    else if (markers >= Math.Abs(item5.数量))
                                    {
                                        flag3 = true;
                                        break;
                                    }
                                }
                                if (flag3)
                                {
                                    continue;
                                }
                                if (item4.AI条件.Count > 0)
                                {
                                    for (int k = 0; k < npc.ai.Count(); k++)
                                    {
                                        string key = k.ToString();
                                        if (item4.AI条件.ContainsKey(key) && item4.AI条件.TryGetValue(key, out var value3) && npc.ai[k] != value3)
                                        {
                                            flag3 = true;
                                            break;
                                        }
                                        key = "!" + k;
                                        if (item4.AI条件.ContainsKey(key) && item4.AI条件.TryGetValue(key, out var value4) && npc.ai[k] == value4)
                                        {
                                            flag3 = true;
                                            break;
                                        }
                                        key = ">" + k;
                                        if (item4.AI条件.ContainsKey(key) && item4.AI条件.TryGetValue(key, out var value5) && npc.ai[k] <= value5)
                                        {
                                            flag3 = true;
                                            break;
                                        }
                                        key = "<" + k;
                                        if (item4.AI条件.ContainsKey(key) && item4.AI条件.TryGetValue(key, out var value6) && npc.ai[k] >= value6)
                                        {
                                            flag3 = true;
                                            break;
                                        }
                                    }
                                    if (flag3)
                                    {
                                        continue;
                                    }
                                }
                                if (Sundry.MonsterRequirement(item4.怪物条件, npc))
                                {
                                    continue;
                                }
                                foreach (读配置文件.玩家条件节 monster2 in item4.玩家条件)
                                {
                                    if (monster2.范围内 <= 0)
                                    {
                                        continue;
                                    }
                                    int num21 = 0;
                                    num21 = monster2.范围起 <= 0 ? TShock.Players.Count((p) => p != null && p.Active && !p.Dead && p.TPlayer.statLife > 0 && npc.WithinRange(p.TPlayer.position, monster2.范围内 << 4)) : TShock.Players.Count((p) => p != null && p.Active && !p.Dead && p.TPlayer.statLife > 0 && !npc.WithinRange(p.TPlayer.position, monster2.范围起 << 4) && npc.WithinRange(p.TPlayer.position, monster2.范围内 << 4));
                                    if (monster2.符合数 == 0)
                                    {
                                        continue;
                                    }
                                    if (monster2.符合数 > 0)
                                    {
                                        if (num21 < monster2.符合数)
                                        {
                                            flag3 = true;
                                            break;
                                        }
                                    }
                                    else if (num21 >= Math.Abs(monster2.符合数))
                                    {
                                        flag3 = true;
                                        break;
                                    }
                                }
                                if (flag3)
                                {
                                    continue;
                                }
                                if (item4.能够穿墙 == -1)
                                {
                                    npc.noTileCollide = false;
                                    npc.netUpdate = true;
                                }
                                if (item4.能够穿墙 == 1)
                                {
                                    npc.noTileCollide = true;
                                    npc.netUpdate = true;
                                }
                                if (item4.无视重力 == -1)
                                {
                                    npc.noGravity = false;
                                    npc.netUpdate = true;
                                }
                                if (item4.无视重力 == 1)
                                {
                                    npc.noGravity = true;
                                    npc.netUpdate = true;
                                }
                                if (item4.怪物无敌 == -1)
                                {
                                    npc.immortal = false;
                                    npc.netUpdate = true;
                                }
                                if (item4.怪物无敌 == 1)
                                {
                                    npc.immortal = true;
                                    npc.netUpdate = true;
                                }
                                if (item4.切换智慧 >= 0 && item4.切换智慧 != 27)
                                {
                                    npc.aiStyle = item4.切换智慧;
                                    npc.netUpdate = true;
                                }
                                if (item4.修改防御)
                                {
                                    npc.defDefense = item4.怪物防御;
                                    npc.defense = item4.怪物防御;
                                    npc.netUpdate = true;
                                }
                                if (item4.AI赋值.Count > 0)
                                {
                                    for (int l = 0; l < npc.ai.Count(); l++)
                                    {
                                        if (item4.AI赋值.ContainsKey(l) && item4.AI赋值.TryGetValue(l, out var value7))
                                        {
                                            npc.ai[l] = value7;
                                            npc.netUpdate = true;
                                        }
                                    }
                                }
                                foreach (读配置文件.声音节 item6 in item4.播放声音)
                                {
                                    if (item6.声音ID >= 0)
                                    {
                                        Terraria.NetMessage.PlayNetSound(new Terraria.NetMessage.NetSoundInfo(npc.Center, 1, item6.声音ID, item6.声音规模, item6.高音补偿), -1, -1);
                                    }
                                }
                                foreach (string item7 in item4.释放命令)
                                {
                                    Commands.HandleCommand((TSPlayer)(object)TSPlayer.Server, ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.CommandSilentSpecifier + item7);
                                }
                                if (item4.喊话 != "")
                                {
                                    TShock.Utils.Broadcast(npc.FullName + ": " + item4.喊话, Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255));
                                }
                                if (item4.玩家复活时间 >= -1)
                                {
                                    lNPC.RespawnSeconds = item4.玩家复活时间;
                                }
                                if (item4.阻止传送器 != 0)
                                {
                                    lNPC.BlockTeleporter = item4.阻止传送器;
                                }
                                Sundry.HurtMonster(item4.杀伤怪物, npc);
                                Sundry.LaunchProjectile(item4.释放弹幕, npc, lNPC);
                                foreach (KeyValuePair<int, int> item8 in item4.召唤怪物)
                                {
                                    if (item8.Value >= 1 && item8.Key != 0)
                                    {
                                        int num22 = Math.Min(item8.Value, 200);
                                        NPC nPCById2 = TShock.Utils.GetNPCById(item8.Key);
                                        if (nPCById2 != null && nPCById2.type != 113 && nPCById2.type != 0 && nPCById2.type < NPCID.Count)
                                        {
                                            TSPlayer.Server.SpawnNPC(nPCById2.type, nPCById2.FullName, num22, Terraria.Utils.ToTileCoordinates(npc.position).X, Terraria.Utils.ToTileCoordinates(npc.position).Y, 5, 5);
                                        }
                                    }
                                }
                                foreach (读配置文件.指示物节 item9 in item4.指示物修改)
                                {
                                    lNPC.setMarkers(item9.名称, item9.数量, item9.清除);
                                }
                                if (item4.状态范围 > 0)
                                {
                                    lNPC.BuffR = item4.状态范围;
                                    lNPC.RBuff = item4.周围状态;
                                }
                                if (item4.恢复血量 > 0)
                                {
                                    NPC val2 = npc;
                                    val2.life += item4.恢复血量;
                                    if (npc.life > npc.lifeMax)
                                    {
                                        npc.life = npc.lifeMax;
                                    }
                                    if (npc.life < 1)
                                    {
                                        npc.life = 1;
                                    }
                                    npc.HealEffect(item4.恢复血量, true);
                                    npc.netUpdate = true;
                                }
                                if (item4.比例回血 > 0)
                                {
                                    if (item4.比例回血 > 100)
                                    {
                                        item4.比例回血 = 100;
                                    }
                                    int num23 = npc.lifeMax * item4.比例回血 / 100;
                                    NPC val2 = npc;
                                    val2.life += num23;
                                    if (npc.life > npc.lifeMax)
                                    {
                                        npc.life = npc.lifeMax;
                                    }
                                    if (npc.life < 1)
                                    {
                                        npc.life = 1;
                                    }
                                    npc.HealEffect(num23, true);
                                    npc.netUpdate = true;
                                }
                                if (item4.可触发次 != -1)
                                {
                                    读配置文件.时间节 时间节 = item4;
                                    时间节.可触发次--;
                                }
                                if (item4.击退范围 > 0 && item4.击退力度 > 0)
                                {
                                    num8 = item4.击退范围;
                                    num9 = item4.击退力度;
                                }
                                if (item4.杀伤范围 > 0 && item4.杀伤伤害 != 0)
                                {
                                    num10 = item4.杀伤范围;
                                    num11 = item4.杀伤伤害;
                                }
                                if (item4.拉取范围 > 0)
                                {
                                    num12 = item4.拉取起始;
                                    num13 = item4.拉取范围;
                                    num14 = item4.拉取止点;
                                    num15 = item4.拉取点X轴偏移;
                                    num16 = item4.拉取点Y轴偏移;
                                }
                                if (item4.反射范围 > 0)
                                {
                                    num17 = item4.反射范围;
                                }
                                if (item4.直接撤退)
                                {
                                    flag2 = true;
                                }
                                if (item4.跳出事件)
                                {
                                    break;
                                }
                                时事件限--;
                            }
                            lNPC.LTime = lNPC.TiemN;
                        }
                        if (lNPC.LifeP != num18)
                        {
                            lNPC.LifeP = num18;
                            if (lNPC.LifeP != lNPC.LLifeP)
                            {
                                int 血事件限 = lNPC.Config.血事件限;
                                foreach (比例节 item10 in lNPC.PLife)
                                {
                                    if (血事件限 <= 0)
                                    {
                                        break;
                                    }
                                    if (item10.血量剩余比例 <= 0 || item10.可触发次 <= 0 && item10.可触发次 != -1 || item10.血量剩余比例 < lNPC.LifeP || item10.血量剩余比例 >= lNPC.LLifeP)
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
                                            if (lNPC.PlayerCount < item10.人数条件)
                                            {
                                                continue;
                                            }
                                        }
                                        else if (lNPC.PlayerCount >= Math.Abs(item10.人数条件))
                                        {
                                            continue;
                                        }
                                    }
                                    if (item10.耗时条件 != 0)
                                    {
                                        if (item10.耗时条件 > 0)
                                        {
                                            if (lNPC.TiemN < item10.耗时条件)
                                            {
                                                continue;
                                            }
                                        }
                                        else if (lNPC.TiemN >= Math.Abs(item10.耗时条件))
                                        {
                                            continue;
                                        }
                                    }
                                    if (item10.ID条件 != 0 && item10.ID条件 != npc.netID)
                                    {
                                        continue;
                                    }
                                    if (item10.杀死条件 != 0)
                                    {
                                        if (item10.杀死条件 > 0)
                                        {
                                            if (lNPC.KillPlay < item10.杀死条件)
                                            {
                                                continue;
                                            }
                                        }
                                        else if (lNPC.KillPlay >= Math.Abs(item10.杀死条件))
                                        {
                                            continue;
                                        }
                                    }
                                    if (item10.开服条件 != 0)
                                    {
                                        if (item10.开服条件 > 0)
                                        {
                                            if (lNPC.OSTime < item10.开服条件)
                                            {
                                                continue;
                                            }
                                        }
                                        else if (lNPC.OSTime >= Math.Abs(item10.开服条件))
                                        {
                                            continue;
                                        }
                                    }
                                    if (item10.昼夜条件 == -1 && Main.dayTime || item10.昼夜条件 == 1 && !Main.dayTime || item10.血月条件 == -1 && Main.bloodMoon || item10.血月条件 == 1 && !Main.bloodMoon || item10.肉山条件 == -1 && Main.hardMode || item10.肉山条件 == 1 && !Main.hardMode || item10.巨人条件 == -1 && NPC.downedGolemBoss || item10.巨人条件 == 1 && !NPC.downedGolemBoss || item10.月总条件 == -1 && NPC.downedMoonlord || item10.月总条件 == 1 && !NPC.downedMoonlord)
                                    {
                                        continue;
                                    }
                                    if (item10.Y轴条件 != 0)
                                    {
                                        if (item10.Y轴条件 > 0)
                                        {
                                            if (npc.Center.Y < item10.Y轴条件 << 4)
                                            {
                                                continue;
                                            }
                                        }
                                        else if (npc.Center.Y >= Math.Abs(item10.Y轴条件 << 4))
                                        {
                                            continue;
                                        }
                                    }
                                    if (item10.面向条件 == 1 && (npc.direction != 1 || npc.directionY != 0) || item10.面向条件 == 2 && (npc.direction != 1 || npc.directionY != 1) || item10.面向条件 == 3 && (npc.direction != 0 || npc.directionY != 1) || item10.面向条件 == 4 && (npc.direction != -1 || npc.directionY != 1) || item10.面向条件 == 5 && (npc.direction != -1 || npc.directionY != 0) || item10.面向条件 == 6 && (npc.direction != -1 || npc.directionY != -1) || item10.面向条件 == 7 && (npc.direction != 0 || npc.directionY != -1) || item10.面向条件 == 8 && (npc.direction != 1 || npc.directionY != -1) || item10.面向条件 == 9 && npc.direction != 1 || item10.面向条件 == 10 && npc.directionY != 1 || item10.面向条件 == 11 && npc.direction != -1 || item10.面向条件 == 12 && npc.directionY != -1 || item10.触发率子 <= 0 || item10.触发率母 <= 0 || item10.触发率子 < item10.触发率母 && random.Next(1, item10.触发率母 + 1) > item10.触发率子 || Sundry.MonsterRequirement(item10.怪物条件, npc))
                                    {
                                        continue;
                                    }
                                    bool flag4 = false;
                                    foreach (读配置文件.玩家条件节 monster in item10.玩家条件)
                                    {
                                        if (monster.范围内 <= 0)
                                        {
                                            continue;
                                        }
                                        int num24 = 0;
                                        num24 = monster.范围起 <= 0 ? TShock.Players.Count((p) => p != null && p.Active && !p.Dead && p.TPlayer.statLife > 0 && npc.WithinRange(p.TPlayer.position, monster.范围内 << 4)) : TShock.Players.Count((p) => p != null && p.Active && !p.Dead && p.TPlayer.statLife > 0 && !npc.WithinRange(p.TPlayer.position, monster.范围起 << 4) && npc.WithinRange(p.TPlayer.position, monster.范围内 << 4));
                                        if (monster.符合数 == 0)
                                        {
                                            continue;
                                        }
                                        if (monster.符合数 > 0)
                                        {
                                            if (num24 < monster.符合数)
                                            {
                                                flag4 = true;
                                                break;
                                            }
                                        }
                                        else if (num24 >= Math.Abs(monster.符合数))
                                        {
                                            flag4 = true;
                                            break;
                                        }
                                    }
                                    if (flag4)
                                    {
                                        continue;
                                    }
                                    if (item10.能够穿墙 == -1)
                                    {
                                        npc.noTileCollide = false;
                                        npc.netUpdate = true;
                                    }
                                    if (item10.能够穿墙 == 1)
                                    {
                                        npc.noTileCollide = true;
                                        npc.netUpdate = true;
                                    }
                                    if (item10.无视重力 == -1)
                                    {
                                        npc.noGravity = false;
                                        npc.netUpdate = true;
                                    }
                                    if (item10.无视重力 == 1)
                                    {
                                        npc.noGravity = true;
                                        npc.netUpdate = true;
                                    }
                                    if (item10.怪物无敌 == -1)
                                    {
                                        npc.immortal = false;
                                        npc.netUpdate = true;
                                    }
                                    if (item10.怪物无敌 == 1)
                                    {
                                        npc.immortal = true;
                                        npc.netUpdate = true;
                                    }
                                    if (item10.切换智慧 >= 0 && item10.切换智慧 != 27)
                                    {
                                        npc.aiStyle = item10.切换智慧;
                                        npc.netUpdate = true;
                                    }
                                    if (item10.修改防御)
                                    {
                                        npc.defDefense = item10.怪物防御;
                                        npc.defense = item10.怪物防御;
                                        npc.netUpdate = true;
                                    }
                                    if (item10.喊话 != "")
                                    {
                                        TShock.Utils.Broadcast(npc.FullName + ": " + item10.喊话, Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255));
                                    }
                                    if (item10.玩家复活时间 >= -1)
                                    {
                                        lNPC.RespawnSeconds = item10.玩家复活时间;
                                    }
                                    Sundry.HurtMonster(item10.杀伤怪物, npc);
                                    Sundry.LaunchProjectile(item10.释放弹幕, npc, lNPC);
                                    foreach (KeyValuePair<int, int> item11 in item10.召唤怪物)
                                    {
                                        if (item11.Value >= 1 && item11.Key != 0)
                                        {
                                            int num25 = Math.Min(item11.Value, 200);
                                            NPC nPCById3 = TShock.Utils.GetNPCById(item11.Key);
                                            if (nPCById3 != null && nPCById3.type != 113 && nPCById3.type != 0 && nPCById3.type < NPCID.Count)
                                            {
                                                TSPlayer.Server.SpawnNPC(nPCById3.type, nPCById3.FullName, num25, Terraria.Utils.ToTileCoordinates(npc.position).X, Terraria.Utils.ToTileCoordinates(npc.position).Y, 15, 15);
                                            }
                                        }
                                    }
                                    if (item10.状态范围 > 0)
                                    {
                                        lNPC.BuffR = item10.状态范围;
                                        lNPC.RBuff = item10.周围状态;
                                    }
                                    if (item10.恢复血量 > 0)
                                    {
                                        NPC val2 = npc;
                                        val2.life += item10.恢复血量;
                                        if (npc.life > npc.lifeMax)
                                        {
                                            npc.life = npc.lifeMax;
                                        }
                                        if (npc.life < 1)
                                        {
                                            npc.life = 1;
                                        }
                                        npc.HealEffect(item10.恢复血量, true);
                                        npc.netUpdate = true;
                                    }
                                    if (item10.比例回血 > 0)
                                    {
                                        if (item10.比例回血 > 100)
                                        {
                                            item10.比例回血 = 100;
                                        }
                                        NPC val2 = npc;
                                        val2.life += (int)(npc.lifeMax * (item10.比例回血 / 100.0));
                                        if (npc.life > npc.lifeMax)
                                        {
                                            npc.life = npc.lifeMax;
                                        }
                                        if (npc.life < 1)
                                        {
                                            npc.life = 1;
                                        }
                                        npc.HealEffect(item10.恢复血量, true);
                                        npc.netUpdate = true;
                                    }
                                    if (item10.可触发次 != -1)
                                    {
                                        比例节 比例节 = item10;
                                        比例节.可触发次--;
                                    }
                                    if (item10.击退范围 > 0 && item10.击退力度 > 0)
                                    {
                                        num8 = item10.击退范围;
                                        num9 = item10.击退力度;
                                    }
                                    if (item10.杀伤范围 > 0 && item10.杀伤伤害 != 0)
                                    {
                                        num10 = item10.杀伤范围;
                                        num11 = item10.杀伤伤害;
                                    }
                                    if (item10.拉取范围 > 0)
                                    {
                                        num12 = item10.拉取起始;
                                        num13 = item10.拉取范围;
                                        num14 = item10.拉取止点;
                                        num15 = item10.拉取点X轴偏移;
                                        num16 = item10.拉取点Y轴偏移;
                                    }
                                    if (item10.直接撤退)
                                    {
                                        flag2 = true;
                                    }
                                    if (item10.跳出事件)
                                    {
                                        break;
                                    }
                                    血事件限--;
                                }
                                lNPC.LLifeP = lNPC.LifeP;
                            }
                        }
                        if (num17 > 0)
                        {
                            for (int m = 0; m < 1000; m++)
                            {
                                if (Main.projectile[m].active && Main.projectile[m].CanBeReflected() && npc.WithinRange(Main.projectile[m].position, num17 << 4))
                                {
                                    npc.ReflectProjectile(Main.projectile[m]);
                                    Terraria.NetMessage.SendData(27, -1, -1, null, m, 0f, 0f, 0f, 0, 0, 0);
                                }
                            }
                        }
                        if (num14 > num12)
                        {
                            num14 = num12;
                        }
                        num14 *= 16;
                        if (lNPC.BuffR > 0 || num8 > 0 && num9 > 0 || num10 > 0 && num11 != 0 || num13 > 0)
                        {
                            TSPlayer[] players = TShock.Players;
                            foreach (TSPlayer val3 in players)
                            {
                                if (val3 == null)
                                {
                                    continue;
                                }
                                if (Timeout(now))
                                {
                                    ULock = false;
                                    return;
                                }
                                if (!val3.Active || val3.Dead || val3.TPlayer.statLife < 1)
                                {
                                    continue;
                                }
                                if (num13 > 0 && val3.IsInRange(Terraria.Utils.ToTileCoordinates(npc.position).X, Terraria.Utils.ToTileCoordinates(npc.position).Y, num13))
                                {
                                    if (num12 > 0)
                                    {
                                        if (!val3.IsInRange(Terraria.Utils.ToTileCoordinates(npc.position).X, Terraria.Utils.ToTileCoordinates(npc.position).Y, num12))
                                        {
                                            Sundry.PullTP(val3, npc.position.X + num15, npc.position.Y + num16, num14);
                                        }
                                    }
                                    else
                                    {
                                        Sundry.PullTP(val3, npc.position.X + num15, npc.position.Y + num16, num14);
                                    }
                                }
                                if (num10 > 0 && num11 != 0 && val3.IsInRange(Terraria.Utils.ToTileCoordinates(npc.position).X, Terraria.Utils.ToTileCoordinates(npc.position).Y, num10))
                                {
                                    if (num11 < 0)
                                    {
                                        if (num11 > val3.TPlayer.statLifeMax2)
                                        {
                                            num11 = val3.TPlayer.statLifeMax2;
                                        }
                                        val3.Heal(Math.Abs(num11));
                                    }
                                    else if (num11 > val3.TPlayer.statLifeMax2 + val3.TPlayer.statDefense)
                                    {
                                        val3.KillPlayer();
                                    }
                                    else
                                    {
                                        val3.DamagePlayer(num11);
                                    }
                                }
                                if (num8 > 0 && num9 > 0 && val3.IsInRange(Terraria.Utils.ToTileCoordinates(npc.position).X, Terraria.Utils.ToTileCoordinates(npc.position).Y, num8))
                                {
                                    int num26 = -1;
                                    if (npc.Center.Y < val3.Y)
                                    {
                                        num26 = 1;
                                    }
                                    int num27 = -1;
                                    if (npc.Center.X < val3.X)
                                    {
                                        num27 = 1;
                                    }
                                    val3.TPlayer.velocity = new Vector2(num27 * num9, num26 * num9);
                                    Terraria.NetMessage.SendData(13, -1, -1, NetworkText.Empty, val3.Index, 0f, 0f, 0f, 0, 0, 0);
                                }
                                if (lNPC.BuffR <= 0 || lNPC.Time % 10 != 0 || !val3.IsInRange(Terraria.Utils.ToTileCoordinates(npc.position).X, Terraria.Utils.ToTileCoordinates(npc.position).Y, lNPC.BuffR))
                                {
                                    continue;
                                }
                                foreach (状态节 item12 in lNPC.RBuff)
                                {
                                    if ((item12.状态起始范围 <= 0 || !val3.IsInRange(Terraria.Utils.ToTileCoordinates(npc.position).X, Terraria.Utils.ToTileCoordinates(npc.position).Y, item12.状态起始范围)) && (item12.状态结束范围 <= 0 || val3.IsInRange(Terraria.Utils.ToTileCoordinates(npc.position).X, Terraria.Utils.ToTileCoordinates(npc.position).Y, item12.状态结束范围)))
                                    {
                                        val3.SetBuff(item12.状态ID, 100, false);
                                        if (item12.头顶提示 != "")
                                        {
                                            string 头顶提示 = item12.头顶提示;
                                            Color val4 = new Color(255, 255, 255);
                                            val3.SendData((PacketTypes)119, 头顶提示, (int)val4.PackedValue, val3.X, val3.Y, 0f, 0);
                                        }
                                    }
                                }
                            }
                        }
                        if (flag2)
                        {
                            int whoAmI2 = npc.whoAmI;
                            Main.npc[whoAmI2] = new NPC();
                            Terraria.NetMessage.SendData(23, -1, -1, NetworkText.Empty, whoAmI2, 0f, 0f, 0f, 0, 0, 0);
                            if (!lNPC.Config.不宣读信息)
                            {
                                TShock.Utils.Broadcast("攻略失败: " + npc.FullName + " 已撤离.", Convert.ToByte(190), Convert.ToByte(150), Convert.ToByte(150));
                            }
                        }
                    end_IL_0134:;
                    }
                }
                if (config.启动死亡队友视角 && (!config.队友视角仅BOSS时 || flag))
                {
                    if (TeamP <= config.队友视角流畅度)
                    {
                        TeamP++;
                    }
                    else
                    {
                        TeamP = 0;
                        TSPlayer[] players2 = TShock.Players;
                        foreach (TSPlayer val5 in players2)
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
                            if (!val5.Active || !val5.Dead || val5.TPlayer.statLife >= 1 || val5.Team == 0)
                            {
                                continue;
                            }
                            int num29 = -1;
                            float num30 = -1f;
                            for (int num31 = 0; num31 < 255; num31++)
                            {
                                if (Main.player[num31] != null && val5.Team == Main.player[num31].team && Main.player[num31].active && !Main.player[num31].dead)
                                {
                                    float num32 = Math.Abs(Main.player[num31].position.X + Main.player[num31].width / 2 - (val5.TPlayer.position.X + val5.TPlayer.width / 2)) + Math.Abs(Main.player[num31].position.Y + Main.player[num31].height / 2 - (val5.TPlayer.position.Y + val5.TPlayer.height / 2));
                                    if (num30 == -1f || num32 < num30)
                                    {
                                        num30 = num32;
                                        num29 = num31;
                                    }
                                }
                            }
                            if (num29 != -1 && !val5.IsInRange(Terraria.Utils.ToTileCoordinates(Main.player[num29].position).X, Terraria.Utils.ToTileCoordinates(Main.player[num29].position).Y, config.队友视角等待范围))
                            {
                                val5.TPlayer.position = Main.player[num29].position;
                                Terraria.NetMessage.SendData(13, -1, -1, NetworkText.Empty, val5.Index, 0f, 0f, 0f, 0, 0, 0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (config.启动错误报告)
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

        private long getLNKC(int id)
        {
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


    }
}