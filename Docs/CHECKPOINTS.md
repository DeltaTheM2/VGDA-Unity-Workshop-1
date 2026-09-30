# Checkpoints

Every step of the workshop ends in a **checkpoint**: a complete, working copy of the project at that moment. Each one is a **git tag** and also a **zip** on the Releases page. If you fall behind or break something, jump to a checkpoint and keep going.

| Checkpoint (tag) | What's in it | Jump here if… |
|---|---|---|
| `step-0-starter` | The arena (start and goal platforms, lighting, follow camera, invisible fall zone), the Win scene, art, materials, `_Backup` parts, and `HelloWorld.cs`. No player, no gameplay scripts, no prefabs. | you're starting the workshop |
| `step-1-lifecycle` | + `LifecycleLogger.cs` on a `LifecycleDemo` cube that logs Awake / OnEnable / Start / Update / FixedUpdate / OnDisable. | you want to start Step 2 |
| `step-2-player` | + the blue capsule **Player**: Rigidbody (rotation frozen on X/Z), `NoFriction` physics material, `PlayerController.cs` (run + jump + ground check). The camera follows the player. | you want to start Step 3 |
| `step-3-prefabs` | + `Block`, `Ramp` and `Trap` prefabs (colliders, Ground layer), placed in front of the start. The trap is only a placeholder. | you want to start Remix #1 or Step 5 |
| `step-5-hazards` | + `KillOnTouch.cs` on the Trap and the FallZone, `Patrol.cs` + `StickyPlatform.cs` on a `MovingPlatform` prefab. | you want to start Step 6 |
| `step-6-goal` | + `Coin.cs` (Coin prefab), `GameManager.cs` (score text, win when all coins are collected), `GoalTrigger.cs` on the goal platform. | you want to start Remix #2 |
| `final` | + the full demo course and the bonus scripts (`Scripts/Bonus/`: DoubleJump, BouncePad, Spinner, Checkpoint + PlayerRespawn + RespawnOnTouch). | you want to see the finished game |

There's no checkpoint for Step 4 (Remix #1). It's free building with no new code.

---

## Option A: download the zip (no git needed)

1. On this repository's **Releases** page, download `unity-workshop-<checkpoint>.zip`, for example `unity-workshop-step-2-player.zip`.
2. Unzip it into a **new** folder. Don't unzip on top of your own project.
3. Unity Hub > **Projects > Add > Add project from disk** and select the **`UnityProject`** folder inside.
4. Open it. The first open imports everything again, which takes a few minutes.

Tip: download the checkpoint you might need **before** the session, while the Wi-Fi is quiet.

## Option B: git checkout

Close Unity first (or at least save everything). Then:

```bash
git fetch --tags
```

```bash
git checkout step-2-player
```

Now reopen (or switch back to) the project in Unity. You're in "detached HEAD" state, which is fine for looking around. To keep building from there on your own branch:

```bash
git switch -c my-course step-2-player
```

If git refuses because you changed files, commit your work first (`git add -A` then `git commit -m "my work"`), or throw the changes away with `git stash`.

Checkouts from git also contain an `Assets/_Instructor` folder and a **Workshop** menu. Those are the instructor's build tools, so you can ignore them. The zips leave them out.

---

## What's where in the project

```
UnityProject/Assets/_Workshop/
  Scenes/            Arena (the level) and Win (the "You Win!" screen)
  Scripts/           everything you write during the workshop
  Materials/         PlayerBlue, CoinYellow, HazardRed, and Remix colors to paint parts with
  PhysicsMaterials/  NoFriction (made in Step 2)
  Prefabs/           the parts you build (Step 3 onward)
  _Backup/           ready-made parts, in case you fall behind or want more pieces
  Art/KayKit/        raw 3D models (no colliders, on purpose)
  Art/Kenney/        traps, coin, saw, and a character for part 2
  Prebuilt/          things that aren't part of the lesson (Play Again button, sky, first-open setup)
```
