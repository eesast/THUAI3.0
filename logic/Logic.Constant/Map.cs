﻿using System;
using System.Collections.Generic;
using System.Text;
using Logic.Constant;

//using UnityEngine;

public static class Map
{

    public static uint[,] map = new uint[,]
    {
{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 6, 6, 0, 1, 0, 0, 0, 0, 4, 4, 4, 4, 4, 0, 0, 0, 0, 1, 0, 6, 6, 0, 1},
{ 1, 0, 6, 6, 0, 1, 0, 0, 0, 0, 4, 4, 4, 4, 4, 0, 0, 0, 0, 1, 0, 6, 6, 0, 1},
{ 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 4, 4, 4, 4, 4, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 4, 4, 4, 4, 4, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1},
{ 1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 3, 3, 3, 3, 3, 3, 0, 0, 0, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 1},
{ 1, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 5, 5, 5, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 1},
{ 1, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 5, 5, 5, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 1},
{ 1, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 5, 5, 5, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 1},
{ 1, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 5, 5, 5, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 1},
{ 1, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 3, 3, 3, 3, 3, 3, 0, 0, 0, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1},
{ 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 4, 4, 4, 4, 4, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 4, 4, 4, 4, 4, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1},
{ 1, 0, 6, 6, 0, 1, 0, 0, 0, 0, 4, 4, 4, 4, 4, 0, 0, 0, 0, 1, 0, 6, 6, 0, 1},
{ 1, 0, 6, 6, 0, 1, 0, 0, 0, 0, 4, 4, 4, 4, 4, 0, 0, 0, 0, 1, 0, 6, 6, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
 };
    public static int WORLD_MAP_WIDTH = map.GetLength(0);
    public static int WORLD_MAP_HEIGHT = map.GetLength(1);
    public static List<Obj>[,] WORLD_MAP = new List<Obj>[WORLD_MAP_WIDTH, WORLD_MAP_HEIGHT];


    // Start is called before the first frame update
    public static void InitializeMap()
    {
        for (uint x = 0; x < WORLD_MAP_WIDTH; x++)
            for (uint y = 0; y < WORLD_MAP_HEIGHT; y++)
            {
                if (Convert.ToInt32(map[x, y]) == 3)
                {
                    WORLD_MAP[x, y] = new List<Obj>();
                    WORLD_MAP[x, y].Add(new Block(x + 0.5, y + 0.5, Block.Type.Wall));
                }
                else
                {
                    WORLD_MAP[x, y] = new List<Obj>();
                }
            }

    }

    // Update is called once per frame
    public static void Update()
    {

    }

}