using System.IO;
using JetBrains.Annotations;
using UI;
using UnityEngine;
using Utility;

namespace Main.Building
{
    public class BuildHandler : MonoBehaviour
    {

        public bool interactable = true;
        public TireNameInputHandler tireNameInputHandler;
        
        public GameObject currentItemPrefab;

        private BuildObjectComponent _currentItemBuildComponent;

        private GameObject _objectLastClickedOn;

        public void Start()
        {

            SetCurrentObject(currentItemPrefab); // updates the build component reference

        }

        public void SetCurrentObject(GameObject prefab)
        {

            currentItemPrefab = prefab;
            _currentItemBuildComponent = currentItemPrefab.GetComponent<BuildObjectComponent>();
            
        }

        public void Update()
        {

            if (!interactable) return;

            if (!Input.GetKey(KeyCode.LeftShift)) return;

            if (Input.GetMouseButtonDown(0))
            {
                RegisterMouseDown(); // we want to make sure we don't drag and then place.
            }

            if (Input.GetMouseButtonUp(0))
            {
                PlaceItemIfPossible();
            }
            else if (Input.GetMouseButtonUp(1))
            {
                RemoveItemIfPossible();
            }

        }

        private void RegisterMouseDown()
        {

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit))
            {

                _objectLastClickedOn = hit.collider.gameObject;

            }

        }

        private void PlaceItemIfPossible()
        {
            
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!BuildObjectRaycast(mouseRay, out var hitInfo, out var hitBuildComponent))
                return;

            if (_objectLastClickedOn && 
                hitInfo.collider.gameObject.GetInstanceID() != _objectLastClickedOn.GetInstanceID())
                return;

            var connection = hitBuildComponent.GetConnection(hitInfo.point);
            
            if (!connection)
                return;  // in deadZone or on illegal face

            // we can now be sure we are placing a new item
            
            var newObjectRotation = 
                Quaternion.FromToRotation(_currentItemBuildComponent.GetConnectingFaceOutwardsDirection(),
                -connection.outwardsDirection); // rotate the item so that the two connecting faces are facing each other
            
            var newObjectCenter = connection.connectionCenter;
            newObjectCenter += _currentItemBuildComponent.GetRadius() * connection.outwardsDirection;
            //displaces by radius of the object
            
            Instantiate(currentItemPrefab, newObjectCenter, newObjectRotation, transform);

            var possibleTireComponent = _currentItemBuildComponent as BuildTireComponent;
            if (possibleTireComponent)
            {
                
                Debug.Log("Naming!");
                tireNameInputHandler.ShowInputWindow(possibleTireComponent);
                
            }

        }

        private static void RemoveItemIfPossible()
        {
            
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (!BuildObjectRaycast(mouseRay, out var hitInfo, out var bc) || !bc.removable) return;
            
            Destroy(hitInfo.transform.gameObject);
            
        }

        private static bool BuildObjectRaycast(Ray ray, out RaycastHit hitInfo, out BuildObjectComponent buildComponent)
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
        
        private Vector3 GetLocalCenterOfMass()
        {

            var rigidbody = gameObject.AddComponent<Rigidbody>();
            var com = rigidbody.centerOfMass;
            Destroy(rigidbody);
            return com;

        }

        /*private string GetOrCreateRobotsDirectory()
        {

            var dataDirectory = SystemUtility.GetAndCreateDataDirectory();
            var robotsDirectory = dataDirectory + 

        }*/

    }

}
