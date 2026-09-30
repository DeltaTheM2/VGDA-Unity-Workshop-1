using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Workshop.Instructor
{
    // Menu items for the instructor, plus the entry point used by Tools/build-checkpoints.sh:
    //   Unity -batchmode -projectPath UnityProject -executeMethod Workshop.Instructor.WorkshopMenu.BuildFromCommandLine -checkpoint step-2-player
    public static class WorkshopMenu
    {
        // In order. Step 4 (Remix #1) has no checkpoint: it's free building with no new code.
        public static readonly (string tag, Action build)[] Checkpoints =
        {
            ("step-0-starter", WorkshopBuilder.BuildStarter),
            ("step-1-lifecycle", WorkshopBuilder.BuildStep1Lifecycle),
            ("step-2-player", WorkshopBuilder.BuildStep2Player),
            ("step-3-prefabs", WorkshopBuilder.BuildStep3Prefabs),
            ("step-5-hazards", WorkshopBuilder.BuildStep5Hazards),
            ("step-6-goal", WorkshopBuilder.BuildStep6Goal),
            ("final", WorkshopBuilder.BuildFinalDemoCourse),
        };

        [MenuItem("Workshop/Build Checkpoint/Step 0 - Starter", priority = 0)]
        static void MenuStarter() => BuildInteractive("step-0-starter");
        [MenuItem("Workshop/Build Checkpoint/Step 1 - Lifecycle", priority = 1)]
        static void MenuStep1() => BuildInteractive("step-1-lifecycle");
        [MenuItem("Workshop/Build Checkpoint/Step 2 - Player", priority = 2)]
        static void MenuStep2() => BuildInteractive("step-2-player");
        [MenuItem("Workshop/Build Checkpoint/Step 3 - Prefabs", priority = 3)]
        static void MenuStep3() => BuildInteractive("step-3-prefabs");
        [MenuItem("Workshop/Build Checkpoint/Step 5 - Hazards", priority = 5)]
        static void MenuStep5() => BuildInteractive("step-5-hazards");
        [MenuItem("Workshop/Build Checkpoint/Step 6 - Goal", priority = 6)]
        static void MenuStep6() => BuildInteractive("step-6-goal");
        [MenuItem("Workshop/Build Checkpoint/Final - Demo Course", priority = 7)]
        static void MenuFinal() => BuildInteractive("final");

        [MenuItem("Workshop/Validate Project", priority = 20)]
        static void MenuValidate()
        {
            string checkpoint = WorkshopValidator.DetectCheckpoint();
            bool ok = WorkshopValidator.Validate(checkpoint);
            EditorUtility.DisplayDialog("Workshop", ok
                ? $"Looks like '{checkpoint}' and everything checks out."
                : $"Looks like '{checkpoint}' but some checks failed. See the Console.", "OK");
        }

        static void BuildInteractive(string tag)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            WorkshopBuilder.WithTmpEssentials(() =>
            {
                Checkpoints.First(c => c.tag == tag).build();
                WorkshopValidator.Validate(tag);
            }, Debug.LogError);
        }

        // Does not return control to Unity until it calls EditorApplication.Exit (the TMP import may be async).
        public static void BuildFromCommandLine()
        {
            string tag = ArgumentAfter("-checkpoint");
            WorkshopBuilder.WithTmpEssentials(() => BuildValidateAndExit(tag), error =>
            {
                Debug.LogError(error);
                EditorApplication.Exit(1);
            });
        }

        static void BuildValidateAndExit(string tag)
        {
            int exitCode = 0;
            try
            {
                var checkpoint = Checkpoints.FirstOrDefault(c => c.tag == tag);
                if (checkpoint.build == null)
                    throw new ArgumentException($"Unknown checkpoint '{tag}'. Use one of: {string.Join(", ", Checkpoints.Select(c => c.tag))}");
                checkpoint.build();
                if (!WorkshopValidator.Validate(tag))
                    exitCode = 2;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        public static void ValidateFromCommandLine()
        {
            string tag = ArgumentAfter("-checkpoint") ?? WorkshopValidator.DetectCheckpoint();
            EditorApplication.Exit(WorkshopValidator.Validate(tag) ? 0 : 2);
        }

        static string ArgumentAfter(string flag)
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, flag);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
    }
}
