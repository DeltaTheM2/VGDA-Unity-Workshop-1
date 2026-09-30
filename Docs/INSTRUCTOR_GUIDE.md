# Instructor Guide: Build an Obstacle Course

Run-of-show for the VGDA Unity Basics workshop. The session is 60 minutes, and everyone builds along on their own laptop while you build on the projector.

- Unity **6000.3.8f1** (Unity 6.3 LTS), Universal 3D (URP), Input System (project-wide actions), Cinemachine 3.1.7
- Attendee setup: [README.md](README.md) · Checkpoint tags: [CHECKPOINTS.md](CHECKPOINTS.md) · Things done by hand: [MANUAL_SETUP.md](MANUAL_SETUP.md)

> **This file is also source code.** Every lesson script below sits under a `<!-- lesson-script: ... -->` marker. `Tools/build-checkpoints.sh` copies those code blocks into the project to build each checkpoint. If you change a script, change it **here**. Then rebuild, or run `Tools/extract-lesson-scripts.sh final --check` to confirm the project still matches.

---

## At a glance

| # | Segment | Time | Clock | Tag at the end | If someone is behind |
|---|---------|------|-------|----------------|----------------------|
| 0 | Welcome + "here's what you'll build" | 4 min | 0:00–0:04 | — | — |
| 1 | Script lifecycle | 7 min | 0:04–0:11 | `step-1-lifecycle` | start from `step-0-starter` |
| 2 | Player movement + jump | 14 min | 0:11–0:25 | `step-2-player` | jump to `step-1-lifecycle` |
| 3 | Build your own parts (prefabs) | 7 min | 0:25–0:32 | `step-3-prefabs` | use `_Backup/`, or jump to `step-2-player` |
| 4 | Remix #1 (no code) | 5 min | 0:32–0:37 | — (no tag) | jump to `step-3-prefabs` |
| 5 | Moving platforms, traps, respawn | 11 min | 0:37–0:48 | `step-5-hazards` | jump to `step-3-prefabs` |
| 6 | Coins + win ✂️ *first to cut* | 7 min | 0:48–0:55 | `step-6-goal` | jump to `step-5-hazards` |
| R2 | Remix #2 + wrap-up ✂️ *second to cut* | 5 min | 0:55–1:00 | `final` is the reference | — |

**Cut lines if you're running late:**

1. **At 0:45**, if Step 5 isn't finished, **cut Step 6.** Finish Step 5 and go straight to Remix #2 (hazards only). Point people at `step-6-goal` to try later.
2. **Then shorten Remix #2** to about 2 minutes: "put one hazard and one moving platform in your course," then wrap up.
3. **Extended session?** Grow Remix #2 and hand out the [bonus challenges](#bonus-challenges-for-fast-finishers).

**Say this at the start of every step:** "If you get lost, the tag at the end of each step is a full working project. See CHECKPOINTS.md. You can always catch up."

---

## Before the session

- [ ] Open `final` and play it start to finish (Play Again should work). This is the demo you show first.
- [ ] Open a fresh copy of `step-0-starter` for the live build. Unzip it or clone it. Don't reuse your rehearsal copy.
- [ ] On the projector machine, turn on **Edit > Preferences > Colors > Playmode tint** and pick something loud (light red). Beginners edit in Play mode constantly, and the tint makes it obvious.
- [ ] Bump the font size in your code editor (VS Code: Ctrl/Cmd + `=` a few times). Use a big Unity layout with the Console docked next to the Game view.
- [ ] Post all the checkpoint zips (GitHub Release) and the Discord thread link where people can see them.
- [ ] Optional: run `Tools/run-tests.sh` on each tag. The instructor PlayMode tests play every checkpoint with a virtual gamepad, including a bot that finishes the demo course.

---

## Tuned values (why the player feels the way it does)

| Setting | Value | Where | Why |
|---|---|---|---|
| Gravity | **(0, -25, 0)** | Project Settings > Physics | Earth's -9.81 feels floaty for a platformer. -25 gives a quick, snappy arc. The starter already has this. |
| Move Speed | **6** m/s | PlayerController | Fast enough to be fun, slow enough to steer onto 2 m blocks. |
| Jump Force | **9** (Impulse, mass 1) | PlayerController | 9 m/s upward → apex **≈ 1.6 m**, ≈ 0.72 s in the air, ≈ **4.3 m** jump on flat ground. |
| Ground Check Distance | **1.1** m | PlayerController | Capsule is 2 m tall (feet 1 m below center) plus 0.1 m slack. |
| Ground Layer | **Ground** (layer 6) | PlayerController + every walkable part | The ground ray only hits this layer. |
| Rigidbody | mass 1, **Interpolate: None**, freeze rotation X+Z | Player | Keep interpolation **off**. With it on, the physics engine ignores StickyPlatform's parenting and you slide off moving platforms. |
| NoFriction | friction 0/0, combine **Minimum** | Player's Capsule Collider | Otherwise you stick to walls when you jump into them. |
| Fixed timestep | 0.02 s (50 Hz, default) | Project Settings > Time | — |

