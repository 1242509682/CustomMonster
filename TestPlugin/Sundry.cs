﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using TShockAPI;

namespace TestPlugin.读配置文件;

public class Sundry
{
    public static void LaunchProjectile(List<弹幕节> Projectiles, NPC npc, LNPC lnpc)
    {
        foreach (弹幕节 proj in Projectiles)
        {
            if (proj.弹幕ID <= 0)
            {
                continue;
            }
            float num = 0f;
            float num2 = 0f;
            if (!proj.初始坐标为零)
            {
                num = npc.Center.X;
                num2 = npc.Center.Y;
            }
            if (proj.锁定范围 > 0 || proj.锁定范围 == -1)
            {
                List<int> list = new List<int>();
                if (proj.锁定范围 == -1)
                {
                    list.Add(-1);
                }
                else
                {
                    for (int i = 0; i < proj.最大锁定数 && i <= TShock.Utils.GetActivePlayerCount(); i++)
                    {
                        int num3 = -1;
                        float num4 = -1f;
                        int? num5 = null;
                        int? num6 = null;
                        int? num7 = null;
                        for (int j = 0; j < 255; j++)
                        {
                            if (list.Contains(j) || Main.player[j] == null || !(Main.player[j]).active || Main.player[j].dead || (proj.仅攻击对象 && j != npc.target))
                            {
                                continue;
                            }
                            if (proj.仅扇形锁定)
                            {
                                if (proj.扇形半偏角 > 180)
                                {
                                    proj.扇形半偏角 = 180;
                                }
                                if (proj.扇形半偏角 < 1)
                                {
                                    proj.扇形半偏角 = 1;
                                }
                                float num8 = (Main.player[j]).Center.X - num;
                                float num9 = (Main.player[j]).Center.Y - num2;
                                if ((num8 != 0f || num9 != 0f) && (npc.direction != 0 || npc.directionY != 0))
                                {
                                    double num10 = Math.Atan2(num9, num8) * 180.0 / Math.PI;
                                    double num11 = Math.Atan2(npc.directionY, npc.direction) * 180.0 / Math.PI;
                                    double num12 = num11 + proj.扇形半偏角;
                                    double num13 = num11 - proj.扇形半偏角;
                                    if (num12 > 360.0)
                                    {
                                        num12 -= 360.0;
                                    }
                                    if (num13 < 0.0)
                                    {
                                        num13 += 360.0;
                                    }
                                    if (num10 > num12 && num10 < num13)
                                    {
                                        continue;
                                    }
                                }
                            }
                            float num14 = Math.Abs((Main.player[j]).Center.X - num + Math.Abs((Main.player[j]).Center.Y - num2));
                            if ((num4 == -1f || num14 < num4) && (!proj.计入仇恨 || !num6.HasValue || (proj.逆仇恨锁定 ? (Main.player[j].aggro < num6) : (Main.player[j].aggro > num6))) && (!proj.锁定血少 || !num5.HasValue || (proj.逆血量锁定 ? (Main.player[j].statLife > num5) : (Main.player[j].statLife < num5))) && (!proj.锁定低防 || !num7.HasValue || (proj.逆防御锁定 ? (Main.player[j].statDefense > num7) : (Main.player[j].statDefense < num7))))
                            {
                                if (proj.计入仇恨)
                                {
                                    num6 = Main.player[j].aggro;
                                }
                                if (proj.锁定血少)
                                {
                                    num5 = Main.player[j].statLife;
                                }
                                if (proj.锁定低防)
                                {
                                    num7 = Main.player[j].statDefense;
                                }
                                num4 = num14;
                                num3 = j;
                            }
                        }
                        if (num3 != -1)
                        {
                            list.Add(num3);
                        }
                    }
                }
                foreach (int item in list)
                {
                    float num15;
                    float num16;
                    if (item == -1)
                    {
                        num15 = num;
                        num16 = num2;
                    }
                    else
                    {
                        Player val = Main.player[item];
                        if (val == null || val.dead || val.statLife < 1)
                        {
                            continue;
                        }
                        if (proj.锁定范围 > 350)
                        {
                            proj.锁定范围 = 350;
                        }
                        if (!WithinRange(num, num2, (val).Center, proj.锁定范围 << 4))
                        {
                            continue;
                        }
                        num15 = (val).Center.X;
                        num16 = (val).Center.Y;
                    }
                    float num17 = proj.怪面向X偏移修正 * npc.direction;
                    float num18 = proj.怪面向Y偏移修正 * npc.directionY;
                    float ai = proj.弹幕Ai0 + lnpc.getMarkers(proj.指示物数量注入Ai0名) * proj.指示物数量注入Ai0系数;
                    float ai2 = proj.弹幕Ai1 + lnpc.getMarkers(proj.指示物数量注入Ai1名) * proj.指示物数量注入Ai1系数;
                    float ai3 = proj.弹幕Ai2 + lnpc.getMarkers(proj.指示物数量注入Ai2名) * proj.指示物数量注入Ai2系数;
                    float num19 = proj.锁定速度 + lnpc.getMarkers(proj.指示物数量注入锁定速度名) * proj.指示物数量注入锁定速度系数;
                    float num20;
                    float num21;
                    double num25;
                    float num22;
                    float num23;
                    if (proj.以锁定为点)
                    {
                        num20 = num15;
                        num21 = num16;
                        num22 = proj.X轴速度 + lnpc.getMarkers(proj.指示物数量注入X轴速度名) * proj.指示物数量注入X轴速度系数 + proj.怪面向X速度修正 * npc.direction;
                        num23 = proj.Y轴速度 + lnpc.getMarkers(proj.指示物数量注入Y轴速度名) * proj.指示物数量注入Y轴速度系数 + proj.怪面向Y速度修正 * npc.directionY;
                        float num24 = (float)Math.Sqrt(Math.Pow(num22, 2.0) + Math.Pow(num23, 2.0));
                        num25 = Math.Atan2(num23, num22) * 180.0 / Math.PI;
                        float num26 = proj.角度偏移 + lnpc.getMarkers(proj.指示物数量注入角度名) * proj.指示物数量注入角度系数;
                        if (num26 != 0f)
                        {
                            num25 += (double)num26;
                            num22 = (float)((double)num24 * Math.Cos(num25 * Math.PI / 180.0));
                            num23 = (float)((double)num24 * Math.Sin(num25 * Math.PI / 180.0));
                        }
                    }
                    else
                    {
                        num20 = num;
                        num21 = num2;
                        num22 = num15 - (num + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数);
                        num23 = num16 - (num2 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数);
                        if (num22 == 0f && num23 == 0f)
                        {
                            num22 = 1f;
                        }
                        num25 = Math.Atan2(num23, num22) * 180.0 / Math.PI;
                        num25 += (double)(proj.角度偏移 + lnpc.getMarkers(proj.指示物数量注入角度名) * proj.指示物数量注入角度系数);
                        num22 = (float)((double)num19 * Math.Cos(num25 * Math.PI / 180.0));
                        num23 = (float)((double)num19 * Math.Sin(num25 * Math.PI / 180.0));
                        num22 += proj.X轴速度 + lnpc.getMarkers(proj.指示物数量注入X轴速度名) * proj.指示物数量注入X轴速度系数 + proj.怪面向X速度修正 * npc.direction;
                        num23 += proj.Y轴速度 + lnpc.getMarkers(proj.指示物数量注入Y轴速度名) * proj.指示物数量注入Y轴速度系数 + proj.怪面向Y速度修正 * npc.directionY;
                    }
                    if (proj.以弹为位)
                    {
                        float num27 = num20 + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num17;
                        float num28 = num21 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num18;
                        num22 = num15 - (num27 + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数);
                        num23 = num16 - (num28 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数);
                        if (num22 == 0f && num23 == 0f)
                        {
                            num22 = 1f;
                        }
                        num25 = Math.Atan2(num23, num22) * 180.0 / Math.PI;
                        num25 += (double)(proj.角度偏移 + lnpc.getMarkers(proj.指示物数量注入角度名) * proj.指示物数量注入角度系数);
                        num22 = (float)((double)num19 * Math.Cos(num25 * Math.PI / 180.0));
                        num23 = (float)((double)num19 * Math.Sin(num25 * Math.PI / 180.0));
                        num22 += proj.X轴速度 + lnpc.getMarkers(proj.指示物数量注入X轴速度名) * proj.指示物数量注入X轴速度系数 + proj.怪面向X速度修正 * npc.direction;
                        num23 += proj.Y轴速度 + lnpc.getMarkers(proj.指示物数量注入Y轴速度名) * proj.指示物数量注入Y轴速度系数 + proj.怪面向Y速度修正 * npc.directionY;
                    }
                    if (proj.速度注入AI0)
                    {
                        ai = (float)Math.Atan2(num23, num22);
                        num22 = proj.速度注入AI0后X轴速度;
                        num23 = proj.速度注入AI0后Y轴速度;
                    }
                    float num29 = num20 + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num17;
                    float num30 = num21 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num18;
                    if (list.IndexOf(item) == 0)
                    {
                        if (proj.射出始弹X轴注入指示物名 != "")
                        {
                            lnpc.setMarkers(proj.射出始弹X轴注入指示物名, (int)num29, reset: false);
                        }
                        if (proj.射出始弹Y轴注入指示物名 != "")
                        {
                            lnpc.setMarkers(proj.射出始弹Y轴注入指示物名, (int)num30, reset: false);
                        }
                        if (proj.锁定玩家序号注入指示物名 != "")
                        {
                            lnpc.setMarkers(proj.锁定玩家序号注入指示物名, item, reset: false);
                        }
                    }
                    if (!proj.不射原始)
                    {
                        if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                        {
                            NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num29, num30, num22, num23, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai, ai2, ai3, proj.持续时间);
                        }
                        else if (proj.弹点召唤怪物 != 0)
                        {
                            LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num29, num30);
                        }
                    }
                    if (proj.始弹点怪物传送)
                    {
                        npc.Teleport(new Vector2(num29, num30), proj.始弹点怪物传送类型, proj.始弹点怪物传送信息);
                    }
                    int num31 = (int)(lnpc.getMarkers(proj.指示物数量注入差度位射数名) * proj.指示物数量注入差度位射数系数);
                    int num32 = (int)(lnpc.getMarkers(proj.指示物数量注入差度位射角名) * proj.指示物数量注入差度位射角系数);
                    int num33 = (int)(lnpc.getMarkers(proj.指示物数量注入差度位半径名) * proj.指示物数量注入差度位半径系数);
                    int num34 = (int)(lnpc.getMarkers(proj.指示物数量注入差度射数名) * proj.指示物数量注入差度射数系数);
                    int num35 = (int)(lnpc.getMarkers(proj.指示物数量注入差度射角名) * proj.指示物数量注入差度射角系数);
                    int num36 = (int)(lnpc.getMarkers(proj.指示物数量注入差位射数名) * proj.指示物数量注入差位射数系数);
                    int num37 = (int)(lnpc.getMarkers(proj.指示物数量注入差位偏移X名) * proj.指示物数量注入差位偏移X系数);
                    int num38 = (int)(lnpc.getMarkers(proj.指示物数量注入差位偏移Y名) * proj.指示物数量注入差位偏移Y系数);
                    if (proj.差度位射数 + num31 > 0 && proj.差度位射角 + num32 != 0 && proj.差度位半径 + num33 > 0)
                    {
                        double num39 = proj.差度位始角 + (int)(lnpc.getMarkers(proj.指示物数量注入差度位始角名) * proj.指示物数量注入差度位始角系数);
                        for (int k = 0; k < proj.差度位射数 + num31; k++)
                        {
                            num39 += (double)(proj.差度位射角 + num32);
                            float num40 = (float)((double)(proj.差度位半径 + num33) * Math.Cos(num39 * Math.PI / 180.0));
                            float num41 = (float)((double)(proj.差度位半径 + num33) * Math.Sin(num39 * Math.PI / 180.0));
                            if (proj.以弹为位)
                            {
                                float num42 = num20 + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num40 + num17;
                                float num43 = num21 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num41 + num18;
                                num22 = num15 - (num42 + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数);
                                num23 = num16 - (num43 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数);
                                if (num22 == 0f && num23 == 0f)
                                {
                                    num22 = 1f;
                                }
                                num25 = Math.Atan2(num23, num22) * 180.0 / Math.PI;
                                num25 += (double)(proj.角度偏移 + lnpc.getMarkers(proj.指示物数量注入角度名) * proj.指示物数量注入角度系数);
                                num22 = (float)((double)num19 * Math.Cos(num25 * Math.PI / 180.0));
                                num23 = (float)((double)num19 * Math.Sin(num25 * Math.PI / 180.0));
                                num22 += proj.X轴速度 + lnpc.getMarkers(proj.指示物数量注入X轴速度名) * proj.指示物数量注入X轴速度系数 + proj.怪面向X速度修正 * npc.direction;
                                num23 += proj.Y轴速度 + lnpc.getMarkers(proj.指示物数量注入Y轴速度名) * proj.指示物数量注入Y轴速度系数 + proj.怪面向Y速度修正 * npc.directionY;
                            }
                            if (proj.速度注入AI0)
                            {
                                ai = (float)Math.Atan2(num23, num22);
                                num22 = proj.速度注入AI0后X轴速度;
                                num23 = proj.速度注入AI0后Y轴速度;
                            }
                            num29 = num20 + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num40 + num17;
                            num30 = num21 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num41 + num18;
                            if (!proj.不射差度位)
                            {
                                if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                                {
                                    NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num29, num30, num22, num23, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai, ai2, ai3, proj.持续时间);
                                }
                                else if (proj.弹点召唤怪物 != 0)
                                {
                                    LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num29, num30);
                                }
                            }
                            if (proj.差度射数 + num34 > 0 && proj.差度射角 + num35 != 0f)
                            {
                                for (int l = 0; l < proj.差度射数 + num34; l++)
                                {
                                    num25 += (double)(proj.差度射角 + num35);
                                    num22 = (float)((double)num19 * Math.Cos(num25 * Math.PI / 180.0));
                                    num23 = (float)((double)num19 * Math.Sin(num25 * Math.PI / 180.0));
                                    if (proj.速度注入AI0)
                                    {
                                        ai = (float)Math.Atan2(num23, num22);
                                        num22 = proj.速度注入AI0后X轴速度;
                                        num23 = proj.速度注入AI0后Y轴速度;
                                    }
                                    num29 = num20 + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num17;
                                    num30 = num21 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num18;
                                    if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                                    {
                                        NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num29, num30, num22, num23, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai, ai2, ai3, proj.持续时间);
                                    }
                                    else if (proj.弹点召唤怪物 != 0)
                                    {
                                        LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num29, num30);
                                    }
                                    if (proj.差位射数 + num36 <= 0 || (proj.差位偏移X + num37 == 0f && proj.差位偏移Y + num38 == 0f))
                                    {
                                        continue;
                                    }
                                    float num44 = num20 + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num17;
                                    float num45 = num21 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num18;
                                    for (int m = 0; m < proj.差位射数 + num36; m++)
                                    {
                                        num44 += proj.差位偏移X + num37;
                                        num45 += proj.差位偏移Y + num38;
                                        num29 = num44;
                                        num30 = num45;
                                        if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                                        {
                                            NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num29, num30, num22, num23, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai, ai2, ai3, proj.持续时间);
                                        }
                                        else if (proj.弹点召唤怪物 != 0)
                                        {
                                            LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num29, num30);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (proj.差位射数 + num36 <= 0 || (proj.差位偏移X + num37 == 0f && proj.差位偏移Y + num38 == 0f))
                                {
                                    continue;
                                }
                                float num46 = num20 + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num17;
                                float num47 = num21 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num18;
                                for (int n = 0; n < proj.差位射数; n++)
                                {
                                    num46 += proj.差位偏移X + num37;
                                    num47 += proj.差位偏移Y + num38;
                                    if (proj.以弹为位)
                                    {
                                        num22 = num15 - (num46 + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数);
                                        num23 = num16 - (num47 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数);
                                        if (num22 == 0f && num23 == 0f)
                                        {
                                            num22 = 1f;
                                        }
                                        num25 = Math.Atan2(num23, num22) * 180.0 / Math.PI;
                                        num25 += (double)(proj.角度偏移 + lnpc.getMarkers(proj.指示物数量注入角度名) * proj.指示物数量注入角度系数);
                                        num22 = (float)((double)num19 * Math.Cos(num25 * Math.PI / 180.0));
                                        num23 = (float)((double)num19 * Math.Sin(num25 * Math.PI / 180.0));
                                        num22 += proj.X轴速度 + lnpc.getMarkers(proj.指示物数量注入X轴速度名) * proj.指示物数量注入X轴速度系数 + proj.怪面向X速度修正 * npc.direction;
                                        num23 += proj.Y轴速度 + lnpc.getMarkers(proj.指示物数量注入Y轴速度名) * proj.指示物数量注入Y轴速度系数 + proj.怪面向Y速度修正 * npc.directionY;
                                        if (proj.速度注入AI0)
                                        {
                                            ai = (float)Math.Atan2(num23, num22);
                                            num22 = proj.速度注入AI0后X轴速度;
                                            num23 = proj.速度注入AI0后Y轴速度;
                                        }
                                    }
                                    num29 = num46;
                                    num30 = num47;
                                    if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                                    {
                                        NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num29, num30, num22, num23, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai, ai2, ai3, proj.持续时间);
                                    }
                                    else if (proj.弹点召唤怪物 != 0)
                                    {
                                        LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num29, num30);
                                    }
                                }
                            }
                        }
                    }
                    else if (proj.差度射数 + num34 > 0 && proj.差度射角 + num35 != 0f)
                    {
                        for (int num48 = 0; num48 < proj.差度射数 + num34; num48++)
                        {
                            num25 += (double)(proj.差度射角 + num35);
                            num22 = (float)((double)num19 * Math.Cos(num25 * Math.PI / 180.0));
                            num23 = (float)((double)num19 * Math.Sin(num25 * Math.PI / 180.0));
                            if (proj.速度注入AI0)
                            {
                                ai = (float)Math.Atan2(num23, num22);
                                num22 = proj.速度注入AI0后X轴速度;
                                num23 = proj.速度注入AI0后Y轴速度;
                            }
                            num29 = num20 + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num17;
                            num30 = num21 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num18;
                            if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                            {
                                NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num29, num30, num22, num23, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai, ai2, ai3, proj.持续时间);
                            }
                            else if (proj.弹点召唤怪物 != 0)
                            {
                                LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num29, num30);
                            }
                            if (proj.差位射数 + num36 <= 0 || (proj.差位偏移X + num37 == 0f && proj.差位偏移Y + num38 == 0f))
                            {
                                continue;
                            }
                            float num49 = num20 + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num17;
                            float num50 = num21 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num18;
                            for (int num51 = 0; num51 < proj.差位射数 + num36; num51++)
                            {
                                num49 += proj.差位偏移X + num37;
                                num50 += proj.差位偏移Y + num38;
                                num29 = num49;
                                num30 = num50;
                                if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                                {
                                    NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num29, num30, num22, num23, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai, ai2, ai3, proj.持续时间);
                                }
                                else if (proj.弹点召唤怪物 != 0)
                                {
                                    LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num29, num30);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (proj.差位射数 + num36 <= 0 || (proj.差位偏移X + num37 == 0f && proj.差位偏移Y + num38 == 0f))
                        {
                            continue;
                        }
                        float num52 = num20 + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num17;
                        float num53 = num21 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num18;
                        for (int num54 = 0; num54 < proj.差位射数 + num36; num54++)
                        {
                            num52 += proj.差位偏移X + num37;
                            num53 += proj.差位偏移Y + num38;
                            num29 = num52;
                            num30 = num53;
                            if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                            {
                                NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num29, num30, num22, num23, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai, ai2, ai3, proj.持续时间);
                            }
                            else if (proj.弹点召唤怪物 != 0)
                            {
                                LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num29, num30);
                            }
                        }
                    }
                }
                continue;
            }
            float num55 = proj.X轴速度 + lnpc.getMarkers(proj.指示物数量注入X轴速度名) * proj.指示物数量注入X轴速度系数 + proj.怪面向X速度修正 * npc.direction;
            float num56 = proj.Y轴速度 + lnpc.getMarkers(proj.指示物数量注入Y轴速度名) * proj.指示物数量注入Y轴速度系数 + proj.怪面向Y速度修正 * npc.directionY;
            float num57 = (float)Math.Sqrt(Math.Pow(num55, 2.0) + Math.Pow(num56, 2.0));
            double num58 = Math.Atan2(num56, num55) * 180.0 / Math.PI;
            float num59 = proj.怪面向X偏移修正 * npc.direction;
            float num60 = proj.怪面向Y偏移修正 * npc.directionY;
            float ai4 = proj.弹幕Ai0 + lnpc.getMarkers(proj.指示物数量注入Ai0名) * proj.指示物数量注入Ai0系数;
            float ai5 = proj.弹幕Ai1 + lnpc.getMarkers(proj.指示物数量注入Ai1名) * proj.指示物数量注入Ai1系数;
            float ai6 = proj.弹幕Ai2 + lnpc.getMarkers(proj.指示物数量注入Ai2名) * proj.指示物数量注入Ai2系数;
            float num61 = proj.角度偏移 + lnpc.getMarkers(proj.指示物数量注入角度名) * proj.指示物数量注入角度系数;
            if (num61 != 0f)
            {
                num58 += (double)num61;
                num55 = (float)((double)num57 * Math.Cos(num58 * Math.PI / 180.0));
                num56 = (float)((double)num57 * Math.Sin(num58 * Math.PI / 180.0));
            }
            if (proj.速度注入AI0)
            {
                ai4 = (float)Math.Atan2(num56, num55);
                num55 = proj.速度注入AI0后X轴速度;
                num56 = proj.速度注入AI0后Y轴速度;
            }
            float num62 = num + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num59;
            float num63 = num2 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num60;
            if (proj.射出始弹X轴注入指示物名 != "")
            {
                lnpc.setMarkers(proj.射出始弹X轴注入指示物名, (int)num62, reset: false);
            }
            if (proj.射出始弹Y轴注入指示物名 != "")
            {
                lnpc.setMarkers(proj.射出始弹Y轴注入指示物名, (int)num63, reset: false);
            }
            if (!proj.不射原始)
            {
                if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                {
                    NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num62, num63, num55, num56, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai4, ai5, ai6, proj.持续时间);
                }
                else if (proj.弹点召唤怪物 != 0)
                {
                    LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num62, num63);
                }
            }
            if (proj.始弹点怪物传送)
            {
                npc.Teleport(new Vector2(num62, num63), proj.始弹点怪物传送类型, proj.始弹点怪物传送信息);
            }
            int num64 = (int)(lnpc.getMarkers(proj.指示物数量注入差度位射数名) * proj.指示物数量注入差度位射数系数);
            int num65 = (int)(lnpc.getMarkers(proj.指示物数量注入差度位射角名) * proj.指示物数量注入差度位射角系数);
            int num66 = (int)(lnpc.getMarkers(proj.指示物数量注入差度位半径名) * proj.指示物数量注入差度位半径系数);
            int num67 = (int)(lnpc.getMarkers(proj.指示物数量注入差度射数名) * proj.指示物数量注入差度射数系数);
            int num68 = (int)(lnpc.getMarkers(proj.指示物数量注入差度射角名) * proj.指示物数量注入差度射角系数);
            int num69 = (int)(lnpc.getMarkers(proj.指示物数量注入差位射数名) * proj.指示物数量注入差位射数系数);
            int num70 = (int)(lnpc.getMarkers(proj.指示物数量注入差位偏移X名) * proj.指示物数量注入差位偏移X系数);
            int num71 = (int)(lnpc.getMarkers(proj.指示物数量注入差位偏移Y名) * proj.指示物数量注入差位偏移Y系数);
            if (proj.差度位射数 + num64 > 0 && proj.差度位射角 + num65 != 0 && proj.差度位半径 + num66 > 0)
            {
                double num72 = proj.差度位始角 + (int)(lnpc.getMarkers(proj.指示物数量注入差度位始角名) * proj.指示物数量注入差度位始角系数);
                for (int num73 = 0; num73 < proj.差度位射数 + num64; num73++)
                {
                    num72 += proj.差度位射角 + num65;
                    float num74 = (float)((proj.差度位半径 + num66) * Math.Cos(num72 * Math.PI / 180.0));
                    float num75 = (float)((proj.差度位半径 + num66) * Math.Sin(num72 * Math.PI / 180.0));
                    num62 = num + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num74 + num59;
                    num63 = num2 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num75 + num60;
                    if (!proj.不射差度位)
                    {
                        if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                        {
                            NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num62, num63, num55, num56, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai4, ai5, ai6, proj.持续时间);
                        }
                        else if (proj.弹点召唤怪物 != 0)
                        {
                            LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num62, num63);
                        }
                    }
                    if (proj.差度射数 + num67 > 0 && proj.差度射角 + num68 != 0f)
                    {
                        for (int num76 = 0; num76 < proj.差度射数 + num67; num76++)
                        {
                            num58 += (double)(proj.差度射角 + num68);
                            num55 = (float)((double)num57 * Math.Cos(num58 * Math.PI / 180.0));
                            num56 = (float)((double)num57 * Math.Sin(num58 * Math.PI / 180.0));
                            if (proj.速度注入AI0)
                            {
                                ai4 = (float)Math.Atan2(num56, num55);
                                num55 = proj.速度注入AI0后X轴速度;
                                num56 = proj.速度注入AI0后Y轴速度;
                            }
                            num62 = num + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num59;
                            num63 = num2 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num60;
                            if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                            {
                                NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num62, num63, num55, num56, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai4, ai5, ai6, proj.持续时间);
                            }
                            else if (proj.弹点召唤怪物 != 0)
                            {
                                LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num62, num63);
                            }
                            if (proj.差位射数 + num69 <= 0 || (proj.差位偏移X + num70 == 0f && proj.差位偏移Y + num71 == 0f))
                            {
                                continue;
                            }
                            float num77 = num + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num59;
                            float num78 = num2 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num60;
                            for (int num79 = 0; num79 < proj.差位射数 + num69; num79++)
                            {
                                num77 += proj.差位偏移X + num70;
                                num78 += proj.差位偏移Y + num71;
                                num62 = num77;
                                num63 = num78;
                                if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                                {
                                    NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num62, num63, num55, num56, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai4, ai5, ai6, proj.持续时间);
                                }
                                else if (proj.弹点召唤怪物 != 0)
                                {
                                    LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num62, num63);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (proj.差位射数 + num69 <= 0 || (proj.差位偏移X + num70 == 0f && proj.差位偏移Y + num71 == 0f))
                        {
                            continue;
                        }
                        float num80 = num + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num59;
                        float num81 = num2 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num60;
                        for (int num82 = 0; num82 < proj.差位射数 + num69; num82++)
                        {
                            num80 += proj.差位偏移X + num70;
                            num81 += proj.差位偏移Y + num71;
                            num62 = num80;
                            num63 = num81;
                            if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                            {
                                NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num62, num63, num55, num56, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai4, ai5, ai6, proj.持续时间);
                            }
                            else if (proj.弹点召唤怪物 != 0)
                            {
                                LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num62, num63);
                            }
                        }
                    }
                }
            }
            else if (proj.差度射数 + num67 > 0 && proj.差度射角 + num68 != 0f)
            {
                for (int num83 = 0; num83 < proj.差度射数 + num67; num83++)
                {
                    num58 += (double)(proj.差度射角 + num68);
                    num55 = (float)((double)num57 * Math.Cos(num58 * Math.PI / 180.0));
                    num56 = (float)((double)num57 * Math.Sin(num58 * Math.PI / 180.0));
                    if (proj.速度注入AI0)
                    {
                        ai4 = (float)Math.Atan2(num56, num55);
                        num55 = proj.速度注入AI0后X轴速度;
                        num56 = proj.速度注入AI0后Y轴速度;
                    }
                    num62 = num + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num59;
                    num63 = num2 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num60;
                    if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                    {
                        NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num62, num63, num55, num56, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai4, ai5, ai6, proj.持续时间);
                    }
                    else if (proj.弹点召唤怪物 != 0)
                    {
                        LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num62, num63);
                    }
                    if (proj.差位射数 + num69 <= 0 || (proj.差位偏移X + num70 == 0f && proj.差位偏移Y + num71 == 0f))
                    {
                        continue;
                    }
                    float num84 = num + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num59;
                    float num85 = num2 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num60;
                    for (int num86 = 0; num86 < proj.差位射数 + num69; num86++)
                    {
                        num84 += proj.差位偏移X + num70;
                        num85 += proj.差位偏移Y + num71;
                        num62 = num84;
                        num63 = num85;
                        if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                        {
                            NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num62, num63, num55, num56, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai4, ai5, ai6, proj.持续时间);
                        }
                        else if (proj.弹点召唤怪物 != 0)
                        {
                            LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num62, num63);
                        }
                    }
                }
            }
            else
            {
                if (proj.差位射数 + num69 <= 0 || (proj.差位偏移X + num70 == 0f && proj.差位偏移Y + num71 == 0f))
                {
                    continue;
                }
                float num87 = num + proj.X轴偏移 + lnpc.getMarkers(proj.指示物数量注入X轴偏移名) * proj.指示物数量注入X轴偏移系数 + num59;
                float num88 = num2 + proj.Y轴偏移 + lnpc.getMarkers(proj.指示物数量注入Y轴偏移名) * proj.指示物数量注入Y轴偏移系数 + num60;
                for (int num89 = 0; num89 < proj.差位射数 + num69; num89++)
                {
                    num87 += proj.差位偏移X + num70;
                    num88 += proj.差位偏移Y + num71;
                    num62 = num87;
                    num63 = num88;
                    if (proj.弹点召唤怪物 == 0 || !proj.弹点召唤怪物无弹)
                    {
                        NewProjectile(npc.whoAmI, proj.标志, Terraria.Projectile.GetNoneSource(), num62, num63, num55, num56, proj.弹幕ID, proj.弹幕伤害, proj.弹幕击退, Main.myPlayer, ai4, ai5, ai6, proj.持续时间);
                    }
                    else if (proj.弹点召唤怪物 != 0)
                    {
                        LaunchProjectileSpawnNPC(proj.弹点召唤怪物, num62, num63);
                    }
                }
            }
        }
    }

    public static void LaunchProjectileSpawnNPC(int npcID, float X, float Y)
    {
        NPC nPCById = TShock.Utils.GetNPCById(npcID);
        int num = (int)X >> 4;
        int num2 = (int)Y >> 4;
        if (nPCById != null && nPCById.type != 113 && nPCById.type != 0 && nPCById.type < NPCID.Count)
        {
            TSPlayer.Server.SpawnNPC(nPCById.type, nPCById.FullName, 1, num, num2, 1, 1);
        }
    }

    public static int NewProjectile(int useIndex, string notes, IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, int timeLeft = -1)
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
        if (timeLeft != 0)
        {
            addPrjsOfUse(num, useIndex, Type, notes);
        }
        return num;
    }

    public static void HurtMonster(List<怪物杀伤节> Hmonster, NPC npc)
    {
        LNPC lNPC = TestPlugin.LNpcs[npc.whoAmI];
        if (lNPC == null || lNPC.Config == null)
        {
            return;
        }
        foreach (怪物杀伤节 item in Hmonster)
        {
            if (item.怪物ID == 0 || item.怪物ID == 488)
            {
                continue;
            }
            int 造成伤害 = item.造成伤害;
            造成伤害 += (int)(lNPC.getMarkers(item.指示物数量注入造成伤害名) * item.指示物数量注入造成伤害系数);
            if (造成伤害 == 0 && !item.直接清除)
            {
                continue;
            }
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i] == null || !(Main.npc[i]).active || Main.npc[i].netID != item.怪物ID || (Main.npc[i]).whoAmI == npc.whoAmI || (item.范围内 > 0 && !npc.WithinRange((Main.npc[i]).Center, item.范围内 << 4)) || (item.指示物 != null && TestPlugin.LNpcs[(Main.npc[i]).whoAmI] != null && !TestPlugin.LNpcs[(Main.npc[i]).whoAmI].haveMarkers(item.指示物, npc)) || (item.查标志 != "" && TestPlugin.LNpcs[(Main.npc[i]).whoAmI] != null && TestPlugin.LNpcs[(Main.npc[i]).whoAmI].Config != null && TestPlugin.LNpcs[(Main.npc[i]).whoAmI].Config.标志 != item.查标志))
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
                    obj.life -= 造成伤害;
                    if (Main.npc[i].life <= 0)
                    {
                        Main.npc[i].life = 1;
                    }
                    if (Main.npc[i].life > Main.npc[i].lifeMax)
                    {
                        Main.npc[i].life = Main.npc[i].lifeMax;
                    }
                    if (造成伤害 < 0)
                    {
                        Main.npc[i].HealEffect(Math.Abs(造成伤害), true);
                    }
                    Main.npc[i].netUpdate = true;
                }
                else
                {
                    TSPlayer.Server.StrikeNPC(i, 造成伤害, 0f, 0);
                }
            }
        }
    }

    public static void SetMonsterMarkers(List<怪物指示物修改节> Hmonster, NPC npc, ref Random rd)
    {
        LNPC lNPC = TestPlugin.LNpcs[npc.whoAmI];
        if (lNPC == null || lNPC.Config == null)
        {
            return;
        }
        foreach (怪物指示物修改节 item in Hmonster)
        {
            if (item.怪物ID == 0 || item.怪物ID == 488 || item.指示物修改 == null || item.指示物修改.Count < 1)
            {
                continue;
            }
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i] == null || !(Main.npc[i]).active || Main.npc[i].netID != item.怪物ID || (Main.npc[i]).whoAmI == npc.whoAmI || (item.范围内 > 0 && !npc.WithinRange((Main.npc[i]).Center, item.范围内 << 4)) || (item.指示物条件 != null && TestPlugin.LNpcs[(Main.npc[i]).whoAmI] != null && !TestPlugin.LNpcs[(Main.npc[i]).whoAmI].haveMarkers(item.指示物条件, npc)) || (item.查标志 != "" && TestPlugin.LNpcs[(Main.npc[i]).whoAmI] != null && TestPlugin.LNpcs[(Main.npc[i]).whoAmI].Config != null && TestPlugin.LNpcs[(Main.npc[i]).whoAmI].Config.标志 != item.查标志) || item.指示物修改 == null || TestPlugin.LNpcs[(Main.npc[i]).whoAmI] == null || TestPlugin.LNpcs[(Main.npc[i]).whoAmI].Config == null)
                {
                    continue;
                }
                foreach (指示物节 item2 in item.指示物修改)
                {
                    TestPlugin.LNpcs[(Main.npc[i]).whoAmI].setMarkers(item2.名称, item2.数量, item2.清除, item2.指示物注入数量名, item2.指示物注入数量系数, item2.指示物注入数量运算符, item2.随机小, item2.随机大, ref rd, npc);
                }
            }
        }
    }

    public static void PullTP(TSPlayer user, float x, float y, int r)
    {
        if (r == 0)
        {
            user.Teleport(x, y, (byte)1);
            return;
        }
        float x2 = (user.TPlayer).Center.X;
        float y2 = (user.TPlayer).Center.Y;
        x2 -= x;
        y2 -= y;
        if (x2 != 0f || y2 != 0f)
        {
            double num = Math.Atan2(y2, x2) * 180.0 / Math.PI;
            x2 = (float)((double)r * Math.Cos(num * Math.PI / 180.0));
            y2 = (float)((double)r * Math.Sin(num * Math.PI / 180.0));
            x2 += x;
            y2 += y;
            user.Teleport(x2, y2, (byte)1);
        }
    }

    public static void UserRepel(TSPlayer user, float x, float y, int r)
    {
        float x2 = (user.TPlayer).Center.X;
        float y2 = (user.TPlayer).Center.Y;
        x2 -= x;
        y2 -= y;
        if (x2 != 0f || y2 != 0f)
        {
            double num = Math.Atan2(y2, x2) * 180.0 / Math.PI;
            x2 = (float)((double)r * Math.Cos(num * Math.PI / 180.0));
            y2 = (float)((double)r * Math.Sin(num * Math.PI / 180.0));
            (user.TPlayer).velocity = new Vector2(x2, y2);
            NetMessage.SendData(13, -1, -1, NetworkText.Empty, user.Index, 0f, 0f, 0f, 0, 0, 0);
        }
    }

    public static int intoperation(string operation, int a, int b)
    {
        int result = 0;
        if (operation == "" || operation == "+")
        {
            result = a + b;
        }
        else
        {
            switch (operation)
            {
                case "-":
                    result = a - b;
                    break;
                case "*":
                    result = a * b;
                    break;
                case "/":
                    result = a / b;
                    break;
                case "%":
                    result = a % b;
                    break;
            }
        }
        return result;
    }

    public static bool NPCKillRequirement(Dictionary<int, long> Rmonster)
    {
        bool result = false;
        if (Rmonster.Count > 0)
        {
            foreach (KeyValuePair<int, long> item in Rmonster)
            {
                if (item.Key == 0 || item.Value == 0)
                {
                    continue;
                }
                long lNKC = TestPlugin.getLNKC(item.Key);
                if (item.Value == 0)
                {
                    continue;
                }
                if (item.Value > 0)
                {
                    if (lNKC < item.Value)
                    {
                        result = true;
                        break;
                    }
                }
                else if (lNKC >= Math.Abs(item.Value))
                {
                    result = true;
                    break;
                }
            }
        }
        return result;
    }

    public static bool AIRequirement(Dictionary<string, float> Rmonster, NPC npc)
    {
        bool result = false;
        if (Rmonster.Count > 0)
        {
            for (int i = 0; i < npc.ai.Count(); i++)
            {
                string key = i.ToString();
                if (Rmonster.ContainsKey(key) && Rmonster.TryGetValue(key, out var value) && npc.ai[i] != value)
                {
                    result = true;
                    break;
                }
                key = "!" + i;
                if (Rmonster.ContainsKey(key) && Rmonster.TryGetValue(key, out var value2) && npc.ai[i] == value2)
                {
                    result = true;
                    break;
                }
                key = ">" + i;
                if (Rmonster.ContainsKey(key) && Rmonster.TryGetValue(key, out var value3) && npc.ai[i] <= value3)
                {
                    result = true;
                    break;
                }
                key = "<" + i;
                if (Rmonster.ContainsKey(key) && Rmonster.TryGetValue(key, out var value4) && npc.ai[i] >= value4)
                {
                    result = true;
                    break;
                }
            }
        }
        return result;
    }

    public static bool AIRequirementP(Dictionary<string, float> Rmonster, Projectile Projectiles)
    {
        bool result = false;
        if (Rmonster.Count > 0)
        {
            for (int i = 0; i < Projectiles.ai.Count(); i++)
            {
                string key = i.ToString();
                if (Rmonster.ContainsKey(key) && Rmonster.TryGetValue(key, out var value) && Projectiles.ai[i] != value)
                {
                    result = true;
                    break;
                }
                key = "!" + i;
                if (Rmonster.ContainsKey(key) && Rmonster.TryGetValue(key, out var value2) && Projectiles.ai[i] == value2)
                {
                    result = true;
                    break;
                }
                key = ">" + i;
                if (Rmonster.ContainsKey(key) && Rmonster.TryGetValue(key, out var value3) && Projectiles.ai[i] <= value3)
                {
                    result = true;
                    break;
                }
                key = "<" + i;
                if (Rmonster.ContainsKey(key) && Rmonster.TryGetValue(key, out var value4) && Projectiles.ai[i] >= value4)
                {
                    result = true;
                    break;
                }
            }
        }
        return result;
    }

    public static bool WithinRange(Vector2 Center, Vector2 Target, float MaxRange)
    {
        return Vector2.DistanceSquared(Center, Target) <= MaxRange * MaxRange;
    }

    public static bool WithinRange(float x, float y, Vector2 target, float maxRange)
    {
        Vector2 pos = new Vector2(x, y);
        return Vector2.DistanceSquared(pos, target) <= maxRange * maxRange;
    }

    public static bool WithinRange(Vector2 target, float x, float y, float maxRange)
    {
        Vector2 pos = new Vector2(x, y);
        return Vector2.DistanceSquared(target, pos) <= maxRange * maxRange;
    }

    public static bool WithinRange(float x, float y, float x2, float y2, float maxRange)
    {
        Vector2 pos1 = new Vector2(x, y);
        Vector2 pos2 = new Vector2(x2, y2);
        return Vector2.DistanceSquared(pos1, pos2) <= maxRange * maxRange;
    }

    public static bool SeedRequirement(string[] Rmonster)
    {
        bool result = false;
        List<string> list = new List<string>();
        if (Main.getGoodWorld)
        {
            list.Add("getGoodWorld");
        }
        if (Main.tenthAnniversaryWorld)
        {
            list.Add("tenthAnniversaryWorld");
        }
        if (Main.notTheBeesWorld)
        {
            list.Add("notTheBeesWorld");
        }
        if (Main.dontStarveWorld)
        {
            list.Add("dontStarveWorld");
        }
        if (Main.drunkWorld)
        {
            list.Add("drunkWorld");
        }
        if (Main.remixWorld)
        {
            list.Add("remixWorld");
        }
        if (Main.noTrapsWorld)
        {
            list.Add("noTrapsWorld");
        }
        if (Main.zenithWorld)
        {
            list.Add("zenithWorld");
        }
        foreach (string text in Rmonster)
        {
            if (text == "!getGoodWorld")
            {
                if (list.Contains("getGoodWorld"))
                {
                    result = true;
                    break;
                }
            }
            else if (text == "getGoodWorld" && !list.Contains("getGoodWorld"))
            {
                result = true;
                break;
            }
            if (text == "!tenthAnniversaryWorld")
            {
                if (list.Contains("tenthAnniversaryWorld"))
                {
                    result = true;
                    break;
                }
            }
            else if (text == "tenthAnniversaryWorld" && !list.Contains("tenthAnniversaryWorld"))
            {
                result = true;
                break;
            }
            if (text == "!notTheBeesWorld")
            {
                if (list.Contains("notTheBeesWorld"))
                {
                    result = true;
                    break;
                }
            }
            else if (text == "notTheBeesWorld" && !list.Contains("notTheBeesWorld"))
            {
                result = true;
                break;
            }
            if (text == "!dontStarveWorld")
            {
                if (list.Contains("dontStarveWorld"))
                {
                    result = true;
                    break;
                }
            }
            else if (text == "dontStarveWorld" && !list.Contains("dontStarveWorld"))
            {
                result = true;
                break;
            }
            if (text == "!drunkWorld")
            {
                if (list.Contains("drunkWorld"))
                {
                    result = true;
                    break;
                }
            }
            else if (text == "drunkWorld" && !list.Contains("drunkWorld"))
            {
                result = true;
                break;
            }
            if (text == "!remixWorld")
            {
                if (list.Contains("remixWorld"))
                {
                    result = true;
                    break;
                }
            }
            else if (text == "remixWorld" && !list.Contains("remixWorld"))
            {
                result = true;
                break;
            }
            if (text == "!noTrapsWorld")
            {
                if (list.Contains("noTrapsWorld"))
                {
                    result = true;
                    break;
                }
            }
            else if (text == "noTrapsWorld" && !list.Contains("noTrapsWorld"))
            {
                result = true;
                break;
            }
            if (text == "!zenithWorld")
            {
                if (list.Contains("zenithWorld"))
                {
                    result = true;
                    break;
                }
            }
            else if (text == "zenithWorld" && !list.Contains("zenithWorld"))
            {
                result = true;
                break;
            }
        }
        return result;
    }

    public static bool PlayerRequirement(List<玩家条件节> Rmonster, NPC npc)
    {
        NPC Npc = npc;
        bool result = false;
        foreach (玩家条件节 monster in Rmonster)
        {
            if (monster.范围内 <= 0)
            {
                continue;
            }
            int num = 0;
            num = (monster.范围起 <= 0) ? TShock.Players.Count(p => p != null && p.Active && !p.Dead && p.TPlayer.statLife > 0 && (monster.生命值 == 0 || ((monster.生命值 > 0) ? (p.TPlayer.statLife >= monster.生命值) : (p.TPlayer.statLife < Math.Abs(monster.生命值)))) && Npc.WithinRange(p.TPlayer.Center, monster.范围内 << 4)) : TShock.Players.Count(p => p != null && p.Active && !p.Dead && p.TPlayer.statLife > 0 && (monster.生命值 == 0 || ((monster.生命值 > 0) ? (p.TPlayer.statLife >= monster.生命值) : (p.TPlayer.statLife < Math.Abs(monster.生命值)))) && !Npc.WithinRange(p.TPlayer.Center, monster.范围起 << 4) && Npc.WithinRange(p.TPlayer.Center, monster.范围内 << 4));
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

    public static bool MonsterRequirement(List<怪物条件节> Rmonster, NPC npc)
    {
        NPC Npc = npc;
        bool result = false;
        foreach (怪物条件节 monster in Rmonster)
        {
            int num = 0;
            num = ((monster.范围内 <= 0) ? Main.npc.Count(p => p != null && p.active && (monster.怪物ID == 0 || p.netID == monster.怪物ID) && p.whoAmI != Npc.whoAmI && (monster.血量比 == 0 || p.lifeMax < 1 || ((monster.血量比 > 0) ? (p.life * 100 / p.lifeMax >= monster.血量比) : (p.life * 100 / p.lifeMax < Math.Abs(monster.血量比)))) && (monster.指示物 == null || (TestPlugin.LNpcs[p.whoAmI] != null && TestPlugin.LNpcs[p.whoAmI].haveMarkers(monster.指示物, Npc))) && (monster.查标志 == "" || (TestPlugin.LNpcs[p.whoAmI] != null && TestPlugin.LNpcs[p.whoAmI].Config != null && TestPlugin.LNpcs[p.whoAmI].Config.标志 == monster.查标志))) : Main.npc.Count(p => p != null && p.active && (monster.怪物ID == 0 || p.netID == monster.怪物ID) && p.whoAmI != Npc.whoAmI && Npc.WithinRange(p.Center, monster.范围内 << 4) && (monster.血量比 == 0 || p.lifeMax < 1 || ((monster.血量比 > 0) ? (p.life * 100 / p.lifeMax >= monster.血量比) : (p.life * 100 / p.lifeMax < Math.Abs(monster.血量比)))) && (monster.指示物 == null || (TestPlugin.LNpcs[p.whoAmI] != null && TestPlugin.LNpcs[p.whoAmI].haveMarkers(monster.指示物, Npc))) && (monster.查标志 == "" || (TestPlugin.LNpcs[p.whoAmI] != null && TestPlugin.LNpcs[p.whoAmI].Config != null && TestPlugin.LNpcs[p.whoAmI].Config.标志 == monster.查标志))));
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

    public static bool ProjectileRequirement(List<弹幕条件节> Rmonster, NPC npc)
    {
        NPC Npc = npc;
        bool result = false;
        foreach (弹幕条件节 rmonster in Rmonster)
        {
            int num = 0;
            num = (rmonster.范围内 <= 0) ? Main.projectile.Count(p => p != null && p.active && p.owner == Main.myPlayer && (rmonster.弹幕ID == 0 || p.type == rmonster.弹幕ID) && (!rmonster.全局弹幕 || (TestPlugin.LPrjs[p.whoAmI] != null && TestPlugin.LPrjs[p.whoAmI].UseI == Npc.whoAmI)) && (rmonster.查标志 == "" || (TestPlugin.LPrjs[p.whoAmI] != null && TestPlugin.LPrjs[p.whoAmI].Notes == rmonster.查标志))) : Main.projectile.Count(p => p != null && p.active && p.owner == Main.myPlayer && (rmonster.弹幕ID == 0 || p.type == rmonster.弹幕ID) && Npc.WithinRange(p.Center, rmonster.范围内 << 4) && (!rmonster.全局弹幕 || (TestPlugin.LPrjs[p.whoAmI] != null && TestPlugin.LPrjs[p.whoAmI].UseI == Npc.whoAmI)) && (rmonster.查标志 == "" || (TestPlugin.LPrjs[p.whoAmI] != null && TestPlugin.LPrjs[p.whoAmI].Notes == rmonster.查标志)));
            if (rmonster.符合数 == 0)
            {
                continue;
            }
            if (rmonster.符合数 > 0)
            {
                if (num < rmonster.符合数)
                {
                    result = true;
                    break;
                }
            }
            else if (num >= Math.Abs(rmonster.符合数))
            {
                result = true;
                break;
            }
        }
        return result;
    }

    public static float StrToFloat(string FloatString, float DefaultFloat = 0f)
    {
        return FloatString != null && FloatString != "" ? float.TryParse(FloatString, out var result) ? result : DefaultFloat : DefaultFloat;
    }

    public static void updataProjectile(List<弹幕更新节> Projectiles, NPC npc, LNPC lnpc)
    {
        List<int> list = new List<int>();
        foreach (弹幕更新节 Projectile in Projectiles)
        {
            if (Projectile.弹幕ID <= 0)
            {
                continue;
            }
            bool flag = false;
            lock (TestPlugin.LPrjs)
            {
                for (int i = 0; i < TestPlugin.LPrjs.Length; i++)
                {
                    if (TestPlugin.LPrjs[i] == null || TestPlugin.LPrjs[i].Index < 0 || TestPlugin.LPrjs[i].Type != Projectile.弹幕ID || !(TestPlugin.LPrjs[i].Notes == Projectile.标志) || TestPlugin.LPrjs[i].UseI != npc.whoAmI)
                    {
                        continue;
                    }
                    int index = TestPlugin.LPrjs[i].Index;
                    if (Main.projectile[index] == null || !Main.projectile[index].active || Main.projectile[index].type != Projectile.弹幕ID || Main.projectile[index].owner != Main.myPlayer || AIRequirementP(Projectile.AI条件, Main.projectile[index]))
                    {
                        continue;
                    }
                    float x = Main.projectile[index].position.X;
                    float y = Main.projectile[index].position.Y;
                    if (!flag)
                    {
                        flag = true;
                        if (Projectile.弹幕X轴注入指示物名 != "")
                        {
                            lnpc.setMarkers(Projectile.弹幕X轴注入指示物名, (int)x, reset: false);
                        }
                        if (Projectile.弹幕Y轴注入指示物名 != "")
                        {
                            lnpc.setMarkers(Projectile.弹幕Y轴注入指示物名, (int)y, reset: false);
                        }
                    }
                    if (Projectile.弹点召唤怪物 != 0)
                    {
                        LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, x, y);
                    }
                    float x2 = Main.projectile[index].velocity.X;
                    float y2 = Main.projectile[index].velocity.Y;
                    int damage = Main.projectile[index].damage;
                    float knockBack = Main.projectile[index].knockBack;
                    float num = x2;
                    float num2 = y2;
                    int num3 = damage;
                    float num4 = knockBack;
                    num3 += Projectile.弹幕伤害;
                    num4 += Projectile.弹幕击退;
                    if (Projectile.锁定范围 > 0 || Projectile.锁定范围 == -1)
                    {
                        int num5 = -2;
                        if (Projectile.锁定范围 == -1)
                        {
                            num5 = -1;
                        }
                        else
                        {
                            int num6 = -1;
                            float num7 = -1f;
                            int? num8 = null;
                            int? num9 = null;
                            int? num10 = null;
                            for (int j = 0; j < 255; j++)
                            {
                                if (num5 == j || Main.player[j] == null || !Main.player[j].active || Main.player[j].dead || (Projectile.仅攻击对象 && j != npc.target))
                                {
                                    continue;
                                }
                                float num11 = Math.Abs(Main.player[j].Center.X - x + Math.Abs(Main.player[j].Center.Y - y));
                                if ((num7 == -1f || num11 < num7) && (!Projectile.计入仇恨 || !num9.HasValue || (Projectile.逆仇恨锁定 ? (Main.player[j].aggro < num9) : (Main.player[j].aggro > num9))) && (!Projectile.锁定血少 || !num8.HasValue || (Projectile.逆血量锁定 ? (Main.player[j].statLife > num8) : (Main.player[j].statLife < num8))) && (!Projectile.锁定低防 || !num10.HasValue || (Projectile.逆防御锁定 ? (Main.player[j].statDefense > num10) : (Main.player[j].statDefense < num10))))
                                {
                                    if (Projectile.计入仇恨)
                                    {
                                        num9 = Main.player[j].aggro;
                                    }
                                    if (Projectile.锁定血少)
                                    {
                                        num8 = Main.player[j].statLife;
                                    }
                                    if (Projectile.锁定低防)
                                    {
                                        num10 = Main.player[j].statDefense;
                                    }
                                    num7 = num11;
                                    num6 = j;
                                }
                            }
                            if (num6 != -1)
                            {
                                num5 = num6;
                            }
                        }
                        if (num5 != -2)
                        {
                            float x3 = npc.Center.X;
                            float y3 = npc.Center.Y;
                            bool flag2 = false;
                            float num12;
                            float num13;
                            if (num5 == -1)
                            {
                                num12 = x3;
                                num13 = y3;
                            }
                            else
                            {
                                Player val = Main.player[num5];
                                if (val == null)
                                {
                                    flag2 = true;
                                }
                                if (val.dead || val.statLife < 1)
                                {
                                    flag2 = true;
                                }
                                if (Projectile.锁定范围 > 350)
                                {
                                    Projectile.锁定范围 = 350;
                                }
                                if (!WithinRange(x3, y3, val.Center, Projectile.锁定范围 << 4))
                                {
                                    flag2 = true;
                                }
                                num12 = val.Center.X;
                                num13 = val.Center.Y;
                            }
                            if (!flag2)
                            {
                                float num14 = Projectile.锁定速度 + lnpc.getMarkers(Projectile.指示物数量注入锁定速度名) * Projectile.指示物数量注入锁定速度系数;
                                float num15 = num12 - x;
                                float num16 = num13 - y;
                                if (num15 == 0f && num16 == 0f)
                                {
                                    num15 = 1f;
                                }
                                double num17 = Math.Atan2(num16, num15) * 180.0 / Math.PI;
                                num17 += (double)(Projectile.角度偏移 + lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数);
                                num = (float)((double)num14 * Math.Cos(num17 * Math.PI / 180.0));
                                num2 = (float)((double)num14 * Math.Sin(num17 * Math.PI / 180.0));
                                num += Projectile.X轴速度 + lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数;
                                num2 += Projectile.Y轴速度 + lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数;
                            }
                        }
                    }
                    else
                    {
                        num = Projectile.X轴速度 + lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数;
                        num2 = Projectile.Y轴速度 + lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数;
                        float num18 = (float)Math.Sqrt(Math.Pow(num, 2.0) + Math.Pow(num2, 2.0));
                        double num19 = Math.Atan2(num2, num) * 180.0 / Math.PI;
                        float num20 = Projectile.角度偏移 + lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数;
                        if (num20 != 0f)
                        {
                            num19 += (double)num20;
                            num = (float)((double)num18 * Math.Cos(num19 * Math.PI / 180.0));
                            num2 = (float)((double)num18 * Math.Sin(num19 * Math.PI / 180.0));
                        }
                    }
                    if (num3 != damage)
                    {
                        Main.projectile[index].damage = num3;
                        if (!list.Contains(index))
                        {
                            list.Add(index);
                        }
                    }
                    if (num4 != knockBack)
                    {
                        Main.projectile[index].knockBack = num4;
                        if (!list.Contains(index))
                        {
                            list.Add(index);
                        }
                    }
                    if (num != x2)
                    {
                        Main.projectile[index].velocity.X = num;
                        if (!list.Contains(index))
                        {
                            list.Add(index);
                        }
                    }
                    if (num != x2)
                    {
                        Main.projectile[index].velocity.X = num;
                        if (!list.Contains(index))
                        {
                            list.Add(index);
                        }
                    }
                    if (num2 != y2)
                    {
                        Main.projectile[index].velocity.Y = num2;
                        if (!list.Contains(index))
                        {
                            list.Add(index);
                        }
                    }
                    if (Projectile.速度注入AI0)
                    {
                        float num21 = (float)Math.Atan2(num2, num);
                        num = Projectile.速度注入AI0后X轴速度;
                        num2 = Projectile.速度注入AI0后Y轴速度;
                        Main.projectile[index].ai[0] = num21;
                        if (!list.Contains(index))
                        {
                            list.Add(index);
                        }
                    }
                    if (Projectile.AI赋值.Count > 0)
                    {
                        for (int k = 0; k < Main.projectile[index].ai.Count(); k++)
                        {
                            if (Projectile.AI赋值.ContainsKey(k) && Projectile.AI赋值.TryGetValue(k, out var value))
                            {
                                Main.projectile[index].ai[k] = value;
                                if (!list.Contains(index))
                                {
                                    list.Add(index);
                                }
                            }
                        }
                    }
                    if (Projectile.指示物注入AI赋值.Count > 0)
                    {
                        for (int l = 0; l < Main.projectile[index].ai.Count(); l++)
                        {
                            if (Projectile.指示物注入AI赋值.ContainsKey(l) && Projectile.指示物注入AI赋值.TryGetValue(l, out var value2))
                            {
                                string[] array = value2.Split('*');
                                float result = 1f;
                                if (array.Length == 2 && array[1] != "")
                                {
                                    float.TryParse(array[1], out result);
                                }
                                Main.projectile[index].ai[l] = lnpc.getMarkers(value2) * result;
                                if (!list.Contains(index))
                                {
                                    list.Add(index);
                                }
                            }
                        }
                    }
                    if (Projectile.持续时间 == 0)
                    {
                        Main.projectile[index].Kill();
                    }
                    else if (Projectile.持续时间 > 0)
                    {
                        Main.projectile[index].timeLeft = Projectile.持续时间;
                    }
                    if (Projectile.清除弹幕)
                    {
                        Main.projectile[index].active = false;
                        Main.projectile[index].type = 0;
                        if (!list.Contains(index))
                        {
                            list.Add(index);
                        }
                    }
                }
            }
        }
        foreach (int item in list)
        {
            TSPlayer.All.SendData((PacketTypes)27, "", item, 0f, 0f, 0f, 0);
        }
    }

    public static void addPrjsOfUse(int pid, int useIndex, int type, string notes)
    {
        lock (TestPlugin.LPrjs)
        {
            TestPlugin.LPrjs[pid] = new LPrj(pid, useIndex, type, notes);
        }
    }
}
