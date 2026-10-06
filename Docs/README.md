# Unity Basics Workshop: Build an Obstacle Course

VGDA @ CSULB · hands-on, 60 minutes · no experience needed

In one hour you'll build a 3D platformer obstacle course: a player you control, platforms, ramps, traps, moving platforms, coins, and a finish line.

![The finished demo course](images/demo-course-overview.png)

> **Please do the [night-before checklist](#night-before-checklist) before the workshop.** The first time Unity opens the project it takes several minutes, and there's no time for installs during the session.

---

## 1. Install the exact Unity version

| | |
|---|---|
| **Unity version** | **6000.3.8f1** (Unity 6.3 LTS) |
| **Hub link** | [Unity 6000.3.8f1 release page](https://unity.com/releases/editor/whats-new/6000.3.8f1): its Unity Hub install link opens Hub straight to this version. Or paste `unityhub://6000.3.8f1/1c7db571dde0` into your browser. |
| **No Hub link?** | [Unity download archive](https://unity.com/releases/editor/archive) → Unity 6 → **6000.3.8f1** → *Install* |

It has to be **exactly 6000.3.8f1**. A different version will want to "upgrade" the project and things can break.

1. Install **[Unity Hub](https://unity.com/download)** and sign in. The download page asks you to create or sign in to a free Unity account first; a free Unity Personal license is fine.
2. Click the Hub link above, or go to **Installs > Install Editor > Archive** and find 6000.3.8f1.
3. Modules: you don't need any build modules. If you don't already have a code editor, tick **Microsoft Visual Studio Community** (Windows), or see [Code editor setup](#code-editor-setup).
4. On an Apple Silicon Mac, pick the **Apple silicon** editor.

## 2. Get the project

Download **`unity-workshop-step-0-starter.zip`** from the **[Releases page](https://github.com/DeltaTheM2/VGDA-Unity-Workshop-1/releases)** (or from the link in the Discord thread) and unzip it somewhere easy to find.

- **On Windows, keep the path short**, for example `C:\UnityWorkshop\`. Unity's package files are deeply nested, and Windows has a 260-character path limit.
- Avoid OneDrive/iCloud-synced folders if you can, because they slow Unity down.

Using git instead? Clone it, then switch to the starter (a fresh clone is the finished project):

```bash
git clone https://github.com/DeltaTheM2/VGDA-Unity-Workshop-1.git unity-workshop
```

```bash
git -C unity-workshop checkout step-0-starter
```

## 3. Open it with Unity Hub

1. Unity Hub > **Projects > Add > Add project from disk**.
2. Select the **`UnityProject`** folder, the one that contains `Assets`, `Packages` and `ProjectSettings`. Don't pick the outer folder.
3. Make sure the version column says **6000.3.8f1**, then click the project to open it.

---

## Night-before checklist

1. **Open the project.** The first import takes a while (5–15 minutes on most laptops). Let it finish.
2. **Press Play** (▶ at the top) and **see the arena**: a green start platform floating in a blue sky. Press ▶ again to stop.
   *Only seeing an empty scene?* In the **Project** window, open `Assets/_Workshop/Scenes/` and double-click **Arena**.
3. **Check your code editor.** In the Project window, open `Assets/_Workshop/Scripts/` and **double-click `HelloWorld.cs`**. Your code editor should open it. Inside `Start()`, type `Debug.` and check that **autocomplete suggestions** pop up (like `Log`). Then delete what you typed and close the file without saving.
4. **Attach it and run it.** Drag `HelloWorld` from the Project window onto **Main Camera** in the Hierarchy, then press **Play**. The **Console** (Window > General > Console) should show **`Hello from Unity!`**. Stop Play mode, then remove the component (click the ⋮ on the Hello World component in the Inspector > **Remove Component**).
5. **Reply ✅ in the Discord thread.** Stuck on a step? Post a screenshot there instead. We'll sort it out before the session.

---

## Code editor setup

Any of these works. Pick one and point Unity at it: **Edit > Preferences > External Tools > External Script Editor** (on a Mac: **Unity > Settings > External Tools**).

### VS Code (Windows / Mac / Linux)

1. Install [VS Code](https://code.visualstudio.com/) and the **[Unity extension by Microsoft](https://marketplace.visualstudio.com/items?itemName=visualstudiotoolsforunity.vstuc)**. It pulls in *C#* and *C# Dev Kit* automatically.
2. Install the **[.NET SDK](https://dotnet.microsoft.com/download)** (the current LTS). C# Dev Kit needs it for autocomplete.
3. In Unity, set **External Script Editor → Visual Studio Code** and click **Regenerate project files**.
4. Always open scripts **by double-clicking them in Unity**. That opens the whole project in VS Code, which autocomplete needs.

**No autocomplete in VS Code?**

- Wait a moment after VS Code opens. The status bar shows the C# projects loading.
- Check that VS Code opened the **`UnityProject`** folder (the Explorer sidebar should show `Assets`), not just a single file.
- "The .NET SDK could not be found": install the .NET SDK, then restart VS Code *and* Unity.
- Open **View > Output**, pick **C# Dev Kit** in the dropdown, and look for errors. Then, in Unity, click **Regenerate project files** again.
- You don't need a C# Dev Kit account or sign-in to use it for this workshop.

### JetBrains Rider (Windows / Mac / Linux)

1. Install [Rider](https://www.jetbrains.com/rider/). It's free for non-commercial use, so pick the non-commercial license when asked.
2. In Unity, set **External Script Editor → Rider** (the Rider package is already in the project).
3. **No autocomplete?** Make sure you opened the file from Unity (double-click), and wait for Rider to finish indexing (progress bar at the bottom). If Rider isn't in the list, click **Browse…** and pick the Rider executable.

### Visual Studio 2022 (Windows)

Install it with the **Game development with Unity** workload (the Unity Hub module does this for you). Then set **External Script Editor → Visual Studio 2022**.

---

## Other problems

| Problem | Fix |
|---|---|
| Hub says the project needs a different editor version | Install **6000.3.8f1** exactly (see step 1). Don't click "upgrade". |
| Unity opens in **Safe Mode** | There's a compile error, usually from editing `HelloWorld.cs`. Undo the change, or re-download the zip. |
| Everything is **magenta/pink** | The wrong Unity version or a broken import. Close Unity, delete the `Library` folder inside `UnityProject`, and reopen with 6000.3.8f1. |
| The first import seems stuck | Give it time. If it's still going after 30 minutes, restart the computer and reopen. |
| Red errors like `DirectoryNotFoundException` / "Could not find a part of the path … Library\PackageCache …" (Windows) | The folder path is too long. Close Unity, move the project to a short path like `C:\UnityWorkshop\`, delete its `Library` folder, and reopen. |
| It asks you to sign in or activate a license | Sign in to Unity Hub with a free Unity account. Unity Personal is fine. |

## Controls

| | Keyboard | Gamepad |
|---|---|---|
| Move | WASD / arrow keys | Left stick |
| Jump | Space | South button (A / ✕) |

## Checkpoints

Fell behind during the workshop? In the Project window, open **`_Workshop > Checkpoints`** and double-click the scene for the step we just finished. It's a complete, working copy, already inside your project. More options (including full-project zips for after the workshop) are in **[CHECKPOINTS.md](CHECKPOINTS.md)**.

## Credits

- 3D models: **[KayKit Platformer Pack](https://kaylousberg.itch.io/kaykit-platformer)** by Kay Lousberg (CC0) and **[Kenney Platformer Kit](https://kenney.nl/assets/platformer-kit)** by Kenney (CC0). Their licenses are in `Assets/_Workshop/Art/`.
- Made with Unity 6.3 LTS, the Universal Render Pipeline, the Input System, and Cinemachine.
