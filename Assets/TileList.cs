using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileList : MonoBehaviour
{
    [Header("Tiles")]
    public Tilemap map;
    public RuleTile waterTile;
    public Tile sandTile;
    public Tile grassTile;

    [Header("Resources")]
    public GameObject tree;
}
