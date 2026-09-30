using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Workshop.Instructor
{
    // Checks that the project matches what a checkpoint should contain. Used after every
    // automated build, and from Workshop > Validate Project.
    public static class WorkshopValidator
    {
        static readonly (int level, string[] scripts)[] ScriptsByLevel =
        {
            (0, new[] { "HelloWorld" }),
            (1, new[] { "LifecycleLogger" }),
            (2, new[] { "PlayerController" }),
            (4, new[] { "Patrol", "StickyPlatform", "KillOnTouch" }),
            (5, new[] { "Coin", "GameManager", "GoalTrigger" }),
            (6, new[] { "Bonus/DoubleJump", "Bonus/BouncePad", "Bonus/Spinner", "Bonus/PlayerRespawn", "Bonus/Checkpoint", "Bonus/RespawnOnTouch" }),
        };

        static List<string> errors;

        static int Level(string tag) => Array.FindIndex(WorkshopMenu.Checkpoints, c => c.tag == tag);

        public static string DetectCheckpoint()
        {
            bool Has(string path) => File.Exists(path);
            if (Has($"{WorkshopBuilder.PrefabsDir}/SpinningSaw.prefab")) return "final";
            if (Has($"{WorkshopBuilder.PrefabsDir}/Coin.prefab")) return "step-6-goal";
            if (Has($"{WorkshopBuilder.PrefabsDir}/MovingPlatform.prefab")) return "step-5-hazards";
            if (Has($"{WorkshopBuilder.PrefabsDir}/Block.prefab")) return "step-3-prefabs";
            if (Has($"{WorkshopBuilder.PhysicsMaterialsDir}/NoFriction.physicsMaterial")) return "step-2-player";
            if (Has($"{WorkshopBuilder.ScriptsDir}/LifecycleLogger.cs")) return "step-1-lifecycle";
            return "step-0-starter";
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
            CheckArena(level);
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
            var scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
            Expect(scenes.Length == 2 && scenes[0] == WorkshopBuilder.ArenaScenePath && scenes[1] == WorkshopBuilder.WinScenePath,
                "Build scenes should be exactly Arena (0) and Win (1).");
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
            if (level < 3)
                Expect(prefabCount == 0, "Prefabs/ should be empty before Step 3.");
            if (level < 2)
                Expect(!File.Exists($"{WorkshopBuilder.PhysicsMaterialsDir}/NoFriction.physicsMaterial"), "PhysicsMaterials/ should be empty before Step 2.");
            if (level < 3)
                return;

            var block = AssetDatabase.LoadAssetAtPath<GameObject>($"{WorkshopBuilder.PrefabsDir}/Block.prefab");
            var ramp = AssetDatabase.LoadAssetAtPath<GameObject>($"{WorkshopBuilder.PrefabsDir}/Ramp.prefab");
            var trap = AssetDatabase.LoadAssetAtPath<GameObject>($"{WorkshopBuilder.PrefabsDir}/Trap.prefab");
            Expect(block != null && block.GetComponent<BoxCollider>() != null && block.layer == WorkshopBuilder.GroundLayer, "Block prefab needs a BoxCollider and the Ground layer.");
            Expect(ramp != null && ramp.GetComponent<MeshCollider>() != null && ramp.layer == WorkshopBuilder.GroundLayer, "Ramp prefab needs a MeshCollider and the Ground layer.");
            Expect(trap != null, "Missing Trap prefab.");
            if (trap != null && level < 4)
                Expect(trap.GetComponent<Collider>() == null && !HasScript(trap, "KillOnTouch"), "In Step 3 the Trap is only a placeholder (no collider, no script).");
            if (level < 4)
                return;

            var trapTrigger = trap != null ? trap.GetComponent<BoxCollider>() : null;
            Expect(trapTrigger != null && trapTrigger.isTrigger && HasScript(trap, "KillOnTouch"), "Trap prefab needs a trigger BoxCollider and KillOnTouch.");

            var mover = AssetDatabase.LoadAssetAtPath<GameObject>($"{WorkshopBuilder.PrefabsDir}/MovingPlatform.prefab");
            var platform = mover != null ? mover.transform.Find("Platform") : null;
            var patrol = Script(platform != null ? platform.gameObject : null, "Patrol");
            var waypoints = Field(patrol, "waypoints");
            Expect(waypoints != null && waypoints.arraySize == 2, "MovingPlatform/Platform needs Patrol with 2 waypoints.");
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
            if (level < 5)
                return;

            var coin = AssetDatabase.LoadAssetAtPath<GameObject>($"{WorkshopBuilder.PrefabsDir}/Coin.prefab");
            var coinTrigger = coin != null ? coin.GetComponent<Collider>() : null;
            Expect(coinTrigger != null && coinTrigger.isTrigger && HasScript(coin, "Coin") && coin.CompareTag(WorkshopBuilder.CoinTag),
                "Coin prefab needs a trigger collider, the Coin script, and the Coin tag.");
        }

        static void CheckArena(int level)
        {
            var scene = EditorSceneManager.OpenScene(WorkshopBuilder.ArenaScenePath, OpenSceneMode.Single);
            var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToArray();
            GameObject Find(string name) => all.FirstOrDefault(g => g.name == name);

            int missing = all.Sum(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount);
            Expect(missing == 0, $"Arena has {missing} missing script(s).");

            var mainCamera = Find("Main Camera");
            Expect(mainCamera != null && mainCamera.GetComponent<CinemachineBrain>() != null, "Main Camera needs a CinemachineBrain.");
            var followCamera = Find("Follow Camera")?.GetComponent<CinemachineCamera>();
            Expect(followCamera != null, "Missing the Follow Camera (CinemachineCamera).");
            Expect(Find("PlayerSpawn") != null, "Missing PlayerSpawn.");
            foreach (var platformName in new[] { "StartPlatform", "GoalPlatform" })
            {
                var platform = Find(platformName);
                Expect(platform != null && platform.layer == WorkshopBuilder.GroundLayer && platform.GetComponent<Collider>() != null,
                    $"{platformName} needs a collider and the Ground layer.");
            }
            var fallZone = Find("FallZone");
            Expect(fallZone != null && fallZone.GetComponent<BoxCollider>().isTrigger, "FallZone needs a trigger BoxCollider.");
            Expect(HasScript(fallZone, "KillOnTouch") == level >= 4, level >= 4 ? "FallZone needs KillOnTouch from Step 5 on." : "FallZone should not have a script yet.");
            var scoreText = Find("ScoreText");
            Expect(scoreText != null && scoreText.GetComponent<TMPro.TMP_Text>() != null, "Missing Canvas/ScoreText (TMP).");
            Expect(scoreText != null && scoreText.activeSelf == level >= 5, level >= 5 ? "ScoreText should be enabled from Step 6 on." : "ScoreText should start disabled.");

            Expect((Find("LifecycleDemo") != null) == (level == 1), level == 1 ? "Step 1 needs the LifecycleDemo cube." : "LifecycleDemo should only exist in Step 1.");
            if (level == 1)
                Expect(HasScript(Find("LifecycleDemo"), "LifecycleLogger"), "LifecycleDemo needs LifecycleLogger.");

            var player = Find("Player");
            Expect((player != null) == (level >= 2), level >= 2 ? "Missing the Player." : "No Player before Step 2.");
            if (level >= 2 && player != null)
            {
                Expect(player.CompareTag(WorkshopBuilder.PlayerTag), "Player must be tagged Player.");
                var body = player.GetComponent<Rigidbody>();
                Expect(body != null && (body.constraints & RigidbodyConstraints.FreezeRotationX) != 0 && (body.constraints & RigidbodyConstraints.FreezeRotationZ) != 0,
                    "Player needs a Rigidbody with rotation frozen on X and Z.");
                Expect(body != null && body.interpolation == RigidbodyInterpolation.None, "Player Rigidbody interpolation must stay None (it fights StickyPlatform's parenting).");
                var capsule = player.GetComponent<CapsuleCollider>();
                Expect(capsule != null && capsule.sharedMaterial != null && capsule.sharedMaterial.name == "NoFriction", "Player's CapsuleCollider needs the NoFriction material.");
                var groundLayer = Field(Script(player, "PlayerController"), "groundLayer");
                Expect(groundLayer != null && groundLayer.intValue == 1 << WorkshopBuilder.GroundLayer, "PlayerController's Ground Layer must be set to Ground.");
                Expect(followCamera != null && followCamera.Follow == player.transform, "Follow Camera should track the Player.");
            }

            if (level >= 5)
            {
                var gameManager = Find("GameManager");
                var scoreField = Field(Script(gameManager, "GameManager"), "scoreText");
                Expect(scoreField != null && scoreField.objectReferenceValue != null, "GameManager needs its Score Text assigned.");
                Expect(all.Count(g => HasScript(g, "Coin")) >= 3, "Expected coins in the Arena.");
                var goalZone = Find("GoalZone");
                Expect(goalZone != null && goalZone.GetComponent<Collider>().isTrigger && HasScript(goalZone, "GoalTrigger"), "GoalPlatform needs a GoalZone trigger with GoalTrigger.");
            }
            if (level >= 6)
            {
                Expect(Find("DemoCourse") != null, "Final should contain the DemoCourse.");
                Expect(all.Any(g => HasScript(g, "BouncePad")) && all.Any(g => HasScript(g, "Spinner")), "Demo course should show off BouncePad and Spinner.");
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
