# Instructor Guide: Build an Obstacle Course

Run-of-show for the VGDA Unity Basics workshop. The session is about **75 minutes**, and everyone builds along on their own laptop while you build on the projector. Unity basics come first, then scripting.

- Unity **6000.3.8f1** (Unity 6.3 LTS), Universal 3D (URP), Input System (project-wide actions), Cinemachine 3.1.7
- Attendee setup: [README.md](README.md) · Catching up: [CHECKPOINTS.md](CHECKPOINTS.md) · Things done by hand: [MANUAL_SETUP.md](MANUAL_SETUP.md)
- Slides: the workshop deck follows this guide step for step.

> **This file is also source code.** Every lesson script below sits under a `<!-- lesson-script: ... -->` marker. `Tools/build-checkpoints.sh` copies those code blocks into the project to build each checkpoint, and into `Checkpoints/_Scripts` for the catch-up scenes. If you change a script, change it **here**. Then rebuild, or run `Tools/extract-lesson-scripts.sh final --check` to confirm the project still matches.

---

## At a glance

| # | Segment | Time | Clock | Catch-up scene at the end |
|---|---------|------|-------|---------------------------|
| 0 | Welcome + "here's what you'll build" | 4 min | 0:00–0:04 | — |
| 1 | Unity basics: windows, GameObjects, Transform, components | 10 min | 0:04–0:14 | `1 - Unity Basics` |
| 2 | Build your own parts (colliders, layers, prefabs) | 8 min | 0:14–0:22 | `2 - Parts` |
| 3 | Scripting basics (your first scripts) | 12 min | 0:22–0:34 | `3 - Scripting` |
| 4 | The player: move and jump | 14 min | 0:34–0:48 | `4 - Player` |
| 5 | Remix #1 (no code) | 5 min | 0:48–0:53 | — |
| 6 | Moving platforms, traps, respawn | 10 min | 0:53–1:03 | `6 - Hazards` |
| 7 | Coins + win ✂️ *first to cut* | 7 min | 1:03–1:10 | `7 - Coins and Win` |
| 8 | Remix #2 + wrap-up ✂️ *second to cut* | 5 min | 1:10–1:15 | `Final - Demo Course` |

**Catching up (say this at the start of every step):** "Fell behind? In the Project window, open `_Workshop > Checkpoints` and double-click the scene for the step we just finished. It's a complete, working copy. Keep building in it." Every copy of the project has all seven checkpoint scenes, so there's nothing to download and nothing to re-import. The scenes use their own copies of every script and prefab, so they never clash with what attendees build themselves.

**Cut lines if you're running late:**

