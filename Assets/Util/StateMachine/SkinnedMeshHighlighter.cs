using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshHighlighter : MonoBehaviour
{
    [SerializeField] List<SkinnedMeshRenderer> meshesToHighlight;
    [SerializeField] Material originalMaterial;
    [SerializeField] Material highlightedlMaterial;

    public void HighlightMesh(bool highlight){
        foreach (var mesh in meshesToHighlight)
        {
            mesh.material = (highlight) ? highlightedlMaterial : originalMaterial;
        }
    }
}
