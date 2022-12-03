using UnityEngine;
using Mesh = UnityEngine.Mesh;

public class MeshView : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshFilter meshFilter;

    public Mesh Mesh
    {
        get => meshFilter.sharedMesh;
        set => meshFilter.sharedMesh = value;
    }

    public Material Material
    {
        get => meshRenderer.sharedMaterial;
        set => meshRenderer.sharedMaterial = value;
    }
}