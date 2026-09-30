using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace Workshop.Instructor
{
    public static partial class WorkshopBuilder
    {
        // Sample pieces the instructor drags in during Steps 3, 5 and 6.
        static readonly Vector3 SampleBlockPosition = new Vector3(0f, -1f, 4f);   // top at y = 0, touching the start platform
        static readonly Vector3 SampleRampPosition = new Vector3(0f, -1f, 8f);    // rotated 180: slope climbs from y = 0 to y = 3 going +Z
        static readonly Vector3 SampleTrapPosition = new Vector3(2f, 0f, -1.5f);  // on the start platform, beside the spawn
        static readonly Vector3 SampleMovingPlatformPosition = new Vector3(5f, -1f, 0f); // rides +X off the start platform's right side
        static readonly Vector3[] SampleCoinPositions =
        {
            new Vector3(0f, 0.3f, 4f),   // on the block
            new Vector3(0f, 2.3f, 8f),   // halfway up the ramp
            new Vector3(0f, 3.8f, 10f),  // near the top of the ramp
            new Vector3(9f, 0.3f, 0f),   // along the moving platform's path
            new Vector3(2f, 0.3f, 1.2f), // right next to the trap
        };

        // ================= Step 1: script lifecycle demo =================
        public static void BuildStep1Lifecycle()
        {
            var scene = OpenScene(ArenaScenePath);
            DestroyAllNamed(scene, "LifecycleDemo");
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "LifecycleDemo";
            cube.transform.position = new Vector3(2f, 0.5f, 2f);
            AddLessonScript(cube, "LifecycleLogger");
            SaveScene(scene);
            SaveAssetsAndLog("step-1-lifecycle");
        }

        // ================= Step 2: the player =================
        public static void BuildStep2Player()
        {
            var noFriction = CreateNoFrictionMaterial();

            var scene = OpenScene(ArenaScenePath);
            DestroyAllNamed(scene, "LifecycleDemo", "Player");

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = PlayerTag;
            player.transform.position = FindRequired(scene, "PlayerSpawn").transform.position;
            player.GetComponent<MeshRenderer>().sharedMaterial = WorkshopMaterial("PlayerBlue");
            player.GetComponent<CapsuleCollider>().sharedMaterial = noFriction;

            var body = player.AddComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var controller = AddLessonScript(player, "PlayerController");
            SetSerialized(controller, "groundLayer", p => p.intValue = 1 << GroundLayer);

            FindRequired(scene, "Follow Camera").GetComponent<CinemachineCamera>().Follow = player.transform;

            SaveScene(scene);
            SaveAssetsAndLog("step-2-player");
        }

        static PhysicsMaterial CreateNoFrictionMaterial()
        {
            string path = $"{PhysicsMaterialsDir}/NoFriction.physicsMaterial";
            var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
            bool isNew = material == null;
            if (isNew)
                material = new PhysicsMaterial("NoFriction");
            material.dynamicFriction = 0f;
            material.staticFriction = 0f;
            material.frictionCombine = PhysicsMaterialCombine.Minimum;
            material.bounciness = 0f;
            material.bounceCombine = PhysicsMaterialCombine.Minimum;
            if (isNew)
                AssetDatabase.CreateAsset(material, path);
            else
                EditorUtility.SetDirty(material);
            return material;
        }

        // ================= Step 3: prefabs built from raw models =================
        public static void BuildStep3Prefabs()
        {
            var block = CreatePartPrefab($"{PrefabsDir}/Block.prefab", "platform_2x2x1_blue", ColliderKind.Box, true, null);
            var ramp = CreatePartPrefab($"{PrefabsDir}/Ramp.prefab", "platform_slope_4x6x4_blue", ColliderKind.Mesh, true, null);
            var trap = CreatePartPrefab($"{PrefabsDir}/Trap.prefab", "Kenney/spike-block", ColliderKind.None, false, "HazardRed");

            var scene = OpenScene(ArenaScenePath);
            DestroyAllNamed(scene, "Block", "Ramp", "Trap");
            PlacePrefab(block, "Block", null, SampleBlockPosition);
            PlacePrefab(ramp, "Ramp", null, SampleRampPosition, 180f);
            PlacePrefab(trap, "Trap", null, SampleTrapPosition);
            SaveScene(scene);
            SaveAssetsAndLog("step-3-prefabs");
        }

        // ================= Step 5: moving platforms, traps, respawn =================
        public static void BuildStep5Hazards()
        {
            // The Trap prefab gets a trigger and KillOnTouch; every Trap in the scene updates with it.
            string trapPath = $"{PrefabsDir}/Trap.prefab";
            var trapRoot = PrefabUtility.LoadPrefabContents(trapPath);
            AddFittedBoxCollider(trapRoot, isTrigger: true);
            AddLessonScript(trapRoot, "KillOnTouch");
            PrefabUtility.SaveAsPrefabAsset(trapRoot, trapPath);
            PrefabUtility.UnloadPrefabContents(trapRoot);

            var movingPlatform = CreateMovingPlatformPrefab();

            var scene = OpenScene(ArenaScenePath);
            AddLessonScript(FindRequired(scene, "FallZone"), "KillOnTouch");
            DestroyAllNamed(scene, "MovingPlatform");
            PlacePrefab(movingPlatform, "MovingPlatform", null, SampleMovingPlatformPosition);
            SaveScene(scene);
            SaveAssetsAndLog("step-5-hazards");
        }

        // MovingPlatform (empty root)
        //   Platform    <- the part that moves: model, BoxCollider, Ground layer, Patrol
        //     StickyZone  <- trigger on top with StickyPlatform (Default layer)
        //   WaypointA   <- siblings of Platform, so they stay put while it moves
        //   WaypointB
        static GameObject CreateMovingPlatformPrefab()
        {
            var root = new GameObject("MovingPlatform");

            var platform = InstantiateUnpacked(KayKit("platform_4x4x1_yellow"), "Platform");
            platform.transform.SetParent(root.transform, false);
            AddFittedBoxCollider(platform);
            SetLayerRecursively(platform, GroundLayer);

            var stickyZone = CreateEmpty("StickyZone", platform.transform, Vector3.zero);
            var zone = stickyZone.AddComponent<BoxCollider>();
            zone.isTrigger = true;
            zone.center = new Vector3(0f, 1.25f, 0f); // just above the top surface (y = 1)
            zone.size = new Vector3(3.6f, 0.5f, 3.6f);
            AddLessonScript(stickyZone, "StickyPlatform");

            var waypointA = CreateEmpty("WaypointA", root.transform, Vector3.zero);
            var waypointB = CreateEmpty("WaypointB", root.transform, new Vector3(6f, 0f, 0f));
            var patrol = AddLessonScript(platform, "Patrol");
            SetObjectArray(patrol, "waypoints", waypointA.transform, waypointB.transform);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabsDir}/MovingPlatform.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        // ================= Step 6: coins, score and winning =================
        public static void BuildStep6Goal()
        {
            var coinPrefab = CreateCoinPrefab();

            var scene = OpenScene(ArenaScenePath);
            DestroyAllNamed(scene, "GameManager", "Coin", "GoalZone");

            var scoreText = FindRequired(scene, "ScoreText");
            scoreText.SetActive(true);
            var gameManager = new GameObject("GameManager");
            var manager = AddLessonScript(gameManager, "GameManager");
            SetSerialized(manager, "scoreText", p => p.objectReferenceValue = scoreText.GetComponent<TMP_Text>());

            foreach (var position in SampleCoinPositions)
                PlacePrefab(coinPrefab, "Coin", null, position);

            AddGoalZone(FindRequired(scene, "GoalPlatform"));

            SaveScene(scene);
            SaveAssetsAndLog("step-6-goal");
        }

        static GameObject CreateCoinPrefab()
        {
            var coin = InstantiateUnpacked(Kenney("coin-gold"), "Coin");
            coin.tag = CoinTag;
            SetMaterial(coin, WorkshopMaterial("CoinYellow"));
            var trigger = coin.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.5f, 0f);
            trigger.radius = 0.6f;
            AddLessonScript(coin, "Coin");
            var prefab = PrefabUtility.SaveAsPrefabAsset(coin, $"{PrefabsDir}/Coin.prefab");
            Object.DestroyImmediate(coin);
            return prefab;
        }

        static void AddGoalZone(GameObject goalPlatform)
        {
            var goalZone = CreateEmpty("GoalZone", goalPlatform.transform, Vector3.zero);
            var trigger = goalZone.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 3.25f, 0f); // the space just above the platform's top (y = 2)
            trigger.size = new Vector3(5.5f, 2.5f, 5.5f);
            AddLessonScript(goalZone, "GoalTrigger");
        }
    }
}
