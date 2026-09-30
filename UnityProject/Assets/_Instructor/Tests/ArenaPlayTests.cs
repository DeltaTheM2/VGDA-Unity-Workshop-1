using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Workshop.Instructor.Tests
{
    // PlayMode tests that play the Arena the way a person would, through a virtual gamepad.
    // The same suite runs at every checkpoint: tests whose objects don't exist yet are Ignored.
    // Any error or exception logged during a test fails it (Unity Test Framework default).
    public class ArenaPlayTests
    {
        const float RunSpeed = 6f;
        const float JumpVelocity = 9f;
        const float Gravity = 25f;
        static readonly float ExpectedJumpHeight = JumpVelocity * JumpVelocity / (2f * Gravity); // ~1.62 m

        Gamepad pad;
        InputSettings originalSettings;
        Vector2 stick;
        int arenaReloads;
        string lastLoadedScene;

        static GameObject Player => GameObject.FindWithTag("Player");
        static Vector3 PlayerPosition => Player.transform.position;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            originalSettings = InputSystem.settings;
            var settings = ScriptableObject.CreateInstance<InputSettings>();
            settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings = settings;
            pad = InputSystem.AddDevice<Gamepad>();
            stick = Vector2.zero;
            Time.captureFramerate = 60;

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("Arena");
            yield return null;
            yield return null;
            arenaReloads = 0; // only count reloads caused by the test itself
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            InputSystem.RemoveDevice(pad);
            InputSystem.settings = originalSettings;
            Time.captureFramerate = 0;
            yield return null;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            lastLoadedScene = scene.name;
            if (scene.name == "Arena")
                arenaReloads++;
        }

        // ---------- Input and helpers ----------
        void SetStick(Vector2 value)
        {
            stick = value;
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = value });
        }

        IEnumerator PressSouthButton()
        {
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = stick }.WithButton(GamepadButton.South));
            yield return null;
            yield return null;
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = stick });
            yield return null;
        }

        static void RequirePlayer()
        {
            if (Player == null)
                Assert.Ignore("No Player in this checkpoint yet.");
        }

        static bool HasScript(GameObject go, string className) =>
            go != null && go.GetComponents<MonoBehaviour>().Any(m => m != null && m.GetType().Name == className);

        static void Teleport(Vector3 position)
        {
            var body = Player.GetComponent<Rigidbody>();
            Player.transform.SetParent(null);
            body.linearVelocity = Vector3.zero;
            body.position = position;
            Player.transform.position = position;
        }

        IEnumerator TrackMaxHeight(float seconds, float[] maxY)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                if (Player != null)
                    maxY[0] = Mathf.Max(maxY[0], PlayerPosition.y);
                yield return null;
            }
        }

        // Steers toward (x, z) until close enough. Optionally jumps once when passing jumpAtZ.
        IEnumerator MoveTo(float x, float z, float tolerance = 0.25f, float timeout = 6f, float jumpAtZ = float.NaN)
        {
            bool jumped = false;
            float elapsed = 0f;
            while (true)
            {
                Assert.AreEqual(0, arenaReloads, "The player died (the level restarted) on the way.");
                var p = PlayerPosition;
                var toTarget = new Vector2(x - p.x, z - p.z);
                if (toTarget.magnitude <= tolerance)
                    break;
                Assert.Less(elapsed, timeout, $"Couldn't reach ({x}, {z}); stuck at {p}.");
                SetStick(toTarget.normalized);
                if (!float.IsNaN(jumpAtZ) && !jumped && p.z >= jumpAtZ)
                {
                    jumped = true;
                    yield return PressSouthButton();
                    elapsed += 3f * Time.deltaTime;
                    continue;
                }
                yield return null;
                elapsed += Time.deltaTime;
            }
        }

        // ---------- Every checkpoint ----------
        [UnityTest]
        public IEnumerator Arena_RunsWithoutErrors()
        {
            yield return new WaitForSeconds(2f);
            Assert.AreEqual("Arena", SceneManager.GetActiveScene().name);
        }

        [UnityTest]
        public IEnumerator WinScreen_PlayAgainWithGamepadReturnsToArena()
        {
            SceneManager.LoadScene("Win");
            yield return null;
            yield return null;
            Assert.IsNotNull(GameObject.Find("PlayAgainButton").GetComponent<Button>());
            yield return PressSouthButton(); // the button is pre-selected, so "A" clicks it
            yield return null;
            Assert.AreEqual("Arena", SceneManager.GetActiveScene().name);
        }

        // ---------- Step 1 ----------
        [UnityTest]
        public IEnumerator Lifecycle_LogsEventsInOrder()
        {
            if (GameObject.Find("LifecycleDemo") == null)
                Assert.Ignore("No LifecycleDemo in this checkpoint.");
            var logs = new List<string>();
            Application.LogCallback capture = (message, stack, type) =>
            {
                if (message.Contains("LifecycleDemo: ")) logs.Add(message);
            };
            Application.logMessageReceived += capture;
            SceneManager.LoadScene("Arena");
            yield return new WaitForSeconds(0.5f);
            Application.logMessageReceived -= capture;

            int IndexOf(string e) => logs.FindLastIndex(l => l.EndsWith(": " + e));
            Assert.That(IndexOf("Awake"), Is.LessThan(IndexOf("OnEnable")), string.Join("\n", logs));
            Assert.That(IndexOf("OnEnable"), Is.LessThan(IndexOf("Start")));
            Assert.That(IndexOf("Start"), Is.LessThan(IndexOf("Update")));
            Assert.That(IndexOf("Start"), Is.LessThan(IndexOf("FixedUpdate")));
            int reloadStart = logs.FindLastIndex(l => l.EndsWith(": Awake"));
            Assert.AreEqual(1, logs.Skip(reloadStart).Count(l => l.EndsWith(": Update")), "Update should log only once.");
            Assert.AreEqual(1, logs.Skip(reloadStart).Count(l => l.EndsWith(": FixedUpdate")), "FixedUpdate should log only once.");
        }

        // ---------- Step 2 ----------
        [UnityTest]
        public IEnumerator Player_StandsStillOnTheStartPlatform()
        {
            RequirePlayer();
            yield return new WaitForSeconds(1.5f);
            Assert.AreEqual(1f, PlayerPosition.y, 0.1f, "Capsule center should rest 1 m above the platform top.");
            Assert.Less(Player.GetComponent<Rigidbody>().linearVelocity.magnitude, 0.1f);
        }

        [UnityTest]
        public IEnumerator Player_RunsAtMoveSpeed([Values(30, 60, 144)] int framesPerSecond)
        {
            RequirePlayer();
            Time.captureFramerate = framesPerSecond;
            yield return new WaitForSeconds(0.5f);
            SetStick(Vector2.left); // left is clear at every checkpoint
            yield return new WaitForSeconds(0.3f);
            var velocity = Player.GetComponent<Rigidbody>().linearVelocity;
            SetStick(Vector2.zero);
            Assert.AreEqual(-RunSpeed, velocity.x, 0.05f);
            Assert.AreEqual(0f, velocity.z, 0.05f);
            Assert.Less(PlayerPosition.x, -1.2f, "The player should have moved left.");
        }

        [UnityTest]
        public IEnumerator Player_JumpReachesExpectedHeight([Values(30, 60, 144)] int framesPerSecond)
        {
            RequirePlayer();
            Time.captureFramerate = framesPerSecond;
            yield return new WaitForSeconds(1f);
            float startY = PlayerPosition.y;
            yield return PressSouthButton();
            var maxY = new[] { startY };
            yield return TrackMaxHeight(1f, maxY);
            Assert.AreEqual(ExpectedJumpHeight, maxY[0] - startY, 0.15f);
            Assert.Less(Vector3.Angle(Player.transform.up, Vector3.up), 1f, "The capsule tipped over.");
        }

        [UnityTest]
        public IEnumerator Player_CannotJumpAgainInMidAir()
        {
            RequirePlayer();
            yield return new WaitForSeconds(1f);
            float startY = PlayerPosition.y;
            yield return PressSouthButton();
            yield return new WaitForSeconds(0.3f);
            yield return PressSouthButton();
            var maxY = new[] { startY };
            yield return TrackMaxHeight(1f, maxY);
            Assert.Less(maxY[0] - startY, ExpectedJumpHeight + 0.15f);
        }

        // Step 2's planned failures must really happen on stage, so test them too.
        [UnityTest]
        public IEnumerator PlannedFailure_CapsuleTipsOverWithoutFrozenRotation()
        {
            RequirePlayer();
            yield return new WaitForSeconds(0.5f);
            // The state before the fixes in Step 2: default friction, no rotation constraints.
            Player.GetComponent<CapsuleCollider>().sharedMaterial = null;
            Player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            SetStick(Vector2.left);
            yield return new WaitForSeconds(0.4f);
            SetStick(Vector2.right);
            float maxTilt = 0f;
            for (float t = 0f; t < 1f; t += Time.deltaTime)
            {
                maxTilt = Mathf.Max(maxTilt, Vector3.Angle(Player.transform.up, Vector3.up));
                yield return null;
            }
            SetStick(Vector2.zero);
            Assert.Greater(maxTilt, 30f, "Without frozen rotation the capsule should visibly tip over.");
        }

        [UnityTest]
        public IEnumerator PlannedFailure_StickyWallsWithoutNoFriction()
        {
            RequirePlayer();
            yield return new WaitForSeconds(0.5f);
            var capsule = Player.GetComponent<CapsuleCollider>();
            var noFriction = capsule.sharedMaterial;
            Assert.IsNotNull(noFriction, "The Player should have the NoFriction material.");

            // In mid-air beside the start platform's back wall (z = -3), pushing into it.
            capsule.sharedMaterial = null;
            Teleport(new Vector3(0f, -1f, -3.55f));
            SetStick(Vector2.up);
            yield return new WaitForSeconds(0.1f);
            float startY = PlayerPosition.y;
            yield return new WaitForSeconds(0.5f);
            float fallWithFriction = startY - PlayerPosition.y;

            capsule.sharedMaterial = noFriction;
            Teleport(new Vector3(0f, -1f, -3.55f));
            yield return new WaitForSeconds(0.1f);
            startY = PlayerPosition.y;
            yield return new WaitForSeconds(0.5f);
            float fallWithoutFriction = startY - PlayerPosition.y;
            SetStick(Vector2.zero);

            Assert.Less(fallWithFriction, 0.5f, "With default friction the player should stick to the wall.");
            Assert.Greater(fallWithoutFriction, 2f, "With NoFriction the player should slide down the wall.");
        }

        // ---------- Step 3 ----------
        [UnityTest]
        public IEnumerator Player_CanClimbTheRamp()
        {
            RequirePlayer();
            var ramp = GameObject.Find("Ramp");
            if (ramp == null || ramp.GetComponent<Collider>() == null)
                Assert.Ignore("No Ramp with a collider in this checkpoint.");
            // Ramps are placed rotated 180 degrees: 6 m long along Z, the slope climbing from 1 m to 4 m
            // above the pivot going +Z, and fed by a block whose top is level with the ramp's foot.
            // Start on that block, 1.5 m before the ramp, so we also test walking onto it.
            var beforeRamp = ramp.transform.position + new Vector3(0f, 2.05f, -4.5f);
            Teleport(beforeRamp);
            yield return new WaitForSeconds(0.2f);
            SetStick(Vector2.up);
            var maxY = new[] { PlayerPosition.y };
            yield return TrackMaxHeight(1.8f, maxY);
            SetStick(Vector2.zero);
            Assert.Greater(maxY[0], ramp.transform.position.y + 4f + 1f - 0.3f, "The player should reach the top of the ramp.");
        }

        // ---------- Step 5 ----------
        [UnityTest]
        public IEnumerator Trap_RestartsTheLevel()
        {
            RequirePlayer();
            var trap = GameObject.Find("Trap");
            if (!HasScript(trap, "KillOnTouch"))
                Assert.Ignore("The Trap is still a placeholder in this checkpoint.");
            Teleport(trap.transform.position + Vector3.up);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(1, arenaReloads, "Touching the trap should reload the Arena.");
        }

        [UnityTest]
        public IEnumerator FallZone_RestartsTheLevel()
        {
            RequirePlayer();
            if (!HasScript(GameObject.Find("FallZone"), "KillOnTouch"))
                Assert.Ignore("The FallZone has no KillOnTouch yet.");
            Teleport(new Vector3(-12f, -5f, 20f)); // over empty space
            yield return new WaitForSeconds(3f);
            Assert.AreEqual(1, arenaReloads, "Falling into the FallZone should reload the Arena.");
        }

        [UnityTest]
        public IEnumerator MovingPlatform_CarriesThePlayer()
        {
            RequirePlayer();
            var platform = GameObject.Find("Platform");
            if (platform == null)
                Assert.Ignore("No moving platform in this checkpoint.");
            Teleport(platform.transform.position + new Vector3(0f, 2.05f, 0f));
            yield return new WaitForSeconds(0.2f);
            Vector3 offset = PlayerPosition - platform.transform.position;
            float travelled = 0f;
            Vector3 last = platform.transform.position;
            for (float t = 0f; t < 2.5f; t += Time.deltaTime)
            {
                yield return null;
                travelled += Vector3.Distance(last, platform.transform.position);
                last = platform.transform.position;
            }
            Vector3 drift = PlayerPosition - platform.transform.position - offset;
            Assert.Greater(travelled, 4f, "The platform should be moving.");
            Assert.Less(new Vector2(drift.x, drift.z).magnitude, 0.3f, "The player slid on the platform instead of riding it.");
            Assert.AreEqual(2f, PlayerPosition.y - platform.transform.position.y, 0.15f, "The player should be standing on the platform.");
        }

        // ---------- Step 6 ----------
        [UnityTest]
        public IEnumerator Coins_CountUpAndCollectingAllLoadsWin()
        {
            RequirePlayer();
            var coins = GameObject.FindGameObjectsWithTag("Coin");
            if (coins.Length == 0 || GameObject.Find("GameManager") == null)
                Assert.Ignore("No coins in this checkpoint.");
            var score = GameObject.Find("ScoreText").GetComponent<TMP_Text>();
            Assert.AreEqual($"Coins: 0 / {coins.Length}", score.text);
            for (int i = 0; i < coins.Length; i++)
            {
                Teleport(coins[i].transform.position + new Vector3(0f, 0.5f, 0f));
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return null;
                if (i < coins.Length - 1)
                {
                    Assert.IsTrue(coins[i] == null, $"Coin {i} should have been collected.");
                    Assert.AreEqual($"Coins: {i + 1} / {coins.Length}", score.text);
                }
            }
            yield return null;
            Assert.AreEqual("Win", SceneManager.GetActiveScene().name, "Collecting every coin should load Win.");
        }

        [UnityTest]
        public IEnumerator Goal_LoadsWin()
        {
            RequirePlayer();
            var goal = GameObject.Find("GoalZone");
            if (goal == null)
                Assert.Ignore("No GoalZone in this checkpoint.");
            Teleport(goal.transform.position + new Vector3(0f, 3.25f, 0f));
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual("Win", SceneManager.GetActiveScene().name);
        }

        // ---------- Final ----------
        [UnityTest]
        public IEnumerator BouncePad_LaunchesThePlayerToTheSkyCoin()
        {
            RequirePlayer();
            var bouncePad = GameObject.Find("BouncePad");
            if (bouncePad == null)
                Assert.Ignore("No BouncePad in this checkpoint.");
            Teleport(bouncePad.transform.position + new Vector3(0f, 2.3f, 0f));
            var maxY = new[] { PlayerPosition.y };
            yield return TrackMaxHeight(1.5f, maxY);
            Assert.Greater(maxY[0], bouncePad.transform.position.y + 6.5f, "The bounce pad should launch the player high.");
        }

        [UnityTest]
        public IEnumerator DemoCourse_BotRunsStartToFinish()
        {
            RequirePlayer();
            if (GameObject.Find("DemoCourse") == null)
                Assert.Ignore("No demo course in this checkpoint.");
            var platform = GameObject.Find("Platform").transform;
            var score = GameObject.Find("ScoreText").GetComponent<TMP_Text>();
            yield return new WaitForSeconds(0.5f);

            // 1) Hop the 2 m gap between the stepping stones, run up the ramp, jump the 3 m gap.
            yield return MoveTo(0f, 8.5f, jumpAtZ: 4.3f);
            yield return MoveTo(0f, 20f, jumpAtZ: 14.2f, timeout: 5f);
            // 2) Wait at the edge of the landing for the moving platform, hop on and ride.
            yield return MoveTo(0f, 23.3f);
            SetStick(Vector2.zero);
            float waited = 0f;
            while (platform.position.z > 26.4f)
            {
                waited += Time.deltaTime;
                Assert.Less(waited, 8f, "The moving platform never came back.");
                yield return null;
            }
            SetStick(Vector2.up);
            yield return new WaitForSeconds(0.3f);
            SetStick(Vector2.zero);
            while (platform.position.z < 32f)
            {
                Assert.Less(waited += Time.deltaTime, 16f, "Stuck on the moving platform.");
                yield return null;
            }
            // 3) Step off into the trap corridor and weave: right, left (past the saw), right.
            yield return MoveTo(1f, 35.8f, timeout: 3f);
            yield return MoveTo(1f, 38.0f);
            yield return MoveTo(-1f, 39.4f);
            yield return MoveTo(-1f, 41.4f);
            yield return MoveTo(1f, 42.9f);
            yield return MoveTo(1f, 46.3f);
            int collected = int.Parse(score.text.Split(' ')[1]);
            Assert.GreaterOrEqual(collected, 7, $"The bot should grab most coins on the way ({score.text}).");
            // 4) Cross the finish line.
            SetStick(Vector2.up);
            yield return new WaitForSeconds(1.5f);
            Assert.AreEqual("Win", SceneManager.GetActiveScene().name, "Reaching the goal should load Win.");
        }
    }
}
