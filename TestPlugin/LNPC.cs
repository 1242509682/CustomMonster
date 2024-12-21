using Terraria;
using TestPlugin.读配置文件;

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

    public void setMarkers(string name, int num, bool reset)
    {
        string name2 = name;
        if (!Markers.Exists((指示物组节 t) => t.名称 == name2))
        {
            Markers.Add(new 指示物组节(name2, 0));
        }
        foreach (指示物组节 marker in Markers)
        {
            if (marker.名称 == name2)
            {
                if (reset)
                {
                    marker.数量 += num;
                }
                else
                {
                    marker.数量 += num;
                }
                break;
            }
        }
    }

    public void setMarkers(string name, int num, bool reset, string inname, float infactor, string inop, int rmin, int rmax, ref Random rd, NPC npc)
    {
        string name2 = name;
        if (!Markers.Exists((指示物组节 t) => t.名称 == name2))
        {
            Markers.Add(new 指示物组节(name2, 0));
        }
        int num2 = 0;
        if (rmax > rmin)
        {
            num2 = rd.Next(rmin, rmax);
        }
        int num3 = addMarkersIn(inname, infactor, npc);
        foreach (指示物组节 marker in Markers)
        {
            if (marker.名称 == name2)
            {
                if (reset)
                {
                    marker.数量 = Sundry.intoperation(inop, 0, num + num2 + num3);
                }
                else
                {
                    marker.数量 = Sundry.intoperation(inop, marker.数量, num + num2 + num3);
                }
                break;
            }
        }
    }

    public int addMarkersIn(string inname, float infactor, NPC npc)
    {
        float num = 0f;
        if (TestPlugin.LNpcs[npc.whoAmI] == null)
        {
            return (int)num;
        }
        if (inname != "")
        {
            if (inname == "[序号]" && npc != null)
            {
                num = npc.whoAmI;
            }
            else
            {
                switch (inname)
                {
                    case "[被击]":
                        num = TestPlugin.LNpcs[npc.whoAmI].Struck;
                        break;
                    case "[击杀]":
                        num = TestPlugin.LNpcs[npc.whoAmI].KillPlay;
                        break;
                    case "[耗时]":
                        num = (int)TestPlugin.LNpcs[npc.whoAmI].TiemN;
                        break;
                    case "[X坐标]":
                        if (npc != null)
                        {
                            num = (int)npc.Center.X;
                            break;
                        }
                        goto default;
                    default:
                        {
                            if (inname == "[Y坐标]" && npc != null)
                            {
                                num = (int)npc.Center.Y;
                                break;
                            }
                            if (inname == "[血量]" && npc != null)
                            {
                                num = npc.life;
                                break;
                            }
                            if (!(inname == "[被杀]") || npc == null)
                            {
                                num = (inname == "[AI0]" && npc != null) ? npc.ai[0] : ((inname == "[AI1]" && npc != null) ? npc.ai[1] : ((inname == "[AI2]" && npc != null) ? npc.ai[2] : ((!(inname == "[AI3]") || npc == null) ? TestPlugin.LNpcs[npc.whoAmI].getMarkers(inname) : npc.ai[3])));
                                break;
                            }
                            long num2 = TestPlugin.getLNKC(npc.netID);
                            if (num2 > int.MaxValue)
                            {
                                num2 = 2147483647L;
                            }
                            num = (int)num2;
                            break;
                        }
                }
            }
        }
        if (num != 0f)
        {
            num *= infactor;
        }
        return (int)num;
    }

    public int getMarkers(string name)
    {
        if (name == "")
        {
            return 0;
        }
        foreach (指示物组节 marker in Markers)
        {
            if (marker.名称 == name)
            {
                return marker.数量;
            }
        }
        return 0;
    }

    public string ReplaceMarkers(string text)
    {
        string text2 = text;
        foreach (指示物组节 marker in Markers)
        {
            text2 = text2.Replace("[" + marker.名称 + "]", marker.数量.ToString());
        }
        return text2;
    }

    public bool haveMarkers(List<指示物组节> list, NPC npc)
    {
        bool flag = false;
        foreach (指示物组节 item in list)
        {
            int markers = getMarkers(item.名称);
            int num = item.数量 + addMarkersIn(item.指示物注入数量名, item.指示物注入数量系数, npc);
            if (item.重定义判断符号 != "")
            {
                if (item.重定义判断符号 == "=")
                {
                    if (markers != num)
                    {
                        flag = true;
                        break;
                    }
                }
                else if (item.重定义判断符号 == "!")
                {
                    if (markers == num)
                    {
                        flag = true;
                        break;
                    }
                }
                else if (item.重定义判断符号 == ">")
                {
                    if (markers <= num)
                    {
                        flag = true;
                        break;
                    }
                }
                else if (item.重定义判断符号 == "<" && markers >= num)
                {
                    flag = true;
                    break;
                }
            }
            else
            {
                if (num == 0)
                {
                    continue;
                }
                if (num > 0)
                {
                    if (markers < num)
                    {
                        flag = true;
                        break;
                    }
                }
                else if (markers >= Math.Abs(num))
                {
                    flag = true;
                    break;
                }
            }
        }
        if (flag)
        {
            return false;
        }
        return true;
    }
}
