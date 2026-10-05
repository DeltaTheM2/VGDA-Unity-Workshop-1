# Checkpoints

Fell behind? Broke something? Every step of the workshop has a finished copy you can jump to.

## During the workshop: open a checkpoint scene

Your project already contains a complete, working scene for every step. Nothing to download, nothing to re-import.

1. In the **Project** window, open **`Assets > _Workshop > Checkpoints`**.
2. Double-click the scene for the **step we just finished**. If Unity asks about saving your current scene, choose **Save**: your own work stays in `Arena`.
3. Keep building in that scene. Press Play, follow along, it all works the same.

| Checkpoint scene | What's in it | Open it to start… |
|---|---|---|
| `1 - Unity Basics` | The blue **Player** capsule built from components (Capsule Collider + Rigidbody), and the camera following it. | Step 2 |
| `2 - Parts` | + `Block`, `Ramp` and `Trap` prefabs (colliders, Ground layer) placed in front of the start. The trap is only a placeholder. | Step 3 |
| `3 - Scripting` | + a `ScriptDemo` cube with your first two scripts: `Spinner` and `LifecycleLogger`. | Step 4 |
| `4 - Player` | + `PlayerController` on the Player (run, jump, ground check), rotation frozen on X/Z, `NoFriction` physics material. | Remix #1 or Step 6 |
| `6 - Hazards` | + `KillOnTouch` on the Trap and the FallZone, and a `MovingPlatform` with `Patrol` + `StickyPlatform`. | Step 7 |
| `7 - Coins and Win` | + coins (`Coin`), `GameManager` with the score text, and `GoalTrigger` on the goal platform. | Remix #2 |
| `Final - Demo Course` | The full demo course: ramp jump, bounce pad, moving platform, trap corridor, coins, goal. | Playing for fun |

There's no checkpoint for Step 5 (Remix #1) or Step 8 (Remix #2). They're free building with no new code.

The checkpoint scenes use their **own copies** of every script and prefab (in `Checkpoints/_Scripts` and `Checkpoints/_Parts`). That way they keep working no matter what you've typed, and they never clash with your own scripts. The copies are hidden from Add Component, so when you add a script you always get your own.

---

## After the workshop: the full project at each step

Each step also has a **git tag** and a **zip** on the Releases page with the whole project as it looks at the end of that step: your scripts in `Scripts/`, your prefabs in `Prefabs/`, and the `Arena` scene built up. Use these if your whole project is broken, or to compare your work with the reference.

| Tag / zip | End of |
|---|---|
| `step-0-starter` | Before Step 1: what you open the night before. |
| `step-1-basics` | Step 1: Unity basics |
| `step-2-parts` | Step 2: build your parts |
| `step-3-scripting` | Step 3: scripting basics |
| `step-4-player` | Step 4: the player |
| `step-6-hazards` | Step 6: moving platforms, traps, respawn |
| `step-7-goal` | Step 7: coins and win |
| `final` | Everything, plus the demo course and the bonus scripts (`Scripts/Bonus/`). |

### Option A: download the zip (no git needed)

1. On this repository's **Releases** page, download `unity-workshop-<tag>.zip`, for example `unity-workshop-step-4-player.zip`.
2. Unzip it into a **new** folder with a short path (Windows: something like `C:\UnityWorkshop\`). Don't unzip on top of your own project.
3. Unity Hub > **Projects > Add > Add project from disk** and select the **`UnityProject`** folder inside.
4. Open it. The first open imports everything again, which takes a few minutes.

### Option B: git checkout

Close Unity first (or at least save everything). Then:

```bash
git fetch --tags
```

```bash
git checkout step-4-player
```

Now reopen (or switch back to) the project in Unity. You're in "detached HEAD" state, which is fine for looking around. To keep building from there on your own branch:

```bash
git switch -c my-course step-4-player
```

If git refuses because you changed files, commit your work first (`git add -A` then `git commit -m "my work"`), or put the changes aside with `git stash`.

Checkouts from git also contain an `Assets/_Instructor` folder and a **Workshop** menu. Those are the instructor's build tools, so you can ignore them. The zips leave them out.

---

## What's where in the project

```
UnityProject/Assets/_Workshop/
  Scenes/            Arena (where you build) and Win (the "You Win!" screen)
  Checkpoints/       one finished scene per step, to catch up during the workshop
  Scripts/           everything you write during the workshop
  Materials/         PlayerBlue, CoinYellow, HazardRed, and Remix colors to paint parts with
  PhysicsMaterials/  NoFriction (made in Step 4)
  Prefabs/           the parts you build (Step 2 onward)
  _Backup/           ready-made parts, in case you fall behind or want more pieces
  Art/KayKit/        raw 3D models (no colliders, on purpose)
  Art/Kenney/        traps, coin, saw, and a character for part 2
  Prebuilt/          things that aren't part of the lesson (Play Again button, sky, first-open setup)
```