What that means for level design (the demo course follows these rules):

- **1 m step up: jumpable. 2 m wall: not jumpable** (use a ramp).
- **Gaps up to 3 m** are comfortable at the same height. 4 m is possible, 5 m is not.
- KayKit slopes sit on a **1 m base**: `platform_slope_4x6x4` climbs from 1 m to 4 m (26.6°). A ramp and a block placed at the same height join seamlessly. Sink the ramp 1 m to meet a lower floor.
- The start platform's top is at **y = 0** and the goal's top is at **y = 2**, 50 m away. Blocks dropped on the grid (y = 0) stick up 1 m.

---

## 0 · Welcome + demo (4 min, 0:00–0:04)

**Live:**

1. Open the **`final`** project, press **Play**, and run the demo course. Ride the moving platform, hit the bounce pad, weave through the spikes, and touch the goal. **Play Again**.
2. "In the next hour you'll build every piece of this yourself. The player, the prefabs, the traps, the moving platform, the coins."
3. Switch to the fresh **`step-0-starter`** project. Tour it quickly: **Hierarchy** (the Level, the Follow Camera, the FallZone, the disabled ScoreText), **Project** (`_Workshop/Art`, `Materials`, the empty `Scripts` and `Prefabs` folders), **Inspector**, **Scene** vs **Game**, and the **Console**.

**Talking points:**

- A **GameObject** is an empty thing. **Components** give it behavior (Transform, Mesh Renderer, Collider, and our scripts).
- Everything you see was pre-built so we can spend the hour on the fun parts.

---

## 1 · Script lifecycle (7 min, 0:04–0:11)

**Goal:** see *when* Unity runs our code. This is the key to getting Step 2 right.
**Starts from:** `step-0-starter` · **Ends at:** `step-1-lifecycle`

**Live:**

1. **Hierarchy > + > 3D Object > Cube.** Rename it `LifecycleDemo` and set Position **(2, 0.5, 2)** so it sits on the start platform.
2. Project window: open `Assets/_Workshop/Scripts`, then **right-click > Create > Scripting > MonoBehaviour Script**. Name it **`LifecycleLogger`**. The file name must match the class name exactly, capital letters included.
3. Double-click it and replace everything with the code below. Save (Ctrl/Cmd + S) and switch back to Unity so it compiles.
4. Drag `LifecycleLogger` onto `LifecycleDemo`.
5. Open the **Console** (Window > General > Console). Make sure **Clear on Play** is on and **Collapse** is off.
6. **Play.** Read the Console top to bottom: `Awake → OnEnable → Start → FixedUpdate / Update`, each with its frame number.
7. Experiments (the teaching goals):
   - **Toggle the GameObject.** While playing, untick the checkbox next to `LifecycleDemo`'s name → `OnDisable`. Tick it again → `OnEnable`, but **no second `Start`**.
   - **Disable only the component.** Stop. Untick the checkbox on the **LifecycleLogger component**, then Play → only **`Awake`** prints. Tick the component during Play → `OnEnable`, `Start`, then updates. *Awake runs even when the component is disabled. Start waits.*
   - **Different rates.** Stop, tick **Log Every Frame**, Play for about 2 seconds, then **Pause**. Scroll: `Update` appears on *every* frame number. `FixedUpdate` skips frames on a fast laptop and repeats a frame number on a slow one.
8. Stop Play mode.

<!-- lesson-script: step-1-lifecycle Assets/_Workshop/Scripts/LifecycleLogger.cs -->
```csharp
using UnityEngine;

public class LifecycleLogger : MonoBehaviour
{
    [Header("Logging")]
    [Tooltip("Off: Update and FixedUpdate log only once. On: they log every time they run, so you can compare how often each one happens.")]
    [SerializeField] private bool logEveryFrame = false;

    // Keeps Update and FixedUpdate from flooding the Console.
    private bool hasLoggedUpdate = false;
    private bool hasLoggedFixedUpdate = false;

    private void Awake()
    {
        Log("Awake");
    }

    private void OnEnable()
    {
        Log("OnEnable");
    }

    private void Start()
    {
        Log("Start");
    }

    private void Update()
    {
        if (logEveryFrame || !hasLoggedUpdate)
        {
            Log("Update");
            hasLoggedUpdate = true;
        }
    }

    private void FixedUpdate()
    {
        if (logEveryFrame || !hasLoggedFixedUpdate)
        {
            Log("FixedUpdate");
            hasLoggedFixedUpdate = true;
        }
    }

    private void OnDisable()
    {
        Log("OnDisable");
    }

    private void Log(string eventName)
    {
        Debug.Log("[Frame " + Time.frameCount + "] " + gameObject.name + ": " + eventName);
    }
}
```

