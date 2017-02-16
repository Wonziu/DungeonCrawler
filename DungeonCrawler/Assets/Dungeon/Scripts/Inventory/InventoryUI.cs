﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine.UI;


[RequireComponent(typeof(GridLayoutGroup))]
public class InventoryUI : MonoBehaviour
{

    public GameObject Prefab;
    public List<Transform> Slots;
    public GridLayoutGroup LayoutGroup;
    public Vector2 Size;
    public void Start()
    {
        LayoutGroup = GetComponent<GridLayoutGroup>();

        //SetSize();
    }
    public void GenerateSlots()
    {
        Slots.Add(Prefab.transform);

        int slots = Mathf.RoundToInt(Size.x*Size.y);
        var currentChildCount = transform.childCount;
        for (int i = currentChildCount; i < slots + 1; i++)
        {
            var s = Instantiate(Prefab);
            s.transform.SetParent(transform);
            Slots.Add(s.transform);
        }

        //if (currentChildCount <= slots) return; TODO :)

        //for (int i = transform.childCount; i > slots; i--)
        //{
        //    Destroy(transform.GetChild(0).gameObject);
        //}

    }
    //[ContextMenu("Regenerate")]

    public void Generate(Action callback)
    {
        SetSize();
        callback.Invoke();
    }

    public void SetSize()
    {
        LayoutGroup.constraint = Size.x<Size.y?GridLayoutGroup.Constraint.FixedRowCount : GridLayoutGroup.Constraint.FixedColumnCount;
        LayoutGroup.constraintCount = Mathf.RoundToInt(Size.x>Size.y?Size.x:Size.y);
        GenerateSlots();
    }
}