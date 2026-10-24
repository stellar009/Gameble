using UnityEngine;
using UnityEngine.InputSystem;

// Singleton that handles game input via Unity's new Input System.
// Listens for the "Pull" action and tells SpinManager to open the bet panel.
public class GameInputManager : MonoBehaviour
{
    // Singleton access — anyone can read, only this class can set.
    public static GameInputManager Instance { get; private set; }

    private GameInput inputActions;   // Auto-generated C# wrapper for our .inputactions asset
    private SpinManager spinManager;  // Cached reference (avoids FindObjectOfType on every input)

    private void Awake()
    {
        // Singleton guard: if another instance exists, destroy this duplicate.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        inputActions = new GameInput();
        spinManager = FindObjectOfType<SpinManager>();
    }

    private void OnEnable()
    {
        // Enable the "Slot" action map and subscribe to the Pull action.
        inputActions.Slot.Enable();
        inputActions.Slot.Pull.performed += Pull;
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks and destroyed-object errors.
        inputActions.Slot.Pull.performed -= Pull;
        inputActions.Slot.Disable();
    }

    // Called by the Input System when the player performs the "Pull" action.
    void Pull(InputAction.CallbackContext context)
    {
        if (spinManager != null)
            spinManager.EnableBetPanel(true);
    }
}