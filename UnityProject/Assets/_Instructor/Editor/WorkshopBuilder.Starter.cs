using System;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Workshop.Instructor
{
    public static partial class WorkshopBuilder
    {
        // Colors for the pre-made material palette (URP Lit).
        static readonly (string name, Color color)[] Palette =
        {
            ("PlayerBlue", new Color(0.16f, 0.44f, 0.96f)),
            ("CoinYellow", new Color(1.00f, 0.80f, 0.10f)),
            ("HazardRed", new Color(0.90f, 0.12f, 0.12f)),
            ("RemixGreen", new Color(0.30f, 0.80f, 0.35f)),
            ("RemixOrange", new Color(1.00f, 0.55f, 0.15f)),
            ("RemixPurple", new Color(0.58f, 0.36f, 0.90f)),
            ("RemixPink", new Color(1.00f, 0.47f, 0.72f)),
            ("RemixWhite", new Color(0.95f, 0.95f, 0.95f)),
        };

        // Import scale for Kenney models so they match KayKit's 1 unit = 1 m grid
        // (a 2 m trap, a 1 m coin, a 2 m tall character like the player capsule).
        static readonly (string model, float scale)[] KenneyScales =
        {
            ("character-oobi", 2.2f),
            ("coin-gold", 2.5f),
            ("spike-block", 2.2f),
            ("saw", 2.5f),
            ("flag", 2.5f),
        };

        enum ColliderKind { None, Box, Mesh }

        // One entry per ready-made part in _Backup/. The first three match the parts
        // built live in Step 2 exactly (Trap is still the Step 2 placeholder).
        static readonly (string name, string model, ColliderKind collider, bool ground, string material)[] BackupParts =
        {
            ("Block", "platform_2x2x1_blue", ColliderKind.Box, true, null),
            ("Ramp", "platform_slope_4x6x4_blue", ColliderKind.Mesh, true, null),
            ("Trap", "Kenney/spike-block", ColliderKind.None, false, "HazardRed"),
            ("BlockSmall", "platform_1x1x1_blue", ColliderKind.Box, true, null),
            ("BlockTall", "platform_2x2x2_blue", ColliderKind.Box, true, null),
            ("BlockLong", "platform_6x2x1_blue", ColliderKind.Box, true, null),
            ("PlatformWide", "platform_6x6x1_blue", ColliderKind.Box, true, null),
            ("RampSmall", "platform_slope_2x2x2_blue", ColliderKind.Mesh, true, null),
            ("Pipe", "pipe_straight_A_blue", ColliderKind.Box, true, null),
            ("Arch", "arch_blue", ColliderKind.Mesh, true, null),
        };

        // ================= Step 0: the starter project =================
        public static void BuildStarter()
        {
            foreach (var folder in new[] { ScenesDir, ScriptsDir, MaterialsDir, PhysicsMaterialsDir, PrefabsDir, BackupDir, PrebuiltDir })
                EnsureFolder(folder);

            ConfigureProjectSettings();
            ConfigureModelImporters();
            ImportTmpEssentials();
            CreatePalette();
            foreach (var part in BackupParts)
                CreatePartPrefab($"{BackupDir}/{part.name}.prefab", part.model, part.collider, part.ground, part.material);
            BuildWinScene();
            BuildArenaScene();
            BuildCheckpointScenes();
            // Checkpoint scenes must be listed too: KillOnTouch reloads the active scene by name.
            EditorBuildSettings.scenes = new[] { ArenaScenePath, WinScenePath }.Concat(CheckpointScenePaths)
                .Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
            OpenScene(ArenaScenePath);
            SaveAssetsAndLog("step-0-starter");
        }

        static void ConfigureProjectSettings()
        {
            PlayerSettings.companyName = "VGDA CSULB";
            PlayerSettings.productName = "Obstacle Course";

            // The template remembers its SampleScene as the scene to open on first launch. Point it at the Arena.
            var playerSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            playerSettings.FindProperty("templateDefaultScene").stringValue = ArenaScenePath;
            playerSettings.ApplyModifiedPropertiesWithoutUndo();

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tags = tagManager.FindProperty("tags");
            bool hasCoinTag = Enumerable.Range(0, tags.arraySize).Any(i => tags.GetArrayElementAtIndex(i).stringValue == CoinTag);
            if (!hasCoinTag)
            {
                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = CoinTag;
            }
            tagManager.FindProperty("layers").GetArrayElementAtIndex(GroundLayer).stringValue = GroundLayerName;
            tagManager.ApplyModifiedPropertiesWithoutUndo();

            // Stronger gravity than Earth's -9.81 makes jumps snappy instead of floaty.
            var dynamics = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset")[0]);
            dynamics.FindProperty("m_Gravity").vector3Value = Gravity;
            dynamics.ApplyModifiedPropertiesWithoutUndo();
        }

        static void ConfigureModelImporters()
        {
            // Raw models: no colliders (added live), no animation components on static props.
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { KayKitDir, KenneyDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (ModelImporter)AssetImporter.GetAtPath(path);
                string model = Path.GetFileNameWithoutExtension(path);
                bool isCharacter = model.StartsWith("character-");
                var kenneyScale = KenneyScales.FirstOrDefault(k => k.model == model);

                importer.globalScale = kenneyScale.model != null ? kenneyScale.scale : 1f;
                importer.addCollider = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.importBlendShapes = false;
                if (!isCharacter)
                {
                    importer.animationType = ModelImporterAnimationType.None;
                    importer.importAnimation = false;
                }
                importer.SaveAndReimport();
            }
        }

        static void ImportTmpEssentials()
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath) == null)
                throw new InvalidOperationException("[Workshop] TMP Essential Resources are missing. Build through WithTmpEssentials().");
            // HDRP-only shader graphs: unusable in URP and they print "Shader Unsupported" warnings on import.
            foreach (var hdrpOnly in new[] { "TMP_SDF-HDRP LIT", "TMP_SDF-HDRP UNLIT" })
                AssetDatabase.DeleteAsset($"Assets/TextMesh Pro/Shaders/{hdrpOnly}.shadergraph");
        }

        // Package imports finish asynchronously, so anything that needs TMP runs from the callback.
        public static void WithTmpEssentials(Action then, Action<string> onError)
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath) != null)
            {
                then();
                return;
            }
            AssetDatabase.ImportPackageCallback completed = null;
            AssetDatabase.ImportPackageFailedCallback failed = null;
            completed = _ =>
            {
                AssetDatabase.importPackageCompleted -= completed;
                AssetDatabase.importPackageFailed -= failed;
                AssetDatabase.Refresh();
                then();
            };
            failed = (_, message) =>
            {
                AssetDatabase.importPackageCompleted -= completed;
                AssetDatabase.importPackageFailed -= failed;
                onError($"[Workshop] TMP Essential Resources failed to import: {message}");
            };
            AssetDatabase.importPackageCompleted += completed;
            AssetDatabase.importPackageFailed += failed;
            var ugui = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.ugui/package.json");
            AssetDatabase.ImportPackage(Path.Combine(ugui.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage"), false);
        }

        static void CreatePalette()
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            foreach (var (name, color) in Palette)
            {
                string path = $"{MaterialsDir}/{name}.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                bool isNew = material == null;
                if (isNew)
                    material = new Material(lit);
                material.shader = lit;
                material.SetColor("_BaseColor", color);
                material.SetFloat("_Smoothness", 0.35f);
                if (name == "CoinYellow")
                {
                    // A little glow so coins stand out.
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", new Color(0.45f, 0.30f, 0.0f));
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }
                if (isNew)
                    AssetDatabase.CreateAsset(material, path);
                else
                    EditorUtility.SetDirty(material);
            }
        }

        // Builds a prefab from a raw model: collider, layer and material, then saves it
        // as an ORIGINAL prefab (not a variant of the model).
        static GameObject CreatePartPrefab(string prefabPath, string model, ColliderKind collider, bool ground, string material)
        {
            var source = model.StartsWith("Kenney/") ? Kenney(model.Substring("Kenney/".Length)) : KayKit(model);
            var part = InstantiateUnpacked(source, Path.GetFileNameWithoutExtension(prefabPath));
            if (collider == ColliderKind.Box) AddFittedBoxCollider(part);
            if (collider == ColliderKind.Mesh) AddMeshCollider(part);
            if (ground) SetLayerRecursively(part, GroundLayer);
            if (material != null) SetMaterial(part, WorkshopMaterial(material));
            var prefab = PrefabUtility.SaveAsPrefabAsset(part, prefabPath);
            UnityEngine.Object.DestroyImmediate(part);
            return prefab;
        }

        static void BuildArenaScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Lighting
            ApplySkyAndFog();
            var sunObject = new GameObject("Directional Light");
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.96f, 0.86f);
            sun.intensity = 1.3f;
            sun.shadows = LightShadows.Soft;
            sunObject.AddComponent<UniversalAdditionalLightData>();
            sunObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.sun = sun;

            // --- Main camera (driven by Cinemachine)
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 500f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraObject.AddComponent<CinemachineBrain>();

            // --- Level: start platform, goal platform, decoration
            var level = new GameObject("Level").transform;
            var start = PlaceModel(KayKit("platform_6x6x2_green"), "StartPlatform", level, StartPlatformPosition);
            AddFittedBoxCollider(start);
            SetLayerRecursively(start, GroundLayer);
            PlaceModel(KayKit("pillar_2x2x8"), "Pillar", start.transform, StartPlatformPosition + new Vector3(0f, -8f, 0f));

            var goal = PlaceModel(KayKit("platform_6x6x2_green"), "GoalPlatform", level, GoalPlatformPosition);
            AddFittedBoxCollider(goal);
            SetLayerRecursively(goal, GroundLayer);
            PlaceModel(KayKit("pillar_2x2x8"), "Pillar", goal.transform, GoalPlatformPosition + new Vector3(0f, -8f, 0f));
            PlaceModel(KayKit("signage_finish"), "FinishLine", goal.transform, GoalPlatformPosition + new Vector3(0f, 2f, -1.5f));
            PlaceModel(KayKit("flag_A_blue"), "Flag", goal.transform, GoalPlatformPosition + new Vector3(-2.4f, 2f, 2.4f));
            PlaceModel(KayKit("flag_A_blue"), "Flag", goal.transform, GoalPlatformPosition + new Vector3(2.4f, 2f, 2.4f));

            // --- Where the player will go (the camera looks here until we have a player)
            var spawn = new GameObject("PlayerSpawn");
            spawn.transform.position = PlayerSpawnPosition;

            // --- Cinemachine follow camera: fixed offset behind and above, always looking at the target
            var followObject = new GameObject("Follow Camera");
            var followCamera = followObject.AddComponent<CinemachineCamera>();
            followCamera.Follow = spawn.transform;
            var lens = followCamera.Lens;
            lens.FieldOfView = 50f;
            lens.FarClipPlane = 500f;
            followCamera.Lens = lens;
            var follow = followObject.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0f, 6f, -10f);
            follow.TrackerSettings.BindingMode = BindingMode.WorldSpace;
            follow.TrackerSettings.PositionDamping = new Vector3(0.5f, 0.5f, 0.5f);
            followObject.AddComponent<CinemachineRotationComposer>();
            followObject.transform.position = PlayerSpawnPosition + follow.FollowOffset;
            followObject.transform.LookAt(PlayerSpawnPosition);
            cameraObject.transform.SetPositionAndRotation(followObject.transform.position, followObject.transform.rotation);

            // --- Invisible trigger far below the level. It gets KillOnTouch in Step 6.
            var fallZone = new GameObject("FallZone");
            fallZone.transform.position = new Vector3(0f, -30f, 25f);
            var fallBox = fallZone.AddComponent<BoxCollider>();
            fallBox.isTrigger = true;
            fallBox.size = new Vector3(400f, 10f, 400f);

            // --- UI: an empty, disabled score label in the top-left corner (used in Step 7)
            var canvas = CreateCanvas();
            var scoreText = CreateText("ScoreText", canvas.transform, "", 60, TextAlignmentOptions.TopLeft, Color.white);
            var scoreRect = (RectTransform)scoreText.transform;
            scoreRect.anchorMin = scoreRect.anchorMax = scoreRect.pivot = new Vector2(0f, 1f);
            scoreRect.anchoredPosition = new Vector2(40f, -30f);
            scoreRect.sizeDelta = new Vector2(700f, 90f);
            scoreText.gameObject.SetActive(false);

            EditorSceneManager.SaveScene(scene, ArenaScenePath);
            BakeEnvironmentLighting(scene);
        }

        // A procedural sky that stays blue below the horizon too, so the floating course
        // hangs in open sky instead of over the default grey-brown "ground".
        internal static void ApplySkyAndFog()
        {
            string skyPath = $"{PrebuiltDir}/Sky.mat";
            var sky = AssetDatabase.LoadAssetAtPath<Material>(skyPath);
            if (sky == null)
            {
                sky = new Material(Shader.Find("Skybox/Procedural"));
                AssetDatabase.CreateAsset(sky, skyPath);
            }
            sky.SetColor("_SkyTint", new Color(0.42f, 0.62f, 0.95f));
            sky.SetColor("_GroundColor", new Color(0.62f, 0.78f, 0.96f));
            sky.SetFloat("_AtmosphereThickness", 0.75f);
            sky.SetFloat("_Exposure", 1.25f);
            sky.SetFloat("_SunSize", 0.035f);
            EditorUtility.SetDirty(sky);

            RenderSettings.skybox = sky;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.70f, 0.83f, 0.97f);
            RenderSettings.fogStartDistance = 80f;
            RenderSettings.fogEndDistance = 250f;
        }

        // Bakes only the sky's ambient light and reflections (no lightmaps). Without this,
        // a scene reloaded by SceneManager.LoadScene in the Editor can come back darker.
        internal static void BakeEnvironmentLighting(Scene scene)
        {
            string settingsPath = $"{ScenesDir}/ArenaLighting.lighting";
            var settings = AssetDatabase.LoadAssetAtPath<LightingSettings>(settingsPath);
            if (settings == null)
            {
                settings = new LightingSettings { name = "ArenaLighting" };
                AssetDatabase.CreateAsset(settings, settingsPath);
            }
            settings.bakedGI = false;
            settings.realtimeGI = false;
            EditorUtility.SetDirty(settings);
            Lightmapping.lightingSettings = settings;
            if (!Lightmapping.Bake())
                throw new InvalidOperationException("[Workshop] Lighting bake failed.");
            SaveScene(scene);
        }

        static void BuildWinScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.25f, 0.55f, 0.95f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<UniversalAdditionalCameraData>();

            var canvas = CreateCanvas();
            var title = CreateText("YouWinText", canvas.transform, "You Win!", 180, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f));
            var titleRect = (RectTransform)title.transform;
            titleRect.anchoredPosition = new Vector2(0f, 150f);
            titleRect.sizeDelta = new Vector2(1400f, 260f);

            var buttonObject = new GameObject("PlayAgainButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.layer = 5;
            buttonObject.transform.SetParent(canvas.transform, false);
            var buttonRect = (RectTransform)buttonObject.transform;
            buttonRect.anchoredPosition = new Vector2(0f, -140f);
            buttonRect.sizeDelta = new Vector2(480f, 130f);
            var image = buttonObject.GetComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            var label = CreateText("Label", buttonObject.transform, "Play Again", 64, TextAlignmentOptions.Center, new Color(0.15f, 0.17f, 0.25f));
            label.fontSharedMaterial = label.font.material; // no outline on the button label
            var labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;

            var playAgain = AddLessonScript(buttonObject, "PlayAgainButton");
            var button = buttonObject.GetComponent<Button>();
            var onClick = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), playAgain, "PlayAgain");
            UnityEventTools.AddPersistentListener(button.onClick, onClick);

            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.GetComponent<EventSystem>().firstSelectedGameObject = buttonObject; // gamepad can press it too

            EditorSceneManager.SaveScene(scene, WinScenePath);
        }

        static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("Canvas", typeof(RectTransform)) { layer = 5 };
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        static TextMeshProUGUI CreateText(string name, Transform parent, string content, float size, TextAlignmentOptions alignment, Color color)
        {
            var textObject = new GameObject(name, typeof(RectTransform)) { layer = 5 };
            textObject.transform.SetParent(parent, false);
            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = LoadRequired<TMP_FontAsset>(TmpFontPath);
            text.fontSharedMaterial = LoadRequired<Material>(TmpOutlineMaterialPath);
            text.text = content;
            text.fontSize = size;
            text.fontStyle = FontStyles.Bold;
            text.alignment = alignment;
            text.color = color;
            return text;
        }
    }
}
