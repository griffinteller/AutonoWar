using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UI;
using UnityEngine;
using Utility;

namespace Building
{
    public class BuildHandler : MonoBehaviour
    {
        private const string RobotFileName = "robot.json";

        private BuildObjectComponent _currentItemBuildComponent;
        private GameObject _objectLastClickedOn;
        private string originalName;
        public GameObject currentItemPrefab;

        public bool interactable; // can the player currently place blocks?
        public TireNameInputHandler tireNameInputHandler;

        public void Start()
        {
            SetCurrentObject(currentItemPrefab); // updates the build component reference
            var buildData = FindObjectOfType<CrossSceneDataContainer>();
            originalName = (string) buildData.data["robotName"];  // if we renamed, this was the original name
        }

        public void SetCurrentObject(GameObject prefab)
        {
            currentItemPrefab = prefab;
            _currentItemBuildComponent = currentItemPrefab.GetComponent<BuildObjectComponent>();
        }

        public void Update()
        {
            if (!interactable)
                return;

            if (!Input.GetKey(KeyCode.LeftShift))
                return;

            if (Input.GetMouseButtonDown(0)) RegisterMouseDown(); // we want to make sure we don't drag and then place.

            if (Input.GetMouseButtonUp(0))
                PlaceItemIfPossible();
            else if (Input.GetMouseButtonUp(1)) RemoveItemIfPossible();
        }

        private void RegisterMouseDown()
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit))
                _objectLastClickedOn = hit.collider.gameObject;
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
                return; // in deadZone or on illegal face

            // we can now be sure we are placing a new item

            var newObjectRotation =
                Quaternion.FromToRotation(_currentItemBuildComponent.GetConnectingFaceOutwardsDirection(),
                    -connection
                        .outwardsDirection); // rotate the item so that the two connecting faces are facing each other

            var newObjectCenter = connection.connectionCenter;
            newObjectCenter += _currentItemBuildComponent.GetRadius() * connection.outwardsDirection;
            //displaces by radius of the object

            // is this a tire, and if it is, name it

            var obj = Instantiate(currentItemPrefab, newObjectCenter, newObjectRotation, transform);
            var instantiatedBuildComponent = obj.GetComponent<BuildObjectComponent>();

            var possibleTireComponent = instantiatedBuildComponent as BuildTireComponent;
            if (possibleTireComponent)
            {
                Debug.Log("Naming!");
                tireNameInputHandler.ShowInputWindow(possibleTireComponent);
            }
        }

        private List<BuildObjectComponent> LoadParts()
        {
            var parts = new List<BuildObjectComponent>();

            foreach (Transform child in transform) parts.Add(child.GetComponent<BuildObjectComponent>());

            return parts;
        }

        private void RemoveItemIfPossible()
        {
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!BuildObjectRaycast(mouseRay, out var hitInfo, out var bc) || !bc.removable)
                return;

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

        private Vector3 GetWorldCenterOfMass()
        {
            var rigidbody = gameObject.AddComponent<Rigidbody>();
            var com = rigidbody.centerOfMass;
            com = transform.TransformPoint(com);
            Destroy(rigidbody);
            return com;
        }

        public void SaveDesign(string robotName)
        {
            if (originalName != null)  // if we aren't creating a new robot, delete the old file
                File.Delete(SystemUtility.GetAndCreateRobotsDirectory() + originalName + ".json");
            
            var robotCenterOfMass = GetWorldCenterOfMass();
            var parts = LoadParts();

            var structure = new RobotStructure(robotName, parts, robotCenterOfMass);
            var structureJson = JsonUtility.ToJson(structure);

            var file = new FileStream(
                SystemUtility.GetAndCreateRobotsDirectory() + robotName + ".json", 
                FileMode.Create,
                FileAccess.Write);
            var writer = new StreamWriter(file);

            writer.Write(structureJson);
            writer.Close();
            
            PlayerPrefs.SetString(PlayerPrefKeys.SelectedRobotNameKey, robotName); // set selected robot

            originalName = robotName;  // if we do a second rename, we must delete our new file
        }
    }
}