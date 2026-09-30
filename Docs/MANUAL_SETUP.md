# Manual Setup

Almost everything in this repo is built by script: the Unity builder in `UnityProject/Assets/_Instructor/Editor/` plus `Tools/*.sh`. This page lists **everything that is not**, as click-by-click checklists: one-time machine setup, how the raw project and art were brought in, publishing, and prep on the day.

All the `Tools/` scripts are bash. On Windows, run them in **Git Bash** from the repository root, and close the Unity Editor first.

---

## A. One-time instructor machine setup

1. Install **Unity Hub**, then **Unity 6000.3.8f1** (Hub link: `unityhub://6000.3.8f1/1c7db571dde0`). No extra modules are needed.
2. Install **git** (on Windows, Git for Windows includes Git Bash). Optional: the **GitHub CLI** (`gh`) for publishing releases.
3. The tools look for Unity at the Hub's default path. If yours lives elsewhere, set it first, for example:

   ```bash
   export UNITY="/d/Unity/6000.3.8f1/Editor/Unity.exe"
   ```

---

## B. How this repo was created (only needed to redo it from nothing)

The builder takes over after these steps. You don't need them to rebuild checkpoints; they're here for the record.

### B1. Unity project from the Universal 3D template

This is equivalent to Unity Hub > New project > **Universal 3D**, named `UnityProject`:

```bash
"/c/Program Files/Unity/Hub/Editor/6000.3.8f1/Editor/Unity.exe" -batchmode -quit -createProject "UnityProject" -cloneFromTemplate "C:/Program Files/Unity/Hub/Editor/6000.3.8f1/Editor/Data/Resources/PackageManager/ProjectTemplates/com.unity.template.3d-cross-platform-17.0.14.tgz"
```

Then, by hand:

1. Delete `Assets/Readme.asset`, `Assets/TutorialInfo/`, `Assets/Scenes/` (SampleScene) and `Assets/Settings/SampleSceneProfile.asset`.
2. In `Packages/manifest.json`, **remove** `com.unity.ai.navigation`, `com.unity.collab-proxy`, `com.unity.multiplayer.center`, `com.unity.timeline` and `com.unity.visualscripting` (less to import on attendee laptops). **Add** `"com.unity.cinemachine": "3.1.7"`.
3. Leave the template's `Assets/InputSystem_Actions.inputactions` as it is. It's already registered as the **project-wide actions** (Project Settings > Input System Package), and Active Input Handling is already **Input System Package (New)**.

### B2. Art packs

1. **KayKit Platformer Pack (Free, 22 MB):** https://kaylousberg.itch.io/kaykit-platformer > **Download Now** > **No thanks, just take me to the downloads** > download `KayKit_Platformer_Pack_1.0_FREE.zip`.
2. **Kenney Platformer Kit:** https://kenney.nl/assets/platformer-kit > **Download** > *Continue without donating* > `kenney_platformer-kit.zip`.
3. Copy this curated subset (fewer files = faster first import and less to wade through for beginners):
   - Into `Assets/_Workshop/Art/KayKit/`, from `KayKit_Platformer_Pack_1.0_FREE/Assets/fbx(unity)/`:
     - `blue/`: `platform_1x1x1`, `platform_2x2x1`, `platform_2x2x2`, `platform_4x2x1`, `platform_4x4x1`, `platform_4x4x2`, `platform_6x2x1`, `platform_6x6x1`, `platform_arrow_2x2x1`, `platform_slope_2x2x2`, `platform_slope_2x6x4`, `platform_slope_4x6x4`, `pipe_straight_A`, `pipe_90_A`, `pipe_end`, `arch`, `arch_wide`, `barrier_1x1x1`, `barrier_2x1x1`, `barrier_4x1x1`, `railing_straight_single`, `flag_A`, `spring_pad`, `star` (each `*_blue.fbx`)
     - `yellow/`: `platform_2x2x1_yellow`, `platform_4x4x1_yellow` (moving platforms)
     - `green/`: `platform_2x2x1_green`, `platform_6x6x1_green`, `platform_6x6x2_green` (start and goal)
     - `red/`: `platform_2x2x1_red`, `barrier_2x1x1_red`
     - `neutral/`: `signage_finish`, `pillar_2x2x4`, `pillar_2x2x8`, `floor_wood_2x2`
     - plus `blue/platformer_texture.png` (every color uses the same texture) and the pack's `License.txt`
   - Into `Assets/_Workshop/Art/Kenney/`, from `Models/FBX format/`: `character-oobi.fbx`, `coin-gold.fbx`, `spike-block.fbx`, `saw.fbx`, `flag.fbx`, plus `Textures/colormap.png` (keep it in a `Textures/` subfolder) and the kit's `License.txt`.
4. Don't set any import settings by hand. **Workshop > Build Checkpoint > Step 0** sets them: no colliders, no animation on props, and Kenney scaled to match KayKit's 1 m grid (coin 1 m, spike block 2 m, character 2 m tall).

### B3. Everything else

Everything else (materials, `_Backup` prefabs, both scenes, lighting, tags and layers, gravity, build scenes, TMP Essentials, every checkpoint) comes from:

```bash
Tools/build-checkpoints.sh
```

---

## C. Publishing: repo, tags and zips

1. Create an **empty** GitHub repository (no README), then push the branch and all the checkpoint tags:

   ```bash
   git remote add origin https://github.com/<you>/<repo>.git
   ```

   ```bash
   git push -u origin main --tags
   ```

   If you rebuilt checkpoints later (the tags move), push them with `git push --force --tags`.
2. Build the zips (one per tag, instructor files left out automatically):

   ```bash
   Tools/make-zips.sh
   ```

3. Create a release and attach every zip. Either use GitHub's web UI (Releases > Draft a new release > tag `final` > drag in `dist/*.zip`), or:

   ```bash
   gh release create workshop-2026 dist/*.zip --title "Unity Workshop checkpoints" --notes "Download unity-workshop-step-0-starter.zip before the workshop. See Docs/README.md."
   ```

4. Pin the Releases link and the night-before checklist ([README.md](README.md#night-before-checklist)) in the Discord thread, and ask for ✅ replies.

---

## D. On the day (can't be scripted: these are editor preferences on your machine)

1. **Playmode tint:** Edit > Preferences > **Colors > Playmode tint** → a loud color. It makes the "changes in Play mode get lost" moment land.
2. **Readable projector:** Edit > Preferences > **General > Editor Theme** (Light often projects better), and **UI Scaling** 125–150%. Zoom the code editor way in.
3. **Layout:** Scene and Game side by side, Console visible, Inspector wide enough for tooltips.
4. **Two projects open:** `final` (for the opening demo) and a **fresh** unzip of `step-0-starter` (for building live).
5. **Grid snapping:** attendee projects set it on first open (`Prebuilt/Editor/WorkshopProjectSetup.cs`: 1 m moves, 90° rotation steps, Global handles). Your rehearsal copy has it too. If snapping seems off, check the grid-snap toggle in the Scene view toolbar and that the handle is set to **Global**.

---

## E. Verifying a checkpoint by hand (after changing anything)

1. **Workshop > Validate Project**. Every check should pass.
2. Close Unity, then run `Tools/run-tests.sh`. All non-skipped tests should pass.
3. Open the checkpoint, press Play, and check the Console: no warnings or errors from our code.
