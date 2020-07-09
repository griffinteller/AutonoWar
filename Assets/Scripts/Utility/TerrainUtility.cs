using UnityEngine;

namespace Utility
{
    public static class TerrainUtility
    {
        public static Terrain GetClosestCurrentTerrain(Vector3 playerPos)
        {
            //Get all terrain
            var terrains = Terrain.activeTerrains;

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

                if (terrainBounds.Contains(yCenteredPlayerPos)) return terrain;
            }

            return null;
        }

        public static Vector3 GetTerrainNormal(Vector3 worldPosition, float sampleRadius = 1)
        {
            var terrain = GetClosestCurrentTerrain(worldPosition);

            var xLow = terrain.SampleHeight(worldPosition - Vector3.right * sampleRadius);
            var xHigh = terrain.SampleHeight(worldPosition + Vector3.right * sampleRadius);
            var zLow = terrain.SampleHeight(worldPosition - Vector3.forward * sampleRadius);
            var zHigh = terrain.SampleHeight(worldPosition + Vector3.forward * sampleRadius);

            var xDisplacement = new Vector3(sampleRadius * 2, xHigh - xLow, 0);
            var zDisplacement = new Vector3(0, zHigh - zLow, sampleRadius * 2);

            return Vector3.Cross(zDisplacement, xDisplacement).normalized;
        }
    }
}