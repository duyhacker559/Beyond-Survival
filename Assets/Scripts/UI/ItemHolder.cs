using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemHolder : MonoBehaviour, IDropHandler
{

    [SerializeReference] public string ItemType;
    [SerializeReference] public int SlotType = 0;

    public ItemInstance item = null;

    private Image showImage;

    [SerializeReference] private bool sell = false;

    public void OnDrop(PointerEventData eventData)
    {
        if (DragPayload.ItemData != null)
        {
            if (DragPayload.dragType == ItemType || sell)
            {
                if (ItemType == "Armor" && !sell)
                {
                    if (DragPayload.ItemData.itemRef.wearSlot != SlotType)
                    {
                        DragPayload.ItemData = null;
                        DragPayload.icon = null;
                        DragPayload.dragType = "";
                        return;
                    }
                }

                if (item != null)
                {
                    if (item.holder != null)
                    {
                        item.holder.GetComponent<ItemHolder>().Unequip(false);
                    }
                }

                item = DragPayload.ItemData;

                if (item != null)
                {
                    if (item.holder != null)
                    {
                        item.holder.GetComponent<ItemHolder>().Unequip(false);
                    }
                }

                if (sell)
                {
                    PlayerUI.UI.GetComponent<PlayerUI>().SellItem(item);
                    PlayerUI.UI.GetComponent<PlayerUI>().UpdateInventory();
                } else
                {
                    item.holder = transform;
                    showImage.sprite = item.itemRef.icon;
                    showImage.enabled = true;
                }

                if (item.itemRef.itemType == "Armor")
                {
                    PlayerUI.UI.GetComponent<PlayerUI>().ToggleArmor(this);
                }
                else
                {
                    PlayerUI.UI.GetComponent<PlayerUI>().UpdateLoadOut();
                }
            }

        } else
        {
            showImage.enabled = false;
        }

        if (DragPayload.dragger)
        {
            Destroy(DragPayload.dragger.transform.GetComponent<Image>());
            Destroy(DragPayload.dragger);
        }

        DragPayload.ItemData = null;
        DragPayload.icon = null;
        DragPayload.dragType = "";
    }

    public void Unequip(bool update)
    {
        if (item != null)
        {
            item.holder = null;

            showImage.enabled = false;

            item.storage.GetComponent<ItemInstanceButton>().UpdateUI();

            bool canUpdate = (item.itemRef.itemType != "Armor");

            item = null;

            if (update)
            {
                if (canUpdate)
                {
                    PlayerUI.UI.GetComponent<PlayerUI>().UpdateLoadOut();
                } else
                {
                    PlayerUI.UI.GetComponent<PlayerUI>().ToggleArmor(this);
                }
            }
        }
    }

    public void Select()
    {
        if (item != null && item.holder == transform)
        {
            SelectedItem.ItemData = item;
            SelectedItem.action = "Unequip";
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        showImage = transform.Find("Icon").transform.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
