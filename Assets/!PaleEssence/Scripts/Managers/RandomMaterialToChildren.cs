using UnityEngine;
using System.Collections.Generic;

public class ApplyRandomMaterialToChildren : MonoBehaviour
{
    [SerializeField]
    private List<Material> materials = new List<Material>();

    void Start()
    {
        if (materials.Count == 0)
        {
            return;
        }
        int randomIndex = Random.Range(0, materials.Count);
        Material selectedMaterial = materials[randomIndex];
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>(includeInactive: false);

        int appliedCount = 0;
        foreach (Renderer renderer in childRenderers)
        {
            if (renderer.transform == this.transform)
            {
                continue;
            }
            renderer.material = selectedMaterial;
            appliedCount++;
        }
    }
}