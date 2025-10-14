using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Renderer))]
public class CharacterSelectable : MonoBehaviour
{
    private Renderer rend;
    private Material[] originalMats;
    private Material[] overlayMats;
    public bool Hovered
    {
        get => _hovered;
        set
        {
            if (_hovered != value)
            {
                _hovered = value;
                UpdateVisual();
            }
        }
    }
    private bool _hovered = false;

    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected != value)
            {
                _selected = value;
                UpdateVisual();
            }
        }
    }
    private bool _selected = false;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        originalMats = rend.sharedMaterials.ToArray();

        if (CharacterSelector.Instance != null)
        {
            PrepareOverlay(CharacterSelector.Instance.overlayDark);
            Selected = false;
            Hovered = false;
        }
    }

    public void PrepareOverlay(Material overlay)
    {
        if (overlay == null)
        {
            Debug.LogError("Overlay material is null.");
            overlayMats = originalMats;
            return;
        }

        overlayMats = new Material[originalMats.Length + 1];
        originalMats.CopyTo(overlayMats, 0);
        overlayMats[originalMats.Length] = overlay;

        rend.sharedMaterials = overlayMats;
    }

    private void UpdateVisual()
    {
        if (Selected || Hovered)
        {
            rend.sharedMaterials = originalMats;
        }
        else
        {
            if (overlayMats == null) PrepareOverlay(CharacterSelector.Instance.overlayDark);

            rend.sharedMaterials = overlayMats;
        }
    }
}