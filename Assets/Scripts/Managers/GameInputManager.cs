using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance;

    private GameInput inputActions;

    private void Awake()
    {
        inputActions = new GameInput();

        if (!Instance)
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        inputActions.Slot.Enable();

        inputActions.Slot.Pull.performed += Pull;
    }

    private void OnDisable()
    {
        inputActions.Slot.Pull.performed -= Pull;

        inputActions.Slot.Disable();
    }

    void Pull(InputAction.CallbackContext context)
    {
        FindObjectOfType<SpinManager>().EnableBetPanel(true);
    }
}