**Talking points:**

- Unity calls these methods for you. You never call `Start()` yourself.
- `Update` = once per **frame**, which varies by computer. `FixedUpdate` = once per **physics step**, a steady 50 per second.
- Next step's rule of thumb: **read input in `Update`, move physics objects in `FixedUpdate`.**
- `[SerializeField] private` shows a field in the Inspector without letting other scripts change it. `[Header]` and `[Tooltip]` make the Inspector friendlier (hover the field).

---

## 2 · Player movement + jump (14 min, 0:11–0:25)

**Goal:** a capsule you can run and jump with, using physics.
**Starts from:** `step-1-lifecycle` · **Ends at:** `step-2-player`

**Live:**

1. Delete `LifecycleDemo` (select it, then Delete). The script stays in the project.
2. **Hierarchy > + > 3D Object > Capsule.** Rename it **`Player`** and set Position **(0, 1, 0)**, the same spot as `PlayerSpawn`.
3. Drag **`Materials/PlayerBlue`** onto the capsule.
4. Inspector, top: **Tag > Player**.
5. **Add Component > Rigidbody.** Leave everything default for now (Interpolate: None).
6. Point the camera at the player: select **Follow Camera**, then drag `Player` into **Cinemachine Camera > Tracking Target**.
7. In `Scripts`, **Create > Scripting > MonoBehaviour Script**, name it **`PlayerController`**, and type the code below.
8. Drag `PlayerController` onto `Player`. Set **Ground Layer → Ground**. Hover the fields to show the tooltips.
9. **Play** and move with WASD. 💥 **Planned failure: the capsule tips over** and rolls around. Stop.
   **Fix:** Rigidbody > **Constraints > Freeze Rotation X and Z**. Play again. It stays upright, and Space jumps.
10. Turn on **Gizmos** in the Scene view, then Play with Scene and Game views side by side. The ray under the player is **green** on the ground and **red** in the air. That's `Debug.DrawRay`.
11. 💥 **Planned failure: wall sticking.** Walk off the side of the start platform and, while falling, hold the direction back *into* the platform's side. The player sticks to the wall because friction holds it up. Stop.
    **Fix:** `PhysicsMaterials` folder > **right-click > Create > Physics Material**, name it **`NoFriction`**. Set Dynamic Friction **0**, Static Friction **0**, Friction Combine **Minimum**. Drag it into **Player > Capsule Collider > Material**. Try again: the player slides down.
12. 💥 **Planned failure: Play-mode changes get lost.** While playing, set Move Speed to **12** and Jump Force to **14**. It feels wild. **Stop**, and the values snap back to 6 and 9. "Anything you change in Play mode is thrown away when you stop. That's why we tinted Play mode red."
    Tip: right-click the component header > **Copy Component**, stop, then **Paste Component Values** to keep them.

<!-- lesson-script: step-2-player Assets/_Workshop/Scripts/PlayerController.cs -->
```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Running speed, in meters per second.")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Jumping")]
    [Tooltip("How hard we kick the player upward. Bigger = higher jump.")]
    [SerializeField] private float jumpForce = 9f;

    [Header("Ground Check")]
    [Tooltip("How far down from the player's center we look for ground. The capsule is 2 m tall, so its feet are 1 m down, plus 0.1 m of wiggle room.")]
    [SerializeField] private float groundCheckDistance = 1.1f;
    [Tooltip("Which layers count as ground you can jump off. Set this to Ground.")]
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction jumpAction;

    private Vector2 moveInput;
    private bool jumpQueued;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    // Read input in Update: a tap lasts one frame, and FixedUpdate might not run on that frame.
    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        if (jumpAction.WasPressedThisFrame())
        {
            jumpQueued = true;
        }
    }

    // Change the Rigidbody in FixedUpdate, in step with the physics engine.
    private void FixedUpdate()
    {
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        Color rayColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, rayColor);

        // No Time.deltaTime: velocity is already in meters per second.
        rb.linearVelocity = new Vector3(moveInput.x * moveSpeed, rb.linearVelocity.y, moveInput.y * moveSpeed);

        if (jumpQueued && isGrounded)
        {
            // Cancel any vertical speed so every jump is the same height.
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        jumpQueued = false;
    }
}
```

