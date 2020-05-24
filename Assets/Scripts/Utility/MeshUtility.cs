using System.Runtime.InteropServices;
using UnityEngine;

namespace Utility
{
    public static class MeshUtility
    {

        public static Mesh GetCombinedMeshes(MeshFilter[] meshFilters, Transform parent)
        {
            
            var combineInstances = new CombineInstance[meshFilters.Length];
            for (var i = 0; i < meshFilters.Length; i++)
            {
                combineInstances[i] = new CombineInstance();
                combineInstances[i].mesh = meshFilters[i].sharedMesh;

                var objectTransform = meshFilters[i].transform;
                
                var positionRelativeToParent =
                    parent.InverseTransformPoint(objectTransform.position);
                var rotationRelativeToParent = Quaternion.Inverse(parent.rotation) * objectTransform.rotation;
                var scaleRelativeToParent = objectTransform.localScale;

                var matrix = Matrix4x4.TRS(
                    positionRelativeToParent,
                    rotationRelativeToParent,
                    scaleRelativeToParent);

                combineInstances[i].transform = matrix;
            }
            
            var result = new Mesh();
            result.CombineMeshes(combineInstances);
            return result;
        }

    }
}