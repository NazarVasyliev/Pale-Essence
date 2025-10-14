using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject menuPanel;
    private InputAction menuInput;
    private bool isPaused = false;

    void Start()
    {
        menuInput = InputSystem.actions.FindAction("Menu");
        if (menuPanel != null)
            menuPanel.SetActive(false);

        Resume();
    }

    void Update()
    {
        if (menuInput.WasPressedThisFrame())
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (menuPanel != null)
            menuPanel.SetActive(true);

        StartCoroutine(ShowCursorAfterFrame());
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (menuPanel != null)
            menuPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    IEnumerator ShowCursorAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