**Talking points (point at the code):**

- **Payoff of Step 1:** `WasPressedThisFrame()` is only true for a single frame, and `FixedUpdate` might not run on that frame. So `Update` *remembers* the press in `jumpQueued`, and `FixedUpdate` *uses* it.
- **Why no `Time.deltaTime` on velocity?** Velocity is already "meters per **second**". The physics engine multiplies by the step time when it moves the body. Multiplying again would make the player crawl.
- **Preserve Y:** we build a new velocity from input but copy the current Y, so gravity and jumps keep working.
- **Impulse:** one instant kick (like a bat hitting a ball), instead of a push over time.
- **Ground check:** a raycast downward, filtered by the Ground layer. It can't hit the player's own capsule, which is on the Default layer.
- **Input:** `InputSystem.actions` holds the project-wide actions in `Assets/InputSystem_Actions`. "Move" already knows WASD, the arrow keys, and gamepad sticks. No `Input.GetKey`.
- **Gravity is -25** in Project Settings > Physics. Default gravity feels floaty.

**Likely accidental failure:** *the player won't jump.* Check that Ground Layer is set to Ground on PlayerController, and that you're standing on something on the Ground layer. The debug ray tells you: red means "not grounded".

---

## 3 · Build your own parts: prefabs (7 min, 0:25–0:32)

**Goal:** turn raw 3D models into reusable, solid parts.
**Starts from:** `step-2-player` · **Ends at:** `step-3-prefabs` · **Stragglers:** `Assets/_Workshop/_Backup/` has Block, Ramp and Trap ready to drag in.

**Live:**

1. **Block.** Drag **`Art/KayKit/platform_2x2x1_blue`** into the Scene. Rename it `Block` and set Position **(0, -1, 4)**, so it touches the start platform with its top level at y = 0.
   **Add Component > Box Collider** (it wraps the model automatically). Set **Layer → Ground**.
   Drag `Block` from the Hierarchy into the **`Prefabs`** folder and pick **Original Prefab**. It turns blue in the Hierarchy.
2. **Ramp.** Drag **`Art/KayKit/platform_slope_4x6x4_blue`** in and rename it `Ramp`. Set Rotation Y **180** so it climbs away from you, and Position **(0, -1, 8)**.
   💥 **Planned failure: Play and run up the ramp. You fall straight through it.** "Models are just pictures. Physics can't see them until we add a collider."
   Quickly add a **Box Collider** and show its green box outline covering empty air above the slope: an invisible wall. Remove it (component ⋮ > Remove Component), then **Add Component > Mesh Collider**, which matches the real shape. Set **Layer → Ground**. Play: you can run up it. Drag it into `Prefabs` and pick **Original Prefab**.
3. **Trap.** Drag **`Art/Kenney/spike-block`** in, rename it `Trap`, and set Position **(2, 0, -1.5)** on the start platform. Drag **`Materials/HazardRed`** onto it. Drag it into `Prefabs` and pick **Original Prefab**. It has no collider on purpose: its behavior comes in Step 5.
4. **Prefab power.** Double-click the `Block` prefab to open Prefab Mode, make a change (for example, drag `RemixOrange` onto it), then click the **<** arrow to go back. Every Block updates. Undo the color if you like.

**Talking points:**

- **Why don't the models have colliders?** On purpose. The artist ships looks, and you decide the physics. **Box** = cheap and simple. **Mesh** = exact shape, for slopes and odd shapes.
- **Layer = Ground** is what makes something jumpable (our ground ray only looks at that layer).
- A **prefab** is a blueprint. Edit the prefab and every copy changes.
- **Original Prefab vs Variant:** "Original" makes a new independent blueprint. That's what we want.

---

## 4 · Remix #1: build a course (5 min, 0:32–0:37)

**Goal:** get from the start platform to the goal (50 m away and 2 m higher) using Blocks, Ramps and anything in `_Backup/`. **No code.**
**Stragglers:** jump to `step-3-prefabs`.

**Tips to put on screen:**

- **Hold Ctrl (Cmd on Mac) while dragging** to move in 1 m steps (90° steps with the rotate tool). **Ctrl/Cmd + D** duplicates.
- **F** frames the selected object in the Scene view.
- The jump reaches about **1.6 m high and 4 m far**. Use 1 m steps or ramps to climb, not 2 m walls.
- More parts: `_Backup/` (BlockSmall, BlockTall, BlockLong, PlatformWide, RampSmall, Pipe, Arch). Recolor anything by dragging a `Materials/Remix*` color onto it.
- **Build in Edit mode, test in Play mode.** (Remember what happens to Play-mode changes.)

