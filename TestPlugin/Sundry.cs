using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using TShockAPI;

namespace TestPlugin.读配置文件
{
    public class Sundry
    {
        public static void LaunchProjectile(List<弹幕节> Projectiles, NPC npc, LNPC lnpc)
        {
            foreach (弹幕节 Projectile in Projectiles)
            {
                if (Projectile.弹幕ID <= 0)
                {
                    continue;
                }
                if (Projectile.锁定范围 > 0 || Projectile.锁定范围 == -1)
                {
                    List<int> list = new List<int>();
                    if (Projectile.锁定范围 == -1)
                    {
                        list.Add(-1);
                    }
                    else
                    {
                        for (int i = 0; i < Projectile.最大锁定数 && i <= TShock.Utils.GetActivePlayerCount(); i++)
                        {
                            int num = -1;
                            float num2 = -1f;
                            int? num3 = null;
                            int? num4 = null;
                            int? num5 = null;
                            for (int j = 0; j < 255; j++)
                            {
                                if (list.Contains(j) || Main.player[j] == null || !Main.player[j].active || Main.player[j].dead || Projectile.仅攻击对象 && j != npc.target)
                                {
                                    continue;
                                }
                                if (Projectile.仅扇形锁定)
                                {
                                    if (Projectile.扇形半偏角 > 180)
                                    {
                                        Projectile.扇形半偏角 = 180;
                                    }
                                    if (Projectile.扇形半偏角 < 1)
                                    {
                                        Projectile.扇形半偏角 = 1;
                                    }
                                    float num6 = Main.player[j].Center.X - npc.Center.X;
                                    float num7 = Main.player[j].Center.Y - npc.Center.Y;
                                    if ((num6 != 0f || num7 != 0f) && (npc.direction != 0 || npc.directionY != 0))
                                    {
                                        double num8 = Math.Atan2(num7, num6) * 180.0 / Math.PI;
                                        double num9 = Math.Atan2(npc.directionY, npc.direction) * 180.0 / Math.PI;
                                        double num10 = num9 + Projectile.扇形半偏角;
                                        double num11 = num9 - Projectile.扇形半偏角;
                                        if (num10 > 360.0)
                                        {
                                            num10 -= 360.0;
                                        }
                                        if (num11 < 0.0)
                                        {
                                            num11 += 360.0;
                                        }
                                        if (num8 > num10 && num8 < num11)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                float num12 = Math.Abs(Main.player[j].position.X + Main.player[j].width / 2 - (npc.position.X + npc.width / 2)) + Math.Abs(Main.player[j].position.Y + Main.player[j].height / 2 - (npc.position.Y + npc.height / 2));
                                if ((num2 == -1f || num12 < num2) && (!Projectile.计入仇恨 || !num4.HasValue || (Projectile.逆仇恨锁定 ? Main.player[j].aggro < num4 : Main.player[j].aggro > num4)) && (!Projectile.锁定血少 || !num3.HasValue || (Projectile.逆血量锁定 ? Main.player[j].statLife > num3 : Main.player[j].statLife < num3)) && (!Projectile.锁定低防 || !num5.HasValue || (Projectile.逆防御锁定 ? Main.player[j].statDefense > num5 : Main.player[j].statDefense < num5)))
                                {
                                    if (Projectile.计入仇恨)
                                    {
                                        num4 = Main.player[j].aggro;
                                    }
                                    if (Projectile.锁定血少)
                                    {
                                        num3 = Main.player[j].statLife;
                                    }
                                    if (Projectile.锁定低防)
                                    {
                                        num5 = Main.player[j].statDefense;
                                    }
                                    num2 = num12;
                                    num = j;
                                }
                            }
                            if (num != -1)
                            {
                                list.Add(num);
                            }
                        }
                    }
                    foreach (int item in list)
                    {
                        float x;
                        float y;
                        if (item == -1)
                        {
                            x = npc.Center.X;
                            y = npc.Center.Y;
                        }
                        else
                        {
                            Player val = Main.player[item];
                            if (val == null || val.dead || val.statLife < 1)
                            {
                                continue;
                            }
                            if (Projectile.锁定范围 > 350)
                            {
                                Projectile.锁定范围 = 350;
                            }
                            if (!npc.WithinRange(val.position, Projectile.锁定范围 << 4))
                            {
                                continue;
                            }
                            x = val.Center.X;
                            y = val.Center.Y;
                        }
                        float num13 = Projectile.怪面向X偏移修正 * npc.direction;
                        float num14 = Projectile.怪面向Y偏移修正 * npc.directionY;
                        float ai = Projectile.弹幕Ai0;
                        float 弹幕Ai = Projectile.弹幕Ai1;
                        float 弹幕Ai2 = Projectile.弹幕Ai2;
                        float num15;
                        float num16;
                        double num20;
                        float num17;
                        float num18;
                        if (Projectile.以锁定为点)
                        {
                            num15 = x;
                            num16 = y;
                            num17 = Projectile.X轴速度 + lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数 + Projectile.怪面向X速度修正 * npc.direction;
                            num18 = Projectile.Y轴速度 + lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数 + Projectile.怪面向Y速度修正 * npc.directionY;
                            float num19 = (float)Math.Sqrt(Math.Pow(num17, 2.0) + Math.Pow(num18, 2.0));
                            num20 = Math.Atan2(num18, num17) * 180.0 / Math.PI;
                            float num21 = Projectile.角度偏移 + lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数;
                            if (num21 != 0f)
                            {
                                num20 += (double)num21;
                                num17 = (float)((double)num19 * Math.Cos(num20 * Math.PI / 180.0));
                                num18 = (float)((double)num19 * Math.Sin(num20 * Math.PI / 180.0));
                            }
                        }
                        else
                        {
                            num15 = npc.Center.X;
                            num16 = npc.Center.Y;
                            num17 = x - (npc.Center.X + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数);
                            num18 = y - (npc.Center.Y + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数);
                            if (num17 == 0f && num18 == 0f)
                            {
                                num17 = 1f;
                            }
                            num20 = Math.Atan2(num18, num17) * 180.0 / Math.PI;
                            num20 += (double)(Projectile.角度偏移 + lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数);
                            num17 = (float)(Projectile.锁定速度 * Math.Cos(num20 * Math.PI / 180.0));
                            num18 = (float)(Projectile.锁定速度 * Math.Sin(num20 * Math.PI / 180.0));
                            num17 += Projectile.X轴速度 + lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数 + Projectile.怪面向X速度修正 * npc.direction;
                            num18 += Projectile.Y轴速度 + lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数 + Projectile.怪面向Y速度修正 * npc.directionY;
                        }
                        if (Projectile.以弹为位)
                        {
                            float num22 = num15 + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num13;
                            float num23 = num16 + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num14;
                            num17 = x - (num22 + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数);
                            num18 = y - (num23 + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数);
                            if (num17 == 0f && num18 == 0f)
                            {
                                num17 = 1f;
                            }
                            num20 = Math.Atan2(num18, num17) * 180.0 / Math.PI;
                            num20 += (double)(Projectile.角度偏移 + lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数);
                            num17 = (float)(Projectile.锁定速度 * Math.Cos(num20 * Math.PI / 180.0));
                            num18 = (float)(Projectile.锁定速度 * Math.Sin(num20 * Math.PI / 180.0));
                            num17 += Projectile.X轴速度 + lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数 + Projectile.怪面向X速度修正 * npc.direction;
                            num18 += Projectile.Y轴速度 + lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数 + Projectile.怪面向Y速度修正 * npc.directionY;
                        }
                        if (Projectile.速度注入AI0)
                        {
                            ai = (float)Math.Atan2(num18, num17);
                            num17 = 0f;
                            num18 = 0f;
                        }
                        if (!Projectile.不射原始)
                        {
                            NewProjectile(Terraria.Projectile.GetNoneSource(), num15 + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num13, num16 + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num14, num17, num18, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, 弹幕Ai, 弹幕Ai2, Projectile.持续时间);
                        }
                        if (Projectile.差度位射数 > 0 && Projectile.差度位射角 != 0 && Projectile.差度位半径 > 0)
                        {
                            double num24 = Projectile.差度位始角;
                            for (int k = 0; k < Projectile.差度位射数; k++)
                            {
                                num24 += Projectile.差度位射角;
                                float num25 = (float)(Projectile.差度位半径 * Math.Cos(num24 * Math.PI / 180.0));
                                float num26 = (float)(Projectile.差度位半径 * Math.Sin(num24 * Math.PI / 180.0));
                                if (Projectile.以弹为位)
                                {
                                    float num27 = num15 + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num25 + num13;
                                    float num28 = num16 + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num26 + num14;
                                    num17 = x - (num27 + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数);
                                    num18 = y - (num28 + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数);
                                    if (num17 == 0f && num18 == 0f)
                                    {
                                        num17 = 1f;
                                    }
                                    num20 = Math.Atan2(num18, num17) * 180.0 / Math.PI;
                                    num20 += (double)(Projectile.角度偏移 + lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数);
                                    num17 = (float)(Projectile.锁定速度 * Math.Cos(num20 * Math.PI / 180.0));
                                    num18 = (float)(Projectile.锁定速度 * Math.Sin(num20 * Math.PI / 180.0));
                                    num17 += Projectile.X轴速度 + lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数 + Projectile.怪面向X速度修正 * npc.direction;
                                    num18 += Projectile.Y轴速度 + lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数 + Projectile.怪面向Y速度修正 * npc.directionY;
                                }
                                if (Projectile.速度注入AI0)
                                {
                                    ai = (float)Math.Atan2(num18, num17);
                                    num17 = 0f;
                                    num18 = 0f;
                                }
                                if (!Projectile.不射差度位)
                                {
                                    NewProjectile(Terraria.Projectile.GetNoneSource(), num15 + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num25 + num13, num16 + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num26 + num14, num17, num18, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, 弹幕Ai, 弹幕Ai2, Projectile.持续时间);
                                }
                                if (Projectile.差度射数 > 0 && Projectile.差度射角 != 0f)
                                {
                                    for (int l = 0; l < Projectile.差度射数; l++)
                                    {
                                        num20 += Projectile.差度射角;
                                        num17 = (float)(Projectile.锁定速度 * Math.Cos(num20 * Math.PI / 180.0));
                                        num18 = (float)(Projectile.锁定速度 * Math.Sin(num20 * Math.PI / 180.0));
                                        if (Projectile.速度注入AI0)
                                        {
                                            ai = (float)Math.Atan2(num18, num17);
                                            num17 = 0f;
                                            num18 = 0f;
                                        }
                                        NewProjectile(Terraria.Projectile.GetNoneSource(), num15 + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num13, num16 + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num14, num17, num18, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, 弹幕Ai, 弹幕Ai2, Projectile.持续时间);
                                        if (Projectile.差位射数 > 0 && (Projectile.差位偏移X != 0f || Projectile.差位偏移Y != 0f))
                                        {
                                            float num29 = num15 + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num13;
                                            float num30 = num16 + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num14;
                                            for (int m = 0; m < Projectile.差位射数; m++)
                                            {
                                                num29 += Projectile.差位偏移X;
                                                num30 += Projectile.差位偏移Y;
                                                NewProjectile(Terraria.Projectile.GetNoneSource(), num29, num30, num17, num18, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, 弹幕Ai, 弹幕Ai2, Projectile.持续时间);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (Projectile.差位射数 <= 0 || Projectile.差位偏移X == 0f && Projectile.差位偏移Y == 0f)
                                    {
                                        continue;
                                    }
                                    float num31 = num15 + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num13;
                                    float num32 = num16 + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num14;
                                    for (int n = 0; n < Projectile.差位射数; n++)
                                    {
                                        num31 += Projectile.差位偏移X;
                                        num32 += Projectile.差位偏移Y;
                                        if (Projectile.以弹为位)
                                        {
                                            num17 = x - (num31 + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数);
                                            num18 = y - (num32 + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数);
                                            if (num17 == 0f && num18 == 0f)
                                            {
                                                num17 = 1f;
                                            }
                                            num20 = Math.Atan2(num18, num17) * 180.0 / Math.PI;
                                            num20 += (double)(Projectile.角度偏移 + lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数);
                                            num17 = (float)(Projectile.锁定速度 * Math.Cos(num20 * Math.PI / 180.0));
                                            num18 = (float)(Projectile.锁定速度 * Math.Sin(num20 * Math.PI / 180.0));
                                            num17 += Projectile.X轴速度 + lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数 + Projectile.怪面向X速度修正 * npc.direction;
                                            num18 += Projectile.Y轴速度 + lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数 + Projectile.怪面向Y速度修正 * npc.directionY;
                                            if (Projectile.速度注入AI0)
                                            {
                                                ai = (float)Math.Atan2(num18, num17);
                                                num17 = 0f;
                                                num18 = 0f;
                                            }
                                        }
                                        NewProjectile(Terraria.Projectile.GetNoneSource(), num31, num32, num17, num18, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, 弹幕Ai, 弹幕Ai2, Projectile.持续时间);
                                    }
                                }
                            }
                        }
                        else if (Projectile.差度射数 > 0 && Projectile.差度射角 != 0f)
                        {
                            for (int num33 = 0; num33 < Projectile.差度射数; num33++)
                            {
                                num20 += Projectile.差度射角;
                                num17 = (float)(Projectile.锁定速度 * Math.Cos(num20 * Math.PI / 180.0));
                                num18 = (float)(Projectile.锁定速度 * Math.Sin(num20 * Math.PI / 180.0));
                                if (Projectile.速度注入AI0)
                                {
                                    ai = (float)Math.Atan2(num18, num17);
                                    num17 = 0f;
                                    num18 = 0f;
                                }
                                NewProjectile(Terraria.Projectile.GetNoneSource(), num15 + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num13, num16 + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num14, num17, num18, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, 弹幕Ai, 弹幕Ai2, Projectile.持续时间);
                                if (Projectile.差位射数 > 0 && (Projectile.差位偏移X != 0f || Projectile.差位偏移Y != 0f))
                                {
                                    float num34 = num15 + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num13;
                                    float num35 = num16 + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num14;
                                    for (int num36 = 0; num36 < Projectile.差位射数; num36++)
                                    {
                                        num34 += Projectile.差位偏移X;
                                        num35 += Projectile.差位偏移Y;
                                        NewProjectile(Terraria.Projectile.GetNoneSource(), num34, num35, num17, num18, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, 弹幕Ai, 弹幕Ai2, Projectile.持续时间);
                                    }
                                }
                            }
                        }
                        else if (Projectile.差位射数 > 0 && (Projectile.差位偏移X != 0f || Projectile.差位偏移Y != 0f))
                        {
                            float num37 = num15 + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num13;
                            float num38 = num16 + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num14;
                            for (int num39 = 0; num39 < Projectile.差位射数; num39++)
                            {
                                num37 += Projectile.差位偏移X;
                                num38 += Projectile.差位偏移Y;
                                NewProjectile(Terraria.Projectile.GetNoneSource(), num37, num38, num17, num18, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, 弹幕Ai, 弹幕Ai2, Projectile.持续时间);
                            }
                        }
                    }
                    continue;
                }
                float num40 = Projectile.X轴速度 + lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数 + Projectile.怪面向X速度修正 * npc.direction;
                float num41 = Projectile.Y轴速度 + lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数 + Projectile.怪面向Y速度修正 * npc.directionY;
                float num42 = (float)Math.Sqrt(Math.Pow(num40, 2.0) + Math.Pow(num41, 2.0));
                double num43 = Math.Atan2(num41, num40) * 180.0 / Math.PI;
                float num44 = Projectile.怪面向X偏移修正 * npc.direction;
                float num45 = Projectile.怪面向Y偏移修正 * npc.directionY;
                float ai2 = Projectile.弹幕Ai0;
                float 弹幕Ai3 = Projectile.弹幕Ai1;
                float 弹幕Ai4 = Projectile.弹幕Ai2;
                float num46 = Projectile.角度偏移 + lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数;
                if (num46 != 0f)
                {
                    num43 += (double)num46;
                    num40 = (float)((double)num42 * Math.Cos(num43 * Math.PI / 180.0));
                    num41 = (float)((double)num42 * Math.Sin(num43 * Math.PI / 180.0));
                }
                if (Projectile.速度注入AI0)
                {
                    ai2 = (float)Math.Atan2(num41, num40);
                    num40 = 0f;
                    num41 = 0f;
                }
                if (!Projectile.不射原始)
                {
                    NewProjectile(Terraria.Projectile.GetNoneSource(), npc.Center.X + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num44, npc.Center.Y + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num45, num40, num41, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai2, 弹幕Ai3, 弹幕Ai4, Projectile.持续时间);
                }
                if (Projectile.差度位射数 > 0 && Projectile.差度位射角 != 0 && Projectile.差度位半径 > 0)
                {
                    double num47 = Projectile.差度位始角;
                    for (int num48 = 0; num48 < Projectile.差度位射数; num48++)
                    {
                        num47 += Projectile.差度位射角;
                        float num49 = (float)(Projectile.差度位半径 * Math.Cos(num47 * Math.PI / 180.0));
                        float num50 = (float)(Projectile.差度位半径 * Math.Sin(num47 * Math.PI / 180.0));
                        if (!Projectile.不射差度位)
                        {
                            NewProjectile(Terraria.Projectile.GetNoneSource(), npc.Center.X + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num49 + num44, npc.Center.Y + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num50 + num45, num40, num41, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai2, 弹幕Ai3, 弹幕Ai4, Projectile.持续时间);
                        }
                        if (Projectile.差度射数 > 0 && Projectile.差度射角 != 0f)
                        {
                            for (int num51 = 0; num51 < Projectile.差度射数; num51++)
                            {
                                num43 += Projectile.差度射角;
                                num40 = (float)((double)num42 * Math.Cos(num43 * Math.PI / 180.0));
                                num41 = (float)((double)num42 * Math.Sin(num43 * Math.PI / 180.0));
                                if (Projectile.速度注入AI0)
                                {
                                    ai2 = (float)Math.Atan2(num41, num40);
                                    num40 = 0f;
                                    num41 = 0f;
                                }
                                NewProjectile(Terraria.Projectile.GetNoneSource(), npc.Center.X + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num44, npc.Center.Y + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num45, num40, num41, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai2, 弹幕Ai3, 弹幕Ai4, Projectile.持续时间);
                                if (Projectile.差位射数 > 0 && (Projectile.差位偏移X != 0f || Projectile.差位偏移Y != 0f))
                                {
                                    float num52 = npc.Center.X + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num44;
                                    float num53 = npc.Center.Y + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num45;
                                    for (int num54 = 0; num54 < Projectile.差位射数; num54++)
                                    {
                                        num52 += Projectile.差位偏移X;
                                        num53 += Projectile.差位偏移Y;
                                        NewProjectile(Terraria.Projectile.GetNoneSource(), num52, num53, num40, num41, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai2, 弹幕Ai3, 弹幕Ai4, Projectile.持续时间);
                                    }
                                }
                            }
                        }
                        else if (Projectile.差位射数 > 0 && (Projectile.差位偏移X != 0f || Projectile.差位偏移Y != 0f))
                        {
                            float num55 = npc.Center.X + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num44;
                            float num56 = npc.Center.Y + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num45;
                            for (int num57 = 0; num57 < Projectile.差位射数; num57++)
                            {
                                num55 += Projectile.差位偏移X;
                                num56 += Projectile.差位偏移Y;
                                NewProjectile(Terraria.Projectile.GetNoneSource(), num55, num56, num40, num41, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai2, 弹幕Ai3, 弹幕Ai4, Projectile.持续时间);
                            }
                        }
                    }
                }
                else if (Projectile.差度射数 > 0 && Projectile.差度射角 != 0f)
                {
                    for (int num58 = 0; num58 < Projectile.差度射数; num58++)
                    {
                        num43 += Projectile.差度射角;
                        num40 = (float)((double)num42 * Math.Cos(num43 * Math.PI / 180.0));
                        num41 = (float)((double)num42 * Math.Sin(num43 * Math.PI / 180.0));
                        if (Projectile.速度注入AI0)
                        {
                            ai2 = (float)Math.Atan2(num41, num40);
                            num40 = 0f;
                            num41 = 0f;
                        }
                        NewProjectile(Terraria.Projectile.GetNoneSource(), npc.Center.X + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num44, npc.Center.Y + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num45, num40, num41, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai2, 弹幕Ai3, 弹幕Ai4, Projectile.持续时间);
                        if (Projectile.差位射数 > 0 && (Projectile.差位偏移X != 0f || Projectile.差位偏移Y != 0f))
                        {
                            float num59 = npc.Center.X + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num44;
                            float num60 = npc.Center.Y + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num45;
                            for (int num61 = 0; num61 < Projectile.差位射数; num61++)
                            {
                                num59 += Projectile.差位偏移X;
                                num60 += Projectile.差位偏移Y;
                                NewProjectile(Terraria.Projectile.GetNoneSource(), num59, num60, num40, num41, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai2, 弹幕Ai3, 弹幕Ai4, Projectile.持续时间);
                            }
                        }
                    }
                }
                else if (Projectile.差位射数 > 0 && (Projectile.差位偏移X != 0f || Projectile.差位偏移Y != 0f))
                {
                    float num62 = npc.Center.X + Projectile.X轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num44;
                    float num63 = npc.Center.Y + Projectile.Y轴偏移 + lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num45;
                    for (int num64 = 0; num64 < Projectile.差位射数; num64++)
                    {
                        num62 += Projectile.差位偏移X;
                        num63 += Projectile.差位偏移Y;
                        NewProjectile(Terraria.Projectile.GetNoneSource(), num62, num63, num40, num41, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai2, 弹幕Ai3, 弹幕Ai4, Projectile.持续时间);
                    }
                }
            }
        }

        public static int NewProjectile(IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, int timeLeft = -1)
        {
            int num = Projectile.NewProjectile(spawnSource, X, Y, SpeedX, SpeedY, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
            if (timeLeft == 0)
            {
                Main.projectile[num].Kill();
            }
            else if (timeLeft > 0)
            {
                Main.projectile[num].timeLeft = timeLeft;
            }
            return num;
        }

        public static void HurtMonster(List<怪物杀伤节> Hmonster, NPC npc)
        {
            foreach (怪物杀伤节 item in Hmonster)
            {
                if (item.怪物ID == 0 || item.怪物ID == 488 || item.造成伤害 == 0 && !item.直接清除)
                {
                    continue;
                }
                for (int i = 0; i < Main.npc.Length; i++)
                {
                    if (Main.npc[i] == null || !Main.npc[i].active || Main.npc[i].netID != item.怪物ID || Main.npc[i].whoAmI == npc.whoAmI || item.范围内 > 0 && !npc.WithinRange(Main.npc[i].position, item.范围内 << 4))
                    {
                        continue;
                    }
                    if (item.直接清除)
                    {
                        Main.npc[i] = new NPC();
                        NetMessage.SendData(23, -1, -1, NetworkText.Empty, i, 0f, 0f, 0f, 0, 0, 0);
                    }
                    else if (item.直接伤害)
                    {
                        NPC obj = Main.npc[i];
                        obj.life -= item.造成伤害;
                        if (Main.npc[i].life <= 0)
                        {
                            Main.npc[i].life = 1;
                        }
                        if (Main.npc[i].life > Main.npc[i].lifeMax)
                        {
                            Main.npc[i].life = Main.npc[i].lifeMax;
                        }
                        if (item.造成伤害 < 0)
                        {
                            Main.npc[i].HealEffect(Math.Abs(item.造成伤害), true);
                        }
                        Main.npc[i].netUpdate = true;
                    }
                    else
                    {
                        TSPlayer.Server.StrikeNPC(i, item.造成伤害, 0f, 0);
                    }
                }
            }
        }

        public static void PullTP(TSPlayer user, float x, float y, int r)
        {
            if (r <= 0)
            {
                user.Teleport(x, y, 1);
                return;
            }
            float x2 = user.TPlayer.Center.X;
            float y2 = user.TPlayer.Center.Y;
            x2 -= x;
            y2 -= y;
            if (x2 != 0f || y2 != 0f)
            {
                double num = Math.Atan2(y2, x2) * 180.0 / Math.PI;
                x2 = (float)(r * Math.Cos(num * Math.PI / 180.0));
                y2 = (float)(r * Math.Sin(num * Math.PI / 180.0));
                x2 += x;
                y2 += y;
                user.Teleport(x2, y2, 1);
            }
        }

        public static bool MonsterRequirement(List<怪物条件节> Rmonster, NPC npc)
        {
            NPC npc2 = npc;
            bool result = false;
            foreach (怪物条件节 monster in Rmonster)
            {
                int num = 0;
                num = monster.范围内 <= 0 ? Main.npc.Count((p) => p != null && p.active && (monster.怪物ID == 0 || p.netID == monster.怪物ID) && p.whoAmI != npc2.whoAmI && (monster.血量比 == 0 || p.lifeMax < 1 || (monster.血量比 > 0 ? p.life * 100 / p.lifeMax >= monster.血量比 : p.life * 100 / p.lifeMax < Math.Abs(monster.血量比))) && (monster.指示物 == null || global::TestPlugin.TestPlugin.LNpcs[p.whoAmI] != null && global::TestPlugin.TestPlugin.LNpcs[p.whoAmI].haveMarkers(monster.指示物)) && (monster.查标志 == "" || global::TestPlugin.TestPlugin.LNpcs[p.whoAmI] != null && global::TestPlugin.TestPlugin.LNpcs[p.whoAmI].Config != null && global::TestPlugin.TestPlugin.LNpcs[p.whoAmI].Config.标志 == monster.查标志)) : Main.npc.Count((p) => p != null && p.active && (monster.怪物ID == 0 || p.netID == monster.怪物ID) && p.whoAmI != npc2.whoAmI && npc2.WithinRange(p.position, monster.范围内 << 4) && (monster.血量比 == 0 || p.lifeMax < 1 || (monster.血量比 > 0 ? p.life * 100 / p.lifeMax >= monster.血量比 : p.life * 100 / p.lifeMax < Math.Abs(monster.血量比))) && (monster.指示物 == null || global::TestPlugin.TestPlugin.LNpcs[p.whoAmI] != null && global::TestPlugin.TestPlugin.LNpcs[p.whoAmI].haveMarkers(monster.指示物)) && (monster.查标志 == "" || global::TestPlugin.TestPlugin.LNpcs[p.whoAmI] != null && global::TestPlugin.TestPlugin.LNpcs[p.whoAmI].Config != null && global::TestPlugin.TestPlugin.LNpcs[p.whoAmI].Config.标志 == monster.查标志));
                if (monster.符合数 == 0)
                {
                    continue;
                }
                if (monster.符合数 > 0)
                {
                    if (num < monster.符合数)
                    {
                        result = true;
                        break;
                    }
                }
                else if (num >= Math.Abs(monster.符合数))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
    }
}
