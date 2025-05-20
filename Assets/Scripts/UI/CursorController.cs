using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (Mouse.current != null)
            InputSystem.DisableDevice(Mouse.current);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
