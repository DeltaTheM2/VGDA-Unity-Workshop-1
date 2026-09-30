using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Workshop.Instructor
{
    // Instructor-only tooling that builds every checkpoint of the workshop project.
    // Each Build* method takes the project from the previous checkpoint to the next one
    // and is safe to re-run (it replaces whatever it created last time).
    // Lesson scripts are looked up by file name (MonoScript), never referenced directly,
    // so this assembly compiles at every checkpoint, even before those scripts exist.
    public static partial class WorkshopBuilder
    {
        // ---------- Paths ----------
        public const string Root = "Assets/_Workshop";
        public const string ScenesDir = Root + "/Scenes";
        public const string ArenaScenePath = ScenesDir + "/Arena.unity";
        public const string WinScenePath = ScenesDir + "/Win.unity";
        public const string ScriptsDir = Root + "/Scripts";
        public const string MaterialsDir = Root + "/Materials";
        public const string PhysicsMaterialsDir = Root + "/PhysicsMaterials";
        public const string KayKitDir = Root + "/Art/KayKit";
        public const string KenneyDir = Root + "/Art/Kenney";
        public const string PrefabsDir = Root + "/Prefabs";
        public const string BackupDir = Root + "/_Backup";
        public const string PrebuiltDir = Root + "/Prebuilt";
        public const string TmpFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        public const string TmpOutlineMaterialPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Outline.mat";

        // ---------- Project conventions ----------
        public const string GroundLayerName = "Ground";
        public const int GroundLayer = 6;
        public const string PlayerTag = "Player";
        public const string CoinTag = "Coin";
        public static readonly Vector3 Gravity = new Vector3(0f, -25f, 0f);

        // ---------- Arena layout (meters; KayKit models have their pivot at the bottom center) ----------
        public static readonly Vector3 StartPlatformPosition = new Vector3(0f, -2f, 0f); // 6x6x2 -> top at y = 0
        public static readonly Vector3 GoalPlatformPosition = new Vector3(0f, 0f, 50f); // 6x6x2 -> top at y = 2, z 47..53
        public static readonly Vector3 PlayerSpawnPosition = new Vector3(0f, 1f, 0f);   // capsule center, feet on y = 0

        // ---------- Asset helpers ----------
        static GameObject KayKit(string modelName) => LoadRequired<GameObject>($"{KayKitDir}/{modelName}.fbx");
        static GameObject Kenney(string modelName) => LoadRequired<GameObject>($"{KenneyDir}/{modelName}.fbx");
        static Material WorkshopMaterial(string name) => LoadRequired<Material>($"{MaterialsDir}/{name}.mat");
        static GameObject Prefab(string name) => LoadRequired<GameObject>($"{PrefabsDir}/{name}.prefab");
        static GameObject BackupPrefab(string name) => LoadRequired<GameObject>($"{BackupDir}/{name}.prefab");

        static T LoadRequired<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
                throw new InvalidOperationException($"[Workshop] Missing asset: {path}");
            return asset;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        // Instantiates a model or prefab and fully unpacks it, like dragging a model into the
        // scene and choosing "Original Prefab" later: the result is a plain GameObject.
        static GameObject InstantiateUnpacked(GameObject source, string name)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            instance.name = name;
            return instance;
        }

        // Instantiates a prefab asset as a linked prefab instance (blue in the Hierarchy).
        static GameObject PlacePrefab(GameObject prefab, string name, Transform parent, Vector3 position, float yRotation = 0f)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            instance.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yRotation, 0f));
            return instance;
        }

        static GameObject PlaceModel(GameObject model, string name, Transform parent, Vector3 position, float yRotation = 0f)
        {
            var instance = InstantiateUnpacked(model, name);
            instance.transform.SetParent(parent, false);
            instance.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yRotation, 0f));
            return instance;
        }

        static GameObject CreateEmpty(string name, Transform parent, Vector3 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go;
        }

        // A BoxCollider that wraps every mesh under the object (in the object's own space),
        // which is what "Add Component > Box Collider" does for a single-mesh model.
        static BoxCollider AddFittedBoxCollider(GameObject go, bool isTrigger = false)
        {
            var bounds = LocalMeshBounds(go);
            var box = GetOrAdd<BoxCollider>(go);
            box.center = bounds.center;
            box.size = bounds.size;
            box.isTrigger = isTrigger;
            return box;
        }

        static MeshCollider AddMeshCollider(GameObject go)
        {
            var meshFilter = go.GetComponent<MeshFilter>();
            var meshCollider = GetOrAdd<MeshCollider>(go);
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            return meshCollider;
        }

        static Bounds LocalMeshBounds(GameObject go)
        {
            var filters = go.GetComponentsInChildren<MeshFilter>(true);
            if (filters.Length == 0)
                throw new InvalidOperationException($"[Workshop] {go.name} has no meshes to fit a collider to.");
            Bounds? total = null;
            foreach (var filter in filters)
            {
                var toRoot = go.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                var meshBounds = filter.sharedMesh.bounds;
                foreach (var corner in Corners(meshBounds))
                {
                    var p = toRoot.MultiplyPoint3x4(corner);
                    if (total == null) total = new Bounds(p, Vector3.zero);
                    else { var b = total.Value; b.Encapsulate(p); total = b; }
                }
            }
            return total.Value;
        }

        static Vector3[] Corners(Bounds b)
        {
            var min = b.min; var max = b.max;
            return new[]
            {
                new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z),
                new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z),
                new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z),
                new Vector3(min.x, max.y, max.z), new Vector3(max.x, max.y, max.z),
            };
        }

        // Note: Unity overloads == for destroyed/missing objects, so never use ?? on components.
        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var existing = go.GetComponent<T>();
            return existing != null ? existing : go.AddComponent<T>();
        }

        static void SetLayerRecursively(GameObject go, int layer)
        {
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = layer;
        }

        static void SetMaterial(GameObject go, Material material)
        {
            foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterials = Enumerable.Repeat(material, renderer.sharedMaterials.Length).ToArray();
        }

        // ---------- Lesson scripts (looked up by file name so this code compiles at every step) ----------
        static Type LessonScriptType(string className)
        {
            string guid = AssetDatabase.FindAssets($"{className} t:Script", new[] { ScriptsDir, PrebuiltDir })
                .FirstOrDefault(g => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(g)) == className);
            var script = guid == null ? null : AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
            var type = script != null ? script.GetClass() : null;
            if (type == null)
                throw new InvalidOperationException($"[Workshop] Script {className}.cs is missing or does not compile. Add it from the instructor guide first.");
            return type;
        }

        static Component AddLessonScript(GameObject go, string className)
        {
            var type = LessonScriptType(className);
            var existing = go.GetComponent(type);
            return existing != null ? existing : go.AddComponent(type);
        }

        static void SetSerialized(Object target, string propertyPath, Action<SerializedProperty> assign)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyPath);
            if (property == null)
                throw new InvalidOperationException($"[Workshop] {target.GetType().Name} has no serialized field '{propertyPath}'.");
            assign(property);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetObjectArray(Object target, string propertyPath, params Object[] values)
        {
            SetSerialized(target, propertyPath, p =>
            {
                p.arraySize = values.Length;
                for (int i = 0; i < values.Length; i++)
                    p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            });
        }

        // ---------- Scenes ----------
        static Scene OpenScene(string path) => EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        static void SaveScene(Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException($"[Workshop] Could not save {scene.path}");
        }

        static GameObject FindInScene(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var match = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
                if (match != null)
                    return match.gameObject;
            }
            return null;
        }

        static GameObject FindRequired(Scene scene, string name)
        {
            var go = FindInScene(scene, name);
            if (go == null)
                throw new InvalidOperationException($"[Workshop] {scene.name} has no object called '{name}'. Build the previous checkpoint first.");
            return go;
        }

        static void DestroyAllNamed(Scene scene, params string[] names)
        {
            foreach (var name in names)
            {
                GameObject go;
                while ((go = FindInScene(scene, name)) != null)
                    Object.DestroyImmediate(go);
            }
        }

        static void SaveAssetsAndLog(string what)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Workshop] Built checkpoint: {what}");
        }
    }
}
