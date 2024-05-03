using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class WalkerGenerator : MonoBehaviour
{
    public Grid[,] gridHandler;
    public List<WalkerObject> Walkers;
    public Tilemap tileMap;
    public Tile Floor;
    public Tile Wall;
    public int MapWidth = 30;
    public int MapHeight = 30;

    public int MaxWalkersCount = 10;
    public int TileCount = default;
    public float FillPercentage = .4f;
    public float WaitTime = .05f;

    private void Start()
    {
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        gridHandler = new Grid[MapWidth, MapHeight];

        for (int x = 0; x < gridHandler.GetLength(0); x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1); y++)
            {
                gridHandler[x, y] = Grid.Empty;
            }
        }

        Walkers = new List<WalkerObject>();

        var tileCenter = new Vector3Int(gridHandler.GetLength(0) / 2, gridHandler.GetLength(1) / 2, 0);

        var currentWalker = new WalkerObject(new Vector2(tileCenter.x, tileCenter.y), GetDirection(), .5f);
        gridHandler[tileCenter.x, tileCenter.y] = Grid.Floor;
        Walkers.Add(currentWalker);

        TileCount++;

        StartCoroutine(CreateFloors());
    }

    private Vector2 GetDirection()
    {
        var choice = Mathf.FloorToInt(UnityEngine.Random.value * 3.99f);

        return choice switch
        {
            0 => Vector2.down,
            1 => Vector2.left,
            2 => Vector2.up,
            3 => Vector2.right,
            _ => Vector2.zero
        };
    }

    IEnumerator CreateFloors()
    {
        while ((float)TileCount / (float)gridHandler.Length < FillPercentage)
        {
            bool hasCreatedFloor = false;
            foreach (WalkerObject curWalker in Walkers)
            {
                Vector3Int curPos = new Vector3Int((int)curWalker.Position.x, (int)curWalker.Position.y, 0);

                if (gridHandler[curPos.x, curPos.y] != Grid.Floor)
                {
                    tileMap.SetTile(curPos, Floor);
                    TileCount++;
                    gridHandler[curPos.x, curPos.y] = Grid.Floor;
                    hasCreatedFloor = true;
                }
            }

            //Walker Methods
            ChanceToRemove();
            ChanceToRedirect();
            ChanceToCreate();
            UpdatePosition();

            if (hasCreatedFloor)
            {
                yield return new WaitForSeconds(WaitTime);
            }
        }

        StartCoroutine(CreateWalls());
    }

    IEnumerator CreateWalls()
    {
        for (int x = 0; x < gridHandler.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1) - 1; y++)
            {
                if (gridHandler[x, y] == Grid.Floor)
                {
                    bool hasCreatedWall = false;

                    if (gridHandler[x + 1, y] == Grid.Empty)
                    {
                        tileMap.SetTile(new Vector3Int(x + 1, y, 0), Wall);
                        gridHandler[x + 1, y] = Grid.Wall;
                        hasCreatedWall = true;
                    }
                    if (gridHandler[x - 1, y] == Grid.Empty)
                    {
                        tileMap.SetTile(new Vector3Int(x - 1, y, 0), Wall);
                        gridHandler[x - 1, y] = Grid.Wall;
                        hasCreatedWall = true;
                    }
                    if (gridHandler[x, y + 1] == Grid.Empty)
                    {
                        tileMap.SetTile(new Vector3Int(x, y + 1, 0), Wall);
                        gridHandler[x, y + 1] = Grid.Wall;
                        hasCreatedWall = true;
                    }
                    if (gridHandler[x, y - 1] == Grid.Empty)
                    {
                        tileMap.SetTile(new Vector3Int(x, y - 1, 0), Wall);
                        gridHandler[x, y - 1] = Grid.Wall;
                        hasCreatedWall = true;
                    }

                    if (hasCreatedWall)
                    {
                        yield return new WaitForSeconds(WaitTime);
                    }
                }
            }
        }
    }

    void ChanceToRemove()
    {
        int updatedCount = Walkers.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count > 1)
            {
                Walkers.RemoveAt(i);
                break;
            }
        }
    }

    void ChanceToRedirect()
    {
        for (int i = 0; i < Walkers.Count; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange)
            {
                WalkerObject curWalker = Walkers[i];
                curWalker.Direction = GetDirection();
                Walkers[i] = curWalker;
            }
        }
    }

    void ChanceToCreate()
    {
        int updatedCount = Walkers.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count < MaxWalkersCount)
            {
                Vector2 newDirection = GetDirection();
                Vector2 newPosition = Walkers[i].Position;

                WalkerObject newWalker = new WalkerObject(newPosition, newDirection, 0.5f);
                Walkers.Add(newWalker);
            }
        }
    }

    void UpdatePosition()
    {
        for (int i = 0; i < Walkers.Count; i++)
        {
            WalkerObject FoundWalker = Walkers[i];
            FoundWalker.Position += FoundWalker.Direction;
            FoundWalker.Position.x = Mathf.Clamp(FoundWalker.Position.x, 1, gridHandler.GetLength(0) - 2);
            FoundWalker.Position.y = Mathf.Clamp(FoundWalker.Position.y, 1, gridHandler.GetLength(1) - 2);
            Walkers[i] = FoundWalker;
        }
    }

    public enum Grid
    {
        Floor,
        Wall,
        Empty
    }
}