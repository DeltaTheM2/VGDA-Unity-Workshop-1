using System;
using System.Linq;
using UnityEditor;

namespace Workshop.Instructor
{
    public static partial class WorkshopBuilder
    {
        // Catch-up scenes shipped in every copy of the project, in teaching order.
        // Each one is the finished state of that step; Step 5 (Remix #1) has none.
        public static readonly (string sceneName, Action build)[] CheckpointSteps =
        {
            ("1 - Unity Basics", BuildStep1Basics),
            ("2 - Parts", BuildStep2Parts),
            ("3 - Scripting", BuildStep3Scripting),
            ("4 - Player", BuildStep4Player),
            ("6 - Hazards", BuildStep6Hazards),
            ("7 - Coins and Win", BuildStep7Goal),
            ("Final - Demo Course", BuildFinalDemoCourse),
        };

        public static string CheckpointScenePath(string sceneName) => $"{CheckpointsDir}/{sceneName}.unity";

        public static string[] CheckpointScenePaths => CheckpointSteps.Select(c => CheckpointScenePath(c.sceneName)).ToArray();

        // Replays the steps on a copy of the starter Arena for each checkpoint. The scenes use the
        // script copies in Checkpoints/_Scripts (namespace WorkshopCheckpoints), written from the
        // instructor guide by Tools/extract-lesson-scripts.sh --checkpoint-copies.
        static void BuildCheckpointScenes()
        {
            if (!AssetDatabase.IsValidFolder(CheckpointScriptsDir))
                throw new InvalidOperationException($"[Workshop] {CheckpointScriptsDir} is missing. Run Tools/extract-lesson-scripts.sh --checkpoint-copies first.");

            for (int n = 0; n < CheckpointSteps.Length; n++)
            {
                string sceneName = CheckpointSteps[n].sceneName;
                string scenePath = CheckpointScenePath(sceneName);
                AssetDatabase.DeleteAsset(scenePath);
                if (!AssetDatabase.CopyAsset(ArenaScenePath, scenePath))
                    throw new InvalidOperationException($"[Workshop] Could not create {scenePath}");

                var checkpoint = new BuildTarget
                {
                    ScenePath = scenePath,
                    PrefabsDir = $"{CheckpointPartsDir}/{sceneName}",
                    PhysicsMaterialsDir = CheckpointPartsDir,
                    ScriptDirs = new[] { CheckpointScriptsDir, PrebuiltDir },
                };
                int last = n;
                WithTarget(checkpoint, () =>
                {
                    for (int step = 0; step <= last; step++)
                        CheckpointSteps[step].build();
                });
                BakeEnvironmentLighting(OpenScene(scenePath));
            }
        }
    }
}