---

## 5 · Moving platforms, traps, respawn (11 min, 0:37–0:48)

**Goal:** things that kill you, things that move, and restarting the level.
**Starts from:** `step-3-prefabs` · **Ends at:** `step-5-hazards`

**Live, part A: traps and falling (about 4 min):**

1. In `Scripts`, create **`KillOnTouch`** and type it (below).
2. Double-click the **`Trap` prefab** to open Prefab Mode. **Add Component > Box Collider**, but **leave Is Trigger unticked for now**. Add Component > **KillOnTouch**. Go back (**<**).
   💥 **Planned failure: trigger doesn't fire.** Play and walk into the trap: you just bump into it. "`OnTriggerEnter` only fires for **triggers**."
   **Fix:** open the prefab again and tick **Is Trigger**. Play: touching it restarts the level. Every Trap in the scene got the fix, because it's a prefab.
3. Select **FallZone** (the big invisible trigger far below the level) and **Add Component > KillOnTouch**. Play and jump off the edge: the level restarts.
4. Explain the *other* half of "trigger doesn't fire": **at least one of the two objects needs a Rigidbody.** Our Player has one, which is why this works. Two plain colliders, like a cube and the trap, never send trigger messages.

<!-- lesson-script: step-5-hazards Assets/_Workshop/Scripts/KillOnTouch.cs -->
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillOnTouch : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
    }
}
```

**Live, part B: moving platform (about 7 min):**

5. Create **`Patrol`** and **`StickyPlatform`** in `Scripts` and type them (below). For `Patrol`, you can paste `OnDrawGizmos` and just explain it.
6. **Hierarchy > Create Empty**, name it `MovingPlatform`, Position **(5, -1, 0)**. It's just beside the start platform's right edge.
7. Drag **`Art/KayKit/platform_4x4x1_yellow`** onto `MovingPlatform` so it becomes a **child**. Rename it `Platform` and set Position (0, 0, 0). Add a **Box Collider** and set **Layer → Ground**.
8. Right-click `MovingPlatform` > **Create Empty** twice: **`WaypointA`** at (0, 0, 0) and **`WaypointB`** at **(6, 0, 0)**. They're children of `MovingPlatform`, **not** of `Platform`, otherwise they'd move with it.
9. Select `Platform`, **Add Component > Patrol**, and drag `WaypointA` and `WaypointB` into **Waypoints**. Yellow gizmos show the path.
10. **Play** and hop on. 💥 **Known issue: the platform slides out from under you** and leaves you behind. Stop.
11. Right-click `Platform` > **Create Empty**, name it `StickyZone`. **Add Component > Box Collider** with **Is Trigger** ticked, **Center (0, 1.25, 0)**, **Size (3.6, 0.5, 3.6)**. That's a thin slab just above the platform's top. Add **StickyPlatform**. Play: now it carries you.
12. Drag `MovingPlatform` into `Prefabs` and pick **Original Prefab**.

<!-- lesson-script: step-5-hazards Assets/_Workshop/Scripts/Patrol.cs -->
```csharp
using UnityEngine;

public class Patrol : MonoBehaviour
{
    [Header("Path")]
    [Tooltip("The points to visit, in order. After the last one, we head back to the first. Don't make them children of this object, or they'll move along with it!")]
    [SerializeField] private Transform[] waypoints;

    [Header("Movement")]
    [Tooltip("Speed in meters per second.")]
    [SerializeField] private float speed = 3f;

    private int targetIndex = 0;

