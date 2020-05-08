using Building;
using UnityEngine;

namespace UI
{
    public class BuildingPartChanger : MonoBehaviour
    {

        public BuildHandler buildHandler;

        public void SetCurrentItem(BuildObjectComponent buildObjectPrefab)
        {

            buildHandler.SetCurrentObject(buildObjectPrefab.gameObject);

        }

    }
}