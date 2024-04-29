using TestPlugin.读配置文件;

namespace TestPlugin
{
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

        public int BlockTeleporter { get; set; }

        public int OSTime { get; set; }

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
            if (Config != null)
            {
                RespawnSeconds = config.玩家复活时间;
                BlockTeleporter = config.阻止传送器;
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
            foreach (指示物组节 marker in Markers)
            {
                if (marker.名称 == name)
                {
                    if (reset)
                    {
                        marker.数量 = num;
                    }
                    else
                    {
                        marker.数量 += num;
                    }
                    return;
                }
            }
            Markers.Add(new 指示物组节(name, num));
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

        public bool haveMarkers(List<指示物组节> list)
        {
            bool flag = false;
            foreach (指示物组节 item in list)
            {
                int markers = getMarkers(item.名称);
                if (item.数量 == 0)
                {
                    continue;
                }
                if (item.数量 > 0)
                {
                    if (markers < item.数量)
                    {
                        flag = true;
                        break;
                    }
                }
                else if (markers >= Math.Abs(item.数量))
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                return false;
            }
            return true;
        }
    }
}
