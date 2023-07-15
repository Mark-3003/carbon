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
    [Range(0.1f, 1f)]
    public float resourceCutoff;

    int[,] map;
    int[,] finalMap;
    int[,] resourceMap1;
    Texture2D tx;
    TileList tiles;
    int mapQuality = 2;

    System.Random rng;
    PlayerMovement player;
    void Awake(){
        tiles = GetComponent<TileList>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }
    void Start()
    {
        if (useRandomSeed)
        {
            seed = (Time.time + System.DateTime.Now.Millisecond);
        }
        rng = new System.Random(seed.GetHashCode());
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
        perlinRENDER.transform.localScale = new Vector2(5, 5);
        perlinRENDER.transform.position = new Vector2(width / 2, height / 2) * mapQuality;
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
        GenerateTrees(finalMap);
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
        rng = new System.Random(seed.GetHashCode());

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
    void GenerateTrees(int[,] _map){
        GameObject[] treeArray = GameObject.FindGameObjectsWithTag("tree");
        for(int i = 0; i < treeArray.Length; i++){
            Destroy(treeArray[i]);
        }

        float[,] noiseMap = new float[width * mapQuality, height * mapQuality];
        
        resourceMap1 = new int[width * mapQuality, height * mapQuality];

        Texture2D _tx = new Texture2D(width * mapQuality, height * mapQuality);
        _tx.filterMode = FilterMode.Point;

        int scale = 50;

        float offsetX = rng.Next(-50000, 50000) / 100;
        float offsetY = rng.Next(-50000, 50000) / 100;
        float offsetX2 = rng.Next(-50000, 50000) / 100;
        float offsetY2 = rng.Next(-50000, 50000) / 100;
        float offsetX3 = rng.Next(-50000, 50000) / 100;
        float offsetY3 = rng.Next(-50000, 50000) / 100;

        for(int y = 0; y < height * mapQuality; y++){

            for(int x = 0; x < width * mapQuality; x++){
                float sampleX = (x + offsetX + 1) / scale;
                float sampleY = (y + offsetY + 1) / scale;
                float sampleX2 = (x + offsetX2 + 1) / scale;
                float sampleY2 = (y + offsetY2 + 1) / scale;
                float sampleX3 = (x + offsetX3 + 1) / 2;
                float sampleY3 = (y + offsetY3 + 1) / 2;

                float _perlin1 = Mathf.PerlinNoise(sampleX, sampleY);
                float _perlin2 = Mathf.PerlinNoise(sampleX2, sampleY2);
                float _perlin3 = Mathf.PerlinNoise(sampleX3, sampleY3);
                noiseMap[x,y] = (_perlin1 + _perlin2) * _perlin3;

                noiseMap[x,y] = (noiseMap[x,y] >= resourceCutoff) ? 1f : 0f;
                
                resourceMap1[x,y] = (int)noiseMap[x,y];
            }
        }
        resourceMap1 = ifMapIntersectsTile(resourceMap1, width * mapQuality, height * mapQuality, 1, 1);
        resourceMap1 = SmoothMap(resourceMap1, width * mapQuality, height * mapQuality, 2);

        for(int y = 0; y < height * mapQuality; y++){
            for(int x = 0; x < width * mapQuality; x++){
                _tx.SetPixel(x, y, Color.Lerp(Color.black, Color.white, resourceMap1[x,y]));
                
                if(resourceMap1[x,y] == 1){
                    float treeOffsetX = (float)rng.Next(-300, 300) / 1000;
                    float treeOffsetY = (float)rng.Next(-150, 150) / 1000;
                    GameObject _obj = Instantiate(tiles.tree, new Vector3(x + 0.5f + treeOffsetX, y + 0.5f + treeOffsetY, y + 0.5f + treeOffsetY), Quaternion.Euler(0,0,0));
                    _obj.GetComponent<ResourceNode>().SetPlayer(player);

                    if(x < (width * mapQuality) - 1){
                        if(resourceMap1[x + 1, y] == 1){
                            treeOffsetX = (float)rng.Next(-300, 300) / 1000;
                            treeOffsetY = (float)rng.Next(-150, 150) / 1000;
                            _obj = Instantiate(tiles.tree, new Vector3(x + 1f + treeOffsetX, y + 1f + treeOffsetY, y + 1f + treeOffsetY), Quaternion.Euler(0,0,0));
                            _obj.GetComponent<ResourceNode>().SetPlayer(player);
                        }
                    }
                }
            }
        }
        _tx.Apply();
        //perlinRENDER.sprite = Sprite.Create(_tx, new Rect(0, 0, _tx.width, _tx.height), new Vector2(0.5f, 0.5f));
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
    int[,] ifMapIntersectsTile(int[,] _map, int _width, int _height, int _tile, int _tileToPlace){
        int[,] _newMap = _map;

        for(int y = 0; y < _height; y++){
            for(int x = 0; x < _width; x++){
                _newMap[x,y] = (finalMap[x,y] == _tile && _newMap[x,y] == _tileToPlace) ? _tileToPlace : 0;
            }
        }
        return _newMap;
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
