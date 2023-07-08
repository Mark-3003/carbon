using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public SpriteRenderer debugRENDER;

    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;
    public bool reRender;

    [Range(0, 100)]
    public int fillPercentage;

    int[,] map;
    Texture2D tx;
    void Start()
    {
        GenerateMap();
    }
    void Update()
    {
        if (reRender)
        {
            reRender = false;
            GenerateMap();
        }
    }
    void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();
       
        for(int i = 0; i < 5; i++)
        {
            SmoothMap();
        }
    }
    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        System.Random rng = new System.Random(seed.GetHashCode());

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                map[x, y] = (rng.Next(0, 100) < fillPercentage) ? 1 : 0;
            }
        }
        DebugImage();
    }
    void SmoothMap()
    {
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                int surroundingTiles = GetSurroundingWallCount(x, y);

                if(surroundingTiles > 4)
                {
                    map[x, y] = 1;
                }
                else if(surroundingTiles < 4)
                {
                    map[x, y] = 0;
                }
            }
        }
        DebugImage();
    }
    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for(int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
        {
            for(int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
            {
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }
    void DebugImage()
    {
        if(map != null)
        {
            tx = new Texture2D(width, height);
            tx.filterMode = FilterMode.Point;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tx.SetPixel(x, y, (map[x, y] == 1) ? Color.black : Color.white);
                }
            }
            tx.Apply();
            debugRENDER.sprite = Sprite.Create(tx, new Rect(0, 0, tx.width, tx.height), new Vector2(0.5f, 0.5f));
        }
    }
}
