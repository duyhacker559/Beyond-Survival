using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] GameObject UI_Name;
    [SerializeField] GameObject UI_Des;
    [SerializeField] GameObject UI_Icon;
    [SerializeField] GameObject UI_Equip;
    [SerializeField] GameObject UI_Button;

    [SerializeField] bool shopUI = false;

    void Start()
    {
        
    }

    private bool equiped = false;

    // Update is called once per frame
    void Update()
    {
        if (shopUI)
        {
            if (SelectedShopItem.ItemData != null)
            {
                Image icon = UI_Icon.GetComponent<Image>();
                icon.sprite = SelectedShopItem.ItemData.itemRef.icon;
                UI_Icon.SetActive(true);

                TextMeshProUGUI name = UI_Name.GetComponent<TextMeshProUGUI>();
                name.SetText(SelectedShopItem.ItemData.itemRef.itemName);

                TextMeshProUGUI des = UI_Des.GetComponent<TextMeshProUGUI>();
                des.SetText(SelectedShopItem.ItemData.itemRef.itemDescription);
            }
            else
            {
                UI_Icon.SetActive(false);

                TextMeshProUGUI name = UI_Name.GetComponent<TextMeshProUGUI>();
                name.SetText("N/A");

                TextMeshProUGUI des = UI_Des.GetComponent<TextMeshProUGUI>();
                des.SetText("");
            }

            if (PlayerUI.UI != null)
            {
                PlayerUI playerUI = PlayerUI.UI.GetComponent<PlayerUI>();

                if (playerUI != null)
                {
                    TextMeshProUGUI cash = UI_Equip.GetComponent<TextMeshProUGUI>();
                    cash.SetText("$" + playerUI.player.cash);
                }
            }
        } else
        {
            if (SelectedItem.ItemData != null)
            {
                Image icon = UI_Icon.GetComponent<Image>();
                icon.sprite = SelectedItem.ItemData.itemRef.icon;
                UI_Icon.SetActive(true);

                TextMeshProUGUI name = UI_Name.GetComponent<TextMeshProUGUI>();
                name.SetText(SelectedItem.ItemData.itemRef.itemName);

                TextMeshProUGUI des = UI_Des.GetComponent<TextMeshProUGUI>();
                des.SetText(SelectedItem.ItemData.itemRef.itemDescription);

                if (SelectedItem.ItemData.holder != null) equiped = true;
                else equiped = false;

                TextMeshProUGUI equip = UI_Equip.GetComponent<TextMeshProUGUI>();
                if (equiped && SelectedItem.action == "Unequip")
                {
                    UI_Button.SetActive(true);
                    equip.SetText("Unequip");
                }
                else
                {
                    if (SelectedItem.ItemData.itemRef.isConsumable)
                    {
                        UI_Button.SetActive(true);
                        equip.SetText("Use");
                    }
                    else
                    {
                        UI_Button.SetActive(false);
                    }
                }
            }
            else
            {
                UI_Icon.SetActive(false);
            }
        }
    }

    public void Toggle()
    {
        if (SelectedItem.ItemData != null)
        {
            Transform holder = SelectedItem.ItemData.holder;
            if (holder != null && SelectedItem.action == "Unequip")
            {
                holder.GetComponent<ItemHolder>().Unequip(true);
            } else
            {

            }
        }
    }
}
