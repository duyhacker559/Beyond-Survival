using Codice.Client.BaseCommands.BranchExplorer;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    bool showBasicInfo = true;
    bool showStats = true;
    bool showEffect = true;
    bool showAttack = true;

    bool showAttackStats = true;
    bool showAttackEffects = true;
    bool showAttackAnimStats = true;
    bool showMeleeStats = true;
    bool showShootingStats = true;
    bool showWearableStats = true;

    private Type[] availableScripts;
    private int selectedIndex = 0;
    private string[] scriptNames;

    void OnEnable()
    {
        availableScripts = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(MonoScript)))
            .ToArray();

        scriptNames = availableScripts.Select(t => t.Name).ToArray();
    }

    public override void OnInspectorGUI()
    {
        Item item = (Item)target;

        // Basic Info
        showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "Basic Info");
        if (showBasicInfo)
        {
            EditorGUI.indentLevel++;
            item.itemName = EditorGUILayout.TextField("Item Name", item.itemName);
            item.itemID = EditorGUILayout.TextField("Item ID", item.itemID);
            item._itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type", item._itemType);
            item.itemType = item._itemType.ToString();
            //item.itemType = EditorGUILayout.TextField("<Item Type>", item.itemType);
            EditorGUILayout.Space();
            item.itemDescription = EditorGUILayout.TextField("Item Description", item.itemDescription);
            item.isWeapon = EditorGUILayout.Toggle("Weapon", item.isWeapon);
            EditorGUILayout.Space();
            item.icon = (Sprite)EditorGUILayout.ObjectField("Icon", item.icon, typeof(Sprite), false);
            EditorGUILayout.Space();
            item.model = (GameObject)EditorGUILayout.ObjectField("Model", item.model, typeof(GameObject), false);

            EditorGUILayout.Space();
            item.inputKey = (KeyCode)EditorGUILayout.EnumPopup("Pick Up Key", item.inputKey);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Stats Group
        if (item._itemType == ItemType.Armor)
        {
            showWearableStats = EditorGUILayout.Foldout(showWearableStats, "Wearable Stats");
            if (showWearableStats)
            {
                EditorGUI.indentLevel++;
                item.armor_modelType = (ArmorModelType)EditorGUILayout.EnumPopup("Model Type", item.armor_modelType);
                if (item.armor_modelType == ArmorModelType.Texture) {
                    item.armor_Sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", item.armor_Sprite, typeof(Sprite), false);
                } else
                {
                    item.armor_Model = (GameObject)EditorGUILayout.ObjectField("Model", item.armor_Model, typeof(GameObject), false);
                }
                EditorGUILayout.Space();

                item.armorType = (ArmorType)EditorGUILayout.EnumPopup("Slot", item.armorType);
                switch (item.armorType)
                {
                    case ArmorType.Body:
                        item.wearSlot = 1;
                        break;
                    case ArmorType.Pant:
                        item.wearSlot = 2;
                        break;
                    default:
                        item.wearSlot = 0;
                        item.armorType = ArmorType.Head;
                        break;
                }
                if (item.wearSlot == 0)
                {
                    item.hide_Hair = EditorGUILayout.Toggle("Hide Hair", item.hide_Hair);
                } 
                EditorGUILayout.Space();

                item.armor = EditorGUILayout.FloatField("Armor", item.armor);
                item.armor_regen = EditorGUILayout.FloatField("Regen", item.armor_regen);

                EditorGUILayout.Space();

                item.defense = EditorGUILayout.FloatField("Defense", item.defense);

                EditorGUILayout.Space();

                item.range = EditorGUILayout.FloatField("Range", item.range);
                item.speed = EditorGUILayout.FloatField("Speed", item.speed);

                EditorGUI.indentLevel--;
            }
        }

        // Stats Group
        showStats = EditorGUILayout.Foldout(showStats, "Stats");
        if (showStats)
        {
            EditorGUI.indentLevel++;
            item.canSell = EditorGUILayout.Toggle("Can Sell", item.canSell);
            EditorGUILayout.Space();
            item.value = EditorGUILayout.IntField("Value", item.value);
            item.amountThreshold = EditorGUILayout.IntField("Amount Threshold", item.amountThreshold);
            EditorGUILayout.Space();
            item.weight = EditorGUILayout.FloatField("Weight", item.weight);
            EditorGUILayout.Space();
            item.lifeTime = EditorGUILayout.FloatField("Life Time", item.lifeTime);
            item.haveLifeTime = EditorGUILayout.Toggle("Have Life Time", item.haveLifeTime);
            EditorGUILayout.Space();
            item.attack = (ItemAttack)EditorGUILayout.ObjectField("Item Sentient", item.attack, typeof(ItemAttack), false);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Effect Group
        showEffect = EditorGUILayout.Foldout(showEffect, "Effect");
        if (showEffect)
        {
            EditorGUI.indentLevel++;
            item.isConsumable = EditorGUILayout.Toggle("Is Consumable", item.isConsumable);
            if (item.isConsumable )
            {
                item.useOnDelete = EditorGUILayout.Toggle("UseOnDelete", item.useOnDelete);
                EditorGUILayout.Space();
                
                item.effectDescription = EditorGUILayout.TextField("Effect Description", item.effectDescription);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Effect", EditorStyles.boldLabel);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.Space();

                EditorGUI.indentLevel++;

                for (int i = 0; i < item.consumeEffect.Count; i++)
                {
                    item.consumeEffect[i].modify_ID = EditorGUILayout.TextField("Key", item.consumeEffect[i].modify_ID);
                    item.consumeEffect[i].modify_Type = (ValueType)EditorGUILayout.EnumPopup("Type", item.consumeEffect[i].modify_Type);
                    switch (item.consumeEffect[i].modify_Type)
                    {
                        case ValueType.Int:
                            item.consumeEffect[i].modify_IntValue = EditorGUILayout.IntField("Value", item.consumeEffect[i].modify_IntValue);
                            break;
                        case ValueType.Float:
                            item.consumeEffect[i].modify_FloatValue = EditorGUILayout.FloatField("Value", item.consumeEffect[i].modify_FloatValue);
                            break;
                        case ValueType.Bool:
                            item.consumeEffect[i].modify_BoolValue = EditorGUILayout.Toggle("Value", item.consumeEffect[i].modify_BoolValue);
                            break;
                        default:
                            item.consumeEffect[i].modify_StringValue = EditorGUILayout.TextField("Value", item.consumeEffect[i].modify_StringValue);
                            break;
                    }
                    item.consumeEffect[i].modify_Des = EditorGUILayout.TextField("Des", item.consumeEffect[i].modify_Des);

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    EditorGUILayout.Space();
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Add Effect"))
                {
                    item.consumeEffect.Add(new Modify());
                }

                if (GUILayout.Button("Remove Last"))
                {
                    if (item.consumeEffect.Count > 0)
                        item.consumeEffect.RemoveAt(item.consumeEffect.Count - 1);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Melee Attack Group
        showAttack = EditorGUILayout.Foldout(showAttack, "Attack");
        if (showAttack)
        {
            EditorGUI.indentLevel++;
            item.canAttack = EditorGUILayout.Toggle("Can Attack", item.canAttack);
            if (item.canAttack)
            {

                showAttackStats = EditorGUILayout.Foldout(showAttackStats, "Basic");

                if (showAttackStats)
                {
                    EditorGUI.indentLevel++;
                    item.damage = EditorGUILayout.FloatField("Damage", item.damage);
                    item.cooldown = EditorGUILayout.FloatField("Cooldown", item.cooldown);
                    EditorGUILayout.Space();

                    item.knockBack = EditorGUILayout.FloatField("Knock Back", item.knockBack);
                    item.knockBack_Duration = EditorGUILayout.FloatField("Knock Back Duration", item.knockBack_Duration);

                    item.durability = EditorGUILayout.IntField("Durability", item.durability);
                    EditorGUILayout.Space();
                    item.weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", item.weaponType);
                    EditorGUI.indentLevel--;
                }

                showAttackEffects = EditorGUILayout.Foldout(showAttackEffects, "Damage Effect");

                if (showAttackEffects)
                {
                    EditorGUI.indentLevel++;


                    for (int j=0; j<item.effects.Count; j++)
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                        DamageEffect effect = item.effects[j];
                        effect._show = EditorGUILayout.Foldout(effect._show, "Effect"+item.effects.IndexOf(effect));

                        if (effect._show)
                        {
                            EditorGUI.indentLevel++;

                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                            for (int i = 0; i < effect.effects.Count; i++)
                            {
                                effect.effects[i].modify_ID = EditorGUILayout.TextField("Key", effect.effects[i].modify_ID);
                                effect.effects[i].modify_Type = (ValueType)EditorGUILayout.EnumPopup("Type", effect.effects[i].modify_Type);
                                switch (effect.effects[i].modify_Type)
                                {
                                    case ValueType.Int:
                                        effect.effects[i].modify_IntValue = EditorGUILayout.IntField("Value", effect.effects[i].modify_IntValue);
                                        break;
                                    case ValueType.Float:
                                        effect.effects[i].modify_FloatValue = EditorGUILayout.FloatField("Value", effect.effects[i].modify_FloatValue);
                                        break;
                                    case ValueType.Bool:
                                        effect.effects[i].modify_BoolValue = EditorGUILayout.Toggle("Value", effect.effects[i].modify_BoolValue);
                                        break;
                                    default:
                                        effect.effects[i].modify_StringValue = EditorGUILayout.TextField("Value", effect.effects[i].modify_StringValue);
                                        break;
                                }
                                effect.effects[i].modify_Des = EditorGUILayout.TextField("Des", effect.effects[i].modify_Des);

                                EditorGUILayout.Space();
                                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                                EditorGUILayout.Space();
                            }

                            EditorGUILayout.BeginHorizontal();

                            if (GUILayout.Button("Add Effect"))
                            {
                                effect.effects.Add(new Modify());
                            }

                            if (GUILayout.Button("Remove Last"))
                            {
                                if (effect.effects.Count > 0)
                                    effect.effects.RemoveAt(effect.effects.Count - 1);
                            }

                            EditorGUILayout.EndHorizontal();

                            EditorGUI.indentLevel--;
                        }

                        effect.tick = EditorGUILayout.FloatField("Tick", effect.tick);
                        effect.lifeTime = EditorGUILayout.FloatField("Time", effect.lifeTime);

                        effect.canStack = EditorGUILayout.Toggle("Can Stack", effect.canStack);
                        effect.canOveride = EditorGUILayout.Toggle("Can Overide", effect.canOveride);

                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("Add Effect", new GUILayoutOption[] {
                        GUILayout.Height(30),
                    }))
                    {
                        item.effects.Add(new DamageEffect(0,1,new List<Modify>()));
                    }

                    if (GUILayout.Button("Remove Last", new GUILayoutOption[] {
                        GUILayout.Width(100),
                        GUILayout.Height(30),
                    }))
                    {
                        if (item.effects.Count > 0)
                            item.effects.RemoveAt(item.effects.Count - 1);
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }

                showAttackAnimStats = EditorGUILayout.Foldout(showAttackAnimStats, "Animation");

                if (showAttackAnimStats)
                {
                    EditorGUI.indentLevel++;
                    item.swing = EditorGUILayout.FloatField("Swing Angle", item.swing);
                    item.swingOffset = EditorGUILayout.FloatField("Swing Angel Offset", item.swingOffset);
                    EditorGUILayout.Space();
                    item.recoil = EditorGUILayout.FloatField("Recoil", item.recoil);
                    EditorGUI.indentLevel--;
                }

                showMeleeStats = EditorGUILayout.Foldout(showMeleeStats, "Melee");

                if (showMeleeStats)
                {
                    EditorGUI.indentLevel++;
                    item.canMelee = EditorGUILayout.Toggle("Can Melee", item.canMelee);
                    if (item.canMelee)
                    {
                        item.hitbox = (GameObject)EditorGUILayout.ObjectField("Hitbox", item.hitbox, typeof(GameObject), false);
                        item.hitboxData = (AreaAttackItem)EditorGUILayout.ObjectField("Data", item.hitboxData, typeof(AreaAttackItem), false);
                        item.mele_manaConsume = EditorGUILayout.FloatField("Mana Cost", item.mele_manaConsume);
                    }
                    EditorGUI.indentLevel--;
                }

                showShootingStats = EditorGUILayout.Foldout(showShootingStats, "Shooting");

                if (showShootingStats)
                {
                    EditorGUI.indentLevel++;
                    item.canShoot = EditorGUILayout.Toggle("Can Shoot", item.canShoot);
                    if (item.canShoot)
                    {
                        item.bulletSpeed = EditorGUILayout.FloatField("Bullet Speed", item.bulletSpeed);
                        item.bulletLifetime = EditorGUILayout.FloatField("Bullet Lifetime", item.bulletLifetime);
                        EditorGUILayout.Space();
                        item.spread = EditorGUILayout.FloatField("Bullet Spread", item.spread);
                        item.shooting_manaConsume = EditorGUILayout.FloatField("Mana Cost", item.shooting_manaConsume);
                        EditorGUILayout.Space();
                        item.reload = EditorGUILayout.FloatField("Reload Time", item.reload);
                        item.reload_manaConsume = EditorGUILayout.FloatField("Mana Cost", item.reload_manaConsume);
                        EditorGUILayout.Space();
                        item.clipSize = EditorGUILayout.IntField("Clip Size", item.clipSize);
                        item.fireAmount = EditorGUILayout.IntField("Amount", item.fireAmount);
                        item.emit = EditorGUILayout.IntField("Emit", item.emit);
                        EditorGUILayout.Space();
                        item.projectile = (GameObject)EditorGUILayout.ObjectField("Projectile", item.projectile, typeof(GameObject), false);
                        EditorGUILayout.Space();
                        item.projectileDat = (ProjectileItem)EditorGUILayout.ObjectField("Projectile Data", item.projectileDat, typeof(ProjectileItem), false);
                    }
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(item);
        }
    }
}
