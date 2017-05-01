﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{

    public List<Item> PlayerInventory;
    private ItemDatabase itemDatabase;
    public InventoryUI UIInventory;

    public Vector2 SelectionPos;
    public Item SelectedItem;

    void Awake()
    {
	    itemDatabase = GameObject.FindGameObjectWithTag("Item Database").GetComponent<ItemDatabase>();
        PlayerInventory = new List<Item>();
        SelectionPos = Vector2.zero;     
    }

    // Use this for initialization  
    void Start ()
	{
	    StartCoroutine(GenerateInventory());
	}

    IEnumerator GenerateInventory()
    {
        while (!itemDatabase.IsLoaded)
        {
            yield return new WaitForSeconds(0.01f);
        }

        AddStartingItems(itemDatabase.Items["Apple"], itemDatabase.Items["Apple"]);
        SelectedItem = PlayerInventory[0];
        UIInventory.Generate(() =>
        {
            int i = 0;
            foreach (var item in PlayerInventory)
            {
                UIInventory.Slots[i].GetChild(0).GetComponent<Image>().enabled = true;
                UIInventory.Slots[i].GetChild(0).GetComponent<Image>().sprite = item.ItemIcon;
                i++;
            } 
        });
    }

    void AddStartingItems(params Item[] item)
    {
        foreach (var i in item)
        {
            PlayerInventory.Add(i);
        }
    }

	// Update is called once per frame

    public void AddItem(Item item)
    {
        PlayerInventory.Add(item);
        RefreshInventory();
    }

    void RefreshInventory()
    {
        int i = 0;
        foreach (var item in PlayerInventory)
        {
            UIInventory.Slots[i].GetChild(0).GetComponent<Image>().enabled = true;
            UIInventory.Slots[i].GetChild(0).GetComponent<Image>().sprite = item.ItemIcon;
            i++;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            AddItem(itemDatabase.Items["Shield"]);
        
        if (UIInventory.gameObject.activeSelf)
        {
            if (MoveSelection())
                SelectNewItem();
        }
    }

    bool MoveSelection()
    {
        foreach (var ControlScheme in GameManager.instance.ControlSchemes)
        {
            if (MoveSelectionX(ControlScheme.Left, -1)) return true;
            if (MoveSelectionX(ControlScheme.Right, 1)) return true;
            if (MoveSelectionY(ControlScheme.Up, 1)) return true;
            if (MoveSelectionY(ControlScheme.Down, -1)) return true;
        }
        return false;
    }

    bool MoveSelectionX(KeyCode kc, int i)
    {    
        if (Input.GetKeyDown(kc))
        {
            if (UIInventory.Size.x > SelectionPos.x + i && 0 <= SelectionPos.x + i)
            {        
                SelectionPos = new Vector2(SelectionPos.x + i, SelectionPos.y);
                UIInventory.SelectedItem.transform.position += new Vector3(i * (UIInventory.LayoutGroup.cellSize.x + UIInventory.LayoutGroup.spacing.x), 0, 0);
                return true;
            }
        }
        return false;
    }

    bool MoveSelectionY(KeyCode kc, int i)
    {
        if (Input.GetKeyDown(kc))
        {
            if (UIInventory.Size.y > SelectionPos.y - i && 0 <= SelectionPos.y - i)
            {
                SelectionPos = new Vector2(SelectionPos.x, SelectionPos.y - i);
                UIInventory.SelectedItem.transform.position += new Vector3(0, i * (UIInventory.LayoutGroup.cellSize.y + UIInventory.LayoutGroup.spacing.y), 0);
                return true;
            }
        }
        return false;
    }

    void SelectNewItem()
    {
        if (PlayerInventory.Count > Mathf.RoundToInt(UIInventory.Size.x*SelectionPos.y + SelectionPos.x))
            SelectedItem = PlayerInventory[Mathf.RoundToInt(UIInventory.Size.x*SelectionPos.y + SelectionPos.x)];
        else SelectedItem = null;
    }
}
 