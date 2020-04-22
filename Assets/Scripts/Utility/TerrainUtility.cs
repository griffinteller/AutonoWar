using JetBrains.Annotations;
using UnityEngine;

namespace Utility
{
    public class TerrainUtility
    {
        
        [CanBeNull]
        public static Terrain GetClosestCurrentTerrain(Vector3 playerPos)
        {
            //Get all terrain
            Terrain[] terrains = Terrain.activeTerrains;

            //Make sure that terrains length is ok
            if (terrains.Length == 0)
                return null;

            //If just one, return that one terrain
            if (terrains.Length == 1)
                return terrains[0];

            foreach (var terrain in terrains)
            {

                var terrainBounds = terrain.terrainData.bounds;
                var yCenteredPlayerPos = terrain.transform.InverseTransformPoint(playerPos);
                yCenteredPlayerPos.y = terrainBounds.center.y;

                if (terrainBounds.Contains(yCenteredPlayerPos))
                {
                    
                    return terrain;

                }
                
            }

            return null;

        }
        
    }
}