using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using TShockAPI;

namespace TestPlugin;

public class Sundry
{
    #region 生成弹幕核心方法
    /// <summary>
    /// 弹幕生成系统的核心方法，处理复杂的弹幕生成逻辑
    /// </summary>
    /// <param name="Projectiles">弹幕配置列表</param>
    /// <param name="npc">发射弹幕的NPC</param>
    /// <param name="lnpc">NPC的自定义数据对象</param>
    /// <remarks>
    /// 这是整个弹幕系统的核心方法，负责处理：
    /// - 目标锁定（玩家/怪物）
    /// - 弹幕位置和速度计算
    /// - 复杂弹幕模式（差度位、差度射、差位射）
    /// - 指示物系统集成
    /// - 特殊效果（传送、召唤等）
    /// </remarks>
    public static void LaunchProjectile(List<弹幕节> Projectiles, NPC npc, LNPC lnpc)
    {
        // 遍历所有弹幕配置
        foreach (弹幕节 Projectile in Projectiles)
        {
            // 跳过无效的弹幕ID
            if (Projectile.弹幕ID <= 0)
            {
                continue;
            }

            // 初始化发射坐标
            float num = 0f;  // 发射点X坐标
            float num2 = 0f; // 发射点Y坐标

            // 如果不以零坐标为起点，使用NPC中心作为发射点
            if (!Projectile.初始坐标为零)
            {
                num = npc.Center.X;
                num2 = npc.Center.Y;
            }

            // ===== 目标锁定逻辑 =====
            if (Projectile.锁定范围 > 0 || Projectile.锁定范围 == -1)
            {
                List<int> list = new List<int>();  // 存储锁定目标列表

                // 特殊锁定模式：锁定到固定点
                if (Projectile.锁定范围 == -1)
                {
                    list.Add(-1);
                }
                else
                {
                    // 玩家锁定逻辑：根据多种条件选择目标
                    for (int i = 0; i < Projectile.最大锁定数 && i <= TShock.Utils.GetActivePlayerCount(); i++)
                    {
                        int num3 = -1;    // 当前最佳目标索引
                        float num4 = -1f; // 当前最佳距离
                        int? num5 = null; // 血量条件
                        int? num6 = null; // 仇恨条件  
                        int? num7 = null; // 防御条件
                        int num8 = -1;    // 指示物指定的玩家索引

                        // 从指示物获取锁定玩家序号
                        if (Projectile.指示物数量注入锁定玩家序号 != "")
                        {
                            num8 = lnpc.getMarkers(Projectile.指示物数量注入锁定玩家序号, -1);
                        }

                        // 如果指示物指定了有效玩家，直接使用
                        if (num8 >= 0 && num8 < 255 && Main.player[num8] != null && Main.player[num8].active && !Main.player[num8].dead)
                        {
                            list.Add(num3);
                        }

                        // 遍历所有玩家寻找最佳目标
                        int j;
                        for (j = 0; j < 255; j++)
                        {
                            // 跳过不符合条件的玩家
                            if (list.Contains(j) || Main.player[j] == null || !Main.player[j].active || Main.player[j].dead || (Projectile.仅攻击对象 && j != npc.target))
                            {
                                continue;
                            }

                            // 扇形锁定检查
                            if (Projectile.仅扇形锁定)
                            {
                                // 限制扇形角度范围
                                if (Projectile.扇形半偏角 > 180)
                                {
                                    Projectile.扇形半偏角 = 180;
                                }
                                if (Projectile.扇形半偏角 < 1)
                                {
                                    Projectile.扇形半偏角 = 1;
                                }

                                // 计算玩家相对于发射点的向量
                                float num9 = Main.player[j].Center.X - num;
                                float num10 = Main.player[j].Center.Y - num2;

                                // 计算角度并进行扇形范围判断
                                if ((num9 != 0f || num10 != 0f) && (npc.direction != 0 || npc.directionY != 0))
                                {
                                    double num11 = Math.Atan2(num10, num9) * 180.0 / Math.PI; // 玩家角度
                                    double num12 = Math.Atan2(npc.directionY, npc.direction) * 180.0 / Math.PI; // NPC朝向角度
                                    double num13 = num12 + (double)Projectile.扇形半偏角; // 扇形右边界
                                    double num14 = num12 - (double)Projectile.扇形半偏角; // 扇形左边界

                                    // 角度周期修正
                                    if (num13 > 360.0) num13 -= 360.0;
                                    if (num14 < 0.0) num14 += 360.0;

                                    // 如果玩家不在扇形范围内，跳过
                                    if (num11 > num13 && num11 < num14)
                                    {
                                        continue;
                                    }
                                }
                            }

                            // buff条件检查
                            if (Projectile.锁定状态条件.Length != 0 && !Projectile.锁定状态条件.All((int x) => Main.player[j].buffType.Contains(x)))
                            {
                                continue;
                            }

                            // 计算距离并使用多种条件选择最佳目标
                            float num15 = Math.Abs(Main.player[j].Center.X - num + Math.Abs(Main.player[j].Center.Y - num2));
                            if ((num4 == -1f || num15 < num4) && (!Projectile.计入仇恨 || !num6.HasValue || (Projectile.逆仇恨锁定 ? (Main.player[j].aggro < num6) : (Main.player[j].aggro > num6))) && (!Projectile.锁定血少 || !num5.HasValue || (Projectile.逆血量锁定 ? (Main.player[j].statLife > num5) : (Main.player[j].statLife < num5))) && (!Projectile.锁定低防 || !num7.HasValue || (Projectile.逆防御锁定 ? (Main.player[j].statDefense > num7) : (Main.player[j].statDefense < num7))))
                            {
                                // 更新最佳目标条件
                                if (Projectile.计入仇恨) num6 = Main.player[j].aggro;
                                if (Projectile.锁定血少) num5 = Main.player[j].statLife;
                                if (Projectile.锁定低防) num7 = Main.player[j].statDefense;
                                num4 = num15;
                                num3 = j;
                            }
                        }

                        // 将找到的最佳目标加入列表
                        if (num3 != -1)
                        {
                            list.Add(num3);
                        }
                    }
                }

                // ===== 对每个锁定目标生成弹幕 =====
                foreach (int item in list)
                {
                    // 计算锁定点的坐标
                    float num16 = 0f; // 锁定点X坐标
                    float num17 = 0f; // 锁定点Y坐标
                    Player val;        // 锁定玩家对象

                    if (item == -1)
                    {
                        // 特殊锁定模式：锁定到怪物或固定点
                        val = null;
                        int num18 = -1;

                        // 从指示物获取锁定怪物序号
                        if (Projectile.指示物数量注入锁定怪物序号 != "")
                        {
                            num18 = lnpc.getMarkers(Projectile.指示物数量注入锁定怪物序号, -1);
                        }

                        // 锁定到指定怪物
                        if (num18 >= 0 && num18 < 200)
                        {
                            if (Main.npc[num18] != null && Main.npc[num18].netID != 0 && Main.npc[num18].active)
                            {
                                num16 = Main.npc[num18].Center.X + Projectile.锁定点X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入锁定点X轴偏移名) * Projectile.指示物数量注入锁定点X轴偏移系数;
                                num17 = Main.npc[num18].Center.Y + Projectile.锁定点Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入锁定点Y轴偏移名) * Projectile.指示物数量注入锁定点Y轴偏移系数;
                            }
                            else
                            {
                                num18 = -1;
                            }
                        }


                        if (num18 == -1)
                        {
                            // 锁定到固定点（基于发射点）
                            num16 = num + Projectile.锁定点X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入锁定点X轴偏移名) * Projectile.指示物数量注入锁定点X轴偏移系数;
                            num17 = num2 + Projectile.锁定点Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入锁定点Y轴偏移名) * Projectile.指示物数量注入锁定点Y轴偏移系数;
                        }
                    }
                    else
                    {
                        // 锁定到玩家
                        val = Main.player[item];
                        if (val == null || val.dead || val.statLife < 1 || !WithinRange(num, num2, val.Center, Projectile.锁定范围 << 4))
                        {
                            continue; // 跳过无效玩家
                        }

                        num16 = val.Center.X + Projectile.锁定点X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入锁定点X轴偏移名) * Projectile.指示物数量注入锁定点X轴偏移系数;
                        num17 = val.Center.Y + Projectile.锁定点Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入锁定点Y轴偏移名) * Projectile.指示物数量注入锁定点Y轴偏移系数;
                    }

                    // ===== 计算弹幕参数 =====
                    // 面向修正
                    float num19 = Projectile.怪面向X偏移修正 * (float)npc.direction;
                    float num20 = Projectile.怪面向Y偏移修正 * (float)npc.directionY;

                    // AI参数（注入指示物系统）
                    float ai = Projectile.弹幕Ai0 + (float)lnpc.getMarkers(Projectile.指示物数量注入Ai0名) * Projectile.指示物数量注入Ai0系数;
                    float ai2 = Projectile.弹幕Ai1 + (float)lnpc.getMarkers(Projectile.指示物数量注入Ai1名) * Projectile.指示物数量注入Ai1系数;
                    float ai3 = Projectile.弹幕Ai2 + (float)lnpc.getMarkers(Projectile.指示物数量注入Ai2名) * Projectile.指示物数量注入Ai2系数;
                    int aiStyle = Projectile.AI风格 + (int)((float)lnpc.getMarkers(Projectile.指示物数量注入AI风格名) * Projectile.指示物数量注入AI风格系数);
                    float num21 = Projectile.锁定速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入锁定速度名) * Projectile.指示物数量注入锁定速度系数;

                    // 速度向量计算
                    float num22, num23;    // 发射点坐标
                    double num27;          // 角度（弧度）
                    float num24, num25;    // 速度向量

                    if (Projectile.以锁定为点)
                    {
                        // 模式1：以锁定点为发射点
                        num22 = num16;
                        num23 = num17;
                        num24 = Projectile.X轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数 + Projectile.怪面向X速度修正 * (float)npc.direction;
                        num25 = Projectile.Y轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数 + Projectile.怪面向Y速度修正 * (float)npc.directionY;

                        // 计算速度和角度
                        float num26 = (float)Math.Sqrt(Math.Pow(num24, 2.0) + Math.Pow(num25, 2.0));
                        num27 = Math.Atan2(num25, num24) * 180.0 / Math.PI;

                        // 应用角度偏移
                        float num28 = Projectile.角度偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数;
                        if (num28 != 0f)
                        {
                            num27 += (double)num28;
                            num24 = (float)((double)num26 * Math.Cos(num27 * Math.PI / 180.0));
                            num25 = (float)((double)num26 * Math.Sin(num27 * Math.PI / 180.0));
                        }
                    }
                    else
                    {
                        // 模式2：以NPC为发射点，朝向锁定点
                        num22 = num;
                        num23 = num2;
                        num24 = num16 - (num + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数);
                        num25 = num17 - (num2 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数);

                        // 避免零向量
                        if (num24 == 0f && num25 == 0f)
                        {
                            num24 = 1f;
                        }

                        // 计算角度并应用偏移
                        num27 = Math.Atan2(num25, num24) * 180.0 / Math.PI;
                        num27 += (double)(Projectile.角度偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数);

                        // 重新计算速度向量
                        num24 = (float)((double)num21 * Math.Cos(num27 * Math.PI / 180.0));
                        num25 = (float)((double)num21 * Math.Sin(num27 * Math.PI / 180.0));

                        // 添加额外速度
                        num24 += Projectile.X轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数 + Projectile.怪面向X速度修正 * (float)npc.direction;
                        num25 += Projectile.Y轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数 + Projectile.怪面向Y速度修正 * (float)npc.directionY;
                    }

                    // 以弹为位模式：动态调整发射点
                    if (Projectile.以弹为位)
                    {
                        float num29 = num22 + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num19;
                        float num30 = num23 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num20;

                        // 重新计算朝向锁定点的向量
                        num24 = num16 - (num29 + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数);
                        num25 = num17 - (num30 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数);

                        if (num24 == 0f && num25 == 0f)
                        {
                            num24 = 1f;
                        }

                        // 重新计算角度和速度
                        num27 = Math.Atan2(num25, num24) * 180.0 / Math.PI;
                        num27 += (double)(Projectile.角度偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数);

                        // 重新计算速度向量
                        num24 = (float)((double)num21 * Math.Cos(num27 * Math.PI / 180.0));
                        num25 = (float)((double)num21 * Math.Sin(num27 * Math.PI / 180.0));

                        // 添加额外速度
                        num24 += Projectile.X轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数 + Projectile.怪面向X速度修正 * (float)npc.direction;
                        num25 += Projectile.Y轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数 + Projectile.怪面向Y速度修正 * (float)npc.directionY;
                    }

                    // 速度注入AI0：将速度角度存入AI0并重置速度
                    if (Projectile.速度注入AI0)
                    {
                        ai = (float)Math.Atan2(num25, num24);
                        num24 = Projectile.速度注入AI0后X轴速度;
                        num25 = Projectile.速度注入AI0后Y轴速度;
                    }

                    // 计算最终发射坐标
                    float num31 = num22 + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num19;
                    float num32 = num23 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num20;

                    // ===== 首个弹幕的特殊处理 =====
                    if (list.IndexOf(item) == 0)
                    {
                        // 注入弹幕坐标到指示物
                        if (Projectile.射出始弹X轴注入指示物名 != "")
                        {
                            lnpc.setMarkers(Projectile.射出始弹X轴注入指示物名, (int)(num31 * Projectile.射出始弹X轴注入指示物系数), reset: true);
                        }
                        if (Projectile.射出始弹Y轴注入指示物名 != "")
                        {
                            lnpc.setMarkers(Projectile.射出始弹Y轴注入指示物名, (int)(num32 * Projectile.射出始弹Y轴注入指示物系数), reset: true);
                        }

                        // 注入锁定信息到指示物
                        if (Projectile.锁定玩家序号注入指示物名 != "")
                        {
                            lnpc.setMarkers(Projectile.锁定玩家序号注入指示物名, item, reset: true);
                        }
                        if (val != null && Projectile.锁定玩家血量注入指示物名 != "")
                        {
                            lnpc.setMarkers(Projectile.锁定玩家序号注入指示物名, val.statLife, reset: true);
                        }

                        // 弹幕点传送NPC
                        if (Projectile.始弹点怪物传送)
                        {
                            npc.Teleport(new Vector2(num31, num32), Projectile.始弹点怪物传送类型, Projectile.始弹点怪物传送信息);
                        }

                        // 弹速注入怪物速度
                        if (Projectile.弹速注入怪物速度)
                        {
                            if (Projectile.弹速注入怪物速度累加)
                            {
                                npc.velocity.X += num24;
                                npc.velocity.Y += num25;
                            }
                            else
                            {
                                npc.velocity = new Vector2(num24, num25);
                            }
                            npc.netUpdate = true;
                        }
                    }

                    // ===== 生成主要弹幕 =====
                    if (!Projectile.不射原始)
                    {
                        if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                        {
                            NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num31, num32, num24, num25, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, ai2, ai3, Projectile.持续时间, aiStyle);
                        }
                        else if (Projectile.弹点召唤怪物 != 0)
                        {
                            LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num31, num32);
                        }
                    }

                    // ===== 复杂弹幕模式处理 =====
                    // 获取指示物注入的复杂模式参数
                    int num33 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差度位射数名) * Projectile.指示物数量注入差度位射数系数);
                    int num34 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差度位射角名) * Projectile.指示物数量注入差度位射角系数);
                    int num35 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差度位半径名) * Projectile.指示物数量注入差度位半径系数);
                    int num36 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差度射数名) * Projectile.指示物数量注入差度射数系数);
                    int num37 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差度射角名) * Projectile.指示物数量注入差度射角系数);
                    int num38 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差位射数名) * Projectile.指示物数量注入差位射数系数);
                    int num39 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差位偏移X名) * Projectile.指示物数量注入差位偏移X系数);
                    int num40 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差位偏移Y名) * Projectile.指示物数量注入差位偏移Y系数);

                    // 差度位射模式：圆形分布弹幕
                    if (Projectile.差度位射数 + num33 > 0 && Projectile.差度位射角 + num34 != 0 && Projectile.差度位半径 + num35 > 0)
                    {
                        double num41 = Projectile.差度位始角 + (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差度位始角名) * Projectile.指示物数量注入差度位始角系数);
                        for (int k = 0; k < Projectile.差度位射数 + num33; k++)
                        {
                            num41 += (double)(Projectile.差度位射角 + num34);

                            // 计算圆形分布坐标
                            float num42 = (float)((double)(Projectile.差度位半径 + num35) * Math.Cos(num41 * Math.PI / 180.0));
                            float num43 = (float)((double)(Projectile.差度位半径 + num35) * Math.Sin(num41 * Math.PI / 180.0));

                            // 以弹为位模式的特殊处理
                            if (Projectile.以弹为位)
                            {
                                // 重新计算朝向锁定点的向量
                                float num44 = num22 + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num42 + num19;
                                float num45 = num23 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num43 + num20;
                                num24 = num16 - (num44 + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数);
                                num25 = num17 - (num45 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数);

                                // 避免零向量
                                if (num24 == 0f && num25 == 0f)
                                {
                                    num24 = 1f;
                                }

                                // 重新计算角度和速度
                                num27 = Math.Atan2(num25, num24) * 180.0 / Math.PI;
                                num27 += (double)(Projectile.角度偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数);

                                // 重新计算速度向量
                                num24 = (float)((double)num21 * Math.Cos(num27 * Math.PI / 180.0));
                                num25 = (float)((double)num21 * Math.Sin(num27 * Math.PI / 180.0));

                                // 添加额外速度
                                num24 += Projectile.X轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数 + Projectile.怪面向X速度修正 * (float)npc.direction;
                                num25 += Projectile.Y轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数 + Projectile.怪面向Y速度修正 * (float)npc.directionY;
                            }

                            // 速度注入AI0处理
                            if (Projectile.速度注入AI0)
                            {
                                ai = (float)Math.Atan2(num25, num24);
                                num24 = Projectile.速度注入AI0后X轴速度;
                                num25 = Projectile.速度注入AI0后Y轴速度;
                            }

                            // 计算最终发射坐标
                            num31 = num22 + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num42 + num19;
                            num32 = num23 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num43 + num20;

                            // 生成差度位弹幕
                            if (!Projectile.不射差度位)
                            {
                                if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                                {
                                    NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num31, num32, num24, num25, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, ai2, ai3, Projectile.持续时间, aiStyle);
                                }
                                else if (Projectile.弹点召唤怪物 != 0)
                                {
                                    LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num31, num32);
                                }
                            }

                            // 差度射模式：角度分散弹幕
                            if (Projectile.差度射数 + num36 > 0 && Projectile.差度射角 + (float)num37 != 0f)
                            {
                                for (int l = 0; l < Projectile.差度射数 + num36; l++)
                                {
                                    num27 += (double)(Projectile.差度射角 + (float)num37);

                                    // 重新计算速度向量
                                    num24 = (float)((double)num21 * Math.Cos(num27 * Math.PI / 180.0));
                                    num25 = (float)((double)num21 * Math.Sin(num27 * Math.PI / 180.0));

                                    // 速度注入AI0处理
                                    if (Projectile.速度注入AI0)
                                    {
                                        ai = (float)Math.Atan2(num25, num24);
                                        num24 = Projectile.速度注入AI0后X轴速度;
                                        num25 = Projectile.速度注入AI0后Y轴速度;
                                    }

                                    // 生成差度射弹幕
                                    num31 = num22 + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num19;
                                    num32 = num23 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num20;

                                    // 生成差度射弹幕
                                    if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                                    {
                                        NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num31, num32, num24, num25, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, ai2, ai3, Projectile.持续时间, aiStyle);
                                    }
                                    else if (Projectile.弹点召唤怪物 != 0)
                                    {
                                        LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num31, num32);
                                    }

                                    // 差位射模式：位置偏移弹幕
                                    if (Projectile.差位射数 + num38 <= 0 || (Projectile.差位偏移X + (float)num39 == 0f && Projectile.差位偏移Y + (float)num40 == 0f))
                                    {
                                        continue;
                                    }

                                    float num46 = num22 + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num19;
                                    float num47 = num23 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num20;
                                    for (int m = 0; m < Projectile.差位射数 + num38; m++)
                                    {
                                        num46 += Projectile.差位偏移X + (float)num39;
                                        num47 += Projectile.差位偏移Y + (float)num40;
                                        num31 = num46;
                                        num32 = num47;

                                        // 生成差位射弹幕
                                        if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                                        {
                                            NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num31, num32, num24, num25, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, ai2, ai3, Projectile.持续时间, aiStyle);
                                        }
                                        else if (Projectile.弹点召唤怪物 != 0)
                                        {
                                            LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num31, num32);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // 仅差位射模式（无差度射）
                                if (Projectile.差位射数 + num38 <= 0 || (Projectile.差位偏移X + (float)num39 == 0f && Projectile.差位偏移Y + (float)num40 == 0f))
                                {
                                    continue;
                                }
                                float num48 = num22 + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num19;
                                float num49 = num23 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num20;
                                for (int n = 0; n < Projectile.差位射数; n++)
                                {
                                    num48 += Projectile.差位偏移X + (float)num39;
                                    num49 += Projectile.差位偏移Y + (float)num40;
                                    if (Projectile.以弹为位)
                                    {
                                        num24 = num16 - (num48 + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数);
                                        num25 = num17 - (num49 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数);
                                        if (num24 == 0f && num25 == 0f)
                                        {
                                            num24 = 1f;
                                        }
                                        num27 = Math.Atan2(num25, num24) * 180.0 / Math.PI;
                                        num27 += (double)(Projectile.角度偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数);
                                        num24 = (float)((double)num21 * Math.Cos(num27 * Math.PI / 180.0));
                                        num25 = (float)((double)num21 * Math.Sin(num27 * Math.PI / 180.0));
                                        num24 += Projectile.X轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数 + Projectile.怪面向X速度修正 * (float)npc.direction;
                                        num25 += Projectile.Y轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数 + Projectile.怪面向Y速度修正 * (float)npc.directionY;
                                        if (Projectile.速度注入AI0)
                                        {
                                            ai = (float)Math.Atan2(num25, num24);
                                            num24 = Projectile.速度注入AI0后X轴速度;
                                            num25 = Projectile.速度注入AI0后Y轴速度;
                                        }
                                    }
                                    num31 = num48;
                                    num32 = num49;
                                    if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                                    {
                                        NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num31, num32, num24, num25, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, ai2, ai3, Projectile.持续时间, aiStyle);
                                    }
                                    else if (Projectile.弹点召唤怪物 != 0)
                                    {
                                        LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num31, num32);
                                    }
                                }
                            }
                        }
                    }

                    // 仅差度射模式（无差度位）
                    else if (Projectile.差度射数 + num36 > 0 && Projectile.差度射角 + (float)num37 != 0f)
                    {
                        for (int num50 = 0; num50 < Projectile.差度射数 + num36; num50++)
                        {
                            num27 += (double)(Projectile.差度射角 + (float)num37);
                            num24 = (float)((double)num21 * Math.Cos(num27 * Math.PI / 180.0));
                            num25 = (float)((double)num21 * Math.Sin(num27 * Math.PI / 180.0));
                            if (Projectile.速度注入AI0)
                            {
                                ai = (float)Math.Atan2(num25, num24);
                                num24 = Projectile.速度注入AI0后X轴速度;
                                num25 = Projectile.速度注入AI0后Y轴速度;
                            }
                            num31 = num22 + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num19;
                            num32 = num23 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num20;
                            if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                            {
                                NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num31, num32, num24, num25, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, ai2, ai3, Projectile.持续时间, aiStyle);
                            }
                            else if (Projectile.弹点召唤怪物 != 0)
                            {
                                LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num31, num32);
                            }
                            if (Projectile.差位射数 + num38 <= 0 || (Projectile.差位偏移X + (float)num39 == 0f && Projectile.差位偏移Y + (float)num40 == 0f))
                            {
                                continue;
                            }
                            float num51 = num22 + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num19;
                            float num52 = num23 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num20;
                            for (int num53 = 0; num53 < Projectile.差位射数 + num38; num53++)
                            {
                                num51 += Projectile.差位偏移X + (float)num39;
                                num52 += Projectile.差位偏移Y + (float)num40;
                                num31 = num51;
                                num32 = num52;
                                if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                                {
                                    NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num31, num32, num24, num25, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, ai2, ai3, Projectile.持续时间, aiStyle);
                                }
                                else if (Projectile.弹点召唤怪物 != 0)
                                {
                                    LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num31, num32);
                                }
                            }
                        }
                    }

                    // 仅差位射模式（无差度位和差度射）
                    else
                    {
                        if (Projectile.差位射数 + num38 <= 0 || (Projectile.差位偏移X + (float)num39 == 0f && Projectile.差位偏移Y + (float)num40 == 0f))
                        {
                            continue;
                        }
                        float num54 = num22 + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num19;
                        float num55 = num23 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num20;
                        for (int num56 = 0; num56 < Projectile.差位射数 + num38; num56++)
                        {
                            num54 += Projectile.差位偏移X + (float)num39;
                            num55 += Projectile.差位偏移Y + (float)num40;
                            num31 = num54;
                            num32 = num55;
                            if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                            {
                                NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num31, num32, num24, num25, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai, ai2, ai3, Projectile.持续时间, aiStyle);
                            }
                            else if (Projectile.弹点召唤怪物 != 0)
                            {
                                LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num31, num32);
                            }
                        }
                    }
                }

                // 继续处理下一个弹幕配置
                continue;
            }

            // ===== 无锁定模式的弹幕生成 =====
            // 计算基础速度
            float num57 = Projectile.X轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数 + Projectile.怪面向X速度修正 * (float)npc.direction;
            float num58 = Projectile.Y轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数 + Projectile.怪面向Y速度修正 * (float)npc.directionY;
           
            // 计算速度和角度
            float num59 = (float)Math.Sqrt(Math.Pow(num57, 2.0) + Math.Pow(num58, 2.0));
            double num60 = Math.Atan2(num58, num57) * 180.0 / Math.PI;

            // 面向修正
            float num61 = Projectile.怪面向X偏移修正 * (float)npc.direction;
            float num62 = Projectile.怪面向Y偏移修正 * (float)npc.directionY;

            // AI参数
            float ai4 = Projectile.弹幕Ai0 + (float)lnpc.getMarkers(Projectile.指示物数量注入Ai0名) * Projectile.指示物数量注入Ai0系数;
            float ai5 = Projectile.弹幕Ai1 + (float)lnpc.getMarkers(Projectile.指示物数量注入Ai1名) * Projectile.指示物数量注入Ai1系数;
            float ai6 = Projectile.弹幕Ai2 + (float)lnpc.getMarkers(Projectile.指示物数量注入Ai2名) * Projectile.指示物数量注入Ai2系数;
            int aiStyle2 = Projectile.AI风格 + (int)((float)lnpc.getMarkers(Projectile.指示物数量注入AI风格名) * Projectile.指示物数量注入AI风格系数);

            // 角度偏移
            float num63 = Projectile.角度偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数;
            if (num63 != 0f)
            {
                num60 += (double)num63;
                num57 = (float)((double)num59 * Math.Cos(num60 * Math.PI / 180.0));
                num58 = (float)((double)num59 * Math.Sin(num60 * Math.PI / 180.0));
            }

            // 速度注入AI0
            if (Projectile.速度注入AI0)
            {
                ai4 = (float)Math.Atan2(num58, num57);
                num57 = Projectile.速度注入AI0后X轴速度;
                num58 = Projectile.速度注入AI0后Y轴速度;
            }

            // 计算发射坐标
            float num64 = num + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num61;
            float num65 = num2 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num62;

            // 首个弹幕的特殊处理
            if (Projectile.射出始弹X轴注入指示物名 != "")
            {
                lnpc.setMarkers(Projectile.射出始弹X轴注入指示物名, (int)(num64 * Projectile.射出始弹X轴注入指示物系数), reset: true);
            }
            if (Projectile.射出始弹Y轴注入指示物名 != "")
            {
                lnpc.setMarkers(Projectile.射出始弹Y轴注入指示物名, (int)(num65 * Projectile.射出始弹Y轴注入指示物系数), reset: true);
            }

            // 弹幕点传送
            if (Projectile.始弹点怪物传送)
            {
                npc.Teleport(new Vector2(num64, num65), Projectile.始弹点怪物传送类型, Projectile.始弹点怪物传送信息);
            }

            // 弹速注入怪物速度
            if (Projectile.弹速注入怪物速度)
            {
                if (Projectile.弹速注入怪物速度累加)
                {
                    npc.velocity.X += num57;
                    npc.velocity.Y += num58;
                }
                else
                {
                    npc.velocity = new Vector2(num57, num58);
                }
                npc.netUpdate = true;
            }

            // 生成主要弹幕
            if (!Projectile.不射原始)
            {
                if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                {
                    NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num64, num65, num57, num58, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai4, ai5, ai6, Projectile.持续时间, aiStyle2);
                }
                else if (Projectile.弹点召唤怪物 != 0)
                {
                    LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num64, num65);
                }
            }

            // 无锁定模式的复杂弹幕模式处理（逻辑与锁定模式类似）
            int num66 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差度位射数名) * Projectile.指示物数量注入差度位射数系数);
            int num67 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差度位射角名) * Projectile.指示物数量注入差度位射角系数);
            int num68 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差度位半径名) * Projectile.指示物数量注入差度位半径系数);
            int num69 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差度射数名) * Projectile.指示物数量注入差度射数系数);
            int num70 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差度射角名) * Projectile.指示物数量注入差度射角系数);
            int num71 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差位射数名) * Projectile.指示物数量注入差位射数系数);
            int num72 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差位偏移X名) * Projectile.指示物数量注入差位偏移X系数);
            int num73 = (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差位偏移Y名) * Projectile.指示物数量注入差位偏移Y系数);

            if (Projectile.差度位射数 + num66 > 0 && Projectile.差度位射角 + num67 != 0 && Projectile.差度位半径 + num68 > 0)
            {
                double num74 = Projectile.差度位始角 + (int)((float)lnpc.getMarkers(Projectile.指示物数量注入差度位始角名) * Projectile.指示物数量注入差度位始角系数);
                for (int num75 = 0; num75 < Projectile.差度位射数 + num66; num75++)
                {
                    num74 += (double)(Projectile.差度位射角 + num67);

                    float num76 = (float)((double)(Projectile.差度位半径 + num68) * Math.Cos(num74 * Math.PI / 180.0));
                    float num77 = (float)((double)(Projectile.差度位半径 + num68) * Math.Sin(num74 * Math.PI / 180.0));

                    num64 = num + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num76 + num61;
                    num65 = num2 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num77 + num62;

                    if (!Projectile.不射差度位)
                    {
                        if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                        {
                            NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num64, num65, num57, num58, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai4, ai5, ai6, Projectile.持续时间, aiStyle2);
                        }
                        else if (Projectile.弹点召唤怪物 != 0)
                        {
                            LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num64, num65);
                        }
                    }

                    if (Projectile.差度射数 + num69 > 0 && Projectile.差度射角 + (float)num70 != 0f)
                    {
                        for (int num78 = 0; num78 < Projectile.差度射数 + num69; num78++)
                        {
                            num60 += (double)(Projectile.差度射角 + (float)num70);

                            num57 = (float)((double)num59 * Math.Cos(num60 * Math.PI / 180.0));
                            num58 = (float)((double)num59 * Math.Sin(num60 * Math.PI / 180.0));

                            if (Projectile.速度注入AI0)
                            {
                                ai4 = (float)Math.Atan2(num58, num57);
                                num57 = Projectile.速度注入AI0后X轴速度;
                                num58 = Projectile.速度注入AI0后Y轴速度;
                            }

                            num64 = num + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num61;
                            num65 = num2 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num62;

                            if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                            {
                                NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num64, num65, num57, num58, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai4, ai5, ai6, Projectile.持续时间, aiStyle2);
                            }
                            else if (Projectile.弹点召唤怪物 != 0)
                            {
                                LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num64, num65);
                            }
                            if (Projectile.差位射数 + num71 <= 0 || (Projectile.差位偏移X + (float)num72 == 0f && Projectile.差位偏移Y + (float)num73 == 0f))
                            {
                                continue;
                            }

                            float num79 = num + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num61;
                            float num80 = num2 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num62;

                            for (int num81 = 0; num81 < Projectile.差位射数 + num71; num81++)
                            {
                                num79 += Projectile.差位偏移X + (float)num72;
                                num80 += Projectile.差位偏移Y + (float)num73;
                                num64 = num79;
                                num65 = num80;

                                if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                                {
                                    NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num64, num65, num57, num58, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai4, ai5, ai6, Projectile.持续时间, aiStyle2);
                                }
                                else if (Projectile.弹点召唤怪物 != 0)
                                {
                                    LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num64, num65);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Projectile.差位射数 + num71 <= 0 || (Projectile.差位偏移X + (float)num72 == 0f && Projectile.差位偏移Y + (float)num73 == 0f))
                        {
                            continue;
                        }

                        float num82 = num + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num61;
                        float num83 = num2 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num62;

                        for (int num84 = 0; num84 < Projectile.差位射数 + num71; num84++)
                        {
                            num82 += Projectile.差位偏移X + (float)num72;
                            num83 += Projectile.差位偏移Y + (float)num73;

                            num64 = num82;
                            num65 = num83;

                            if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                            {
                                NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num64, num65, num57, num58, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai4, ai5, ai6, Projectile.持续时间, aiStyle2);
                            }
                            else if (Projectile.弹点召唤怪物 != 0)
                            {
                                LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num64, num65);
                            }
                        }
                    }
                }
            }
            else if (Projectile.差度射数 + num69 > 0 && Projectile.差度射角 + (float)num70 != 0f)
            {
                for (int num85 = 0; num85 < Projectile.差度射数 + num69; num85++)
                {
                    num60 += (double)(Projectile.差度射角 + (float)num70);

                    num57 = (float)((double)num59 * Math.Cos(num60 * Math.PI / 180.0));
                    num58 = (float)((double)num59 * Math.Sin(num60 * Math.PI / 180.0));

                    if (Projectile.速度注入AI0)
                    {
                        ai4 = (float)Math.Atan2(num58, num57);
                        num57 = Projectile.速度注入AI0后X轴速度;
                        num58 = Projectile.速度注入AI0后Y轴速度;
                    }

                    num64 = num + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num61;
                    num65 = num2 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num62;

                    if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                    {
                        NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num64, num65, num57, num58, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai4, ai5, ai6, Projectile.持续时间, aiStyle2);
                    }
                    else if (Projectile.弹点召唤怪物 != 0)
                    {
                        LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num64, num65);
                    }

                    if (Projectile.差位射数 + num71 <= 0 || (Projectile.差位偏移X + (float)num72 == 0f && Projectile.差位偏移Y + (float)num73 == 0f))
                    {
                        continue;
                    }

                    float num86 = num + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num61;
                    float num87 = num2 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num62;

                    for (int num88 = 0; num88 < Projectile.差位射数 + num71; num88++)
                    {
                        num86 += Projectile.差位偏移X + (float)num72;
                        num87 += Projectile.差位偏移Y + (float)num73;
                        num64 = num86;
                        num65 = num87;
                        if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                        {
                            NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num64, num65, num57, num58, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai4, ai5, ai6, Projectile.持续时间, aiStyle2);
                        }
                        else if (Projectile.弹点召唤怪物 != 0)
                        {
                            LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num64, num65);
                        }
                    }
                }
            }
            else
            {
                if (Projectile.差位射数 + num71 <= 0 || (Projectile.差位偏移X + (float)num72 == 0f && Projectile.差位偏移Y + (float)num73 == 0f))
                {
                    continue;
                }

                float num89 = num + Projectile.X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴偏移名) * Projectile.指示物数量注入X轴偏移系数 + num61;
                float num90 = num2 + Projectile.Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴偏移名) * Projectile.指示物数量注入Y轴偏移系数 + num62;

                for (int num91 = 0; num91 < Projectile.差位射数 + num71; num91++)
                {
                    num89 += Projectile.差位偏移X + (float)num72;
                    num90 += Projectile.差位偏移Y + (float)num73;
                    num64 = num89;
                    num65 = num90;

                    if (Projectile.弹点召唤怪物 == 0 || !Projectile.弹点召唤怪物无弹)
                    {
                        NewProjectile(npc.whoAmI, Projectile.标志, Terraria.Projectile.GetNoneSource(), num64, num65, num57, num58, Projectile.弹幕ID, Projectile.弹幕伤害, Projectile.弹幕击退, Main.myPlayer, ai4, ai5, ai6, Projectile.持续时间, aiStyle2);
                    }
                    else if (Projectile.弹点召唤怪物 != 0)
                    {
                        LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, num64, num65);
                    }
                }
            }
        }
    } 
    #endregion

    #region 辅助方法：根据弹点召唤NPC

    /// <summary>
    /// 在指定坐标位置召唤NPC
    /// </summary>
    /// <param name="npcID">要召唤的NPC ID</param>
    /// <param name="X">召唤位置的X坐标（像素）</param>
    /// <param name="Y">召唤位置的Y坐标（像素）</param>
    /// <remarks>
    /// 此方法用于在弹幕命中点或其他指定位置生成NPC
    /// 会自动将像素坐标转换为游戏内的格子坐标
    /// </remarks>
    public static void LaunchProjectileSpawnNPC(int npcID, float X, float Y)
    {
        // 根据NPC ID获取NPC模板数据
        NPC nPCById = TShock.Utils.GetNPCById(npcID);

        // 将像素坐标转换为格子坐标（右移4位相当于除以16）
        int num = (int)X >> 4;  // X坐标转换
        int num2 = (int)Y >> 4; // Y坐标转换

        // 验证NPC数据的有效性
        if (nPCById != null &&
            nPCById.netID != 113 &&  // 排除特定NPC ID 113
            nPCById.netID != 0 &&    // 排除无效NPC ID 0
            nPCById.type < NPCID.Count) // 确保NPC类型在有效范围内
        {
            // 在指定位置生成NPC
            TSPlayer.Server.SpawnNPC(
                nPCById.netID,      // NPC的网络ID
                nPCById.FullName,   // NPC的完整名称
                1,                  // 生成数量
                num,                // X坐标（格子）
                num2,               // Y坐标（格子）
                1,                  // 起始生命值
                1                   // 最大生命值
            );
        }
    }

    #endregion

    #region 创建弹幕并更新弹幕管理系统

    /// <summary>
    /// 创建新的弹幕并注册到自定义弹幕管理系统中
    /// </summary>
    /// <param name="useIndex">使用者索引（通常是NPC的whoAmI）</param>
    /// <param name="notes">弹幕标志/备注</param>
    /// <param name="spawnSource">弹幕生成源</param>
    /// <param name="X">生成位置的X坐标</param>
    /// <param name="Y">生成位置的Y坐标</param>
    /// <param name="SpeedX">X轴速度</param>
    /// <param name="SpeedY">Y轴速度</param>
    /// <param name="Type">弹幕类型ID</param>
    /// <param name="Damage">弹幕伤害值</param>
    /// <param name="KnockBack">弹幕击退力</param>
    /// <param name="Owner">弹幕所有者，默认为-1（当前玩家）</param>
    /// <param name="ai0">AI参数0，默认为0f</param>
    /// <param name="ai1">AI参数1，默认为0f</param>
    /// <param name="ai2">AI参数2，默认为0f</param>
    /// <param name="timeLeft">弹幕持续时间，默认为-1（使用默认值）</param>
    /// <param name="aiStyle">AI风格，默认为-1（使用默认值）</param>
    /// <returns>创建的弹幕索引</returns>
    /// <remarks>
    /// 此方法是对原版Projectile.NewProjectile的扩展，增加了自定义管理功能
    /// </remarks>
    public static int NewProjectile(int useIndex, string notes, IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, int timeLeft = -1, int aiStyle = -1)
    {
        // 创建新弹幕并获取其索引
        int num = Projectile.NewProjectile(spawnSource, X, Y, SpeedX, SpeedY, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);

        // 处理弹幕持续时间
        if (timeLeft == 0)
        {
            // 立即清除弹幕
            Main.projectile[num].Kill();
        }
        else if (timeLeft > 0)
        {
            // 设置自定义持续时间
            Main.projectile[num].timeLeft = timeLeft;
        }

        // 设置AI风格（如果指定了有效值）
        if (aiStyle >= 0)
        {
            Main.projectile[num].aiStyle = aiStyle;
        }

        // 如果弹幕没有被立即清除，则注册到自定义管理系统中
        if (timeLeft != 0)
        {
            addPrjsOfUse(num, useIndex, Type, notes);
        }

        // 返回创建的弹幕索引
        return num;
    }

    #endregion

    #region 对符合条件的怪物造成伤害或执行清除操作
    /// <summary>
    /// 对符合条件的怪物造成伤害或执行清除操作
    /// </summary>
    /// <param name="Hmonster">怪物杀伤配置列表</param>
    /// <param name="npc">源NPC对象（执行伤害的NPC）</param>
    /// <remarks>
    /// 此方法提供三种处理方式：直接清除、直接伤害、普通伤害攻击
    /// </remarks>
    public static void HurtMonster(List<怪物杀伤节> Hmonster, NPC npc)
    {
        // 获取当前NPC的自定义数据
        LNPC lNPC = TestPlugin.LNpcs[npc.whoAmI];

        // 检查NPC数据有效性
        if (lNPC == null || lNPC.Config == null)
        {
            return;
        }

        // 遍历所有怪物杀伤配置
        foreach (怪物杀伤节 item in Hmonster)
        {
            // 跳过特殊怪物ID 488（可能是特殊标记）
            if (item.怪物ID == 488)
            {
                continue;
            }

            // 计算实际造成的伤害值
            int 造成伤害 = item.造成伤害;

            // 添加指示物注入的伤害值
            造成伤害 += (int)((float)lNPC.getMarkers(item.指示物数量注入造成伤害名) * item.指示物数量注入造成伤害系数);

            // 如果伤害为0且不需要直接清除，则跳过
            if (造成伤害 == 0 && !item.直接清除)
            {
                continue;
            }

            // 遍历所有NPC寻找符合条件的目标
            for (int i = 0; i < Main.npc.Length; i++)
            {
                // 复杂条件筛选：检查NPC是否符合伤害条件
                if (Main.npc[i] == null ||
                    !Terraria.Main.npc[i].active ||
                    (item.怪物ID != 0 && Main.npc[i].netID != item.怪物ID) ||
                    Terraria.Main.npc[i].whoAmI == npc.whoAmI || // 排除自身
                    (item.范围内 > 0 && !npc.WithinRange(Terraria.Main.npc[i].Center, (float)(item.范围内 << 4))) || // 范围检查
                    (item.指示物 != null && TestPlugin.LNpcs[Terraria.Main.npc[i].whoAmI] != null &&
                     !TestPlugin.LNpcs[Terraria.Main.npc[i].whoAmI].haveMarkers(item.指示物, npc)) || // 指示物条件检查
                    (item.查标志 != "" && TestPlugin.LNpcs[Terraria.Main.npc[i].whoAmI] != null &&
                     TestPlugin.LNpcs[Terraria.Main.npc[i].whoAmI].Config != null &&
                     TestPlugin.LNpcs[Terraria.Main.npc[i].whoAmI].Config.标志 != item.查标志)) // 标志匹配检查
                {
                    continue; // 跳过不符合条件的NPC
                }

                // 根据配置执行不同的伤害方式
                if (item.直接清除)
                {
                    // 直接清除：立即移除怪物
                    Main.npc[i] = new NPC();
                    NetMessage.SendData(23, -1, -1, NetworkText.Empty, i, 0f, 0f, 0f, 0, 0, 0);
                }
                else if (item.直接伤害)
                {
                    // 直接伤害：直接修改生命值（不触发死亡）
                    NPC obj = Main.npc[i];
                    obj.life -= 造成伤害;

                    // 确保生命值不低于1（防止死亡）
                    if (Main.npc[i].life <= 0)
                    {
                        Main.npc[i].life = 1;
                    }

                    // 确保生命值不超过最大值
                    if (Main.npc[i].life > Main.npc[i].lifeMax)
                    {
                        Main.npc[i].life = Main.npc[i].lifeMax;
                    }

                    // 如果是治疗效果（负伤害），显示治疗特效
                    if (造成伤害 < 0)
                    {
                        Main.npc[i].HealEffect(Math.Abs(造成伤害), true);
                    }

                    // 标记网络同步
                    Main.npc[i].netUpdate = true;
                }
                else
                {
                    // 普通伤害：使用游戏内置的伤害系统
                    TSPlayer.Server.StrikeNPC(i, 造成伤害, 0f, 0);
                }
            }
        }
    } 
    #endregion

    #region 设置范围内符合条件的怪物的指示物
    /// <summary>
    /// 设置范围内符合条件的怪物的指示物
    /// </summary>
    /// <param name="Hmonster">怪物指示物修改配置列表</param>
    /// <param name="npc">源NPC对象（执行设置的NPC）</param>
    /// <param name="rd">随机数生成器引用</param>
    /// <remarks>
    /// 此方法用于批量修改指定范围内符合条件的怪物的指示物状态
    /// </remarks>
    public static void SetMonsterMarkers(List<怪物指示物修改节> Hmonster, NPC npc, ref Random rd)
    {
        // 获取当前NPC的自定义数据
        LNPC lNPC = TestPlugin.LNpcs[npc.whoAmI];

        // 检查NPC数据有效性
        if (lNPC == null || lNPC.Config == null)
        {
            return;
        }

        // 遍历所有怪物指示物修改配置
        foreach (怪物指示物修改节 item in Hmonster)
        {
            // 跳过无效配置：怪物ID为488、指示物修改为空或数量为0
            if (item.怪物ID == 488 || item.指示物修改 == null || item.指示物修改.Count < 1)
            {
                continue;
            }

            // 遍历所有NPC
            for (int i = 0; i < Main.npc.Length; i++)
            {
                // 复杂条件筛选：检查NPC是否符合修改条件
                if (Main.npc[i] == null ||
                    !Terraria.Main.npc[i].active ||
                    (item.怪物ID != 0 && Main.npc[i].netID != item.怪物ID) ||
                    Terraria.Main.npc[i].whoAmI == npc.whoAmI || // 排除自身
                    (item.范围内 > 0 && !npc.WithinRange(Terraria.Main.npc[i].Center, (float)(item.范围内 << 4))) || // 范围检查
                    (item.指示物条件 != null && TestPlugin.LNpcs[Terraria.Main.npc[i].whoAmI] != null &&
                     !TestPlugin.LNpcs[Terraria.Main.npc[i].whoAmI].haveMarkers(item.指示物条件, npc)) || // 指示物条件检查
                    (item.查标志 != "" && TestPlugin.LNpcs[Terraria.Main.npc[i].whoAmI] != null &&
                     TestPlugin.LNpcs[Terraria.Main.npc[i].whoAmI].Config != null &&
                     TestPlugin.LNpcs[Terraria.Main.npc[i].whoAmI].Config.标志 != item.查标志) || // 标志匹配检查
                    item.指示物修改 == null ||
                    TestPlugin.LNpcs[Terraria.Main.npc[i].whoAmI] == null ||
                    TestPlugin.LNpcs[Terraria.Main.npc[i].whoAmI].Config == null)
                {
                    continue; // 跳过不符合条件的NPC
                }

                // 对符合条件的NPC应用所有指示物修改
                foreach (指示物节 item2 in item.指示物修改)
                {
                    TestPlugin.LNpcs[Terraria.Main.npc[i].whoAmI].setMarkers(
                        item2.名称,           // 指示物名称
                        item2.数量,           // 指示物数量
                        item2.清除,           // 是否清除指示物
                        item2.指示物注入数量名,    // 注入数量来源指示物名
                        item2.指示物注入数量系数,  // 注入数量系数
                        item2.指示物注入数量运算符, // 注入数量运算符
                        item2.随机小,         // 随机范围最小值
                        item2.随机大,         // 随机范围最大值
                        ref rd,              // 随机数生成器
                        npc                  // 源NPC
                    );
                }
            }
        }
    } 
    #endregion

    #region 拉取玩家传送方法

    /// <summary>
    /// 将玩家拉取传送到指定位置或区域边界
    /// </summary>
    /// <param name="user">要传送的玩家对象</param>
    /// <param name="x">目标点X坐标</param>
    /// <param name="y">目标点Y坐标</param>
    /// <param name="r">拉取范围半径（像素）</param>
    /// <param name="rr">拉取模式：true=矩形边界模式，false=圆形边界模式</param>
    /// <remarks>
    /// 此方法提供两种拉取传送模式：
    /// - 矩形边界模式：将玩家拉到以目标点为中心的正方形边界上
    /// - 圆形边界模式：将玩家拉到以目标点为圆心的圆形边界上
    /// </remarks>
    public static void PullTP(TSPlayer user, float x, float y, int r, bool rr)
    {
        // 如果范围为0，直接传送到目标点
        if (r == 0)
        {
            user.Teleport(x, y, (byte)1);
            return;
        }

        // 获取玩家当前位置
        float x2 = user.TPlayer.Center.X;
        float y2 = user.TPlayer.Center.Y;

        // 计算玩家相对于目标点的向量
        x2 -= x;
        y2 -= y;

        // 检查玩家是否已经在目标点（避免除零错误）
        if (x2 != 0f || y2 != 0f)
        {
            if (rr)
            {
                // 矩形边界模式：将玩家拉到正方形的边界上

                // X轴边界计算
                x2 = (!(Math.Abs(x2) > r)) ?
                    user.TPlayer.Center.X : // 如果在范围内，保持原X坐标
                    ((!(x2 < 0f)) ?
                        (x + r) : // 如果在右侧，拉到右边界
                        (x - r)); // 如果在左侧，拉到左边界

                // Y轴边界计算        
                y2 = (!(Math.Abs(y2) > r)) ?
                    user.TPlayer.Center.Y : // 如果在范围内，保持原Y坐标
                    ((!(y2 < 0f)) ?
                        (y + r) : // 如果在上方，拉到上边界
                        (y - r)); // 如果在下方，拉到下边界
            }
            else
            {
                // 圆形边界模式：将玩家拉到圆形的边界上

                // 计算玩家相对于目标点的角度
                double num = Math.Atan2(y2, x2) * 180.0 / Math.PI;

                // 根据角度和半径计算边界点坐标
                x2 = (float)(r * Math.Cos(num * Math.PI / 180.0));
                y2 = (float)(r * Math.Sin(num * Math.PI / 180.0));

                // 将相对坐标转换为绝对坐标
                x2 += x;
                y2 += y;
            }

            // 执行传送
            user.Teleport(x2, y2, (byte)1);
        }
    }

    #endregion

    #region 玩家击退相关方法
    /// <summary>
    /// 对玩家施加击退效果
    /// </summary>
    /// <param name="user">目标玩家</param>
    /// <param name="x">击退源点的X坐标</param>
    /// <param name="y">击退源点的Y坐标</param>
    /// <param name="r">击退力度（速度大小）</param>
    /// <param name="s">是否为柔和击退（true=累加速度，false=设置速度）</param>
    public static void UserRepel(TSPlayer user, float x, float y, float r, bool s)
    {
        // 获取玩家当前位置
        float x2 = user.TPlayer.Center.X;
        float y2 = user.TPlayer.Center.Y;

        // 计算玩家相对于击退源点的向量
        x2 -= x;
        y2 -= y;

        // 如果玩家不在击退源点位置
        if (x2 != 0f || y2 != 0f)
        {
            // 计算玩家相对于击退源点的角度（弧度转角度）
            double num = Math.Atan2(y2, x2) * 180.0 / Math.PI;

            // 根据击退力度和角度计算新的速度向量
            x2 = (float)((double)r * Math.Cos(num * Math.PI / 180.0));
            y2 = (float)((double)r * Math.Sin(num * Math.PI / 180.0));

            // 根据击退模式应用速度
            if (s)
            {
                // 柔和击退：在现有速度基础上累加
                user.TPlayer.velocity.X += x2;
                user.TPlayer.velocity.Y += y2;
            }
            else
            {
                // 强制击退：直接设置速度
                user.TPlayer.velocity = new Vector2(x2, y2);
            }

            // 同步玩家速度数据到所有客户端
            NetMessage.SendData(13, -1, -1, NetworkText.Empty, user.Index, 0f, 0f, 0f, 0, 0, 0);
        }
    }

    #endregion

    #region 解析字符串运算符
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
    #endregion

    #region 杀死怪物条件
    /// <summary>
    /// 检查怪物击杀条件是否满足
    /// </summary>
    /// <param name="Rmonster">怪物击杀要求字典，Key为怪物ID，Value为要求的击杀数量</param>
    /// <returns>true: 条件不满足, false: 条件满足</returns>
    public static bool NPCKillRequirement(Dictionary<int, long> Rmonster)
    {
        bool result = false; // 默认条件满足

        // 如果有击杀条件需要检查
        if (Rmonster.Count > 0)
        {
            // 遍历所有怪物击杀条件
            foreach (KeyValuePair<int, long> item in Rmonster)
            {
                // 跳过无效条件（怪物ID为0或要求数量为0）
                if (item.Key == 0 || item.Value == 0)
                {
                    continue;
                }

                // 获取该类型怪物的当前击杀数量
                long lNKC = TestPlugin.getLNKC(item.Key);

                // 再次检查要求数量是否为0（冗余检查）
                if (item.Value == 0)
                {
                    continue;
                }

                // 检查击杀条件
                if (item.Value > 0)
                {
                    // 正数要求：需要达到最低击杀数量
                    // 如果当前击杀数小于要求数量，条件不满足
                    if (lNKC < item.Value)
                    {
                        result = true; // 标记条件不满足
                        break; // 有一个条件不满足就退出循环
                    }
                }
                else
                {
                    // 负数要求：不能超过最大击杀数量
                    // 如果当前击杀数大于等于要求数量的绝对值，条件不满足
                    if (lNKC >= Math.Abs(item.Value))
                    {
                        result = true; // 标记条件不满足
                        break; // 有一个条件不满足就退出循环
                    }
                }
            }
        }

        return result; // 返回条件检查结果
    }

    #endregion

    #region 怪物AI运算符条件
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
    #endregion

    #region 弹幕AI运算符条件
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
    #endregion

    #region 范围判断
    /// <summary>
    /// 判断两个向量位置是否在指定范围内（圆形范围检测，使用距离平方优化性能）
    /// </summary>
    /// <param name="Center">中心点位置</param>
    /// <param name="Target">目标点位置</param>
    /// <param name="MaxRange">最大范围距离</param>
    /// <returns>true: 目标在范围内, false: 目标不在范围内</returns>
    public static bool WithinRange(Vector2 Center, Vector2 Target, float MaxRange)
    {
        return Vector2.DistanceSquared(Center, Target) <= MaxRange * MaxRange;
    }

    /// <summary>
    /// 判断坐标点与向量位置是否在指定范围内（圆形范围检测）
    /// </summary>
    /// <param name="x">起始点X坐标</param>
    /// <param name="y">起始点Y坐标</param>
    /// <param name="Target">目标点位置</param>
    /// <param name="MaxRange">最大范围距离</param>
    /// <returns>true: 目标在范围内, false: 目标不在范围内</returns>
    public static bool WithinRange(float x, float y, Vector2 Target, float MaxRange)
    {
        Vector2 pos = new Vector2(x, y);
        return Vector2.DistanceSquared(pos, Target) <= MaxRange * MaxRange;
    }

    /// <summary>
    /// 判断向量位置与坐标点是否在指定范围内（圆形范围检测）
    /// </summary>
    /// <param name="Target">目标点位置</param>
    /// <param name="x">起始点X坐标</param>
    /// <param name="y">起始点Y坐标</param>
    /// <param name="MaxRange">最大范围距离</param>
    /// <returns>true: 目标在范围内, false: 目标不在范围内</returns>
    public static bool WithinRange(Vector2 Target, float x, float y, float MaxRange)
    {
        Vector2 pos = new Vector2(x, y);
        return Vector2.DistanceSquared(Target, pos) <= MaxRange * MaxRange;
    }

    /// <summary>
    /// 判断坐标点与向量位置是否在指定范围内（矩形范围检测，曼哈顿距离）
    /// </summary>
    /// <param name="Target">目标点位置</param>
    /// <param name="x">起始点X坐标</param>
    /// <param name="y">起始点Y坐标</param>
    /// <param name="MaxRange">最大范围距离</param>
    /// <returns>true: 目标在范围内, false: 目标不在范围内</returns>
    public static bool WithinRange2(Vector2 Target, float x, float y, float MaxRange)
    {
        Vector2 pos = new Vector2(x, y);
        float num = Math.Abs(Target.X - pos.X);  // X轴方向距离
        float num2 = Math.Abs(Target.Y - pos.Y); // Y轴方向距离
        return num <= MaxRange && num2 <= MaxRange; // 两个方向都满足范围条件
    }

    /// <summary>
    /// 判断两个坐标点是否在指定范围内（圆形范围检测）
    /// </summary>
    /// <param name="x">第一个点的X坐标</param>
    /// <param name="y">第一个点的Y坐标</param>
    /// <param name="x2">第二个点的X坐标</param>
    /// <param name="y2">第二个点的Y坐标</param>
    /// <param name="MaxRange">最大范围距离</param>
    /// <returns>true: 两个点在范围内, false: 两个点不在范围内</returns>
    public static bool WithinRange(float x, float y, float x2, float y2, float MaxRange)
    {
        Vector2 pos1 = new Vector2(x, y);
        Vector2 pos2 = new Vector2(x2, y2);
        return Vector2.DistanceSquared(pos1, pos2) <= MaxRange * MaxRange;
    }

    #endregion

    #region 地图种子条件
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
    #endregion

    #region 怪物玩家条件

    /// <summary>
    /// 检查NPC周围玩家条件是否满足
    /// </summary>
    /// <param name="Rmonster">玩家条件列表</param>
    /// <param name="Npc">要检查的NPC对象</param>
    /// <returns>true: 条件不满足, false: 条件满足</returns>
    public static bool PlayerRequirement(List<玩家条件节> Rmonster, NPC Npc)
    {
        NPC npc = Npc;
        bool result = false; // 默认条件满足

        // 遍历所有玩家条件
        foreach (玩家条件节 monster in Rmonster)
        {
            // 跳过无效的范围条件
            if (monster.范围内 <= 0)
            {
                continue;
            }

            int num = 0; // 符合条件的玩家数量

            // 计算满足条件的玩家数量
            num = ((monster.范围起 <= 0) ?
                // 单范围检测：从NPC位置到指定范围内
                TShock.Players.Count((TSPlayer p) =>
                    p != null && p.Active && !p.Dead && p.TPlayer.statLife > 0 && // 玩家基本状态检查
                    (monster.状态条件.Length == 0 || monster.状态条件.All((int x) => p.TPlayer.buffType.Contains(x))) && // 状态条件检查
                    (monster.生命值 == 0 || ((monster.生命值 > 0) ? (p.TPlayer.statLife >= monster.生命值) : (p.TPlayer.statLife < Math.Abs(monster.生命值)))) && // 生命值条件检查
                    (monster.生命比 == 0 || p.TPlayer.statLifeMax2 < 1 || ((monster.生命比 > 0) ? (p.TPlayer.statLife * 100 / p.TPlayer.statLifeMax2 >= monster.生命比) : (p.TPlayer.statLife * 100 / p.TPlayer.statLifeMax2 < Math.Abs(monster.生命比)))) && // 生命百分比条件检查
                    npc.WithinRange(p.TPlayer.Center, (float)(monster.范围内 << 4))) // 范围条件检查（<<4将格子数转换为像素数）
                :
                // 双范围检测：在指定起始范围外，但在最大范围内（环形区域）
                TShock.Players.Count((TSPlayer p) =>
                    p != null && p.Active && !p.Dead && p.TPlayer.statLife > 0 && // 玩家基本状态检查
                    (monster.状态条件.Length == 0 || monster.状态条件.All((int x) => p.TPlayer.buffType.Contains(x))) && // 状态条件检查
                    (monster.生命值 == 0 || ((monster.生命值 > 0) ? (p.TPlayer.statLife >= monster.生命值) : (p.TPlayer.statLife < Math.Abs(monster.生命值)))) && // 生命值条件检查
                    (monster.生命比 == 0 || p.TPlayer.statLifeMax2 < 1 || ((monster.生命比 > 0) ? (p.TPlayer.statLife * 100 / p.TPlayer.statLifeMax2 >= monster.生命比) : (p.TPlayer.statLife * 100 / p.TPlayer.statLifeMax2 < Math.Abs(monster.生命比)))) && // 生命百分比条件检查
                    !npc.WithinRange(p.TPlayer.Center, (float)(monster.范围起 << 4)) && // 不在起始范围内
                    npc.WithinRange(p.TPlayer.Center, (float)(monster.范围内 << 4)))); // 但在最大范围内

            // 跳过无效的符合数条件
            if (monster.符合数 == 0)
            {
                continue;
            }

            // 检查玩家数量条件
            if (monster.符合数 > 0)
            {
                // 正数要求：需要达到最少玩家数量
                if (num < monster.符合数)
                {
                    result = true; // 条件不满足
                    break; // 有一个条件不满足就退出循环
                }
            }
            else if (num >= Math.Abs(monster.符合数))
            {
                // 负数要求：不能超过最大玩家数量
                result = true; // 条件不满足
                break; // 有一个条件不满足就退出循环
            }
        }

        return result; // 返回条件检查结果
    }

    #endregion

    #region 弹幕玩家条件

    /// <summary>
    /// 检查弹幕周围玩家条件是否满足
    /// </summary>
    /// <param name="Rmonster">玩家条件列表</param>
    /// <param name="projectiles">要检查的弹幕对象</param>
    /// <returns>true: 条件不满足, false: 条件满足</returns>
    public static bool PlayerRequirement(List<玩家条件节> Rmonster, Projectile projectiles)
    {
        Projectile Projectiles = projectiles;
        bool result = false; // 默认条件满足

        // 遍历所有玩家条件
        foreach (玩家条件节 monster in Rmonster)
        {
            // 跳过无效的范围条件
            if (monster.范围内 <= 0)
            {
                continue;
            }

            int num = 0; // 符合条件的玩家数量

            // 计算满足条件的玩家数量
            num = ((monster.范围起 <= 0) ?
                // 单范围检测：从弹幕位置到指定范围内
                TShock.Players.Count((TSPlayer p) =>
                    p != null && p.Active && !p.Dead && p.TPlayer.statLife > 0 && // 玩家基本状态检查
                    (monster.状态条件.Length == 0 || monster.状态条件.All((int x) => p.TPlayer.buffType.Contains(x))) && // 状态条件检查
                    (monster.生命值 == 0 || ((monster.生命值 > 0) ? (p.TPlayer.statLife >= monster.生命值) : (p.TPlayer.statLife < Math.Abs(monster.生命值)))) && // 生命值条件检查
                    (monster.生命比 == 0 || p.TPlayer.statLifeMax2 < 1 || ((monster.生命比 > 0) ? (p.TPlayer.statLife * 100 / p.TPlayer.statLifeMax2 >= monster.生命比) : (p.TPlayer.statLife * 100 / p.TPlayer.statLifeMax2 < Math.Abs(monster.生命比)))) && // 生命百分比条件检查
                    Projectiles.WithinRange(p.TPlayer.Center, (float)(monster.范围内 << 4))) // 范围条件检查（以弹幕为中心）
                :
                // 双范围检测：在指定起始范围外，但在最大范围内（环形区域）
                TShock.Players.Count((TSPlayer p) =>
                    p != null && p.Active && !p.Dead && p.TPlayer.statLife > 0 && // 玩家基本状态检查
                    (monster.状态条件.Length == 0 || monster.状态条件.All((int x) => p.TPlayer.buffType.Contains(x))) && // 状态条件检查
                    (monster.生命值 == 0 || ((monster.生命值 > 0) ? (p.TPlayer.statLife >= monster.生命值) : (p.TPlayer.statLife < Math.Abs(monster.生命值)))) && // 生命值条件检查
                    (monster.生命比 == 0 || p.TPlayer.statLifeMax2 < 1 || ((monster.生命比 > 0) ? (p.TPlayer.statLife * 100 / p.TPlayer.statLifeMax2 >= monster.生命比) : (p.TPlayer.statLife * 100 / p.TPlayer.statLifeMax2 < Math.Abs(monster.生命比)))) && // 生命百分比条件检查
                    !Projectiles.WithinRange(p.TPlayer.Center, (float)(monster.范围起 << 4)) && // 不在起始范围内
                    Projectiles.WithinRange(p.TPlayer.Center, (float)(monster.范围内 << 4)))); // 但在最大范围内

            // 跳过无效的符合数条件
            if (monster.符合数 == 0)
            {
                continue;
            }

            // 检查玩家数量条件
            if (monster.符合数 > 0)
            {
                // 正数要求：需要达到最少玩家数量
                if (num < monster.符合数)
                {
                    result = true; // 条件不满足
                    break; // 有一个条件不满足就退出循环
                }
            }
            else if (num >= Math.Abs(monster.符合数))
            {
                // 负数要求：不能超过最大玩家数量
                result = true; // 条件不满足
                break; // 有一个条件不满足就退出循环
            }
        }

        return result; // 返回条件检查结果
    }

    #endregion

    #region 怪物条件
    /// <summary>
    /// 检查NPC周围其他怪物条件是否满足
    /// </summary>
    /// <param name="Rmonster">怪物条件列表</param>
    /// <param name="Npc">要检查的NPC对象</param>
    /// <returns>true: 条件不满足, false: 条件满足</returns>
    public static bool MonsterRequirement(List<怪物条件节> Rmonster, NPC Npc)
    {
        NPC npc = Npc;
        bool result = false; // 默认条件满足

        // 遍历所有怪物条件
        foreach (怪物条件节 monster in Rmonster)
        {
            int num = 0; // 符合条件的怪物数量

            // 计算满足条件的怪物数量
            num = ((monster.范围内 <= 0) ?
                // 全图检测：不考虑范围限制
                Main.npc.Count((NPC p) =>
                    p != null && p.active && // 怪物基本状态检查
                    (monster.怪物ID == 0 || p.netID == monster.怪物ID) && // 怪物ID条件检查
                    p.whoAmI != npc.whoAmI && // 排除自身
                    (monster.血量比 == 0 || p.lifeMax < 1 || ((monster.血量比 > 0) ? (p.life * 100 / p.lifeMax >= monster.血量比) : (p.life * 100 / p.lifeMax < Math.Abs(monster.血量比)))) && // 血量百分比条件检查
                    (monster.指示物 == null || (TestPlugin.LNpcs[p.whoAmI] != null && TestPlugin.LNpcs[p.whoAmI].haveMarkers(monster.指示物, npc))) && // 指示物条件检查
                    (monster.查标志 == "" || (TestPlugin.LNpcs[p.whoAmI] != null && TestPlugin.LNpcs[p.whoAmI].Config != null && TestPlugin.LNpcs[p.whoAmI].Config.标志 == monster.查标志))) // 标志条件检查
                :
                // 范围检测：在指定范围内的怪物
                Main.npc.Count((NPC p) =>
                    p != null && p.active && // 怪物基本状态检查
                    (monster.怪物ID == 0 || p.netID == monster.怪物ID) && // 怪物ID条件检查
                    p.whoAmI != npc.whoAmI && // 排除自身
                    npc.WithinRange(p.Center, (float)(monster.范围内 << 4)) && // 范围条件检查
                    (monster.血量比 == 0 || p.lifeMax < 1 || ((monster.血量比 > 0) ? (p.life * 100 / p.lifeMax >= monster.血量比) : (p.life * 100 / p.lifeMax < Math.Abs(monster.血量比)))) && // 血量百分比条件检查
                    (monster.指示物 == null || (TestPlugin.LNpcs[p.whoAmI] != null && TestPlugin.LNpcs[p.whoAmI].haveMarkers(monster.指示物, npc))) && // 指示物条件检查
                    (monster.查标志 == "" || (TestPlugin.LNpcs[p.whoAmI] != null && TestPlugin.LNpcs[p.whoAmI].Config != null && TestPlugin.LNpcs[p.whoAmI].Config.标志 == monster.查标志)))); // 标志条件检查

            // 跳过无效的符合数条件
            if (monster.符合数 == 0)
            {
                continue;
            }

            // 检查怪物数量条件
            if (monster.符合数 > 0)
            {
                // 正数要求：需要达到最少怪物数量
                if (num < monster.符合数)
                {
                    result = true; // 条件不满足
                    break; // 有一个条件不满足就退出循环
                }
            }
            else if (num >= Math.Abs(monster.符合数))
            {
                // 负数要求：不能超过最大怪物数量
                result = true; // 条件不满足
                break; // 有一个条件不满足就退出循环
            }
        }

        return result; // 返回条件检查结果
    }

    #endregion

    #region 弹幕条件
    /// <summary>
    /// 检查NPC周围弹幕条件是否满足
    /// </summary>
    /// <param name="Rmonster">弹幕条件列表</param>
    /// <param name="Npc">要检查的NPC对象</param>
    /// <returns>true: 条件不满足, false: 条件满足</returns>
    public static bool ProjectileRequirement(List<弹幕条件节> Rmonster, NPC Npc)
    {
        NPC npc = Npc;
        bool result = false; // 默认条件满足

        // 遍历所有弹幕条件
        foreach (弹幕条件节 rmonster in Rmonster)
        {
            int num = 0; // 符合条件的弹幕数量

            // 计算满足条件的弹幕数量
            num = ((rmonster.范围内 <= 0) ?
                // 全图检测：不考虑范围限制
                Main.projectile.Count((Projectile p) =>
                    p != null && p.active && // 弹幕基本状态检查
                    p.owner == Main.myPlayer && // 弹幕所有者必须是当前玩家
                    (rmonster.弹幕ID == 0 || p.type == rmonster.弹幕ID) && // 弹幕ID条件检查
                    (!rmonster.全局弹幕 || (TestPlugin.LPrjs[p.whoAmI] != null && TestPlugin.LPrjs[p.whoAmI].UseI == npc.whoAmI)) && // 全局弹幕条件检查
                    (rmonster.查标志 == "" || (TestPlugin.LPrjs[p.whoAmI] != null && TestPlugin.LPrjs[p.whoAmI].Notes == rmonster.查标志))) // 标志条件检查
                :
                // 范围检测：在指定范围内的弹幕
                Main.projectile.Count((Projectile p) =>
                    p != null && p.active && // 弹幕基本状态检查
                    p.owner == Main.myPlayer && // 弹幕所有者必须是当前玩家
                    (rmonster.弹幕ID == 0 || p.type == rmonster.弹幕ID) && // 弹幕ID条件检查
                    npc.WithinRange(p.Center, (float)(rmonster.范围内 << 4)) && // 范围条件检查
                    (!rmonster.全局弹幕 || (TestPlugin.LPrjs[p.whoAmI] != null && TestPlugin.LPrjs[p.whoAmI].UseI == npc.whoAmI)) && // 全局弹幕条件检查
                    (rmonster.查标志 == "" || (TestPlugin.LPrjs[p.whoAmI] != null && TestPlugin.LPrjs[p.whoAmI].Notes == rmonster.查标志)))); // 标志条件检查

            // 跳过无效的符合数条件
            if (rmonster.符合数 == 0)
            {
                continue;
            }

            // 检查弹幕数量条件
            if (rmonster.符合数 > 0)
            {
                // 正数要求：需要达到最少弹幕数量
                if (num < rmonster.符合数)
                {
                    result = true; // 条件不满足
                    break; // 有一个条件不满足就退出循环
                }
            }
            else if (num >= Math.Abs(rmonster.符合数))
            {
                // 负数要求：不能超过最大弹幕数量
                result = true; // 条件不满足
                break; // 有一个条件不满足就退出循环
            }
        }

        return result; // 返回条件检查结果
    }

    #endregion

    #region 字符串转浮点数

    /// <summary>
    /// 将字符串安全地转换为浮点数
    /// </summary>
    /// <param name="FloatString">要转换的字符串</param>
    /// <param name="DefaultFloat">转换失败时返回的默认值，默认为0f</param>
    /// <returns>转换后的浮点数，如果转换失败则返回默认值</returns>
    public static float StrToFloat(string FloatString, float DefaultFloat = 0f)
    {
        // 检查字符串是否为空或null
        if (FloatString != null && FloatString != "")
        {
            // 尝试将字符串转换为浮点数
            if (float.TryParse(FloatString, out var result))
            {
                // 转换成功，返回转换结果
                return result;
            }
            // 转换失败，返回默认值
            return DefaultFloat;
        }
        // 字符串为空或null，返回默认值
        return DefaultFloat;
    }

    #endregion

    #region 更改时间
    /// <summary>
    /// 更改游戏内时间（基于24小时制的分钟数）
    /// </summary>
    /// <param name="minutes">分钟数（0-1440，表示一天中的时间）</param>
    /// <remarks>
    /// 泰拉瑞亚时间系统说明：
    /// - 游戏内一天从凌晨4:30开始
    /// - 白天：4:30 - 19:30（15小时）
    /// - 夜晚：19:30 - 次日4:30（9小时）
    /// - 1440分钟 = 24小时
    /// </remarks>
    public static void changeTime(int minutes)
    {
        // 验证输入范围（0-1440分钟）
        if (minutes <= 1440 && minutes >= 0)
        {
            // 将分钟转换为小时（小数）
            decimal num = (decimal)minutes / 60.0m;

            // 调整时间偏移（泰拉瑞亚时间从4:30开始）
            num -= 4.50m;

            // 处理负值（跨天情况）
            if (num < 0.00m)
            {
                num += 24.00m;
            }

            // 判断是白天还是夜晚，并设置对应时间
            if (num >= 15.00m)
            {
                // 夜晚时间：从19:30开始计算
                // (num - 15.00m) 计算从19:30开始的经过时间
                TSPlayer.Server.SetTime(false, (double)((num - 15.00m) * 3600.0m));
            }
            else
            {
                // 白天时间：从4:30开始计算
                TSPlayer.Server.SetTime(true, (double)(num * 3600.0m));
            }
        }
    }

    #endregion

    #region 范围Buff

    /// <summary>
    /// 在指定区域内为玩家施加Buff效果
    /// </summary>
    /// <param name="center">区域中心点坐标</param>
    /// <param name="region">区域半径（以格为单位）</param>
    /// <param name="buffs">Buff字典，Key为BuffID，Value为持续时间（分钟）</param>
    /// <remarks>
    /// 此方法会在指定圆形区域内为所有符合条件的玩家施加指定的Buff效果
    /// </remarks>
    public static void regionBuff(Vector2 center, int region, Dictionary<int, int> buffs)
    {
        // 检查区域半径是否有效
        if (region <= 0)
        {
            return;
        }

        // 获取所有在线玩家
        TSPlayer[] players = TShock.Players;

        // 遍历所有玩家
        foreach (TSPlayer val in players)
        {
            // 检查玩家是否有效：非空、未死亡、生命值大于0且在指定范围内
            if (val == null || val.Dead || val.TPlayer.statLife < 1 || !WithinRange(val.TPlayer.Center, center, region * 16))
            {
                continue; // 跳过不符合条件的玩家
            }

            // 为玩家施加所有指定的Buff
            foreach (KeyValuePair<int, int> buff in buffs)
            {
                // 只应用持续时间大于0的Buff
                if (buff.Value > 0)
                {
                    // 设置Buff：BuffID、持续时间（转换为秒）、不显示提示信息
                    val.SetBuff(buff.Key, buff.Value * 60, false);
                }
            }
        }
    }

    #endregion

    #region 更新弹幕
    /// <summary>
    /// 更新和管理NPC相关的弹幕行为
    /// </summary>
    /// <param name="Projectiles">弹幕更新配置列表</param>
    /// <param name="npc">关联的NPC对象</param>
    /// <param name="lnpc">NPC的自定义数据对象</param>
    /// <remarks>
    /// 此方法负责处理弹幕的多种行为：位置更新、速度调整、目标锁定、状态效果、怪物召唤等
    /// </remarks>      
    public static void updataProjectile(List<弹幕更新节> Projectiles, NPC npc, LNPC lnpc)
    {
        // 存储需要同步更新的弹幕索引列表
        List<int> list = new List<int>();

        // 遍历所有弹幕更新配置
        foreach (弹幕更新节 Projectile in Projectiles)
        {
            bool flag = false; // 标记是否已处理过首个匹配的弹幕
            
            // 使用锁确保线程安全地访问弹幕数据
            lock (TestPlugin.LPrjs)
            {
                // 遍历所有已注册的弹幕
                for (int i = 0; i < TestPlugin.LPrjs.Count(); i++)
                {
                    // 检查弹幕是否匹配当前更新配置
                    if (TestPlugin.LPrjs[i] == null || TestPlugin.LPrjs[i].Index < 0 || (TestPlugin.LPrjs[i].Type != Projectile.弹幕ID && (!Projectile.再匹配.Contains(TestPlugin.LPrjs[i].Type) || Projectile.再匹配例外.Contains(TestPlugin.LPrjs[i].Type))) || !(TestPlugin.LPrjs[i].Notes == Projectile.标志) || TestPlugin.LPrjs[i].UseI != npc.whoAmI)
                    {
                        continue; // 跳过不匹配的弹幕
                    }

                    int index = TestPlugin.LPrjs[i].Index;

                    // 检查弹幕基本状态和条件
                    if (Main.projectile[index] == null || !Main.projectile[index].active || Main.projectile[index].owner != Main.myPlayer || PlayerRequirement(Projectile.玩家条件, Main.projectile[index]) || AIRequirementP(Projectile.AI条件, Main.projectile[index]))
                    {
                        continue;// 跳过不符合条件的弹幕
                    }

                    // 获取弹幕当前位置
                    float x2 = Main.projectile[index].Center.X;
                    float y = Main.projectile[index].Center.Y;

                    // 处理首个匹配弹幕的特殊效果
                    if (!flag)
                    {
                        // 注入弹幕位置到指示物
                        if (Projectile.弹幕X轴注入指示物名 != "")
                        {
                            lnpc.setMarkers(Projectile.弹幕X轴注入指示物名, (int)(x2 * Projectile.弹幕X轴注入指示物系数), reset: true);
                        }
                        if (Projectile.弹幕Y轴注入指示物名 != "")
                        {
                            lnpc.setMarkers(Projectile.弹幕Y轴注入指示物名, (int)(y * Projectile.弹幕Y轴注入指示物系数), reset: true);
                        }
                        if (Projectile.弹点怪物传送)
                        {
                            npc.Teleport(new Vector2(x2, y), Projectile.弹点怪物传送类型, Projectile.弹点怪物传送信息);
                        }
                    }

                    // 弹幕点怪物传送
                    if (Projectile.弹点召唤怪物 != 0)
                    {
                        LaunchProjectileSpawnNPC(Projectile.弹点召唤怪物, x2, y);
                    }

                    // 弹幕周围状态效果
                    regionBuff(new Vector2(x2, y), Projectile.弹周状态范围, Projectile.弹周状态);

                    // 弹幕周围击退效果
                    if (Projectile.弹周击退范围 > 0 && Projectile.弹周击退力度 != 0f)
                    {
                        TSPlayer[] players = TShock.Players;

                        // 遍历所有玩家并应用击退效果
                        foreach (TSPlayer uesr in players)
                        {
                            // 检查玩家是否有效：非空、未死亡、生命值大于0且在指定范围内
                            if (uesr != null && !uesr.Dead && uesr.TPlayer.statLife >= 1 && WithinRange(uesr.TPlayer.Center, new Vector2(x2, y), Projectile.弹周击退范围 * 16))
                            {
                                // 应用击退效果
                                UserRepel(uesr, x2, y, Projectile.弹周击退力度, Projectile.弹周柔和击退);
                            }
                        }
                    }


                    // 获取弹幕当前属性
                    float x3 = Main.projectile[index].velocity.X;
                    float y2 = Main.projectile[index].velocity.Y;
                    int damage = Main.projectile[index].damage;
                    float knockBack = Main.projectile[index].knockBack;

                    // 创建属性副本用于修改
                    float num = x3;
                    float num2 = y2;
                    int num3 = damage;
                    float num4 = knockBack;

                    // 应用基础属性修改
                    num3 += Projectile.弹幕伤害;
                    num4 += (float)Projectile.弹幕击退;

                    // 目标锁定逻辑
                    if (Projectile.锁定范围 > 0 || Projectile.锁定范围 == -1)
                    {
                        int num5 = -2;
                        if (Projectile.锁定范围 == -1)
                        {
                            num5 = -1;
                        }
                        else
                        {
                            // 在指定范围内寻找目标玩家
                            int num6 = -1;
                            float num7 = -1f;
                            int? num8 = null;
                            int? num9 = null;
                            int? num10 = null;
                            int num11 = -1;

                            // 优先锁定指定玩家
                            if (Projectile.指示物数量注入锁定玩家序号 != "")
                            {
                                num11 = lnpc.getMarkers(Projectile.指示物数量注入锁定玩家序号, -1);
                            }

                            // 尝试锁定指定玩家
                            if (num11 >= 0 && num11 < 255 && Main.player[num11] != null && Main.player[num11].active && !Main.player[num11].dead)
                            {
                                num5 = num6;
                            }

                            // 未锁定指定玩家时，寻找最近的符合条件玩家
                            if (num5 == -2)
                            {
                                int j;
                                // 遍历所有玩家以寻找目标
                                for (j = 0; j < 255; j++)
                                {
                                    // 跳过不符合锁定条件的玩家
                                    if (num5 == j || Main.player[j] == null || !Main.player[j].active || Main.player[j].dead || (Projectile.仅攻击对象 && j != npc.target) || (Projectile.锁定状态条件.Length != 0 && !Projectile.锁定状态条件.All((int x) => Main.player[j].buffType.Contains(x))))
                                    {
                                        continue;
                                    }

                                    // 计算玩家与弹幕的距离
                                    float num12 = Math.Abs(Main.player[j].Center.X - x2 + Math.Abs(Main.player[j].Center.Y - y));
                                    if ((num7 == -1f || num12 < num7) && (!Projectile.计入仇恨 || !num9.HasValue || (Projectile.逆仇恨锁定 ? (Main.player[j].aggro < num9) : (Main.player[j].aggro > num9))) && (!Projectile.锁定血少 || !num8.HasValue || (Projectile.逆血量锁定 ? (Main.player[j].statLife > num8) : (Main.player[j].statLife < num8))) && (!Projectile.锁定低防 || !num10.HasValue || (Projectile.逆防御锁定 ? (Main.player[j].statDefense > num10) : (Main.player[j].statDefense < num10))))
                                    {
                                        // 更新锁定目标信息
                                        if (Projectile.计入仇恨)
                                        {
                                            num9 = Main.player[j].aggro;
                                        }

                                        // 锁定血量最少的玩家
                                        if (Projectile.锁定血少)
                                        {
                                            num8 = Main.player[j].statLife;
                                        }

                                        // 锁定防御最低的玩家
                                        if (Projectile.锁定低防)
                                        {
                                            num10 = Main.player[j].statDefense;
                                        }

                                        // 记录当前锁定的玩家
                                        num7 = num12;
                                        num6 = j;
                                    }
                                }
                            }

                            // 确认锁定目标
                            if (num6 != -1)
                            {
                                num5 = num6;
                            }
                        }

                        // 处理锁定目标
                        if (num5 != -2)
                        {
                            // 计算锁定目标位置
                            float x4 = npc.Center.X;
                            float y3 = npc.Center.Y;
                            float num13 = 0f;
                            float num14 = 0f;
                            bool flag2 = false;

                            // 确定锁定目标位置
                            if (num5 == -1)
                            {
                                // 锁定怪物
                                int num15 = -1;

                                // 优先锁定指定怪物
                                if (Projectile.指示物数量注入锁定怪物序号 != "")
                                {
                                    num15 = lnpc.getMarkers(Projectile.指示物数量注入锁定怪物序号, -1);
                                }

                                // 尝试锁定指定怪物
                                if (num15 >= 0 && num15 < 200)
                                {
                                    // 检查指定怪物是否有效
                                    if (Main.npc[num15] != null && Main.npc[num15].netID != 0 && Main.npc[num15].active)
                                    {
                                        num13 = Main.npc[num15].Center.X + Projectile.锁定点X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入锁定点X轴偏移名) * Projectile.指示物数量注入锁定点X轴偏移系数;
                                        num14 = Main.npc[num15].Center.Y + Projectile.锁定点Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入锁定点Y轴偏移名) * Projectile.指示物数量注入锁定点Y轴偏移系数;
                                    }
                                    else
                                    {
                                        num15 = -1;
                                    }
                                }

                                // 未锁定指定怪物时，寻找最近的符合条件怪物
                                if (num15 == -1)
                                {
                                    num13 = x4 + Projectile.锁定点X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入锁定点X轴偏移名) * Projectile.指示物数量注入锁定点X轴偏移系数;
                                    num14 = y3 + Projectile.锁定点Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入锁定点Y轴偏移名) * Projectile.指示物数量注入锁定点Y轴偏移系数;
                                }
                            }
                            else
                            {
                                // 锁定玩家
                                Player val2 = Main.player[num5];
                                if (val2 == null)
                                {
                                    flag2 = true;
                                }
                                else if (val2.dead || val2.statLife < 1) // 无效目标
                                {
                                    flag2 = true;
                                }
                                else if (!WithinRange(x4, y3, val2.Center, Projectile.锁定范围 << 4))  // 超出锁定范围
                                {
                                    flag2 = true;
                                }
                                else // 有效目标
                                {
                                    num13 = val2.Center.X + Projectile.锁定点X轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入锁定点X轴偏移名) * Projectile.指示物数量注入锁定点X轴偏移系数;
                                    num14 = val2.Center.Y + Projectile.锁定点Y轴偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入锁定点Y轴偏移名) * Projectile.指示物数量注入锁定点Y轴偏移系数;
                                }
                            }

                            // 调整弹幕速度以锁定目标
                            if (!flag2)
                            {
                                // 计算当前速度大小
                                float num16 = (float)Math.Sqrt(Math.Pow(num, 2.0) + Math.Pow(num2, 2.0));

                                if (Projectile.速度重定义)
                                {
                                    num16 = 0f;
                                }

                                // 应用锁定速度调整
                                num16 += Projectile.锁定速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入锁定速度名) * Projectile.指示物数量注入锁定速度系数;
                                float num17 = num13 - x2;
                                float num18 = num14 - y;

                                // 避免除以零错误
                                if (num17 == 0f && num18 == 0f)
                                {
                                    num17 = 1f;
                                }

                                // 计算新的速度方向和分量
                                double num19 = Math.Atan2(num18, num17) * 180.0 / Math.PI;
                                num19 += (double)(Projectile.角度偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数);
                                num = (float)((double)num16 * Math.Cos(num19 * Math.PI / 180.0));
                                num2 = (float)((double)num16 * Math.Sin(num19 * Math.PI / 180.0));
                                num += Projectile.X轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数;
                                num2 += Projectile.Y轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数;
                            }
                        }
                    }
                    else
                    {
                        // 非锁定模式下的速度调整
                        if (Projectile.速度重定义)
                        {
                            num = 0f;
                            num2 = 0f;
                        }
                        else
                        {
                            num *= Projectile.速度乘数;
                            num2 *= Projectile.速度乘数;
                        }

                        // 应用速度增量和角度偏移
                        num += Projectile.X轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入X轴速度名) * Projectile.指示物数量注入X轴速度系数;
                        num2 += Projectile.Y轴速度 + (float)lnpc.getMarkers(Projectile.指示物数量注入Y轴速度名) * Projectile.指示物数量注入Y轴速度系数;
                        float num20 = (float)Math.Sqrt(Math.Pow(num, 2.0) + Math.Pow(num2, 2.0));
                        double num21 = Math.Atan2(num2, num) * 180.0 / Math.PI;
                        float num22 = Projectile.角度偏移 + (float)lnpc.getMarkers(Projectile.指示物数量注入角度名) * Projectile.指示物数量注入角度系数;

                        // 应用角度偏移
                        if (num22 != 0f)
                        {
                            num21 += (double)num22;
                            num = (float)((double)num20 * Math.Cos(num21 * Math.PI / 180.0));
                            num2 = (float)((double)num20 * Math.Sin(num21 * Math.PI / 180.0));
                        }
                    }

                    // 应用属性修改
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
                    if (num != x3)
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

                    // 注入速度到AI0
                    if (Projectile.速度注入AI0)
                    {
                        float num23 = (float)Math.Atan2(num2, num);
                        num = Projectile.速度注入AI0后X轴速度;
                        num2 = Projectile.速度注入AI0后Y轴速度;
                        Main.projectile[index].ai[0] = num23;
                        if (!list.Contains(index))
                        {
                            list.Add(index);
                        }
                    }

                    // 注入速度到AI1
                    if (Projectile.AI赋值.Count > 0)
                    {
                        for (int l = 0; l < Main.projectile[index].ai.Count(); l++)
                        {
                            if (Projectile.AI赋值.ContainsKey(l) && Projectile.AI赋值.TryGetValue(l, out var value))
                            {
                                Main.projectile[index].ai[l] = value;
                                if (!list.Contains(index))
                                {
                                    list.Add(index);
                                }
                            }
                        }
                    }

                    // 注入指示物到AI
                    if (Projectile.指示物注入AI赋值.Count > 0)
                    {
                        for (int m = 0; m < Main.projectile[index].ai.Count(); m++)
                        {
                            if (Projectile.指示物注入AI赋值.ContainsKey(m) && Projectile.指示物注入AI赋值.TryGetValue(m, out string value2))
                            {
                                string[] array = value2.Split('*');
                                float result = 1f;
                                if (array.Length == 2 && array[1] != "" && float.TryParse(array[1], out result))
                                {
                                    value2 = array[0];
                                }
                                Main.projectile[index].ai[m] = (float)lnpc.getMarkers(value2) * result;
                                if (!list.Contains(index))
                                {
                                    list.Add(index);
                                }
                            }
                        }
                    }

                    if (Projectile.弹幕变形 >= 0)
                    {
                        Main.projectile[index].SetDefaults(Projectile.弹幕变形);
                        if (!list.Contains(index))
                        {
                            list.Add(index);
                        }
                    }
                    if (Projectile.AI风格赋值 >= 0)
                    {
                        Main.projectile[index].aiStyle = Projectile.AI风格赋值;
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

                    // 标记已处理首个匹配弹幕
                    if (!flag)
                    {
                        flag = true;
                    }
                }
            }
        }

        // 同步更新弹幕数据到客户端
        foreach (int item in list)
        {
            TSPlayer.All.SendData((PacketTypes)27, "", item, 0f, 0f, 0f, 0);
        }
    } 
    #endregion

    #region 添加需要更新的弹幕方法
    public static void addPrjsOfUse(int pid, int useIndex, int type, string notes)
    {
        lock (TestPlugin.LPrjs)
        {
            TestPlugin.LPrjs[pid] = new LPrj(pid, useIndex, type, notes);
        }
    } 
    #endregion
}

