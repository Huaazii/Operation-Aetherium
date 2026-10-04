# Session lifecycle: connection, admission and local readiness

[Architecture](../architecture.md) · [专服与房间](dedicated-server.md) · [技术知识图谱](../knowledge-map.md)

## Requirement

A join crosses service connection, Fusion session entry, server admission, replicated seat binding and scene setup. The player can cancel while an SDK operation is still completing. Progress, outcome and the ownership of that completion need explicit representations.

## Implementation

[ConnectionRequest](../../src/Core/Interfaces/ConnectionRequest.cs) owns the deadline and outcome of one user action. It uses a monotonic clock and shares one total budget across connection, admission and retry work.

| Contract | Current implementation |
|---|---|
| Budget | 60 seconds total; 50-second transport budget and a 10-second admission window, clipped to remaining total time |
| Attempts | At most two |
| Identity | OperationId plus Attempt and RunnerGeneration |
| Outcome | Success, cancellation, supersession, timeout, rejection, connection failure or cleanup failure |
| Completion | TryFinish accepts one terminal outcome; cancelled / expired work cannot commit success |
| SDK waiting | AwaitAsync races cancellation and observes faults from late task completion |

FusionSessionCoordinator owns the lobby and Runner. A healthy compatible lobby Runner can be handed into room entry. Network retry classification is separate from room rejection: a full or closed room has a business result, while a retryable transport failure can use another configured attempt.

Protocol settings are copied per attempt. Photon service protocol and the client-to-game-server route are diagnosed separately. Cleanup has an ownership context and a bounded wait. Runner shutdown completion gates replacement startup.

## Readiness contracts

| Boundary | Meaning |
|---|---|
| Service connected | Photon service communication is available |
| SessionConnected | Fusion session connection is established |
| RoomReady | Authority admission is accepted and the current MatchState / local seat can be bound |
| Local gameplay setup | Player, input, FPV camera and HUD are connected by bootstrap |
| Recovery complete | Seat admission and the recovery-specific local presentation gate both complete |

During recovery, NotifyLocalGameplayReady reports the local presentation boundary. OnSessionRecovered is published after the recovery gates complete. Connection tokens associate the returning participant with a seat; the new transport reference can then bind the current player.

For example, cancelling a join revokes its right to navigate the UI, even if the earlier SDK task later succeeds. Operation identity and Runner generation give that completion a checkable context.

## 工程要点

一次用户动作共用预算和终态，异步任务携带操作身份，网络回调携带实例代次。房间界面依据准入后的席位状态切换，恢复则另外等待本地输入、相机及 HUD 绑定。各阶段由具体条件驱动，可定位等待发生在哪一层。
