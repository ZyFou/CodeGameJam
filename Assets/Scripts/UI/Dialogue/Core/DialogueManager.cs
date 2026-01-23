using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;

    [Header("Prefab Settings")]
    [Tooltip("Default path to DialoguePopup prefab in Resources folder")]
    [SerializeField] private string defaultPrefabPath = "UI/DialoguePopup";

    [Header("Queue Settings")]
    [Tooltip("Allow dialogues to queue when one is already active")]
    [SerializeField] private bool allowQueue = false;

    private DialoguePopup currentPopup;
    private Queue<QueuedDialogue> dialogueQueue = new Queue<QueuedDialogue>();

    private struct QueuedDialogue
    {
        public string prefabPath;
        public DialogueData data;
        public Dictionary<string, string> variables;
        public Action onComplete;
    }

    /// <summary>
    /// Is a dialogue currently active?
    /// </summary>
    public static bool IsDialogueActive
    {
        get
        {
            if (instance == null) return false;
            return instance.currentPopup != null;
        }
    }

    #region Public API

    /// <summary>
    /// Show a dialogue with the default prefab
    /// </summary>
    /// <param name="data">Dialogue data to display</param>
    /// <param name="variables">Global variables for text substitution</param>
    /// <param name="onComplete">Callback when dialogue finishes</param>
    public static void ShowDialogue(DialogueData data, Dictionary<string, string> variables = null, Action onComplete = null)
    {
        DialogueManager manager = EnsureInstance();
        if (manager != null)
        {
            manager.ShowDialogueInternal(manager.defaultPrefabPath, data, variables, onComplete);
        }
    }

    /// <summary>
    /// Show a dialogue with a custom prefab
    /// </summary>
    /// <param name="prefabPath">Path to custom DialoguePopup prefab in Resources</param>
    /// <param name="data">Dialogue data to display</param>
    /// <param name="variables">Global variables for text substitution</param>
    /// <param name="onComplete">Callback when dialogue finishes</param>
    public static void ShowDialogue(string prefabPath, DialogueData data, Dictionary<string, string> variables = null, Action onComplete = null)
    {
        DialogueManager manager = EnsureInstance();
        if (manager != null)
        {
            manager.ShowDialogueInternal(prefabPath, data, variables, onComplete);
        }
    }

    /// <summary>
    /// Close the currently active dialogue
    /// </summary>
    public static void CloseCurrentDialogue()
    {
        if (instance != null && instance.currentPopup != null)
        {
            instance.currentPopup.Close();
        }
    }

    #endregion

    #region Singleton Management

    private static DialogueManager EnsureInstance()
    {
        if (instance != null) return instance;

        instance = FindObjectOfType<DialogueManager>();
        if (instance == null)
        {
            GameObject go = new GameObject("DialogueManager");
            instance = go.AddComponent<DialogueManager>();
        }

        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    #endregion

    #region Internal Implementation

    private void ShowDialogueInternal(string prefabPath, DialogueData data, Dictionary<string, string> variables, Action onComplete)
    {
        if (data == null)
        {
            Debug.LogError("DialogueManager: Cannot show dialogue with null data.");
            onComplete?.Invoke();
            return;
        }

        if (currentPopup != null)
        {
            if (allowQueue)
            {
                // Queue the dialogue for later
                dialogueQueue.Enqueue(new QueuedDialogue
                {
                    prefabPath = prefabPath,
                    data = data,
                    variables = variables,
                    onComplete = onComplete
                });
                Debug.Log($"DialogueManager: Dialogue queued. Queue size: {dialogueQueue.Count}");
                return;
            }
            else
            {
                Debug.LogWarning("DialogueManager: Dialogue already active. Closing current dialogue.");
                CloseCurrentDialogue();
            }
        }

        SpawnDialoguePopup(prefabPath, data, variables, onComplete);
    }

    private void SpawnDialoguePopup(string prefabPath, DialogueData data, Dictionary<string, string> variables, Action onComplete)
    {
        // Load prefab from Resources
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"DialogueManager: Failed to load prefab at 'Resources/{prefabPath}'. Make sure the prefab exists in the Resources folder.");
            onComplete?.Invoke();
            return;
        }

        // Instantiate the prefab
        GameObject popupInstance = Instantiate(prefab);
        currentPopup = popupInstance.GetComponent<DialoguePopup>();

        if (currentPopup == null)
        {
            Debug.LogError("DialogueManager: Prefab does not have DialoguePopup component.");
            Destroy(popupInstance);
            onComplete?.Invoke();
            return;
        }

        // Initialize the dialogue
        currentPopup.Initialize(data, variables, () => OnDialogueComplete(onComplete));
    }

    private void OnDialogueComplete(Action userCallback)
    {
        currentPopup = null;
        userCallback?.Invoke();

        // Process queue if enabled
        if (allowQueue && dialogueQueue.Count > 0)
        {
            QueuedDialogue next = dialogueQueue.Dequeue();
            SpawnDialoguePopup(next.prefabPath, next.data, next.variables, next.onComplete);
        }
    }

    #endregion
}
