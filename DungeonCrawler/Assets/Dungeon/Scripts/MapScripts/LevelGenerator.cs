﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    public Vector3[] Directions = { Vector3.right, Vector3.down, Vector3.left, Vector3.up }; // List for chosing directions for generator

    public LevelGenaratorSettings Settings;
    public LayerMask LayerForItems;
    public LayerMask LayerForEnemies;

    public List<Vector3> CreatedTiles;
    public List<Vector3> Borders;
    private int lastPosition;
    public LayerMask myLayer;

    private Vector3 PlayerPosition;
    private Vector3 ExitPosition;
    private Vector3 minimapCameraPosition;

    public GameObject PotionParent;
    public GameObject WallParent;
    public GameObject TileParent;
    public GameObject ItemParent;
    public GameObject EnemyParent;
    public GameObject Item;
    private ItemDatabase itemDatabase;

    public GameObject Player;

    private int additionalTiles = 0;

    public float MinimapCameraOrthographicSize;
    public Vector3 MinimapCameraPosition;
    private float maxY = 0;
    private float minY = 999;
    private float maxX = 0;
    private float minX = 999;
    private float maxWallsX;
    private float maxWallsY;

    public static LevelGenerator instance;
    public int CurrentSeed;

    void Awake()
    {
        itemDatabase = GameObject.FindGameObjectWithTag("Item Database").GetComponent<ItemDatabase>();

        CreatedTiles = new List<Vector3>();
        Borders = new List<Vector3>();

        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        GenerateSeed();
        GenerateLevel();
        GettingBorders();
        GenerateStartPositions();
        GeneratePotions();
        GenerateEnemies();
        SettingCameraPosition();
        GenerateItems();     
    }

    void GenerateLevel()
    {
        for (int i = 0; i < Settings.NumberOfTiles + additionalTiles; i++)
        {
            InstantiateTile(Random.Range(0, Settings.FloorTiles.Length));
            ChanceGenerator(Random.Range(0f, 1f));
        }

        WallsGenerator();
    }

    void ChanceGenerator(float rnd)
    {
        if (rnd < Settings.ChanceUp)
            MoveGenerator(1);
        else if (rnd < Settings.ChanceRight)
            MoveGenerator(2);
        else if (rnd < Settings.ChanceDown)
            MoveGenerator(-1);
        else MoveGenerator(-2);
    }

    void InstantiateTile(int tileIndex)
    {
        if (!CreatedTiles.Contains(transform.position))
        {
            var g = Instantiate(Settings.FloorTiles[tileIndex], transform.position, Quaternion.identity) as GameObject;
            CreatedTiles.Add(transform.position);
            g.transform.SetParent(TileParent.transform, false);
        }
        else additionalTiles++;
    }

    void MoveGenerator(int rnd)
    {
        if (lastPosition * -1 == rnd)
            return;

        int RandomDir = Random.Range(0, 4);
        var x = Directions[RandomDir];

        transform.position += x;

        lastPosition = RandomDir;
    }

    void WallsGenerator()
    {
        CountingWallValues();
        CreatingWalls();
    }

    void CountingWallValues()
    {
        for (int i = 0; i < CreatedTiles.Count; i++)
        {
            if (CreatedTiles[i].y < minY)
                minY = CreatedTiles[i].y;

            if (CreatedTiles[i].y > maxY)
                maxY = CreatedTiles[i].y;

            if (CreatedTiles[i].x < minX)
                minX = CreatedTiles[i].x;

            if (CreatedTiles[i].x > maxX)
                maxX = CreatedTiles[i].x;
        }

        maxWallsX = (maxX - minX) + Settings.ExtraWallX;

        maxWallsY = (maxY - minY) + Settings.ExtraWallY;
    }

    void CreatingWalls()
    {
        for (int x = 1; x < maxWallsX; x++)
        {
            for (int y = 1; y < maxWallsY; y++)
            {
                var pos = new Vector3(minX + x - Settings.ExtraWallX / 2, minY + y - Settings.ExtraWallY / 2);

                if (!CreatedTiles.Contains(pos))
                {
                    GameObject g;
                                
                    if (Physics2D.OverlapPoint(pos + Vector3.down, myLayer))                     
                        g = Instantiate(Settings.WallTiles[Random.Range(0, Settings.WallTiles.Length)],
                        pos, Quaternion.identity) as GameObject;
                    else
                        g = Instantiate(Settings.Ceiling, pos, Quaternion.identity) as GameObject;

                    g.transform.SetParent(WallParent.transform, false);
                }
            }
        }
    }

    void GenerateSeed()
    {
        CurrentSeed = Settings.Seed == 0 ? Random.Range(int.MinValue, int.MaxValue) : Settings.Seed;
        Random.InitState(CurrentSeed);

    }
    /* IEnumerator SeedCoroutine()
    {
        mySeed.text = Seed.ToString();
        yield return new WaitForSeconds(5);

    } */

    void GenerateStartPositions()
    {
        if (Borders[0].y > Borders[1].y)
            PlayerPosition = Borders[1];

        else PlayerPosition = Borders[0];

        List<Vector3> sortedList = new List<Vector3>(CreatedTiles.OrderByDescending(v => Vector3.Distance(PlayerPosition, v)));
        ExitPosition = sortedList[0];

        Player.transform.position = PlayerPosition;
        var e = Instantiate(Settings.Exit, ExitPosition, Quaternion.identity);

        e.transform.SetParent(WallParent.transform, false);
    }

    void GettingBorders()
    {
        Borders.Add(CreatedTiles.Find(v => v.x == maxX));
        Borders.Add(CreatedTiles.Find(v => v.x == minX));
    }

    /// <summary>
    /// Generates random items
    /// </summary>
    void GeneratePotions()
    {
        for (int i = 0; i < Settings.NumberOfPotions; i++)
        {
            var x = Instantiate(Settings.Potions[Random.Range(0, Settings.Potions.Length)], CreatedTiles[Random.Range(0, CreatedTiles.Count)],
                Quaternion.identity) as GameObject;

            x.transform.SetParent(PotionParent.transform);
        }
    }

    void GenerateItems()
    {
        var n = Settings.NumberOfItems;

        for (int i = 0; i < Settings.NumberOfItems; i++)
        {
            var it = Item;
            var pos = CreatedTiles[Random.Range(0, CreatedTiles.Count)];

            if (!Physics2D.OverlapPoint(pos, LayerForItems))
            {
                var x = Instantiate(it, pos, Quaternion.identity) as GameObject;

                x.GetComponent<ItemHolder>().SetProperties(itemDatabase.Items.ElementAt(Random.Range(0, itemDatabase.Items.Count)).Value);
                x.transform.SetParent(ItemParent.transform);
            }
            else Settings.NumberOfItems++;
        }
        Settings.NumberOfItems = n;
    }

    /// <summary>
    /// Generates random enemies
    /// </summary>
    void GenerateEnemies()
    {   
        for (int i = 0; i < Settings.NumberOfEnemies; i++)
        {
            Vector3 pos = CreatedTiles[Random.Range(0, CreatedTiles.Count)];
            GameObject x;  
            // Zrobić by na podstawie maksymalnej wielkośći sprawdzało dystans miedzy przeciwnikami a graczem

            //do
            //{
            //} while (Vector3.Distance(pos, PlayerPosition) < 10);

            if (!Physics2D.OverlapPoint(pos, LayerForEnemies))
            {
                x = Instantiate(Settings.Enemies[Random.Range(0, Settings.Enemies.Length)], pos, Quaternion.identity) as GameObject;

                
                x.transform.SetParent(EnemyParent.transform);
                GameManager.instance.Enemies.Add(x);
            }
            else Settings.NumberOfEnemies++;
            
        }
    }

    void SettingCameraPosition()
    {
        var centerX = (maxX + minX)/2;
        var centerY = (maxY + minY)/2;

        minimapCameraPosition = new Vector3(centerX + 0.5f, centerY + 0.5f, -5);
        MinimapCameraPosition = minimapCameraPosition;

        float aspect = (float)Screen.width/Screen.height;

        if (maxWallsY * aspect  > maxWallsX)
            MinimapCameraOrthographicSize = maxWallsY / 2 - 0.5f;
        else MinimapCameraOrthographicSize = maxWallsX / (aspect * 2.0f) - 0.3f;
    }

#if UNITY_EDITOR
    [ContextMenu("Save Seed")]
    public void SaveCurrentSeed()
    {
        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Settings), "Assets\\Dungeon\\Settings\\" + CurrentSeed + ".asset");
        var newSettings = AssetDatabase.LoadAssetAtPath<LevelGenaratorSettings>("Assets\\Dungeon\\Settings\\" + CurrentSeed + ".asset");

        newSettings.Seed = CurrentSeed;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (Settings.Seed == 0)
        {
            EditorApplication.isPlaying = false;
        }
    }
#endif
}