    private void Update()
    {
        if (waypoints.Length == 0)
        {
            return;
        }

        Vector3 targetPosition = waypoints[targetIndex].position;

        // Time.deltaTime IS needed here: we move the Transform ourselves, not through a velocity.
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (transform.position == targetPosition)
        {
            targetIndex = targetIndex + 1;
            if (targetIndex >= waypoints.Length)
            {
                targetIndex = 0;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Transform current = waypoints[i];
            Transform next = waypoints[(i + 1) % waypoints.Length];

            if (current == null || next == null)
            {
                continue;
            }

            Gizmos.DrawSphere(current.position, 0.3f);
            Gizmos.DrawLine(current.position, next.position);
        }
    }
}
```

<!-- lesson-script: step-5-hazards Assets/_Workshop/Scripts/StickyPlatform.cs -->
```csharp
using UnityEngine;

// While the player stands in this trigger, they become a child of the platform and ride along.
public class StickyPlatform : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }
    }
}
```

**Talking points:**

- **Collider vs trigger:** a collider blocks you. A trigger just reports "something entered" (`OnTriggerEnter`).
- `CompareTag("Player")` is why tagging the player in Step 2 mattered.
- `SceneManager.LoadScene(current)` is the simplest possible respawn. It resets *everything*, coins included. (Bonus challenge: checkpoints.)
- **deltaTime, part 2:** `Patrol` moves the **Transform** itself, so we *do* multiply by `Time.deltaTime` to get meters *this frame*. `PlayerController` sets a **velocity**, and physics does that math for us.
- **Why parenting works:** a child moves with its parent. StickyZone makes the player a child while they're on top. It also has to stay off the Ground layer, and it is: new empties use the Default layer.

---

## 6 · Coins + win (7 min, 0:48–0:55) ✂️ first to cut

**Goal:** a score, a way to win, and a Win screen.
**Starts from:** `step-5-hazards` · **Ends at:** `step-6-goal`

**Live:**

1. In `Scripts`, create **both** `Coin` and `GameManager` *before* typing. They refer to each other, so Unity shows errors until both are typed and saved. Type `Coin`, then `GameManager`, save both, and switch back to Unity.
2. **Coin prefab:** drag **`Art/Kenney/coin-gold`** into the scene, rename it `Coin`, and drag **`Materials/CoinYellow`** onto it. Set **Tag → Coin**. **Add Component > Sphere Collider**: **Is Trigger** ✓, Center **(0, 0.5, 0)**, Radius **0.6**. Add **Coin**. Drag it into `Prefabs` (**Original Prefab**).
3. Place 4 or 5 coins around your course (Ctrl/Cmd + D, Ctrl-drag to snap). Put a few somewhere risky.
4. **Hierarchy > Create Empty**, name it `GameManager`, and **Add Component > GameManager**.
5. Expand **Canvas**, **tick `ScoreText`** to enable it, and drag it into GameManager's **Score Text**.
6. **Play:** "Coins: 0 / 5" in the corner. Collect them all and the **Win** scene loads. Click **Play Again**.
7. **Goal instead of coins:** create **`GoalTrigger`** (below). Right-click **Level > GoalPlatform** > **Create Empty**, name it `GoalZone`. Add a **Box Collider**: **Is Trigger** ✓, Center **(0, 3.25, 0)**, Size **(5.5, 2.5, 5.5)**. Add **GoalTrigger**. Reaching the goal now wins too.

<!-- lesson-script: step-6-goal Assets/_Workshop/Scripts/Coin.cs -->
```csharp
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Spinning")]
    [Tooltip("How fast the coin spins, in degrees per second.")]
    [SerializeField] private float spinSpeed = 90f;

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddCoin();
            Destroy(gameObject);
        }
    }
}
```

<!-- lesson-script: step-6-goal Assets/_Workshop/Scripts/GameManager.cs -->
```csharp
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton: any script can reach the one GameManager through GameManager.Instance.
    public static GameManager Instance;

    [Header("UI")]
    [Tooltip("The text that shows the score. Drag Canvas > ScoreText here.")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Winning")]
    [Tooltip("The scene to load when every coin is collected. It must be listed in File > Build Profiles.")]
    [SerializeField] private string winSceneName = "Win";

    private int totalCoins = 0;
    private int collectedCoins = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Coin[] coinsInLevel = FindObjectsByType<Coin>(FindObjectsSortMode.None);
        totalCoins = coinsInLevel.Length;
        UpdateScoreText();
    }

    public void AddCoin()
    {
        collectedCoins = collectedCoins + 1;
        UpdateScoreText();

        if (collectedCoins >= totalCoins)
        {
            SceneManager.LoadScene(winSceneName);
        }
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Coins: " + collectedCoins + " / " + totalCoins;
    }
}
```

<!-- lesson-script: step-6-goal Assets/_Workshop/Scripts/GoalTrigger.cs -->
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalTrigger : MonoBehaviour
{
    [Header("Winning")]
    [Tooltip("The scene to load when the player reaches the goal. It must be listed in File > Build Profiles.")]
    [SerializeField] private string winSceneName = "Win";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(winSceneName);
        }
    }
}
```

**Talking points:**

