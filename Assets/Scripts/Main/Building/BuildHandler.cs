using JetBrains.Annotations;
using UnityEngine;

namespace Main.Building
{
    public class BuildHandler : MonoBehaviour
    {

        public GameObject currentItemPrefab;

        private BuildObjectComponent _currentItemBuildComponent;

        public void Start()
        {

            _currentItemBuildComponent = currentItemPrefab.GetComponent<BuildObjectComponent>();

        }

        public void Update()
        {

            if (Input.GetMouseButtonUp(0))
            {
                PlaceItemIfPossible();
            }
            else if (Input.GetMouseButtonUp(1))
            {
                RemoveItemIfPossible();
            }

        }

        private void PlaceItemIfPossible()
        {
            
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!BuildObjectRaycast(mouseRay, out var hitInfo, out var hitBuildComponent)) return;

            var connection = hitBuildComponent.GetConnection(hitInfo.point);
            if (!connection) return;  // in deadzone or on illegal face

            // we can now be sure we are placing a new item
            
            var newObjectRotation = Quaternion.FromToRotation(_currentItemBuildComponent.connectingFaceOutwardsDirection,
                -connection.outwardsDirection); // rotate the item so that the two connecting faces are facing each other
            
            Instantiate(currentItemPrefab, connection.newObjectCenter, newObjectRotation, transform);
            
        }

        private void RemoveItemIfPossible()
        {
            
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (!BuildObjectRaycast(mouseRay, out var hitInfo, out var bc) || !bc.removable) return;
            
            Destroy(hitInfo.transform.gameObject);
            
        }

        private static bool BuildObjectRaycast(Ray ray, out RaycastHit hitInfo, [CanBeNull] out BuildObjectComponent buildComponent)
        {

            if (!Physics.Raycast(ray, out var hit))
            {
                buildComponent = null;
                hitInfo = hit;
                return false;
            }

            var hitObject = hit.transform.gameObject;
            var hitBuildComponent = hitObject.GetComponent<BuildObjectComponent>();

            if (!hitBuildComponent)
            {
                buildComponent = null;
                hitInfo = hit;
                return false;
            }

            buildComponent = hitBuildComponent;
            hitInfo = hit;
            return true;
            
        }

    }
}
