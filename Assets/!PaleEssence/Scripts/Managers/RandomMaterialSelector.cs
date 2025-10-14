using UnityEngine;
using System.Collections.Generic;

public class RandomMaterialSelector : MonoBehaviour
{
    [SerializeField]
    private List<Material> materials = new List<Material>();
    private Renderer objectRenderer;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer == null)
        {
            return;
        }

        if (materials.Count == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, materials.Count);
        Material randomMaterial = materials[randomIndex];
        objectRenderer.material = randomMaterial;
    }
}