1. **At 0:55**, if Step 6 hasn't started, **cut Step 7.** Finish Step 6 and go straight to Remix #2 (hazards only). Point people at the `7 - Coins and Win` scene to explore later.
2. **Then shorten Remix #2** to about 2 minutes: "put one hazard and one moving platform in your course," then wrap up.
3. **Extended session?** Grow both remixes and hand out the [bonus challenges](#bonus-challenges-for-fast-finishers).

---

## Before the session

- [ ] Open the starter project (a fresh unzip of `step-0-starter`) and play the `Final - Demo Course` scene start to finish. This is the demo you show first. Play Again should bring you back to it.
- [ ] Re-open `Arena` afterwards: that's where you build live.
- [ ] Turn on **Edit > Preferences > Colors > Playmode tint** and pick something loud (light red). Beginners edit in Play mode constantly, and the tint makes it obvious.
- [ ] Bump the font size in your code editor (VS Code: Ctrl/Cmd + `=` a few times). Use a big Unity layout with the Console docked next to the Game view.
- [ ] Post the zips (GitHub Release) and the Discord thread link where people can see them.
- [ ] Optional: run `Tools/run-tests.sh tests all` on the starter. The instructor PlayMode tests play the Arena and every checkpoint scene with a virtual gamepad, including a bot that finishes the demo course.

---

## Tuned values (why the player feels the way it does)

| Setting | Value | Where | Why |
|---|---|---|---|
| Gravity | **(0, -25, 0)** | Project Settings > Physics | Earth's -9.81 feels floaty for a platformer. -25 gives a quick, snappy arc. The starter already has this. |
| Move Speed | **6** m/s | PlayerController | Fast enough to be fun, slow enough to steer onto 2 m blocks. |
| Jump Force | **9** (Impulse, mass 1) | PlayerController | 9 m/s upward → apex **≈ 1.6 m**, ≈ 0.72 s in the air, ≈ **4.3 m** jump on flat ground. |
| Ground Check Distance | **1.1** m | PlayerController | Capsule is 2 m tall (feet 1 m below center) plus 0.1 m slack. |
| Ground Layer | **Ground** (layer 6) | PlayerController + every walkable part | The ground ray only hits this layer. |
| Rigidbody | mass 1, **Interpolate: None**, freeze rotation X+Z (from Step 4) | Player | Keep interpolation **off**. With it on, the physics engine ignores StickyPlatform's parenting and you slide off moving platforms. |
| NoFriction | friction 0/0, combine **Minimum** | Player's Capsule Collider (from Step 4) | Otherwise you stick to walls when you jump into them. |
| Fixed timestep | 0.02 s (50 Hz, default) | Project Settings > Time | — |

What that means for level design (the demo course follows these rules):

- **1 m step up: jumpable. 2 m wall: not jumpable** (use a ramp).
- **Gaps up to 3 m** are comfortable at the same height. 4 m is possible, 5 m is not.
- KayKit slopes sit on a **1 m base**: `platform_slope_4x6x4` climbs from 1 m to 4 m (26.6°). A ramp and a block placed at the same height join seamlessly. Sink the ramp 1 m to meet a lower floor.
- The start platform's top is at **y = 0** and the goal's top is at **y = 2**, 50 m away. Blocks dropped on the grid (y = 0) stick up 1 m.

---

## 0 · Welcome + demo (4 min, 0:00–0:04)

**Live:**

1. In the starter project, open **`_Workshop > Checkpoints > Final - Demo Course`**, press **Play**, and run the course. Ride the moving platform, hit the bounce pad, weave through the spikes, and touch the goal. **Play Again**.
2. "In the next hour and a bit you'll build every piece of this yourself. The player, the prefabs, the traps, the moving platform, the coins."
3. Point out the **Checkpoints** folder: "If you fall behind at any point, this is your safety net."
4. Open **`_Workshop > Scenes > Arena`**. That's where everyone builds.

---

## 1 · Unity basics (10 min, 0:04–0:14)

**Goal:** know your way around the editor, and build the player out of GameObjects and components. No code yet.
**Starts from:** the starter `Arena` · **Ends at:** `1 - Unity Basics`

**Live:**

1. **The windows.** Point at each one: **Scene** (the world you build), **Game** (what the player sees), **Hierarchy** (everything in this scene), **Inspector** (settings for whatever is selected), **Project** (all your files), **Console** (messages and errors). The **Play** button at the top runs the game.
2. **Moving around the Scene view.** Hold **right-click** and use **WASD** to fly. **Scroll** to zoom. **Middle-drag** to pan. **Alt + left-drag** to orbit. Select something and press **F** to frame it.
3. **GameObjects.** **Hierarchy > + > 3D Object > Cube.** It shows up in the Hierarchy *and* the Scene. "Everything in a scene is a GameObject: the camera, the light, the platforms."
4. **Components.** With the cube selected, read the Inspector top to bottom: **Transform**, **Mesh Filter** (its shape), **Mesh Renderer** (draws it), **Box Collider** (its physical shape). "A GameObject is just a container. Components decide what it is and does."
5. **Transform.** Press **W** (move), **E** (rotate), **R** (scale) and drag the colored handles: **red = X** (right), **green = Y** (up), **blue = Z** (forward). Type numbers into Position too. Units are meters. ⋮ > **Reset** puts it back at (0, 0, 0).
6. **Parent and child.** Add a **Sphere** and drag it onto the cube in the Hierarchy. Move the cube: the sphere comes along. A child's Position is measured from its parent. Delete both when you're done.
7. **Build the player.** **+ > 3D Object > Capsule**, rename it **`Player`**, and set Position **(0, 5, 0)**, up in the air. Drag **`Materials/PlayerBlue`** onto it. Set **Tag → Player** (top of the Inspector).
8. **Play.** Nothing happens: it just floats. Stop. **Add Component > Rigidbody.** Play again: it falls and lands on the platform. The Rigidbody gives it gravity, and its Capsule Collider hits the platform's Box Collider.
9. 💥 **Planned failure: Play-mode changes get lost.** While playing, drag the Player way up and let it drop again, and change its Mass. **Stop.** Everything snaps back. "Anything you change in Play mode is thrown away when you stop. That's why we tinted Play mode red."
10. In Edit mode, set the Player's Position to **(0, 1, 0)** so it stands on the platform.
11. **Point the camera at it.** Select **Follow Camera** and drag `Player` from the Hierarchy into **Cinemachine Camera > Tracking Target**. Dragging an object into a field like this is how components point at each other.

**Talking points:**

- Unity is GameObjects all the way down. Even the camera is a GameObject with a Camera component.
- **Rigidbody = "physics moves me".** **Collider = "my shape for physics".** Without a collider, physics can't feel you.
- The capsule we just made is the whole player. In Step 4 we write the script that steers it.

---

## 2 · Build your own parts: prefabs (8 min, 0:14–0:22)

**Goal:** turn raw 3D models into reusable, solid parts.
**Starts from:** `1 - Unity Basics` · **Ends at:** `2 - Parts` · **Stragglers:** `Assets/_Workshop/_Backup/` also has Block, Ramp and Trap ready to drag in.

**Live:**

1. **Block.** Drag **`Art/KayKit/platform_2x2x1_blue`** into the Scene. Rename it `Block` and set Position **(0, -1, 4)**, so it touches the start platform with its top level at y = 0.
   **Add Component > Box Collider** (it wraps the model automatically). Set **Layer → Ground** (top right of the Inspector).
   Drag `Block` from the Hierarchy into the **`Prefabs`** folder and pick **Original Prefab**. It turns blue in the Hierarchy.
2. **Ramp.** Drag **`Art/KayKit/platform_slope_4x6x4_blue`** in and rename it `Ramp`. Set Rotation Y **180** so it climbs away from the start, and Position **(0, -1, 8)**.
   💥 **Planned failure: drop the player on it.** Set the Player's Position to **(0, 6, 9)** and press Play. It falls straight through the ramp. "Models are just pictures. Physics can't see them until we add a collider."
   Quickly add a **Box Collider** and show its green outline: a box full of empty air above the slope, an invisible wall. Remove it (component ⋮ > Remove Component), then **Add Component > Mesh Collider**, which matches the real shape. Set **Layer → Ground**. Play again: the capsule lands on the slope and tumbles down it (it can tip over because nothing stops it from rotating yet).
   Drag `Ramp` into `Prefabs` (**Original Prefab**), and put the Player back at **(0, 1, 0)**.
3. **Trap.** Drag **`Art/Kenney/spike-block`** in, rename it `Trap`, and set Position **(2, 0, -1.5)** on the start platform. Drag **`Materials/HazardRed`** onto it. Drag it into `Prefabs` (**Original Prefab**). It has no collider on purpose: it gets one in Step 6.
4. **Prefab power.** Double-click the `Block` prefab to open Prefab Mode, change something (for example, drag `RemixOrange` onto it), then click the **<** arrow to go back. Every Block updates. Undo the color if you like.

**Talking points:**

- **Why don't the models have colliders?** On purpose. The artist ships looks, and you decide the physics. **Box** = cheap and simple. **Mesh** = exact shape, for slopes and odd shapes.
- **Layers** sort objects into groups. **Ground** marks what the player can jump off. That matters in Step 4.
- A **prefab** is a blueprint. Edit the prefab and every copy changes.
- **Original Prefab vs Variant:** "Original" makes a new independent blueprint. That's what we want.

---

## 3 · Scripting basics (12 min, 0:22–0:34)

**Goal:** understand what a script is and how one is put together, then write two small ones.
**Starts from:** `2 - Parts` · **Ends at:** `3 - Scripting`

**Live, part A: your first script (about 6 min):**

1. **+ > 3D Object > Cube**, rename it **`ScriptDemo`**, Position **(2, 0.5, 2)**.
2. In `Scripts`, **right-click > Create > Scripting > MonoBehaviour Script**. Type the name **`Spinner`** right away and press Enter. The file name and the class name inside must match exactly, capital letters included.
3. Double-click it. Walk through what Unity generated: `using UnityEngine;` (lets us use Unity's code), `public class Spinner : MonoBehaviour` (this is a component), and `Start` / `Update` (Unity calls these for us). Every statement ends with `;`, and `{ }` group code together.
4. Replace it with the code below and explain each piece:
   - **Fields** are variables the component keeps: a `Vector3` (three numbers: x, y, z) and a `float` (a decimal number). The `f` in `180f` marks a float.
   - **`[SerializeField] private`** shows a field in the Inspector without letting other scripts change it. `[Header]` and `[Tooltip]` make the Inspector friendlier.
   - **`transform`** is this GameObject's Transform component. `Rotate` turns it.
   - **`Time.deltaTime`** is the time since the last frame. "Degrees per second × seconds this frame = degrees this frame", so it spins at the same speed on a fast laptop and a slow one.
5. Save (Ctrl/Cmd + S) and switch back to Unity so it compiles. Drag `Spinner` onto `ScriptDemo`. The two fields appear in the Inspector.
6. **Play:** it spins. While playing, change **Degrees Per Second** to 720, then set **Axis** to (1, 0, 0). Stop: the values reset (Play mode again!).
7. 💥 **Planned failure: a compile error.** Delete a `;` and save. The Console shows a red error with the file and line. Double-click it to jump there. "Nothing runs until every red error is fixed." Put the `;` back.

<!-- lesson-script: step-3-scripting Assets/_Workshop/Scripts/Spinner.cs -->
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
        // Time.deltaTime turns "degrees per second" into "degrees this frame", so it spins at the same speed on any computer.
        transform.Rotate(axis, degreesPerSecond * Time.deltaTime);
    }
}
```

**Live, part B: who runs first? (about 6 min):**

8. Create **`LifecycleLogger`** in `Scripts` and type it (below). Point out `Debug.Log` (prints to the Console) and the `Log` helper method, which takes a `string` parameter.
9. Drag it onto `ScriptDemo` too. Make sure **Clear on Play** is on in the Console and **Collapse** is off.
10. **Play.** Read the Console top to bottom: `Awake → OnEnable → Start → FixedUpdate / Update`, each with its frame number.
11. Experiments (the teaching goals):
    - **Toggle the GameObject.** While playing, untick the checkbox next to `ScriptDemo`'s name → `OnDisable`. Tick it again → `OnEnable`, but **no second `Start`**.
    - **Disable only the component.** Stop. Untick the checkbox on the **LifecycleLogger component**, then Play → only **`Awake`** prints. Tick the component during Play → `OnEnable`, `Start`, then updates. *Awake runs even when the component is disabled. Start waits.*
    - **Different rates.** Stop, tick **Log Every Frame**, Play for about 2 seconds, then **Pause**. Scroll: `Update` appears on *every* frame number. `FixedUpdate` skips frames on a fast laptop and repeats a frame number on a slow one.
12. Stop Play mode.

<!-- lesson-script: step-3-scripting Assets/_Workshop/Scripts/LifecycleLogger.cs -->
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

- **A script is a component you write.** Anything you can do with the built-in components, a script can do too.
- Unity calls `Awake`, `Start`, `Update` and friends for you. You never call them yourself.
- `Update` = once per **frame**, which varies by computer. `FixedUpdate` = once per **physics step**, a steady 50 per second.
- Next step's rule of thumb: **read input in `Update`, move physics objects in `FixedUpdate`.**
- `if (a || !b)`: `||` means "or", `!` means "not". `+` glues strings together.

---

## 4 · The player: move and jump (14 min, 0:34–0:48)

**Goal:** steer the capsule from Step 1 with the keyboard or a gamepad, using physics.
**Starts from:** `3 - Scripting` · **Ends at:** `4 - Player`

**Live:**

1. Delete `ScriptDemo` (select it, then Delete). The scripts stay in the project.
2. In `Scripts`, **Create > Scripting > MonoBehaviour Script**, name it **`PlayerController`**, and type the code below. Explain as you go:
   - **References:** `GetComponent<Rigidbody>()` finds the Rigidbody on this same GameObject. We look it up once in `Awake` and keep it in `rb`.
   - **Input:** `InputSystem.actions` holds the project-wide actions in `Assets/InputSystem_Actions`. "Move" already knows WASD, the arrow keys and gamepad sticks, and gives a `Vector2` (x = left/right, y = forward/back). "Jump" knows Space and the gamepad's bottom button.
   - **Update vs FixedUpdate:** `WasPressedThisFrame()` is only true for a single frame, and `FixedUpdate` might not run on that frame. So `Update` *remembers* the press in `jumpQueued`, and `FixedUpdate` *uses* it.
   - **Velocity** is meters per second, so we **don't** multiply by `Time.deltaTime` here. The physics engine does that math. We copy the current Y velocity so gravity and jumps keep working.
   - **Ground check:** `Physics.Raycast` fires an invisible ray straight down. The `LayerMask` makes it only hit the Ground layer. `Debug.DrawRay` shows it in the Scene view.
   - **Jump:** `AddForce(..., ForceMode.Impulse)` is one instant kick, like a bat hitting a ball.
3. Drag `PlayerController` onto `Player`. Set **Ground Layer → Ground**. Hover the fields to show the tooltips.
4. **Play** and move with WASD. 💥 **Planned failure: the capsule tips over** and rolls around. Stop.
   **Fix:** Rigidbody > **Constraints > Freeze Rotation X and Z**. Play again. It stays upright, and Space jumps.
5. Turn on **Gizmos** in the Scene view, then Play with Scene and Game views side by side. The ray under the player is **green** on the ground and **red** in the air.
6. 💥 **Planned failure: wall sticking.** Walk off the side of the start platform and, while falling, hold the direction back *into* the platform's side. The player sticks to the wall because friction holds it up. Stop.
   **Fix:** `PhysicsMaterials` folder > **right-click > Create > Physics Material**, name it **`NoFriction`**. Set Dynamic Friction **0**, Static Friction **0**, Friction Combine **Minimum**. Drag it into **Player > Capsule Collider > Material**. Try again: the player slides down.
7. **Tune the jump.** While playing, try Jump Force **14** and Move Speed **12**. Stop, and they snap back to 9 and 6. Tip: right-click the component header > **Copy Component**, stop, then **Paste Component Values** to keep them.

<!-- lesson-script: step-4-player Assets/_Workshop/Scripts/PlayerController.cs -->
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

**Talking points:**

- **Gravity is -25** in Project Settings > Physics. Default gravity feels floaty for a platformer.
- The ground ray can't hit the player's own capsule, which is on the Default layer.

**Likely accidental failure:** *the player won't jump.* Check that Ground Layer is set to Ground on PlayerController, and that you're standing on something on the Ground layer. The debug ray tells you: red means "not grounded".

---

## 5 · Remix #1: build a course (5 min, 0:48–0:53)

**Goal:** get from the start platform to the goal (50 m away and 2 m higher) using Blocks, Ramps and anything in `_Backup/`, then play it. **No code.**
**Stragglers:** open `4 - Player`.

**Tips to put on screen:**

- Hold **Ctrl (Cmd on Mac)** while moving or rotating to snap in fixed steps. **Ctrl/Cmd + D** duplicates.
- **F** frames the selected object in the Scene view.
- The jump reaches about **1.6 m high and 4 m far**. Use 1 m steps or ramps to climb, not 2 m walls.
- More parts: `_Backup/` (BlockSmall, BlockTall, BlockLong, PlatformWide, RampSmall, Pipe, Arch). Recolor anything by dragging a `Materials/Remix*` color onto it.
- **Build in Edit mode, test in Play mode.**

---

## 6 · Moving platforms, traps, respawn (10 min, 0:53–1:03)

**Goal:** things that kill you, things that move, and restarting the level.
**Starts from:** `4 - Player` · **Ends at:** `6 - Hazards`

**Live, part A: traps and falling (about 4 min):**

1. In `Scripts`, create **`KillOnTouch`** and type it (below). `OnTriggerEnter` is called by Unity when something enters this object's trigger. `CompareTag("Player")` checks who it was. `SceneManager.LoadScene` reloads the current level.
2. Double-click the **`Trap` prefab** to open Prefab Mode. **Add Component > Box Collider**, but **leave Is Trigger unticked for now**. Add Component > **KillOnTouch**. Go back (**<**).
   💥 **Planned failure: trigger doesn't fire.** Play and walk into the trap: you just bump into it. "`OnTriggerEnter` only fires for **triggers**."
   **Fix:** open the prefab again and tick **Is Trigger**. Play: touching it restarts the level. Every Trap in the scene got the fix, because it's a prefab.
3. Select **FallZone** (the big invisible trigger far below the level) and **Add Component > KillOnTouch**. Play and jump off the edge: the level restarts.
4. Explain the *other* half of "trigger doesn't fire": **at least one of the two objects needs a Rigidbody.** Our Player has one, which is why this works.

<!-- lesson-script: step-6-hazards Assets/_Workshop/Scripts/KillOnTouch.cs -->
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

**Live, part B: moving platform (about 6 min):**

5. Create **`Patrol`** and **`StickyPlatform`** in `Scripts` and type them (below). For `Patrol`, you can paste `OnDrawGizmos` and just explain it.
6. **Hierarchy > Create Empty**, name it `MovingPlatform`, Position **(5, -1, 0)**. It's just beside the start platform's right edge.
7. Drag **`Art/KayKit/platform_4x4x1_yellow`** onto `MovingPlatform` so it becomes a **child**. Rename it `Platform` and set Position (0, 0, 0). Add a **Box Collider** and set **Layer → Ground**.
8. Right-click `MovingPlatform` > **Create Empty** twice: **`WaypointA`** at (0, 0, 0) and **`WaypointB`** at **(6, 0, 0)**. They're children of `MovingPlatform`, **not** of `Platform`, otherwise they'd move with it.
9. Select `Platform`, **Add Component > Patrol**, and drag `WaypointA` and `WaypointB` into **Waypoints**. Yellow gizmos show the path.
10. **Play** and hop on. 💥 **Known issue: the platform slides out from under you** and leaves you behind. Stop.
11. Right-click `Platform` > **Create Empty**, name it `StickyZone`. **Add Component > Box Collider** with **Is Trigger** ticked, **Center (0, 1.25, 0)**, **Size (3.6, 0.5, 3.6)**. That's a thin slab just above the platform's top. Add **StickyPlatform**. Play: now it carries you.
12. Drag `MovingPlatform` into `Prefabs` and pick **Original Prefab**.

<!-- lesson-script: step-6-hazards Assets/_Workshop/Scripts/Patrol.cs -->
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

<!-- lesson-script: step-6-hazards Assets/_Workshop/Scripts/StickyPlatform.cs -->
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
- **Arrays:** `Transform[] waypoints` is a list of Transforms. `waypoints.Length` is how many, and `waypoints[0]` is the first one. Counting starts at 0.
- `SceneManager.LoadScene(current)` is the simplest possible respawn. It resets *everything*, coins included. (Bonus challenge: checkpoints.)
- **deltaTime, part 2:** `Patrol` moves the **Transform** itself, so we *do* multiply by `Time.deltaTime`, just like `Spinner`. `PlayerController` sets a **velocity**, and physics does that math for us.
- **Why parenting works:** a child moves with its parent (remember Step 1). StickyZone makes the player a child while they're on top.

---

## 7 · Coins + win (7 min, 1:03–1:10) ✂️ first to cut

**Goal:** a score, a way to win, and a Win screen.
**Starts from:** `6 - Hazards` · **Ends at:** `7 - Coins and Win`

**Live:**

1. In `Scripts`, create **both** `Coin` and `GameManager` *before* typing. They refer to each other, so Unity shows errors until both are typed and saved. Type `Coin`, then `GameManager`, save both, and switch back to Unity.
2. **Coin prefab:** drag **`Art/Kenney/coin-gold`** into the scene, rename it `Coin`, and drag **`Materials/CoinYellow`** onto it. Set **Tag → Coin**. **Add Component > Sphere Collider**: **Is Trigger** ✓, Center **(0, 0.5, 0)**, Radius **0.6**. Add **Coin**. Drag it into `Prefabs` (**Original Prefab**).
3. Place 4 or 5 coins around your course (Ctrl/Cmd + D, then move). Put a few somewhere risky.
4. **Hierarchy > Create Empty**, name it `GameManager`, and **Add Component > GameManager**.
5. Expand **Canvas**, **tick `ScoreText`** to enable it, and drag it into GameManager's **Score Text**.
6. **Play:** "Coins: 0 / 5" in the corner. Collect them all and the **Win** scene loads. Click **Play Again**.
7. **Goal instead of coins:** create **`GoalTrigger`** (below). Right-click **Level > GoalPlatform** > **Create Empty**, name it `GoalZone`. Add a **Box Collider**: **Is Trigger** ✓, Center **(0, 3.25, 0)**, Size **(5.5, 2.5, 5.5)**. Add **GoalTrigger**. Reaching the goal now wins too.

<!-- lesson-script: step-7-goal Assets/_Workshop/Scripts/Coin.cs -->
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

<!-- lesson-script: step-7-goal Assets/_Workshop/Scripts/GameManager.cs -->
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

<!-- lesson-script: step-7-goal Assets/_Workshop/Scripts/GoalTrigger.cs -->
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
- `public` methods (like `AddCoin`) can be called by other scripts. `private` ones can't.
- `FindObjectsByType<Coin>` counts the coins once, at the start. Place as many as you like and the total updates itself.
- `Destroy(gameObject)` removes the coin. `LoadScene("Win")` only works because Win is listed in **File > Build Profiles** (the starter already did this).
- **Likely accidental failure:** a `NullReferenceException` pointing at `UpdateScoreText` means **Score Text isn't assigned**. One pointing at `Coin.OnTriggerEnter` means there's **no GameManager in the scene**.

---

## 8 · Remix #2 + wrap-up (5 min, 1:10–1:15) ✂️ shorten if late

- "Add at least one trap, one moving platform and three coins to your course. Then swap laptops with a neighbor and try theirs."
- Show `Final - Demo Course` again and point out the level-design rules from the [tuned values](#tuned-values-why-the-player-feels-the-way-it-does).
- **Part 2 teaser:** the Kenney character (`Art/Kenney/character-oobi`) is already imported with its animations. Next time we swap the capsule for it and wire up an Animator.
- Fast finishers: the bonus challenges below. They're in `Scripts/Bonus/` in the `final` checkpoint.

---

## Bonus challenges (for fast finishers)

Each one is a tiny, standalone script, and they're all in `Assets/_Workshop/Scripts/Bonus/` in the `final` checkpoint. The demo course uses the bounce pad. Paste them into Discord for anyone who wants them.

| Script | One-liner | How to use |
|---|---|---|
| `DoubleJump` | Extra jumps in mid-air. | Add it **next to** PlayerController on the Player. Set its Ground Layer to Ground. `airJumps` = 1 gives a double jump. |
| `BouncePad` | Launches the player upward on contact. | Drag `Art/KayKit/spring_pad_blue` in, add a Box Collider (not a trigger) and BouncePad. |
| `Checkpoint` + `PlayerRespawn` + `RespawnOnTouch` | Respawn at the last checkpoint instead of restarting the level (you keep your coins). | Add `PlayerRespawn` to the Player. Make a trigger with `Checkpoint`. On traps and the FallZone, **replace** KillOnTouch with `RespawnOnTouch`. |
| Spinning saw | A hazard built from scripts you already have. | `Art/Kenney/saw` + convex Mesh Collider (trigger) + KillOnTouch + Spinner with Axis (0, 0, 1). |

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
| Someone is lost or their scene is broken | Open `_Workshop > Checkpoints` and double-click the last finished step's scene. Keep building there. |
| "Can't add script" / "The script class cannot be found" | The file name and class name don't match (`playerController.cs` vs `PlayerController`), or there's a compile error. Check the Console for red errors first. |
| Red errors after typing | Usually a missing `;` or `}`, or a typo in a name. Double-click the error to jump to the line. Nothing plays until every error is fixed. |
| Changes to the script don't show up | Not saved. The editor tab shows a dot or asterisk. Save, then click back into Unity. |
| Player floats in the air | It has no Rigidbody (Step 1). |
| Player falls through a part | The part has no collider (Step 2). |
| Player can't jump | Ground Layer isn't set on PlayerController, or the part isn't on the Ground layer. Watch the debug ray. |
| Player tips over | Freeze Rotation X and Z on the Rigidbody. |
| Player sticks to walls | NoFriction isn't on the Capsule Collider. |
| Camera doesn't follow | Follow Camera > Tracking Target isn't set to Player. |
| Trap or coin does nothing | Is Trigger isn't ticked, the Player isn't tagged `Player`, or the Player has lost its Rigidbody. |
| Slides off the moving platform | StickyZone is missing or not a trigger, the waypoints are children of Platform, or the Player's Rigidbody Interpolate isn't None. |
| NullReferenceException in GameManager or Coin | Score Text isn't assigned, or there's no GameManager in the scene. |
| Win scene doesn't load | It's not in File > Build Profiles, or the name in the Inspector doesn't match (`Win`). |
| All my work vanished | It was built in Play mode. See Step 1, item 9. |
| Magenta/pink materials, or a "different Unity version" dialog | Wrong Unity version. It must be 6000.3.8f1 (see README). |

---

## Instructor tooling (not for attendees)

Everything instructor-only lives in `UnityProject/Assets/_Instructor/` and `Tools/`. Both are left out of the attendee zips.

- **Unity menu: Workshop**
  - **Build Checkpoint > Step N**: rebuilds that checkpoint's Arena scene and prefabs from the previous one. Safe to re-run. Add that step's scripts first (they come from this guide). **Step 0** also regenerates every checkpoint scene.
  - **Validate Project**: checks the project against its checkpoint (colliders, layers, tags, wiring, missing scripts), and checks every checkpoint scene.
  - **Render Doc Screenshots**: regenerates `Docs/images/` from `final`.
- **`Tools/build-checkpoints.sh`**: rebuilds every checkpoint from scratch in order. For each one it extracts scripts from this guide, runs the builder and the validator, runs the PlayMode tests, then commits and tags. Use `--from <tag>` to resume, and `--no-tests` / `--no-commit` as needed. Close Unity first.
- **`Tools/run-tests.sh [name] [scenes]`**: runs the PlayMode tests on the current checkout (scenes: `Arena` by default, or `all` for the Arena plus every checkpoint scene). They move the player with a virtual gamepad (physics drop, run speed at 30/60/144 fps, jump height, no double jump, ramp climb, traps, fall zone, moving platform, coins, goal, Play Again). With the demo course, a bot plays it from start to Win.
- **`Tools/extract-lesson-scripts.sh <tag> [--check]`**: writes or checks the scripts from this guide. `--checkpoint-copies` writes the namespaced copies the checkpoint scenes use.
- **`Tools/make-zips.sh`**: one zip per tag in `dist/`, made with `git archive` so no Library or local junk gets in.
