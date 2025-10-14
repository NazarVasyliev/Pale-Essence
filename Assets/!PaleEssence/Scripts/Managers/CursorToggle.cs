using UnityEngine;

public class CursorToggle : MonoBehaviour
{
    private bool cursorVisible = false;

    void Start()
    {
        SetCursorState(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorVisible = !cursorVisible;
            SetCursorState(cursorVisible);
        }
    }

    void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
