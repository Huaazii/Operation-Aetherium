# Runtime resources: bounded work and scene ownership

[Architecture](../architecture.md) · [Knowledge map](../knowledge-map.md)

## Requirement

Combat creates bursts of impacts, explosions and audio. Scene changes end their useful lifetime. Area effects can encounter many colliders, while HUD and shot history often remain unchanged between frames. The implementation needs explicit capacity, repeated-work and cleanup policies.

## Visual pools

| Pool | Implemented capacity policy | Lifecycle |
|---|---|---|
| BulletImpactPool | Per-prefab buckets; 64 entries for short-lived effects, 512 for longer-lived effects; recycle oldest active entry at capacity | Lifetime expiry, disable and scene destruction |
| TransientVisualPool | Serialized total cap, code default 128; free / active entries per prefab; recycle same-prefab oldest or evict another prefab within the cap | Particle reset, expiry, disable and destruction |

Bullet impact entries cache particle systems and fades. Pool-owned clones configure their self-destruction behaviour for reuse. Transient entries cache particles and reset playback on rental. The pools own their instances and scene storage.

Each pool's prewarm begins after a frame boundary and creates at most two instances per frame. Scene readiness and visual prewarm have separate completion conditions. Disable cancels the routine and clears queued requests. Destroy releases the owned instances and cached resource references. Desert Storm serializes both pool components.

These capacities bound retained pool objects and make scene-owned storage predictable.

## Query completeness

FusionSerumArea uses a reusable Collider buffer and sets that deduplicate player and minion targets. When OverlapSphereNonAlloc fills the buffer, the buffer grows up to a reusable capacity of 1024. At that cap, a complete allocating OverlapSphere query is used.

Ordinary ticks reuse storage; saturated queries retain complete target processing through a fallback query.

## Change-driven consumption

Shot presentation first compares the replicated sequence to its cursor. An unchanged sequence skips the ring scan. New events are collected in a reusable array, consumed in sequence order and placed in a bounded pending queue.

BattlefieldHudContext and GameplayHUDController bind the current player / match context and use events, revisions and cached display data. Serialized TPV bindings build item lookup caches during setup. These mechanisms reduce repeated work for stable state and make object replacement a cache-lifetime boundary.

## 工程要点

优化从资源和工作来源入手：短期对象有上限，预热分帧且可取消，稳定状态按序号或修订跳过重复处理，查询复用同时保留饱和时的完整性。场景禁用和销毁明确结束池、等待任务及缓存的生命周期。
