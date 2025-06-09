using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(Player))]
public class PlayerEditor : Editor
{

    bool showBasicStats = true;

    bool showVisionStats = true;
    bool showVisionAnimStats = true;

    bool showMovementStats = true;

    bool showDashStats = true;

    bool showSkinStats = true;

    public override void OnInspectorGUI()
    {
        Player player = (Player)target;

        showBasicStats = EditorGUILayout.Foldout(showBasicStats, "Movement");
        if (showBasicStats)
        {
            EditorGUI.indentLevel++;

            player.cash = EditorGUILayout.IntField("Cash", player.cash);

            EditorGUILayout.Space();

            player._currentMana = EditorGUILayout.FloatField("Stamina", player._currentMana);
            player.MaxMana = EditorGUILayout.FloatField("Max Stamina", player.MaxMana);

            EditorGUI.indentLevel--;
        }

        showMovementStats = EditorGUILayout.Foldout(showMovementStats, "Movement");
        if (showMovementStats)
        {
            EditorGUI.indentLevel++;

            player.moveSpeed = EditorGUILayout.FloatField("Mov Speed", player.moveSpeed);
            player.maxSpeed = EditorGUILayout.FloatField("Max Speed", player.maxSpeed);

            EditorGUI.indentLevel--;
        }

        showVisionStats = EditorGUILayout.Foldout(showVisionStats, "Vision");
        if (showVisionStats)
        {
            EditorGUI.indentLevel++;
            player.range = EditorGUILayout.FloatField("Range", player.range);
            //EditorGUILayout.Space();

            showVisionAnimStats = EditorGUILayout.Foldout(showVisionAnimStats, "Anim");
            if (showVisionAnimStats)
            {
                EditorGUI.indentLevel++;
                player.minVision = EditorGUILayout.FloatField("Min Vision", player.minVision);
                player.maxVision = EditorGUILayout.FloatField("Max Vision", player.maxVision);
                //EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        showDashStats = EditorGUILayout.Foldout(showDashStats, "Dash");
        if (showDashStats)
        {
            EditorGUI.indentLevel++;

            player.dashSpeed = EditorGUILayout.FloatField("Dash Speed", player.dashSpeed);
            player.dashDuration = EditorGUILayout.FloatField("Dash Duration", player.dashDuration);
            player.dashCooldown = EditorGUILayout.FloatField("Dash Cooldown", player.dashCooldown);
            player.dashManaConsume = EditorGUILayout.FloatField("Dash Consume", player.dashManaConsume);

            EditorGUI.indentLevel--;
        }

        showSkinStats = EditorGUILayout.Foldout(showSkinStats, "Skin");
        if (showSkinStats)
        {
            EditorGUI.indentLevel++;

            EditorGUI.indentLevel--;
        }
    }
}
