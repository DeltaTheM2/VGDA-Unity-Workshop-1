using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Workshop.Instructor
{
    // Checks that the project matches what a checkpoint should contain. Used after every
    // automated build, and from Workshop > Validate Project.
    public static class WorkshopValidator
    {
        // Levels are positions in WorkshopMenu.Checkpoints.
        const int Basics = 1, Parts = 2, Scripting = 3, Player = 4, Hazards = 5, Goal = 6, Final = 7;

        static readonly (int level, string[] scripts)[] ScriptsByLevel =
        {
            (0, new[] { "HelloWorld" }),
            (Scripting, new[] { "Spinner", "LifecycleLogger" }),
            (Player, new[] { "PlayerController" }),
            (Hazards, new[] { "Patrol", "StickyPlatform", "KillOnTouch" }),
            (Goal, new[] { "Coin", "GameManager", "GoalTrigger" }),
            (Final, new[] { "Bonus/DoubleJump", "Bonus/BouncePad", "Bonus/PlayerRespawn", "Bonus/Checkpoint", "Bonus/RespawnOnTouch" }),
        };

        static List<string> errors;

        static int Level(string tag) => Array.FindIndex(WorkshopMenu.Checkpoints, c => c.tag == tag);

        public static string DetectCheckpoint()
        {
            bool Has(string path) => File.Exists(path);
            if (Has($"{WorkshopBuilder.PrefabsDir}/SpinningSaw.prefab")) return "final";
            if (Has($"{WorkshopBuilder.PrefabsDir}/Coin.prefab")) return "step-7-goal";
            if (Has($"{WorkshopBuilder.PrefabsDir}/MovingPlatform.prefab")) return "step-6-hazards";
            if (Has($"{WorkshopBuilder.PhysicsMaterialsDir}/NoFriction.physicsMaterial")) return "step-4-player";
            if (Has($"{WorkshopBuilder.ScriptsDir}/LifecycleLogger.cs")) return "step-3-scripting";
            if (Has($"{WorkshopBuilder.PrefabsDir}/Block.prefab")) return "step-2-parts";
            var arena = File.ReadAllText(WorkshopBuilder.ArenaScenePath);
            return arena.Contains("m_Name: Player\n") ? "step-1-basics" : "step-0-starter";
        }

        public static bool Validate(string tag)
        {
            int level = Level(tag);
            if (level < 0)
                throw new ArgumentException($"Unknown checkpoint '{tag}'");
            errors = new List<string>();

            CheckProjectSettings();
            CheckScripts(level);
            CheckPrefabs(level);
            CheckScene(WorkshopBuilder.ArenaScenePath, level, isCheckpoint: false);
            CheckCheckpointScenes();
            CheckWinScene();

            foreach (var error in errors)
                Debug.LogError($"[Workshop] {tag}: {error}");
            if (errors.Count == 0)
                Debug.Log($"[Workshop] {tag}: all checks passed.");
            if (!Application.isBatchMode)
                EditorSceneManager.OpenScene(WorkshopBuilder.ArenaScenePath);
            return errors.Count == 0;
        }

        static void Expect(bool condition, string message)
        {
            if (!condition)
                errors.Add(message);
        }

        static bool HasScript(GameObject go, string className) =>
            go != null && go.GetComponents<MonoBehaviour>().Any(m => m != null && m.GetType().Name == className);

        static Component Script(GameObject go, string className) =>
            go == null ? null : go.GetComponents<MonoBehaviour>().FirstOrDefault(m => m != null && m.GetType().Name == className);

        static SerializedProperty Field(Component component, string name) =>
            component == null ? null : new SerializedObject(component).FindProperty(name);

        static void CheckProjectSettings()
        {
            var expectedScenes = new[] { WorkshopBuilder.ArenaScenePath, WorkshopBuilder.WinScenePath }.Concat(WorkshopBuilder.CheckpointScenePaths);
            Expect(EditorBuildSettings.scenes.Select(s => s.path).SequenceEqual(expectedScenes),
                "Build scenes should be Arena (0), Win (1), then every checkpoint scene in order.");
            Expect(UnityEditorInternal.InternalEditorUtility.tags.Contains(WorkshopBuilder.CoinTag), "Missing the Coin tag.");
            Expect(LayerMask.NameToLayer(WorkshopBuilder.GroundLayerName) == WorkshopBuilder.GroundLayer, "Ground should be layer 6.");
            Expect(Physics.gravity == WorkshopBuilder.Gravity, $"Gravity should be {WorkshopBuilder.Gravity}, found {Physics.gravity}.");
            var playerSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            Expect(playerSettings.FindProperty("templateDefaultScene").stringValue == WorkshopBuilder.ArenaScenePath,
                "The first-launch scene (templateDefaultScene) should be the Arena.");

            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { WorkshopBuilder.KayKitDir, WorkshopBuilder.KenneyDir }))
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                Expect(!importer.addCollider, $"{importer.assetPath} should import WITHOUT colliders.");
            }
        }

        static void CheckScripts(int level)
        {
            var expected = ScriptsByLevel.Where(s => s.level <= level).SelectMany(s => s.scripts)
                .Select(s => $"{WorkshopBuilder.ScriptsDir}/{s}.cs").OrderBy(s => s).ToArray();
            var actual = Directory.GetFiles(WorkshopBuilder.ScriptsDir, "*.cs", SearchOption.AllDirectories)
                .Select(p => p.Replace('\\', '/')).OrderBy(s => s).ToArray();
            Expect(expected.SequenceEqual(actual),
                $"Scripts folder mismatch. Expected [{string.Join(", ", expected.Select(Path.GetFileName))}], found [{string.Join(", ", actual.Select(Path.GetFileName))}].");
        }

        static void CheckPrefabs(int level)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { WorkshopBuilder.Root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                int missing = prefab.GetComponentsInChildren<Transform>(true)
                    .Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
                Expect(missing == 0, $"{path} has {missing} missing script(s).");
            }

            Expect(AssetDatabase.FindAssets("t:Prefab", new[] { WorkshopBuilder.BackupDir }).Length >= 8, "_Backup should hold at least 8 parts.");
            var prefabCount = AssetDatabase.FindAssets("t:Prefab", new[] { WorkshopBuilder.PrefabsDir }).Length;
            if (level < Parts)
                Expect(prefabCount == 0, "Prefabs/ should be empty before Step 2.");
            if (level < Player)
                Expect(!File.Exists($"{WorkshopBuilder.PhysicsMaterialsDir}/NoFriction.physicsMaterial"), "PhysicsMaterials/ should be empty before Step 4.");
            if (level >= Parts)
                CheckPrefabSet(WorkshopBuilder.PrefabsDir, level);
        }

        // The prefabs a scene at this level relies on (Prefabs/ for the Arena, or a checkpoint's own folder).
        static void CheckPrefabSet(string folder, int level)
        {
            var block = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/Block.prefab");
            var ramp = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/Ramp.prefab");
            var trap = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/Trap.prefab");
            Expect(block != null && block.GetComponent<BoxCollider>() != null && block.layer == WorkshopBuilder.GroundLayer, $"{folder}/Block needs a BoxCollider and the Ground layer.");
            Expect(ramp != null && ramp.GetComponent<MeshCollider>() != null && ramp.layer == WorkshopBuilder.GroundLayer, $"{folder}/Ramp needs a MeshCollider and the Ground layer.");
            Expect(trap != null, $"Missing {folder}/Trap.");
            if (trap != null && level < Hazards)
                Expect(trap.GetComponent<Collider>() == null && !HasScript(trap, "KillOnTouch"), $"{folder}/Trap is only a placeholder before Step 6 (no collider, no script).");
            if (level < Hazards)
                return;

            var trapTrigger = trap != null ? trap.GetComponent<BoxCollider>() : null;
            Expect(trapTrigger != null && trapTrigger.isTrigger && HasScript(trap, "KillOnTouch"), $"{folder}/Trap needs a trigger BoxCollider and KillOnTouch.");

            var mover = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/MovingPlatform.prefab");
            var platform = mover != null ? mover.transform.Find("Platform") : null;
            var waypoints = Field(Script(platform != null ? platform.gameObject : null, "Patrol"), "waypoints");
            Expect(waypoints != null && waypoints.arraySize == 2, $"{folder}/MovingPlatform/Platform needs Patrol with 2 waypoints.");
            if (waypoints != null && platform != null)
                for (int i = 0; i < waypoints.arraySize; i++)
                {
                    var waypoint = waypoints.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                    Expect(waypoint != null && !waypoint.IsChildOf(platform), "Patrol waypoints must exist and must not be children of the moving Platform.");
                }
            var sticky = platform != null ? platform.Find("StickyZone") : null;
            Expect(sticky != null && sticky.GetComponent<BoxCollider>().isTrigger && HasScript(sticky.gameObject, "StickyPlatform")
                   && sticky.gameObject.layer != WorkshopBuilder.GroundLayer, "Platform/StickyZone needs a trigger, StickyPlatform, and a non-Ground layer.");
            Expect(platform != null && platform.gameObject.layer == WorkshopBuilder.GroundLayer, "The moving Platform should be on the Ground layer.");
            if (level < Goal)
                return;

            var coin = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/Coin.prefab");
            var coinTrigger = coin != null ? coin.GetComponent<Collider>() : null;
            Expect(coinTrigger != null && coinTrigger.isTrigger && HasScript(coin, "Coin") && coin.CompareTag(WorkshopBuilder.CoinTag),
                $"{folder}/Coin needs a trigger collider, the Coin script, and the Coin tag.");
        }

        static void CheckCheckpointScenes()
        {
            var checkpoints = WorkshopBuilder.CheckpointSteps;
            for (int i = 0; i < checkpoints.Length; i++)
            {
                string sceneName = checkpoints[i].sceneName;
                string path = WorkshopBuilder.CheckpointScenePath(sceneName);
                if (!File.Exists(path))
                {
                    errors.Add($"Missing checkpoint scene {path}.");
                    continue;
                }
                // Checkpoint i is built by steps 0..i, i.e. it matches the live level of that step.
                int level = Level(WorkshopMenu.Checkpoints.First(c => c.build == checkpoints[i].build).tag);
                CheckScene(path, level, isCheckpoint: true);
                if (level >= Parts)
                    CheckPrefabSet($"{WorkshopBuilder.CheckpointPartsDir}/{sceneName}", level);
            }
        }

        static void CheckScene(string scenePath, int level, bool isCheckpoint)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            string label = Path.GetFileNameWithoutExtension(scenePath);
            var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToArray();
            GameObject Find(string name) => all.FirstOrDefault(g => g.name == name);

            int missing = all.Sum(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount);
            Expect(missing == 0, $"{label} has {missing} missing script(s).");

            // Checkpoint scenes must never depend on the scripts attendees write themselves.
            var scriptPaths = all.SelectMany(g => g.GetComponents<MonoBehaviour>()).Where(m => m != null)
                .Select(m => AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(m))).Where(p => p.StartsWith(WorkshopBuilder.Root)).Distinct();
            foreach (var scriptPath in scriptPaths)
            {
                if (isCheckpoint)
                    Expect(scriptPath.StartsWith(WorkshopBuilder.CheckpointScriptsDir), $"{label} uses {scriptPath}; checkpoint scenes may only use Checkpoints/_Scripts.");
                else
                    Expect(!scriptPath.StartsWith(WorkshopBuilder.CheckpointsDir), $"{label} uses checkpoint script {scriptPath}.");
            }

            var mainCamera = Find("Main Camera");
            Expect(mainCamera != null && mainCamera.GetComponent<CinemachineBrain>() != null, $"{label}: Main Camera needs a CinemachineBrain.");
            var followCamera = Find("Follow Camera")?.GetComponent<CinemachineCamera>();
            Expect(followCamera != null, $"{label}: missing the Follow Camera (CinemachineCamera).");
            Expect(Find("PlayerSpawn") != null, $"{label}: missing PlayerSpawn.");
            foreach (var platformName in new[] { "StartPlatform", "GoalPlatform" })
            {
                var platform = Find(platformName);
                Expect(platform != null && platform.layer == WorkshopBuilder.GroundLayer && platform.GetComponent<Collider>() != null,
                    $"{label}: {platformName} needs a collider and the Ground layer.");
            }
            var fallZone = Find("FallZone");
            Expect(fallZone != null && fallZone.GetComponent<BoxCollider>().isTrigger, $"{label}: FallZone needs a trigger BoxCollider.");
            Expect(HasScript(fallZone, "KillOnTouch") == level >= Hazards, $"{label}: FallZone should have KillOnTouch from Step 6 on, and not before.");
            var scoreText = Find("ScoreText");
            Expect(scoreText != null && scoreText.GetComponent<TMPro.TMP_Text>() != null, $"{label}: missing Canvas/ScoreText (TMP).");
            Expect(scoreText != null && scoreText.activeSelf == level >= Goal, $"{label}: ScoreText should be enabled from Step 7 on, and disabled before.");

            var scriptDemo = Find("ScriptDemo");
            Expect((scriptDemo != null) == (level == Scripting), $"{label}: the ScriptDemo cube belongs to Step 3 only.");
            if (level == Scripting)
                Expect(HasScript(scriptDemo, "Spinner") && HasScript(scriptDemo, "LifecycleLogger"), $"{label}: ScriptDemo needs Spinner and LifecycleLogger.");

            var player = Find("Player");
            Expect((player != null) == (level >= Basics), $"{label}: the Player exists from Step 1 on, and not before.");
            if (player != null)
            {
                Expect(player.CompareTag(WorkshopBuilder.PlayerTag), $"{label}: Player must be tagged Player.");
                var body = player.GetComponent<Rigidbody>();
                Expect(body != null && body.interpolation == RigidbodyInterpolation.None, $"{label}: Player needs a Rigidbody with Interpolate: None.");
                Expect(followCamera != null && followCamera.Follow == player.transform, $"{label}: Follow Camera should track the Player.");
                bool frozen = body != null && (body.constraints & RigidbodyConstraints.FreezeRotationX) != 0 && (body.constraints & RigidbodyConstraints.FreezeRotationZ) != 0;
                var capsule = player.GetComponent<CapsuleCollider>();
                bool noFriction = capsule != null && capsule.sharedMaterial != null && capsule.sharedMaterial.name == "NoFriction";
                var groundLayer = Field(Script(player, "PlayerController"), "groundLayer");
                if (level >= Player)
                {
                    Expect(frozen, $"{label}: Player rotation must be frozen on X and Z.");
                    Expect(noFriction, $"{label}: Player's Capsule Collider needs the NoFriction material.");
                    Expect(groundLayer != null && groundLayer.intValue == 1 << WorkshopBuilder.GroundLayer, $"{label}: PlayerController's Ground Layer must be Ground.");
                }
                else
                {
                    Expect(!frozen && !noFriction && groundLayer == null, $"{label}: before Step 4 the Player is just components (no controller, constraints or NoFriction).");
                }
            }

            bool hasSampleBlock = scene.GetRootGameObjects().Any(r => r.name == "Block");
            Expect(hasSampleBlock == (level >= Parts && level < Final), $"{label}: the sample Block/Ramp/Trap belong to Steps 2-7.");
            if (level >= Goal)
            {
                var scoreField = Field(Script(Find("GameManager"), "GameManager"), "scoreText");
                Expect(scoreField != null && scoreField.objectReferenceValue != null, $"{label}: GameManager needs its Score Text assigned.");
                Expect(all.Count(g => HasScript(g, "Coin")) >= 3, $"{label}: expected coins.");
                var goalZone = Find("GoalZone");
                Expect(goalZone != null && goalZone.GetComponent<Collider>().isTrigger && HasScript(goalZone, "GoalTrigger"), $"{label}: GoalPlatform needs a GoalZone trigger with GoalTrigger.");
            }
            if (level >= Final)
            {
                Expect(Find("DemoCourse") != null, $"{label}: should contain the DemoCourse.");
                Expect(all.Any(g => HasScript(g, "BouncePad")) && all.Any(g => HasScript(g, "Spinner")), $"{label}: demo course should use BouncePad and Spinner.");
            }
        }

        static void CheckWinScene()
        {
            var scene = EditorSceneManager.OpenScene(WorkshopBuilder.WinScenePath, OpenSceneMode.Single);
            var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToArray();
            var button = all.FirstOrDefault(g => g.name == "PlayAgainButton")?.GetComponent<Button>();
            Expect(button != null && button.onClick.GetPersistentEventCount() == 1 && button.onClick.GetPersistentMethodName(0) == "PlayAgain",
                "Win scene's Play Again button should call PlayAgainButton.PlayAgain.");
            Expect(all.Any(g => g.GetComponent<InputSystemUIInputModule>() != null), "Win scene needs an EventSystem with InputSystemUIInputModule.");
            Expect(all.Any(g => g.name == "YouWinText"), "Win scene needs the You Win! text.");
        }
    }
}
