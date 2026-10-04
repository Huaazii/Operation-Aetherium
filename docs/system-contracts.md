# System contracts / 系统职责与契约

[Architecture](architecture.md) · [整体架构](architecture.zh-CN.md) · [Knowledge map](knowledge-map.md)

Each module has a state owner, inputs, outputs and lifetime rules. The tables connect those boundaries to the implemented game.

## 输入、仿真与反馈

| 模块 | 职责与关联 | 对外契约 | 设计约束 |
|---|---|---|---|
| FusionInputCollector | 采集本地输入，供 Motor、Weapon 与 Ability 使用 | [AetheriumInputData](../src/Core/Models/AetheriumInputData.cs)：视角、持续按键、累计边沿和槽位 | 本地操作意图与权威结果分别表达 |
| FusionPlayerMotor | Tick 运动与瞄准，连接输入、武器射线与本地相机 | 位姿、玩法视角、眼部原点和预测校正 | 表现抖动独立于权威命中方向 |
| FusionWeaponController | 弹药、换弹、射击、投掷及后坐力 | [射击 / 换弹事件](../src/Battlefield/Player/WeaponPresentationEvent.cs)、伤害请求和计时状态 | 先计算射线再施加本次后坐力；确认动作保留其物品身份 |
| FusionHealth / DamageResolver | 目标规则、伤害、治疗、死亡与贡献统计 | [DamageRequest](../src/Core/Models/DamageRequest.cs) 与 [IFusionCombatTarget](../src/Core/Interfaces/IFusionCombatTarget.cs) | 实际结算由目标权威路径处理 |
| FusionWeaponPresentation / FPV bridge | 将快照、预测和确认事件映射到 FPV、TPV、声音与特效 | 本地反馈游标、待确认数量、挂载就绪状态 | 已消费序号不重复反馈，旧挂载等待工作按生命周期清理 |

[战斗](cases/networked-combat.md) · [表现](cases/presentation-lifecycle.md)

## 会话、房间与角色

| 模块 | 职责与关联 | 对外契约 | 设计约束 |
|---|---|---|---|
| FusionSessionCoordinator | 大厅、Runner、连接尝试、取消与恢复排序 | [ConnectionRequest / Progress](../src/Core/Interfaces/ConnectionRequest.cs)，会话事件和席位握手接口 | 预算随用户操作，回调随 Runner 代次，终态提交一次 |
| AuthoritativeRoomFlow | 无 UI 的准入、席位释放、结算返回及分配发布 | 准入结果、分配状态、轮次清理和网络场景请求 | 异步场景加载前后核对 Runner / MatchState / RoundId |
| BattlefieldMatchState | 席位、名单、比赛阶段、统计、职业替换版本 | 复制席位与修订、定向准入 / 替换结果 | 客户端请求来源由 RPC 上下文解析 |
| BattlefieldRuntimeBootstrap | 将场景、内容与玩家连接；生成、替换及重新绑定 | 网络角色、玩家索引、输入 / 相机 / HUD 上下文 | 新角色转移成功后提交，旧角色解绑不能移除新绑定 |
| Base class-change flow | 在所属基地验证并替换当前角色 | [权威替换请求](../src/Battlefield/Match/BattlefieldMatchState.ClassChange.cs)、[暂存与转移](../src/Battlefield/Runtime/BattlefieldRuntimeBootstrap.ClassChange.cs)、[几何区域](../src/Battlefield/Runtime/BattlefieldClassChangeZone.cs) | 血量比例、技能冷却、弹药和统计继承各自状态；轮次及版本排除陈旧请求 |

[房间与专服](cases/dedicated-server.md) · [连接](cases/session-lifecycle.md) · [角色替换](cases/class-change.md)

## 内容、战场与资源

| 模块 | 职责与关联 | 对外契约 | 设计约束 |
|---|---|---|---|
| GameplayCatalog / Definitions | ID、参数与 Prefab 映射，供生成、仿真和表现读取 | [Catalog](../src/Core/Catalog/GameplayCatalog.cs) 与类型化定义 | 阵营、职业和席位具有各自含义；新行为通过相应玩法接入 |
| Minion / Structure / ObjectiveRegistry | 波次、路径执行、目标选择和结构前置关系 | 战斗目标接口、复制运动和目标状态 | AI 在权威侧推进，导航与动画分别处理运动和表现 |
| BulletImpactPool / TransientVisualPool | 场景内短期视觉对象的分配、复用和清理 | 有容量及生命周期的播放请求 | 预热可取消，禁用 / 销毁释放场景拥有的状态与资源 |
| GameplayHUDController / BattlefieldHudContext | 玩家、队友、技能和比赛状态展示 | 事件、修订与缓存后的显示数据 | 玩法事实来自绑定上下文，显示缓存随对象更换重置 |
| DedicatedServerBuildTools | Windows 客户端与专服构建及发布工具装配 | Player / Server 子目标，共享启用场景及兼容版本 | 构建角色明确，完成后恢复编辑器设置 |

[内容](cases/content-architecture.md) · [MOBA](cases/moba-battlefield.md) · [资源](cases/runtime-resources.md)
