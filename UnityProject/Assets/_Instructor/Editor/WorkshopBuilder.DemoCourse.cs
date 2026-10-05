using UnityEditor;
using UnityEngine;

namespace Workshop.Instructor
{
    public static partial class WorkshopBuilder
    {
        // ================= Final: the demo course =================
        // Player limits this layout is designed around (see the instructor guide):
        //   run 6 m/s, jump apex ~1.6 m, flat jump distance ~4.3 m, gravity -25.
        // Course (z = forward, all tops in meters):
        //   Start (top 0, z -3..3) -> two blocks with a 2 m gap -> ramp up to 3 -> 3 m gap
        //   -> landing (top 2, z 18..24, bounce pad + arch) -> moving platform (z 24..35)
        //   -> trap corridor (top 2, z 35..47: spikes, spinning saw, spikes) -> goal (top 2, z 47..53)
        public static void BuildFinalDemoCourse()
        {
            var bouncePad = CreateBouncePadPrefab();
            var spinningSaw = CreateSpinningSawPrefab();

            var scene = OpenScene(target.ScenePath);
            // Clear out the sample pieces from Steps 2-7 and any earlier demo course.
            DestroyAllNamed(scene, "Block", "Ramp", "Trap", "MovingPlatform", "Coin", "DemoCourse");
            var course = new GameObject("DemoCourse").transform;

            // 1) Stepping stones: two blocks with a 2 m gap (coin over the gap).
            PlacePrefab(Prefab("Block"), "Block", course, new Vector3(0f, -1f, 4f));
            PlacePrefab(Prefab("Block"), "Block", course, new Vector3(0f, -1f, 8f));

            // 2) Ramp jump: climb to y = 3, jump a 3 m gap down to the landing (top 2).
            //    KayKit slopes sit on a 1 m base, so the ramp is sunk 1 m to meet the y = 0 blocks.
            PlacePrefab(Prefab("Ramp"), "Ramp", course, new Vector3(0f, -1f, 12f), 180f);
            PlacePrefab(BackupPrefab("PlatformWide"), "Landing", course, new Vector3(0f, 1f, 21f));
            PlaceModel(KayKit("pillar_2x2x8"), "Pillar", course, new Vector3(0f, -7f, 21f));
            PlacePrefab(BackupPrefab("Arch"), "Arch", course, new Vector3(0f, 2f, 23.2f));
            PlacePrefab(bouncePad, "BouncePad", course, new Vector3(2.2f, 2f, 20f));

            // 3) Moving platform: shuttles 7 m from the landing to the corridor and back.
            var mover = PlacePrefab(Prefab("MovingPlatform"), "MovingPlatform", course, new Vector3(0f, 1f, 26f));
            mover.transform.Find("WaypointB").localPosition = new Vector3(0f, 0f, 7f);

            // 4) Trap corridor, walled in by low barriers: weave right, left, right.
            PlacePrefab(BackupPrefab("PlatformWide"), "Corridor", course, new Vector3(0f, 1f, 38f));
            PlacePrefab(BackupPrefab("PlatformWide"), "Corridor", course, new Vector3(0f, 1f, 44f));
            PlaceModel(KayKit("pillar_2x2x8"), "Pillar", course, new Vector3(0f, -7f, 38f));
            PlaceModel(KayKit("pillar_2x2x8"), "Pillar", course, new Vector3(0f, -7f, 44f));
            for (float z = 36f; z <= 46f; z += 2f)
            {
                PlaceWall(course, new Vector3(-2.5f, 2f, z));
                PlaceWall(course, new Vector3(2.5f, 2f, z));
            }
            PlacePrefab(Prefab("Trap"), "Trap", course, new Vector3(-1f, 2f, 36.5f));
            PlacePrefab(spinningSaw, "SpinningSaw", course, new Vector3(1f, 3f, 40.5f));
            PlacePrefab(Prefab("Trap"), "Trap", course, new Vector3(-1f, 2f, 44.5f));

            // 5) Coins along the risky line.
            var coin = Prefab("Coin");
            var coins = new[]
            {
                new Vector3(0f, 1.2f, 6f),     // over the first gap
                new Vector3(0f, 0.3f, 8f),     // on the second block
                new Vector3(0f, 2.3f, 12f),    // halfway up the ramp
                new Vector3(0f, 4.8f, 16.5f),  // in the air over the ramp gap: you have to jump for it
                new Vector3(2.2f, 8f, 20f),    // high above the bounce pad
                new Vector3(0f, 2.3f, 29.5f),  // on the moving platform's path
                new Vector3(1f, 2.3f, 36.5f),  // beside the first spikes
                new Vector3(-1f, 2.3f, 40.5f), // beside the saw
                new Vector3(1f, 2.3f, 44.5f),  // beside the last spikes
            };
            foreach (var position in coins)
                PlacePrefab(coin, "Coin", course, position);

            SaveScene(scene);
            SaveAssetsAndLog("final");
        }

        static void PlaceWall(Transform parent, Vector3 position)
        {
            var wall = PlaceModel(KayKit("barrier_2x1x1_blue"), "Wall", parent, position, 90f);
            AddFittedBoxCollider(wall);
            SetLayerRecursively(wall, GroundLayer);
        }

        // Bonus prefab: spring pad + BouncePad.
        static GameObject CreateBouncePadPrefab()
        {
            var pad = InstantiateUnpacked(KayKit("spring_pad_blue"), "BouncePad");
            AddFittedBoxCollider(pad);
            AddLessonScript(pad, "BouncePad");
            var prefab = PrefabUtility.SaveAsPrefabAsset(pad, $"{target.PrefabsDir}/BouncePad.prefab");
            Object.DestroyImmediate(pad);
            return prefab;
        }

        // Bonus prefab: a saw blade that spins like a wheel and restarts the level on touch.
        static GameObject CreateSpinningSawPrefab()
        {
            var saw = InstantiateUnpacked(Kenney("saw"), "SpinningSaw");
            SetMaterial(saw, WorkshopMaterial("HazardRed"));
            var trigger = saw.AddComponent<MeshCollider>();
            trigger.sharedMesh = saw.GetComponent<MeshFilter>().sharedMesh;
            trigger.convex = true; // trigger mesh colliders must be convex
            trigger.isTrigger = true;
            AddLessonScript(saw, "KillOnTouch");
            var spinner = AddLessonScript(saw, "Spinner");
            SetSerialized(spinner, "axis", p => p.vector3Value = Vector3.forward);
            var prefab = PrefabUtility.SaveAsPrefabAsset(saw, $"{target.PrefabsDir}/SpinningSaw.prefab");
            Object.DestroyImmediate(saw);
            return prefab;
        }
    }
}