- **Singleton:** `GameManager.Instance` lets any coin find *the* GameManager without dragging references around. It's fine for one-of-a-kind managers, and we won't use it for anything else.
- `FindObjectsByType<Coin>` counts the coins once, at the start. Place as many as you like and the total updates itself.
- `Destroy(gameObject)` removes the coin. `LoadScene("Win")` only works because Win is listed in **File > Build Profiles** (the starter already did this: Arena = 0, Win = 1).
- **Likely accidental failure:** a `NullReferenceException` pointing at `UpdateScoreText` means **Score Text isn't assigned**. One pointing at `Coin.OnTriggerEnter` means there's **no GameManager in the scene**.

---

## Remix #2 + wrap-up (5 min, 0:55–1:00) ✂️ shorten if late

- "Add at least one trap, one moving platform and three coins to your course. Then swap laptops with a neighbor and try theirs."
- Show `final` again and point out the level-design rules from the [tuned values](#tuned-values-why-the-player-feels-the-way-it-does).
- **Part 2 teaser:** the Kenney character (`Art/Kenney/character-oobi`) is already imported with its animations. Next time we swap the capsule for it and wire up an Animator.
- Fast finishers: the bonus challenges below. They're in `Scripts/Bonus/` in the `final` checkpoint.

---

## Bonus challenges (for fast finishers)

Each one is a tiny, standalone script, and they're all in `Assets/_Workshop/Scripts/Bonus/` in the `final` checkpoint. The demo course uses the bounce pad and the spinner. Paste them into Discord for anyone who wants them.

| Script | One-liner | How to use |
|---|---|---|
| `DoubleJump` | Extra jumps in mid-air. | Add it **next to** PlayerController on the Player. Set its Ground Layer to Ground. `airJumps` = 1 gives a double jump. |
| `BouncePad` | Launches the player upward on contact. | Drag `Art/KayKit/spring_pad_blue` in, add a Box Collider (not a trigger) and BouncePad. |
| `Spinner` | Rotates an object forever around an axis. | Put it on anything. Spinning saw: `Art/Kenney/saw` + convex Mesh Collider (trigger) + KillOnTouch + Spinner with Axis (0, 0, 1). |
| `Checkpoint` + `PlayerRespawn` + `RespawnOnTouch` | Respawn at the last checkpoint instead of restarting the level (you keep your coins). | Add `PlayerRespawn` to the Player. Make a trigger with `Checkpoint`. On traps and the FallZone, **replace** KillOnTouch with `RespawnOnTouch`. |

<!-- lesson-script: final Assets/_Workshop/Scripts/Bonus/DoubleJump.cs -->
```csharp
using UnityEngine;
using UnityEngine.InputSystem;

// Add next to PlayerController. PlayerController still jumps from the ground; this only adds mid-air jumps.
public class DoubleJump : MonoBehaviour
{
    [Header("Air Jumps")]
    [Tooltip("Extra jumps you get before touching the ground again. 1 = double jump, 2 = triple jump...")]
    [SerializeField] private int airJumps = 1;
    [Tooltip("Upward kick for each air jump. Try matching PlayerController's Jump Force.")]
    [SerializeField] private float airJumpForce = 9f;

    [Header("Ground Check (use the same values as PlayerController)")]
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private InputAction jumpAction;
    private int airJumpsLeft = 0;
    private bool airJumpQueued = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void Update()
    {
        if (jumpAction.WasPressedThisFrame())
        {
            airJumpQueued = true;
        }
    }

    private void FixedUpdate()
    {
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if (isGrounded)
        {
            airJumpsLeft = airJumps;
        }
        else if (airJumpQueued && airJumpsLeft > 0)
        {
            airJumpsLeft = airJumpsLeft - 1;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * airJumpForce, ForceMode.Impulse);
        }

        airJumpQueued = false;
    }
}
```

<!-- lesson-script: final Assets/_Workshop/Scripts/Bonus/BouncePad.cs -->
```csharp
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [Header("Bounce")]
    [Tooltip("How hard to launch the player upward.")]
    [SerializeField] private float bounceForce = 16f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerBody = collision.rigidbody;
            playerBody.linearVelocity = new Vector3(playerBody.linearVelocity.x, 0f, playerBody.linearVelocity.z);
            playerBody.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
        }
    }
}
```

<!-- lesson-script: final Assets/_Workshop/Scripts/Bonus/Spinner.cs -->
```csharp
using UnityEngine;

public class Spinner : MonoBehaviour
{
    [Header("Spinning")]
    [Tooltip("The axis to spin around, in the object's own space. (0, 1, 0) spins like a record; (0, 0, 1) spins like a wheel facing you.")]
    [SerializeField] private Vector3 axis = Vector3.up;
    [Tooltip("Degrees per second. Use a negative number to spin the other way.")]
    [SerializeField] private float degreesPerSecond = 180f;

    private void Update()
    {
        transform.Rotate(axis, degreesPerSecond * Time.deltaTime);
    }
}
```

<!-- lesson-script: final Assets/_Workshop/Scripts/Bonus/PlayerRespawn.cs -->
```csharp
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 respawnPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        respawnPosition = transform.position;
    }

    public void SetRespawnPosition(Vector3 newPosition)
    {
        respawnPosition = newPosition;
    }

    public void Respawn()
    {
        transform.SetParent(null);
        rb.linearVelocity = Vector3.zero;
        transform.position = respawnPosition;
    }
}
```

<!-- lesson-script: final Assets/_Workshop/Scripts/Bonus/Checkpoint.cs -->
```csharp
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Respawn")]
    [Tooltip("Where the player reappears, measured from this checkpoint's position. (0, 1, 0) = standing right here.")]
    [SerializeField] private Vector3 respawnOffset = new Vector3(0f, 1f, 0f);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRespawn playerRespawn = other.GetComponent<PlayerRespawn>();
            playerRespawn.SetRespawnPosition(transform.position + respawnOffset);
        }
    }
}
```

<!-- lesson-script: final Assets/_Workshop/Scripts/Bonus/RespawnOnTouch.cs -->
```csharp
using UnityEngine;

// Like KillOnTouch, but sends the player back to their last checkpoint instead of reloading the level.
public class RespawnOnTouch : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRespawn playerRespawn = other.GetComponent<PlayerRespawn>();
            playerRespawn.Respawn();
        }
    }
}
```

---

## Troubleshooting during the session

| Symptom | Cause → fix |
|---|---|
| "Can't add script" / "The script class cannot be found" | The file name and class name don't match (`playerController.cs` vs `PlayerController`), or there's a compile error. Check the Console for red errors first. |
| Red errors after typing | Usually a missing `;` or `}`, or a typo in a name. Double-click the error to jump to the line. Nothing plays until every error is fixed. |
| Changes to the script don't show up | Not saved. The editor tab shows a dot or asterisk. Save, then click back into Unity. |
| Player falls through a part | The part has no collider (Step 3). |
| Player can't jump | Ground Layer isn't set on PlayerController, or the part isn't on the Ground layer. Watch the debug ray. |
| Player tips over | Freeze Rotation X and Z on the Rigidbody. |
| Player sticks to walls | NoFriction isn't on the Capsule Collider. |
| Camera doesn't follow | Follow Camera > Tracking Target isn't set to Player. |
| Trap or coin does nothing | Is Trigger isn't ticked, the Player isn't tagged `Player`, or the Player has lost its Rigidbody. |
| Slides off the moving platform | StickyZone is missing or not a trigger, the waypoints are children of Platform, or the Player's Rigidbody Interpolate isn't None. |
| NullReferenceException in GameManager or Coin | Score Text isn't assigned, or there's no GameManager in the scene. |
| Win scene doesn't load | It's not in File > Build Profiles, or the name in the Inspector doesn't match (`Win`). |
| All my work vanished | It was built in Play mode. See Step 2, item 12. |
| Magenta/pink materials, or a "different Unity version" dialog | Wrong Unity version. It must be 6000.3.8f1 (see README). |

---

## Instructor tooling (not for attendees)

Everything instructor-only lives in `UnityProject/Assets/_Instructor/` and `Tools/`. Both are left out of the attendee zips.

- **Unity menu: Workshop**
  - **Build Checkpoint > Step N**: rebuilds that checkpoint's scenes and prefabs from the previous one. Safe to re-run. Add that step's scripts first (they come from this guide).
  - **Validate Project**: checks the project against its checkpoint (colliders, layers, tags, wiring, missing scripts).
  - **Render Doc Screenshots**: regenerates `Docs/images/` from `final`.
- **`Tools/build-checkpoints.sh`**: rebuilds every checkpoint from scratch in order. For each one it extracts scripts from this guide, runs the builder and the validator, runs the PlayMode tests, then commits and tags. Use `--from <tag>` to resume, and `--no-tests` / `--no-commit` as needed. Close Unity first.
- **`Tools/run-tests.sh`**: runs the PlayMode tests on the current checkout. They move the player with a virtual gamepad (run speed at 30/60/144 fps, jump height, no double jump, ramp climb, traps, fall zone, moving platform, coins, goal, Play Again). On `final`, a bot plays the whole demo course.
- **`Tools/extract-lesson-scripts.sh <tag> [--check]`**: writes or checks the scripts from this guide.
- **`Tools/make-zips.sh`**: one zip per tag in `dist/`, made with `git archive` so no Library or local junk gets in.
