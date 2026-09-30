using UnityEngine;

public class LifecycleLogger : MonoBehaviour
{
    [Header("Logging")]
    [Tooltip("Off: Update and FixedUpdate log only once. On: they log every time they run, so you can compare how often each one happens.")]
    [SerializeField] private bool logEveryFrame = false;

    // Keeps Update and FixedUpdate from flooding the Console.
    private bool hasLoggedUpdate = false;
    private bool hasLoggedFixedUpdate = false;

    private void Awake()
    {
        Log("Awake");
    }

    private void OnEnable()
    {
        Log("OnEnable");
    }

    private void Start()
    {
        Log("Start");
    }

    private void Update()
    {
        if (logEveryFrame || !hasLoggedUpdate)
        {
            Log("Update");
            hasLoggedUpdate = true;
        }
    }

    private void FixedUpdate()
    {
        if (logEveryFrame || !hasLoggedFixedUpdate)
        {
            Log("FixedUpdate");
            hasLoggedFixedUpdate = true;
        }
    }

    private void OnDisable()
    {
        Log("OnDisable");
    }

    private void Log(string eventName)
    {
        Debug.Log("[Frame " + Time.frameCount + "] " + gameObject.name + ": " + eventName);
    }
}
