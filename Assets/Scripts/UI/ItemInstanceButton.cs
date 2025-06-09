using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class SelectedItem
{
    public static ItemInstance ItemData;
    public static string action = "Unequip";
}

public static class SelectedShopItem
{
    public static ItemInstance ItemData;
    public static int ShopID = 0;
}

public class ItemInstanceButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("UI")]

    [SerializeField] public Transform UI;
    [SerializeField] public bool ShopButton = false;
    public int cost = 0;

    [Header("Basic Info")]

    [SerializeReference] public ItemInstance item;
    [SerializeReference] public string itemType;

    [SerializeReference] public Item itemReference;

    GameObject dragger;

    // Update is called once per frame
    public void UpdateUI()
    {
        try
        {
            // Display Ammo
            GameObject ammoDisplay = transform.Find("Ammo").gameObject;
            if (itemReference.canShoot && !itemReference.isConsumable)
            {
                ammoDisplay.transform.GetComponent<TextMeshProUGUI>().SetText("Ammo:" + item.ammo + "/" + itemReference.clipSize);
                ammoDisplay.SetActive(true);
            }
            else ammoDisplay.SetActive(false);

            // Display Name
            GameObject itemNameDisplay = transform.Find("Name").gameObject;
            itemNameDisplay.transform.GetComponent<TextMeshProUGUI>().SetText(itemReference.itemName);

            if (!ShopButton)
            {
                // Display Amount

                GameObject amountDisplay = transform.Find("Amount").gameObject;
                if (item.amount > 1)
                {
                    amountDisplay.transform.GetComponent<TextMeshProUGUI>().SetText("x" + item.amount.ToString());
                    amountDisplay.SetActive(true);
                }
                else amountDisplay.SetActive(false);

                // Display Equiped
                GameObject equipedDisplay = transform.Find("IsUsing").gameObject;
                if (item.holder != null) equipedDisplay.SetActive(true);
                else equipedDisplay.SetActive(false);
            } else
            {
                GameObject equipedDisplay = transform.Find("Cost").gameObject;
                equipedDisplay.transform.GetComponent<TextMeshProUGUI>().SetText("$" + cost.ToString());
            }

            // Display Type
            GameObject typeDisplay = transform.Find("Type").gameObject;
            typeDisplay.transform.GetComponent<TextMeshProUGUI>().SetText(itemReference.itemType);

            // Display Icon
            GameObject iconDisplay = transform.Find("Icon").transform.Find("Item").gameObject;
            iconDisplay.transform.GetComponent<Image>().sprite = itemReference.icon;

        } catch
        {

        }
    }

    void OnEnable()
    {
        UpdateUI();
    }

    void Start()
    {
        UpdateUI();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!ShopButton)
        {
            GameObject icon = transform.Find("Icon").transform.Find("Item").gameObject;

            dragger = Instantiate(icon, UI.transform);

            DragPayload.dragger = dragger;
            DragPayload.ItemData = item;
            DragPayload.icon = icon.transform.GetComponent<Image>().sprite;
            DragPayload.dragType = itemReference.itemType;

            dragger.transform.GetComponent<Image>().raycastTarget = false;
            dragger.transform.SetAsLastSibling();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!ShopButton)
        {
            if (dragger)
            {
                dragger.transform.position = Vector3.Lerp(dragger.transform.position, Input.mousePosition, 10f);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!ShopButton)
        {
            if (dragger)
            {
                Destroy(dragger.transform.GetComponent<Image>());
                Destroy(dragger);
                dragger = null;
            }
            UpdateUI();
        }

        if (dragger)
        {
            Destroy(dragger.transform.GetComponent<Image>());
            Destroy(dragger);
        }
    }

    public void Select()
    {
        if (ShopButton)
        {
            SelectedShopItem.ItemData = item;
        }
        else
        {
            SelectedItem.ItemData = item;
            if (item.itemRef.isConsumable)
            {
                SelectedItem.action = "Use";
            }
            else
            {
                SelectedItem.action = "Unequip";
            }
        }
    }

    void Update()
    {
        
    }
}
