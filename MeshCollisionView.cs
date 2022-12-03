using System.Linq;
using UnityEngine;

namespace TrenchBroom
{
    public class MeshCollisionView : MonoBehaviour
    {
        public MeshCollider meshCollider;

        public void BuildCollider(Mesh[] meshes)
        {
            var collisionMesh = new Mesh();
            
            var combineInstances = new CombineInstance[meshes.Length];

            for (int i = 0; i < meshes.Length; i++)
                combineInstances[i] = new CombineInstance { mesh = meshes[i], transform = Matrix4x4.identity };
            
            collisionMesh.CombineMeshes(combineInstances.ToArray());
            meshCollider.sharedMesh = collisionMesh;
            name = "Collider";
        }
    }
}