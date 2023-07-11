using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class MapGenerator : MonoBehaviour
{
    public SpriteRenderer debugRENDER;
    public SpriteRenderer debugFinalRENDER;
    public SpriteRenderer perlinRENDER;

    public int width;
    public int height;
    
    public float seed;
    public bool useRandomSeed;
    public bool reRender;

    [Range(0.5f, 1f)]
    public float renderScale;
    [Range(30, 60)]
    public int fillPercentage;
    [Range(10, 100)]
    public int resourceCutoff;

    int[,] map;
    int[,] finalMap;
    Texture2D tx;
    TileList tiles;
    int mapQuality = 2;
    void Awake(){
        tiles = GetComponent<TileList>();
    }
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
        debugRENDER.transform.localScale = new Vector2(20, 20) * renderScale;
        debugFinalRENDER.transform.localScale = new Vector2(20, 20) * renderScale;
        perlinRENDER.transform.localScale = new Vector2(20,20) * renderScale;
    }
    void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();
       
        for(int i = 0; i < 5; i++)
        {
            map = SmoothMap(map, width, height, 1);
        }
        RaiseMapQuality();
        AddSandToMap(1);
        //GenerateResources(finalMap);
        //DebugImage(debugRENDER, map, width, height);
        //DebugImage(debugFinalRENDER, finalMap, width * mapQuality, height * mapQuality);
        SendToTilemap(tiles.map, finalMap, width * mapQuality, height * mapQuality);
    }
    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = (Time.time + System.DateTime.Now.Millisecond);
        }
        System.Random rng = new System.Random(seed.GetHashCode());

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                map[x, y] = (rng.Next(0, 100) < fillPercentage) ? 1 : 0;
            }
        }
    }
    int[,] SmoothMap(int[,] _map, int _mapWidth, int _mapHeight, int radius)
    {
        int[,] _newMap = _map;
        for(int y = 0; y < _mapHeight; y++)
        {
            for(int x = 0; x < _mapWidth; x++)
            {
                int surroundingTiles = GetSurroundingWallCount(_map, x, y, _mapWidth, _mapHeight, radius);

                int gridSize = (radius + radius + 1) * (radius + radius + 1);
                int thresHold = (gridSize - 1) / 2;
                if(surroundingTiles > thresHold)
                {
                    _map[x, y] = 1;
                }
                else if(surroundingTiles < thresHold)
                {
                    _map[x, y] = 0;
                }
            }
        }
        return _newMap;
    }
    void AddSandToMap(int radius){

        for(int y = 0; y < height * mapQuality; y++)
        {
            for(int x = 0; x < width * mapQuality; x++)
            {
                int surroundingTiles = GetSurroundingWallCount(finalMap, x, y, width * mapQuality, height * mapQuality, 1);

                int gridSize = (radius + radius + 1) * (radius + radius + 1);
                if(surroundingTiles <= gridSize - 2 && finalMap[x,y] == 1)
                {
                    finalMap[x, y] = 2;
                }
            }
        }
    }
    void RaiseMapQuality(){
        finalMap = new int[width * mapQuality, height * mapQuality];

        for(int y = 0; y < height * mapQuality; y++)
        {
            for(int x = 0; x < width * mapQuality; x++)
            {
                finalMap[x, y] = map[Mathf.FloorToInt(x / mapQuality), Mathf.FloorToInt(y / mapQuality)];
            }
        }
        finalMap = SmoothMap(finalMap, width * mapQuality, height * mapQuality, 1);
        finalMap = SmoothMap(finalMap, width * mapQuality, height * mapQuality, 2);
    }
    void GenerateResources(int[,] _map){
        float[,] noiseMap = new float[width * mapQuality, height * mapQuality];
        Texture2D _tx = new Texture2D(width * mapQuality, height * mapQuality);

        for(int y = 0; y < height * mapQuality; y++){

            for(int x = 0; x < width * mapQuality; x++){

                noiseMap[x, y] = Mathf.PerlinNoise(x + seed, y + seed);

                if(noiseMap[x,y] >= resourceCutoff && _map[x,y] == 0){
                    noiseMap[x,y] = 1;
                }
                else{
                    noiseMap[x,y] = 0;
                }
                Color _color = new Color(noiseMap[x,y] * 255f, noiseMap[x,y] * 255f, noiseMap[x,y] * 255f);
                _tx.SetPixel(x, y, _color);
            }
        }
        _tx.Apply();
        perlinRENDER.sprite = Sprite.Create(_tx, new Rect(0, 0, _tx.width, _tx.height), new Vector2(0.5f, 0.5f));
    }
    int GetSurroundingWallCount(int[,] _map, int gridX, int gridY, int _width, int _height, int radius)
    {
        int wallCount = 0;
        for(int neighbourY = gridY - radius; neighbourY <= gridY + radius; neighbourY++)
        {
            for(int neighbourX = gridX - radius; neighbourX <= gridX + radius; neighbourX++)
            {
                if (neighbourX >= 0 && neighbourX < _width && neighbourY >= 0 && neighbourY < _height)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        if(_map[neighbourX, neighbourY] == 1){
                            wallCount++;
                        }
                        else if(_map[neighbourX, neighbourY] == 2){
                            wallCount++;
                        }
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
    void DebugImage(SpriteRenderer _rend, int[,] _map, int _width, int _height)
    {
        if(_map != null)
        {
            tx = new Texture2D(_width, _height);
            tx.filterMode = FilterMode.Point;

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if(_map[x,y] == 1){
                        tx.SetPixel(x,y, Color.green);
                    }
                    else if(_map[x,y] == 2){
                        tx.SetPixel(x,y, Color.yellow);
                    }
                    else{
                        tx.SetPixel(x,y, Color.blue);
                    }
                }
            }
            tx.Apply();
            _rend.sprite = Sprite.Create(tx, new Rect(0, 0, tx.width, tx.height), new Vector2(0.5f, 0.5f));
        }
    }
    void SendToTilemap(Tilemap mapp, int[,] _map, int _width, int _height){
        if(_map != null)
        {
            Tilemap _tileMap = mapp;

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if(_map[x,y] == 1){
                        _tileMap.SetTile(new Vector3Int(x,y,0), tiles.grassTile);
                    }
                    else if(_map[x,y] == 2){
                        _tileMap.SetTile(new Vector3Int(x,y,0), tiles.sandTile);
                    }
                    else{
                        _tileMap.SetTile(new Vector3Int(x,y,0), tiles.waterTile);
                    }
                }
            }
        }
    }
}
