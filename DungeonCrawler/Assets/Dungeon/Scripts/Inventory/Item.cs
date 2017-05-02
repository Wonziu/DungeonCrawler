﻿using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class Item : ScriptableObject
{
    public string Name;
    public string ItemDesc;
    public Sprite ItemIcon;
    public int ItemPower;
    public ItemType MyItemType;

    public enum ItemType
    {
        LeftHand, 
        RightHand,
        Helmet,
        Chest
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}