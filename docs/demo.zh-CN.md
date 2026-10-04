# 联机玩法展示

[项目首页](../README.zh-CN.md) · [English](demo.md) · [整体架构](architecture.zh-CN.md)

Desert Storm 在一场 3v3 对局中结合第一人称交战、兵种协作与 MOBA 目标推进。

## 小队交战

![防御结构附近的瞄准、射击与换弹](../media/playtest/multiplayer-combat.gif)

瞄准与换弹发生在包含其他玩家、防御结构和区域效果的共享战场中。权威命中判定负责伤害结果，本地反馈与序号协调将结果连接到第一人称武器表现。

[战斗仿真与预测](cases/networked-combat.md) · [武器表现](cases/presentation-lifecycle.md)

## 房间与选角

![创建房间与切换队伍](../media/playtest/room-flow.gif)

房间准入将参与者绑定到持久席位。切换阵营更新共享名单，房间界面消费对应状态。

![兵种预览与选择锁定](../media/playtest/class-selection.gif)

职业身份确定角色与技能配置，锁定选择将房间状态连接到战场角色生成。

[会话与房间归属](cases/dedicated-server.md) · [内容配置](cases/content-architecture.md)

## 兵种能力

### Medic：Serum 区域支援

![Serum 投掷与持续区域效果下的交战](../media/playtest/serum-support.gif)

Serum 将阶段化投掷与治疗友方、伤害敌方的区域规则结合。释放后武器回到常规表现，区域效果沿自己的玩法生命周期继续运行。

[能力规则](gameplay.md#当前兵种) · [动作与挂载生命周期](cases/presentation-lifecycle.md)

### Heavy：护盾

![护盾启动与近距离交战](../media/playtest/shield-combat.gif)

护盾持续时间与减伤由权威能力状态管理，屏幕效果在移动和武器使用过程中呈现技能状态。

[兵种玩法](gameplay.md#当前兵种) · [状态所有权](architecture.zh-CN.md#状态所有权)

## 局内换兵种

![选择 Medic、确认更换与继续对局](../media/playtest/base-class-change.gif)

存活玩家在所属基地更换战术角色。服务端验证请求并暂存替换对象，保留参与者的血量比例、常规装备、适用效果与统计。

[请求验证与状态转移](cases/class-change.md)

## 对局结算

![Victory 与双方对局比分表](../media/playtest/match-settlement.gif)

比赛结果将战场推进连接到胜利画面与小队统计。玩家、小兵和结构共享战斗目标契约，目标前置关系定义通向敌方核心的推进路线。

[AI 与目标推进](cases/moba-battlefield.md) · [轮次生命周期](cases/dedicated-server.md)

## Desert Storm 战场

![Desert Storm 战场概览](../media/desert-storm-overview.jpg)

通路、掩体与防御结构为玩家交战和 AI 导航提供共同的空间上下文。

[战场系统](cases/moba-battlefield.md)
