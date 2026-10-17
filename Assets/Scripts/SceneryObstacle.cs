using System.Linq;
using UnityEngine;

namespace LostDream
{
    // Saved by the editor: no runtime scene scanning or map rebuilding.
    [DisallowMultipleComponent]
    public class SceneryObstacle : MonoBehaviour
    {
        public string category;
        public MeshCollider[] shapes;
        public bool Solid => shapes != null && shapes.Length > 0 &&
            shapes.All(c => c != null && c.enabled && !c.isTrigger && c.sharedMesh != null && c.gameObject.layer == 0);
        public Bounds WorldBounds
        {
            get
            {
                Bounds bounds = shapes[0].bounds;
                foreach (var shape in shapes.Skip(1)) bounds.Encapsulate(shape.bounds);
                return bounds;
            }
        }
    }
}
