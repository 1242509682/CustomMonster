## CustomMonster 自定义怪物血量
```
作者：GK  
修改：羽学  
源码地址：Q群232109072  

这是一个Tshock服务器插件主要用于：  
修改Tshock服务器的怪物， 
可自定义于怪物与BOSS的修改AI、修改血量、  
修改弹幕、修改掉落物、修改范围BUFF、  
修改随从怪物、拉取玩家、  
修改防御、修改无敌、修改免疫岩浆、  
修改免疫陷阱、控制指定怪物数量、NPC保护等等  
```
  
[配置编写教学视频](https://www.bilibili.com/video/BV1Te411U7Ld/?share_source=copy_web&vd_source=93a056214987e80811b94cfc3c9328a6)   
  
## 插件版本
```
2024年6月6日更新日志：
修复无配置版报错提示
并给羽学内嵌版与无配置版加入：/reload 重载报错只报行数  

2024年6月5日更新日志：
加了个【自定义强制隐藏哪些配置项】：
当【是否隐藏没用到的配置项】开启时触发
不写自定义隐藏，只会忽略空值和默认值，
写了哪怕有改动值也会被强制隐藏，并把该【指定的配置项】刷回成默认值（有毁配置风险，不建议碰它）
比如【怪物血量】你改20000，然后在强制隐藏里加了【怪物血量】这个关键词，/reload后就会变成0

羽学内嵌版调整：
修改了“店员”的配置文件，
移除了四子魔眼，加入了史莱姆强化、尖刺史莱姆、暗影焰鬼、雨云怪魔改
调整了所有BOSS单体血量为1.5-2倍
修复由原插件预设配置默认将击退、拉取范围等未改配置自动填值的问题
修复了【人秒系数-10】/【出没秒数600】导致无法生成BOSS/过时消失的问题

2024年6月4日更新日志：
加了【是否隐藏没用到的配置项】开关
加了个指令：/改怪物 用于控制【是否隐藏没用到的配置项】：
此开关可隐藏没用到的空值、默认值的配置项
加入了“店员”的配置文件
相关NPC魔改：光女、双足翼龙、月后四子魔眼、冰雪巨人、
沙尘精、装甲幻影魔、恶魔眼、诅咒锤、猩红斧、附魔剑、骨蛇、
神圣宝箱怪、冰雪女王、飞眼怪、哥布林巫师、哥布林召唤师

更新日志  
1.0.4.9  
加入了死亡执行命令、拉取范围等配置项
此版本源码为无内嵌配置文件版，适用于直接编写。

```
## 命令
```
/Reload -- 重读配置文件
/改怪物 或者 /ggw -- 隐藏没有用到的配置项并重读
```
## 权限
```
重读自定义怪物血量
```
## 权限示例
```
无
```

## 配置示例
```json
{
  "是否隐藏没用到配置项的指令/ggw": false,
  "自定义强制隐藏哪些配置项的指令/Reload": [
    "覆盖原血量",
    "不低于正常",
    "不小于正常",
    "覆盖原强化",
    "出没率子",
    "出没率母",
    "智慧机制",
    "出场放音",
    "死亡放音",
    "播放声音"
  ],
  "启动错误报告": false,
  "启动死亡队友视角": false,
  "队友视角仅BOSS时": true,
  "队友视角流畅度": -1,
  "队友视角等待范围": 18,
  "统一对怪物伤害修正": 1.0,
  "统一怪物弹幕伤害修正": 1.0,
  "统一初始怪物玩家系数": 0,
  "统一初始玩家系数不低于人数": true,
  "统一初始怪物强化系数": 0.0,
  "统一怪物血量倍数": 1.0,
  "统一血量不低于正常": true,
  "统一怪物强化倍数": 1.0,
  "统一强化不低于正常": true,
  "统一免疫熔岩类型": 1,
  "统一免疫陷阱类型": 0,
  "统一设置例外怪物": [
    1,
    2,
    3
  ],
  "启动动态血量上限": true,
  "启动怪物时间限制": true,
  "启动动态时间限制": true,
  "怪物节集": [
    {
      "标志": "1",
      "怪物ID": 0,
      "再匹配": [
        1
      ],
      "初始属性玩家系数": 0,
      "初始属性强化系数": 0.0,
      "初始属性对怪物伤害修正": 1.0,
      "怪物血量": 0,
      "玩家系数": 0,
      "开服系数": 0,
      "杀数系数": 0,
      "开服时间型": 0,
      "覆盖原血量": true,
      "不低于正常": true,
      "强化系数": 0.0,
      "不小于正常": true,
      "玩家强化系数": 0.0,
      "开服强化系数": 0.0,
      "杀数强化系数": 0.0,
      "覆盖原强化": true,
      "玩家复活时间": -1,
      "阻止传送器": 0,
      "出没秒数": 0,
      "人秒系数": 0,
      "开服系秒": 0,
      "杀数系秒": 0,
      "出没率子": 1,
      "出没率母": 1,
      "杀数条件": 0,
      "数量条件": 0,
      "人数条件": 0,
      "昼夜条件": 0,
      "肉山条件": 0,
      "巨人条件": 0,
      "血月条件": 0,
      "月总条件": 0,
      "开服条件": 0,
      "怪物条件": [
        {
          "怪物ID": 0,
          "查标志": "1",
          "指示物": [
            {
              "名称": "1",
              "数量": 0
            }
          ],
          "范围内": 0,
          "血量比": 0,
          "符合数": 0
        }
      ],
      "智慧机制": -1,
      "免疫熔岩": 0,
      "免疫陷阱": 0,
      "能够穿墙": 0,
      "无视重力": 0,
      "设为老怪": 0,
      "修改防御": false,
      "怪物防御": 0,
      "怪物无敌": 0,
      "自定缀称": "",
      "出场喊话": "",
      "死亡喊话": "",
      "不宣读信息": false,
      "状态范围": 100,
      "周围状态": [
        {
          "状态ID": 0,
          "状态起始范围": 0,
          "状态结束范围": 0,
          "头顶提示": "1"
        }
      ],
      "死状范围": 100,
      "死亡状态": {
        "1": 1
      },
      "出场弹幕": [
        {
          "弹幕ID": 0,
          "X轴偏移": 0.0,
          "Y轴偏移": 0.0,
          "X轴速度": 0.0,
          "Y轴速度": 0.0,
          "弹幕伤害": 0,
          "弹幕击退": 0,
          "弹幕Ai0": 0.0,
          "速度注入AI0": false,
          "弹幕Ai1": 0.0,
          "弹幕Ai2": 0.0,
          "角度偏移": 0.0,
          "指示物数量注入X轴偏移名": "",
          "指示物数量注入X轴偏移系数": 0.0,
          "指示物数量注入Y轴偏移名": "",
          "指示物数量注入Y轴偏移系数": 0.0,
          "指示物数量注入X轴速度名": "",
          "指示物数量注入X轴速度系数": 0.0,
          "指示物数量注入Y轴速度名": "",
          "指示物数量注入Y轴速度系数": 0.0,
          "指示物数量注入角度名": "",
          "指示物数量注入角度系数": 0.0,
          "怪面向X偏移修正": 0.0,
          "怪面向Y偏移修正": 0.0,
          "怪面向X速度修正": 0.0,
          "怪面向Y速度修正": 0.0,
          "不射原始": false,
          "差度位始角": 0,
          "差度位射数": 0,
          "差度位射角": 0,
          "差度位半径": 0,
          "不射差度位": false,
          "差度射数": 0,
          "差度射角": 0.0,
          "差位射数": 0,
          "差位偏移X": 0.0,
          "差位偏移Y": 0.0,
          "锁定范围": 0,
          "锁定速度": 0.0,
          "以弹为位": false,
          "持续时间": -1,
          "计入仇恨": false,
          "锁定血少": false,
          "锁定低防": false,
          "仅攻击对象": false,
          "逆仇恨锁定": false,
          "逆血量锁定": false,
          "逆防御锁定": false,
          "仅扇形锁定": false,
          "扇形半偏角": 60,
          "以锁定为点": false,
          "最大锁定数": 1
        }
      ],
      "死亡弹幕": [
        {
          "弹幕ID": 0,
          "X轴偏移": 0.0,
          "Y轴偏移": 0.0,
          "X轴速度": 0.0,
          "Y轴速度": 0.0,
          "弹幕伤害": 0,
          "弹幕击退": 0,
          "弹幕Ai0": 0.0,
          "速度注入AI0": false,
          "弹幕Ai1": 0.0,
          "弹幕Ai2": 0.0,
          "角度偏移": 0.0,
          "指示物数量注入X轴偏移名": "",
          "指示物数量注入X轴偏移系数": 0.0,
          "指示物数量注入Y轴偏移名": "",
          "指示物数量注入Y轴偏移系数": 0.0,
          "指示物数量注入X轴速度名": "",
          "指示物数量注入X轴速度系数": 0.0,
          "指示物数量注入Y轴速度名": "",
          "指示物数量注入Y轴速度系数": 0.0,
          "指示物数量注入角度名": "",
          "指示物数量注入角度系数": 0.0,
          "怪面向X偏移修正": 0.0,
          "怪面向Y偏移修正": 0.0,
          "怪面向X速度修正": 0.0,
          "怪面向Y速度修正": 0.0,
          "不射原始": false,
          "差度位始角": 0,
          "差度位射数": 0,
          "差度位射角": 0,
          "差度位半径": 0,
          "不射差度位": false,
          "差度射数": 0,
          "差度射角": 0.0,
          "差位射数": 0,
          "差位偏移X": 0.0,
          "差位偏移Y": 0.0,
          "锁定范围": 0,
          "锁定速度": 0.0,
          "以弹为位": false,
          "持续时间": -1,
          "计入仇恨": false,
          "锁定血少": false,
          "锁定低防": false,
          "仅攻击对象": false,
          "逆仇恨锁定": false,
          "逆血量锁定": false,
          "逆防御锁定": false,
          "仅扇形锁定": false,
          "扇形半偏角": 60,
          "以锁定为点": false,
          "最大锁定数": 1
        }
      ],
      "出场放音": [
        {
          "声音ID": -1,
          "声音规模": -1.0,
          "高音补偿": -1.0
        }
      ],
      "死亡放音": [
        {
          "声音ID": -1,
          "声音规模": -1.0,
          "高音补偿": -1.0
        }
      ],
      "出场伤怪": [
        {
          "怪物ID": 0,
          "范围内": 0,
          "造成伤害": 0,
          "直接伤害": false,
          "直接清除": false
        }
      ],
      "死亡伤怪": [
        {
          "怪物ID": 0,
          "范围内": 0,
          "造成伤害": 0,
          "直接伤害": false,
          "直接清除": false
        }
      ],
      "出场命令": [
        "/who"
      ],
      "死亡命令": [
        "/who"
      ],
      "血事件限": 1,
      "血量事件": [
        {
          "血量剩余比例": 0,
          "可触发次": 0,
          "触发率子": 1,
          "触发率母": 1,
          "杀数条件": 0,
          "人数条件": 0,
          "杀死条件": 0,
          "昼夜条件": 0,
          "耗时条件": 0,
          "ID条件": 0,
          "肉山条件": 0,
          "巨人条件": 0,
          "血月条件": 0,
          "月总条件": 0,
          "开服条件": 0,
          "X轴条件": 0,
          "Y轴条件": 0,
          "面向条件": 0,
          "跳出事件": false,
          "怪物条件": [
            {
              "怪物ID": 0,
              "查标志": "1",
              "指示物": [
                {
                  "名称": "1",
                  "数量": 0
                }
              ],
              "范围内": 0,
              "血量比": 0,
              "符合数": 0
            }
          ],
          "玩家条件": [
            {
              "范围起": 0,
              "范围内": 0,
              "符合数": 0
            }
          ],
          "直接撤退": false,
          "玩家复活时间": -2,
          "切换智慧": -1,
          "能够穿墙": 0,
          "无视重力": 0,
          "修改防御": false,
          "怪物防御": 0,
          "恢复血量": 0,
          "比例回血": 0,
          "怪物无敌": 0,
          "拉取起始": 0,
          "拉取范围": 0,
          "拉取止点": 0,
          "拉取点X轴偏移": 0.0,
          "拉取点Y轴偏移": 0.0,
          "杀伤范围": 0,
          "杀伤伤害": 0,
          "击退范围": 30,
          "击退力度": 5,
          "释放弹幕": [
            {
              "弹幕ID": 0,
              "X轴偏移": 0.0,
              "Y轴偏移": 0.0,
              "X轴速度": 0.0,
              "Y轴速度": 0.0,
              "弹幕伤害": 0,
              "弹幕击退": 0,
              "弹幕Ai0": 0.0,
              "速度注入AI0": false,
              "弹幕Ai1": 0.0,
              "弹幕Ai2": 0.0,
              "角度偏移": 0.0,
              "指示物数量注入X轴偏移名": "",
              "指示物数量注入X轴偏移系数": 0.0,
              "指示物数量注入Y轴偏移名": "",
              "指示物数量注入Y轴偏移系数": 0.0,
              "指示物数量注入X轴速度名": "",
              "指示物数量注入X轴速度系数": 0.0,
              "指示物数量注入Y轴速度名": "",
              "指示物数量注入Y轴速度系数": 0.0,
              "指示物数量注入角度名": "",
              "指示物数量注入角度系数": 0.0,
              "怪面向X偏移修正": 0.0,
              "怪面向Y偏移修正": 0.0,
              "怪面向X速度修正": 0.0,
              "怪面向Y速度修正": 0.0,
              "不射原始": false,
              "差度位始角": 0,
              "差度位射数": 0,
              "差度位射角": 0,
              "差度位半径": 0,
              "不射差度位": false,
              "差度射数": 0,
              "差度射角": 0.0,
              "差位射数": 0,
              "差位偏移X": 0.0,
              "差位偏移Y": 0.0,
              "锁定范围": 0,
              "锁定速度": 0.0,
              "以弹为位": false,
              "持续时间": -1,
              "计入仇恨": false,
              "锁定血少": false,
              "锁定低防": false,
              "仅攻击对象": false,
              "逆仇恨锁定": false,
              "逆血量锁定": false,
              "逆防御锁定": false,
              "仅扇形锁定": false,
              "扇形半偏角": 60,
              "以锁定为点": false,
              "最大锁定数": 1
            }
          ],
          "状态范围": 0,
          "周围状态": [
            {
              "状态ID": 0,
              "状态起始范围": 0,
              "状态结束范围": 0,
              "头顶提示": "1"
            }
          ],
          "杀伤怪物": [
            {
              "怪物ID": 0,
              "范围内": 0,
              "造成伤害": 0,
              "直接伤害": false,
              "直接清除": false
            }
          ],
          "召唤怪物": {
            "1": 1
          },
          "喊话": "1"
        }
      ],
      "时事件限": 3,
      "时间事件": [
        {
          "消耗时间": 0.0,
          "循环执行": false,
          "延迟秒数": 0.0,
          "可触发次": 0,
          "触发率子": 1,
          "触发率母": 1,
          "杀数条件": 0,
          "人数条件": 0,
          "昼夜条件": 0,
          "血比条件": 0,
          "杀死条件": 0,
          "怪数条件": 0,
          "耗时条件": 0,
          "ID条件": 0,
          "AI条件": {
            "1": 1.0
          },
          "肉山条件": 0,
          "巨人条件": 0,
          "血月条件": 0,
          "月总条件": 0,
          "开服条件": 0,
          "X轴条件": 0,
          "Y轴条件": 0,
          "面向条件": 0,
          "跳出事件": false,
          "怪物条件": [
            {
              "怪物ID": 0,
              "查标志": "",
              "指示物": [
                {
                  "名称": "1",
                  "数量": 0
                }
              ],
              "范围内": 0,
              "血量比": 0,
              "符合数": 0
            }
          ],
          "玩家条件": [
            {
              "范围起": 0,
              "范围内": 0,
              "符合数": 0
            }
          ],
          "指示物条件": [
            {
              "名称": "1",
              "数量": 0
            }
          ],
          "指示物修改": [
            {
              "清除": false,
              "名称": "1",
              "数量": 0
            }
          ],
          "直接撤退": false,
          "玩家复活时间": -2,
          "阻止传送器": 0,
          "切换智慧": -1,
          "能够穿墙": 0,
          "无视重力": 0,
          "修改防御": false,
          "怪物防御": 0,
          "AI赋值": {
            "1": 1.0
          },
          "恢复血量": 0,
          "比例回血": 0,
          "怪物无敌": 0,
          "拉取起始": 50,
          "拉取范围": 100,
          "拉取止点": 0,
          "拉取点X轴偏移": 0.0,
          "拉取点Y轴偏移": 0.0,
          "击退范围": 30,
          "击退力度": 5,
          "杀伤范围": 0,
          "杀伤伤害": 0,
          "反射范围": 0,
          "释放弹幕": [
            {
              "弹幕ID": 0,
              "X轴偏移": 0.0,
              "Y轴偏移": 0.0,
              "X轴速度": 0.0,
              "Y轴速度": 0.0,
              "弹幕伤害": 0,
              "弹幕击退": 0,
              "弹幕Ai0": 0.0,
              "速度注入AI0": false,
              "弹幕Ai1": 0.0,
              "弹幕Ai2": 0.0,
              "角度偏移": 0.0,
              "指示物数量注入X轴偏移名": "",
              "指示物数量注入X轴偏移系数": 0.0,
              "指示物数量注入Y轴偏移名": "",
              "指示物数量注入Y轴偏移系数": 0.0,
              "指示物数量注入X轴速度名": "",
              "指示物数量注入X轴速度系数": 0.0,
              "指示物数量注入Y轴速度名": "",
              "指示物数量注入Y轴速度系数": 0.0,
              "指示物数量注入角度名": "",
              "指示物数量注入角度系数": 0.0,
              "怪面向X偏移修正": 0.0,
              "怪面向Y偏移修正": 0.0,
              "怪面向X速度修正": 0.0,
              "怪面向Y速度修正": 0.0,
              "不射原始": false,
              "差度位始角": 0,
              "差度位射数": 0,
              "差度位射角": 0,
              "差度位半径": 0,
              "不射差度位": false,
              "差度射数": 0,
              "差度射角": 0.0,
              "差位射数": 0,
              "差位偏移X": 0.0,
              "差位偏移Y": 0.0,
              "锁定范围": 0,
              "锁定速度": 0.0,
              "以弹为位": false,
              "持续时间": -1,
              "计入仇恨": false,
              "锁定血少": false,
              "锁定低防": false,
              "仅攻击对象": false,
              "逆仇恨锁定": false,
              "逆血量锁定": false,
              "逆防御锁定": false,
              "仅扇形锁定": false,
              "扇形半偏角": 60,
              "以锁定为点": false,
              "最大锁定数": 1
            }
          ],
          "状态范围": 0,
          "周围状态": [
            {
              "状态ID": 0,
              "状态起始范围": 0,
              "状态结束范围": 0,
              "头顶提示": "1"
            }
          ],
          "杀伤怪物": [
            {
              "怪物ID": 0,
              "范围内": 0,
              "造成伤害": 0,
              "直接伤害": false,
              "直接清除": false
            }
          ],
          "召唤怪物": {
            "1": 1
          },
          "播放声音": [
            {
              "声音ID": -1,
              "声音规模": -1.0,
              "高音补偿": -1.0
            }
          ],
          "释放命令": [
            "/reload"
          ],
          "喊话": "1"
        }
      ],
      "随从怪物": {
        "1": 1
      },
      "遗言怪物": {
        "1": 1
      },
      "掉落组限": 1,
      "额外掉落": [
        {
          "掉落率子": 1,
          "掉落率母": 1,
          "杀数条件": 0,
          "人数条件": 0,
          "杀死条件": 0,
          "昼夜条件": 0,
          "耗时条件": 0,
          "肉山条件": 0,
          "巨人条件": 0,
          "血月条件": 0,
          "月总条件": 0,
          "开服条件": 0,
          "跳出掉落": false,
          "怪物条件": [
            {
              "怪物ID": 0,
              "查标志": "1",
              "指示物": [
                {
                  "名称": "1",
                  "数量": 0
                }
              ],
              "范围内": 0,
              "血量比": 0,
              "符合数": 0
            }
          ],
          "掉落物品": [
            {
              "物品ID": 0,
              "物品数量": 0,
              "物品前缀": -1,
              "独立掉落": false
            }
          ],
          "备注": "1"
        }
      ],
      "备注": "1"
    }
  ]
}
```
