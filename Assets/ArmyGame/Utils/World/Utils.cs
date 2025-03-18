using UnityEngine;

namespace Logic.World
{
    public class Utils
    {

        public static bool IsWithinZone(Grid grid, BoundsInt zone, Transform target)
        {
            var targetCellPosition = grid.WorldToCell(target.position);
            return zone.Contains(targetCellPosition);
        }

        public static Vector3 GetRandomWorldPositionInZone(Grid grid, BoundsInt zone)
        {
            return grid.CellToWorld(Randomizer.Randomize(zone.min, zone.max));
        }
        
        public static class Randomizer
        {
            public static Vector2 Randomize(Vector2 min, Vector2 max)
            {
                return new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
            }

            public static Vector3 Randomize(Vector3 min, Vector3 max)
            {
                return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
            } 
            public static Vector3Int Randomize(Vector3Int min, Vector3Int max)
            {
                return new Vector3Int(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
            }

            public static float GenerateRandom()
            {
                return Random.Range(0f, 1f);
            }
        }
    }
}