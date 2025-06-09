using NUnit.Framework;
using Photon.Pun;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Admin : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public List<Transform> buttons = new List<Transform>();

    private bool loaded = false;

    private PhotonView view;

    void OnEnable()
    {
        view = GetComponent<PhotonView>();
        if (!loaded && view.IsMine && PhotonNetwork.IsMasterClient)
        {
            try
            {
                Transform SpawmItem = buttons[0];
                UnityEngine.UI.Button spawnButton = SpawmItem.Find("Button").GetComponent<UnityEngine.UI.Button>();
                TMP_Dropdown selections = SpawmItem.Find("Dropdown").GetComponent<TMP_Dropdown>();
                TMP_InputField amount = SpawmItem.Find("Amount").GetComponent<TMP_InputField>();
                //Debug.Log(spawnButton);
                //Debug.Log(selections);
                //Debug.Log(amount);
                spawnButton.onClick.AddListener(() => GameManager.Instance.SpawnItem(
                    selections.options[selections.value].text,
                    (int.TryParse(amount.text, out int result))?result:1,
                    Game.localPlayer.transform.position,
                    new Quaternion(0,0,0,0))
                );

                Transform SetHealth = buttons[1];
                UnityEngine.UI.Button setHealthButton = SetHealth.Find("Button").GetComponent<UnityEngine.UI.Button>();
                TMP_InputField setHealthAmount = SetHealth.Find("Amount").GetComponent<TMP_InputField>();
                setHealthButton.onClick.AddListener(() => GameManager.SetHealth(
                    Game.localPlayer.gameObject,
                    (float.TryParse(setHealthAmount.text, out float result)) ? result : 1f
                    )
                );

                Transform SetMana = buttons[2];
                UnityEngine.UI.Button setManaButton = SetMana.Find("Button").GetComponent<UnityEngine.UI.Button>();
                TMP_InputField setManaAmount = SetMana.Find("Amount").GetComponent<TMP_InputField>();
                setManaButton.onClick.AddListener(() => GameManager.SetMana(
                    Game.localPlayer.gameObject,
                    (float.TryParse(setManaAmount.text, out float result)) ? result : 1f
                    )
                );

                Transform SpawmEnemy = buttons[3];
                UnityEngine.UI.Button spawnEnemyButton = SpawmEnemy.Find("Button").GetComponent<UnityEngine.UI.Button>();
                TMP_Dropdown e_selections = SpawmEnemy.Find("Dropdown").GetComponent<TMP_Dropdown>();
                TMP_InputField e_amount = SpawmEnemy.Find("Amount").GetComponent<TMP_InputField>();

                spawnEnemyButton.onClick.AddListener(() => GameManager.Instance.TrySpawnEnemy(
                    Game.GetEnemyData(e_selections.options[e_selections.value].text),
                    Game.localPlayer.transform.position
                ));

                loaded = true;
            }
            catch
            {

            }
        }

        //Debug.Log(Game.items.Length);

        List<string> itemList = new List<string>();
        foreach (var obj in Game.items)
        {
            itemList.Add(obj.name);
        }

        // Spawn Item
        TMP_Dropdown itemID = buttons[0].Find("Dropdown").GetComponent<TMP_Dropdown>();
        itemID.ClearOptions();
        itemID.AddOptions(itemList);

        // Spawn Enemy

        itemList = new List<string>();
        foreach (var obj in Game.enemies)
        {
            itemList.Add(obj.ID);
        }

        itemID = buttons[3].Find("Dropdown").GetComponent<TMP_Dropdown>();
        itemID.ClearOptions();
        itemID.AddOptions(itemList);
    }

    void Start()
    {
        // Spawn Item
        view = GetComponent<PhotonView>();
    }

    void Update()
    {
        
    }

    // Function
}
