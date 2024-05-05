using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralGeneration
{
    public static class PropGenerator
    {
        public static void GenerateProps(int minPropCount, int maxPropCount, TilemapVisualiser visualiser)
        {
            var rooms = RootData.RootInstance.LevelGenerationData.RoomsList;
            foreach (var room in rooms)
            {
                var propsToGenerateCount = Random.Range(minPropCount, maxPropCount);
                GeneratePropsForSingleRoom(room.Value.Floor, propsToGenerateCount, visualiser);
            }
        }

        private static void GeneratePropsForSingleRoom(HashSet<Vector2Int> floor, int propsToGenerateCount,
            TilemapVisualiser visualiser)
        {
            int maxX, maxY;
            int minX = maxX = floor.FirstOrDefault().x;
            int minY = maxY = floor.FirstOrDefault().y;
            int halfPropsCount = propsToGenerateCount / 2;

            HashSet<Vector2Int> minYTiles = new(), maxYTiles = new();
            HashSet<Vector2Int> minXTiles = new(), maxXTiles = new();

            var chanceToPaint = RootData.RootInstance.LevelGenerationData.ChanceToPaintProp;

            foreach (var tile in floor)
            {
                if (tile.x < minX) minX = tile.x;
                if (tile.x > maxX) maxX = tile.x;

                if (tile.y < minY) minY = tile.y;
                if (tile.y > maxY) maxY = tile.y;
            }

            foreach (var tile in floor)
            {
                if (tile.x == minX) minXTiles.Add(tile);
                else if (tile.x == maxX) maxXTiles.Add(tile);
            }

            foreach (var tile in floor)
            {
                if (tile.y == minY) minYTiles.Add(tile);
                else if (tile.y == maxY) maxYTiles.Add(tile);
            }

            TryGenerateProps(minXTiles, maxXTiles, propsToGenerateCount, halfPropsCount, chanceToPaint, visualiser);
            TryGenerateProps(minYTiles, maxYTiles, propsToGenerateCount, 0, chanceToPaint, visualiser);
        }

        private static void TryGenerateProps(HashSet<Vector2Int> minTileSet, HashSet<Vector2Int> maxTileSet,
            int propCount, int generationEndCount, float chanceToPaint, TilemapVisualiser visualiser)
        {
            while (propCount > generationEndCount)
            {
                HashSet<Vector2Int> excludeList = new();
                foreach (var tile in minTileSet)
                {
                    if (Random.Range(0, 100) > chanceToPaint / 2) continue;
                    visualiser.PaintProp(tile);
                    excludeList.Add(tile);
                    propCount--;
                }

                minTileSet.ExceptWith(excludeList);
                excludeList.Clear();

                if (propCount <= 0) break;

                foreach (var tile in maxTileSet)
                {
                    if (Random.Range(0, 100) > chanceToPaint / 2) continue;
                    visualiser.PaintProp(tile);
                    excludeList.Add(tile);
                    propCount--;
                }

                maxTileSet.ExceptWith(excludeList);
            }
        }
    }
}