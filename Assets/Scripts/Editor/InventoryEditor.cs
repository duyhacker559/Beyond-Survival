using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemInstance))]
public class InventoryEditor : Editor
{
    bool showBasicInfo = true;
    bool showStats = true;
    bool showEffect = true;

    bool showAttackStats = true;
    bool showAttackAnimStats = true;
    bool showMeleeStats = true;
    bool showShootingStats = true;

    private Type[] availableScripts;
    private int selectedIndex = 0;
    private string[] scriptNames;

}