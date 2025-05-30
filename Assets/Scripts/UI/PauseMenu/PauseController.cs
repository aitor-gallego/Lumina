using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    public GameObject ui;
    public InputActionReference input;
    public InputActionReference closeinput;
    public GameObject player;
    private bool paused = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void Awake()
    {
        input.action.Enable();
        closeinput.action.Disable();
        ui.SetActive(false);
    }

    void OnEnable()
    {
        input.action.performed += OnOpenPressed;
        input.action.Enable();

        closeinput.action.performed += OnClosePressed;
        closeinput.action.Enable();
    }

    void OnDisable()
    {
        input.action.performed -= OnOpenPressed;
        input.action.Disable();

        closeinput.action.performed -= OnClosePressed;
        closeinput.action.Disable();
    }
    private void OnOpenPressed(InputAction.CallbackContext context)
    {
        if (!paused)
        {
            Pause();
            input.action.Disable();
            closeinput.action.Enable();
        }
    }

    private void OnClosePressed(InputAction.CallbackContext context)
    {
        if (paused)
        {
            Resume();
            input.action.Enable();
            closeinput.action.Disable();
        }
    }

    public void Pause()
    {
        ui.SetActive(true);
        Time.timeScale = 0f;
        paused = true;
        PlayerController.inputBlocked = true;

        if (AudioController.Instance != null)
            AudioController.Instance.SetPausedState(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(ui.GetComponentInChildren<UnityEngine.UI.Button>().gameObject);
    }

    // botones
    public void Resume()
    {
        ui.SetActive(false);
        Time.timeScale = 1f;
        paused = false;
        PlayerController.inputBlocked = false;

        if (AudioController.Instance != null)
            AudioController.Instance.SetPausedState(false);

        input.action.Enable();
        closeinput.action.Disable();
    }

    public void Reset()
    {
        player.GetComponent<PlayerController>().Reset();
        Resume();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
