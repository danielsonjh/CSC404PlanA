﻿
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerActionHud : MonoBehaviour
{
    private static readonly Dictionary<PlayerAction, Color> ActionColorMap = new Dictionary<PlayerAction, Color>
    {
        {PlayerAction.Empty, Color.black * 0.75f},
        {PlayerAction.Fire, Color.red * 0.75f},
        {PlayerAction.Water, Color.blue * 0.75f},
        {PlayerAction.Wind, Color.white * 0.75f},
        {PlayerAction.Earth, Color.green * 0.75f},
    };

    private GameObject[] _actionButtons;

    void Start()
    {
        _actionButtons = new GameObject[transform.childCount];
        for (var i = 0; i < transform.childCount; i++)
        {
            _actionButtons[i] = transform.GetChild(i).gameObject;
            AddAction(PlayerAction.Empty, i);
        }
    }
    
    public void AddAction(PlayerAction action, int index)
    {
        _actionButtons[index].GetComponent<Image>().color = ActionColorMap[action];
    }
}