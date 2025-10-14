using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSelector : MonoBehaviour
{
    public static CharacterSelector Instance;

    [Header("Персонажи для выбора")]
    public List<CharacterSelectable> characters = new List<CharacterSelectable>();
    private CharacterSelectable currentHoveredCharacter = null;

    public AudioSource hoverAudio;
    public AudioSource pressedAudio;

    [Header("Материалы")]
    public Material overlayDark;

    private int selectedIndex = -1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    private void Update()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        HandleMouseInput();
#elif UNITY_ANDROID || UNITY_IOS
        HandleTouchInput();
#else
        HandleGamepadInput();
#endif
    }

    // ==================== PC ====================
    private void HandleMouseInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());

        CharacterSelectable newHoveredCharacter = null;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            newHoveredCharacter = hit.collider.GetComponent<CharacterSelectable>();

            if (newHoveredCharacter != null)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    SelectCharacter(newHoveredCharacter);
                }
            }
        }

        if (newHoveredCharacter != currentHoveredCharacter)
        {
            if (currentHoveredCharacter != null)
            {
                currentHoveredCharacter.Hovered = false;
            }

            if (newHoveredCharacter != null)
            {
                hoverAudio.Play();
                newHoveredCharacter.Hovered = true;
            }

            currentHoveredCharacter = newHoveredCharacter;
        }
    }

    // ==================== Touch ====================
    private void HandleTouchInput()
    {
        if (Touchscreen.current == null) return;
        if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 pos = Touchscreen.current.primaryTouch.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(pos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var character = hit.collider.GetComponent<CharacterSelectable>();
                if (character != null)
                {
                    SelectCharacter(character);
                }
            }
        }
    }

    // ==================== Gamepad ====================
    private void HandleGamepadInput()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) return;

        if (characters.Count == 0) return;

        if (gamepad.dpad.left.wasPressedThisFrame)
            MoveSelection(-1);
        else if (gamepad.dpad.right.wasPressedThisFrame)
            MoveSelection(1);
    }

    private void MoveSelection(int direction)
    {
        if (characters.Count == 0) return;

        if (selectedIndex == -1) selectedIndex = 0;
        else
        {
            selectedIndex += direction;
            if (selectedIndex < 0) selectedIndex = characters.Count - 1;
            if (selectedIndex >= characters.Count) selectedIndex = 0;
        }

        SelectCharacter(characters[selectedIndex]);
    }

    private void SelectCharacter(CharacterSelectable character)
    {
        foreach (var c in characters)
        {
            c.Selected = false;
        }

        pressedAudio.Play();
        character.Selected = true;
        selectedIndex = characters.IndexOf(character);
    }

    public CharacterSelectable GetSelectedCharacter()
    {
        if (selectedIndex >= 0 && selectedIndex < characters.Count)
            return characters[selectedIndex];
        return null;
    }
}