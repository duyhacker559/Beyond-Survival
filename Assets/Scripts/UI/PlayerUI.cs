using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public static bool isChatting = false;

    [SerializeField] public static Transform UI = null;
    [SerializeField] public GameObject Current_Player = null;

    [Header("UI Bar")]

    [SerializeField] GameObject HP_UI = null;
    [SerializeField] GameObject MP_UI = null;
    [SerializeField] GameObject AP_UI = null;

    [Header("UI Display")]

    [SerializeField] GameObject Cash_UI = null;
    [SerializeField] GameObject Selected_UI = null; 

    [Header("UI Inventory")]

    [SerializeField] GameObject Storage = null;
    [SerializeField] GameObject Button = null;

    [Header("UI Inventory")]
    [SerializeField] GameObject ShopStorage = null;
    [SerializeField] GameObject ShopButton = null;

    [Header("UI Chat")]
    [SerializeField] GameObject Chat_UI = null;
    [SerializeField] GameObject ChatText = null;
    [SerializeField] GameObject Chats = null;
    [SerializeField] GameObject Notification = null;
    [SerializeField] GameObject ChatTextSample = null;

    [Header("Other UI")]

    [SerializeField] GameObject Loadout_UI = null;
    [SerializeField] public GameObject Iventory_UI = null;
    [SerializeField] GameObject ItemStats_UI = null;
    [SerializeField] GameObject Admin_UI = null;
    [SerializeField] Transform Fallen_UI = null;
    [SerializeField] Transform Win_UI = null;
    [SerializeField] Transform Admin_Button = null;
    [SerializeField] Transform Pause_UI = null;
    [SerializeField] public Transform Shop_UI = null;
    
    [SerializeField] public Transform Shop_Cash = null;

    [SerializeField] public Transform SellSlot = null;
    [SerializeField] public Transform SellUI = null;

    [SerializeField] public List<GameObject> Weapon_Slot = new List<GameObject>();
    [SerializeField] public List<GameObject> Consumer_Slot = new List<GameObject>();
    [SerializeField] public List<GameObject> Armor_Slot = new List<GameObject>();

    [SerializeField] public static List<Transform> Holder = new List<Transform>();

    [SerializeField] public List<GameObject> Loadout_Consumer = new List<GameObject>();

    [SerializeField] private PlayerInventory Inventory = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is create

    public static bool useMainWP = true;

    private GameObject wp_loadout;
    private ItemInstance usingWP = null;

    HealthSystem health = null;
    public Player player = null;

    private PhotonView view;

    // Item Interaction

    private float timer = 0f;
    private float atkCooldown = 0f;

    void Start()
    {
        if (!Application.isEditor)
        {
            Admin_Button.gameObject.SetActive(false);
        }

        foreach (GameObject item in Weapon_Slot)
        {
            Holder.Add(item.transform);
        }

        foreach (GameObject item in Consumer_Slot)
        {
            Holder.Add(item.transform);
        }

        foreach (GameObject item in Armor_Slot)
        {
            Holder.Add(item.transform);
        }
        
        view = GetComponent<PhotonView>();
        Game.localPlayer = Current_Player;

        UI = transform;

        health = Current_Player.GetComponent<HealthSystem>();
        player = Current_Player.GetComponent<Player>();

        Inventory = Current_Player.GetComponent<PlayerInventory>();

        Loadout_UI = transform.parent.Find("Loadout").gameObject;

        ItemStats_UI = transform.parent.Find("ItemStats").gameObject;

        Admin_UI = transform.parent.Find("AdminPanel").Find("UI").gameObject;

        Fallen_UI = transform.parent.Find("FallenScreen");

        Shop_UI = transform.parent.Find("ShopUI");

        wp_loadout = Loadout_UI.transform.Find("Weapon").Find("Icon").gameObject;
        LoadInventory();
    }

    // Update is called once per frame
    void Update()
    {
        if (HP_UI != null)
        {
            Transform bar = HP_UI.transform.Find("Hider");
            RectTransform hider = bar.GetComponent<RectTransform>();
            float scale = Mathf.Clamp01((float)health.CurrentHealth / (float)health.MaxHealth);
            hider.localScale = new Vector3(1f - scale, 1, 1);

            Transform display = HP_UI.transform.Find("Text");
            TMP_Text text = display.GetComponent<TMP_Text>();
            text.text = $"{health.CurrentHealth}/{health.MaxHealth}";
        }

        if (AP_UI != null)
        {
            Transform bar = AP_UI.transform.Find("Hider");
            RectTransform hider = bar.GetComponent<RectTransform>();
            float scale = 0f;
            if (health.MaxArmor > 0)
            {
                scale = Mathf.Clamp01((float)health.CurrentArmor / (float)health.MaxArmor);
            }
            hider.localScale = new Vector3(1f - scale, 1, 1);

            Transform display = AP_UI.transform.Find("Text");
            TMP_Text text = display.GetComponent<TMP_Text>();
            text.text = $"{health.CurrentArmor}/{health.MaxArmor}";
        }

        if (MP_UI != null)
        {
            Transform bar = MP_UI.transform.Find("Hider");
            RectTransform hider = bar.GetComponent<RectTransform>();
            float scale = Mathf.Clamp01((float)player.CurrentMana / (float)player.MaxMana);
            hider.localScale = new Vector3(1f - scale, 1, 1);

            Transform display = MP_UI.transform.Find("Text");
            TMP_Text text = display.GetComponent<TMP_Text>();
            text.text = $"{player.CurrentMana}/{player.MaxMana}";
        }

        if (Cash_UI != null)
        {
            TMP_Text text = Cash_UI.GetComponent<TMP_Text>();
            text.text = "$" + player.cash;
        }

        if (usingWP != null && usingWP.itemRef)
        {
            UnityEngine.UI.Image Icon = ItemStats_UI.transform.Find("Icon").Find("Image").GetComponent<UnityEngine.UI.Image>();
            Icon.sprite = usingWP.itemRef.icon;

            TextMeshProUGUI itemName = ItemStats_UI.transform.Find("Name").Find("Text").GetComponent<TextMeshProUGUI>();
            itemName.SetText(usingWP.itemRef.itemName);

            RectTransform scale = ItemStats_UI.transform.Find("Amount").Find("Scale").GetComponent<RectTransform>();

            TextMeshProUGUI reserve = ItemStats_UI.transform.Find("Amount").Find("Text").GetComponent<TextMeshProUGUI>();

            if (usingWP.itemRef.canShoot && !usingWP.itemRef.isConsumable)
            {
                reserve.SetText($"{usingWP.ammo}/{usingWP.itemRef.clipSize}");
                scale.localScale = new Vector3(Mathf.Lerp(scale.localScale.x, (float)usingWP.ammo/(float)usingWP.itemRef.clipSize, Time.deltaTime * 10), 1f, 1f);
            }
            else
            {
                scale.localScale = new Vector3(1f, 1f, 1f);
                reserve.SetText("N/A");
            }


            ItemStats_UI.SetActive(true);
        }
        else
        {
            ItemStats_UI.SetActive(false);
        }

        if (SelectedItem.ItemData != null && SelectedItem.ItemData.itemRef)
        {
            Selected_UI.SetActive(true);
        }
        else
        {
            Selected_UI.SetActive(false);
        }

        if (player != null)
        {
            if (player.fallen)
            {
                Fallen_UI.gameObject.SetActive(true);
            } else
            {
                Fallen_UI.gameObject.SetActive(false);
            }
        }

        timer += Time.deltaTime;
        atkCooldown -= Time.deltaTime;

        if (player!= null)
        {
            if (player.fallen)
            {
                return;
            }
        }

        if (usingWP != null && usingWP.itemRef)
        {
            if (Input.GetMouseButton(0) && !Iventory_UI.activeSelf && !Admin_UI.activeSelf && !Shop_UI.gameObject.activeSelf && !Pause_UI.gameObject.activeSelf)
            {
                if (atkCooldown <= 0f)
                {
                    atkCooldown = usingWP.itemRef.cooldown;
                    Inventory.Attack();
                }
            }
        }

        if (!PlayerUI.isChatting)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UseItem(Consumer_Slot[0].GetComponent<ItemHolder>());
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                UseItem(Consumer_Slot[1].GetComponent<ItemHolder>());
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                UseItem(Consumer_Slot[2].GetComponent<ItemHolder>());
            }

            if (PhotonNetwork.IsMasterClient)
            {
                if (Input.GetKeyDown(KeyCode.Keypad9))
                {
                    Admin_UI.gameObject.SetActive(true);
                }
            }
        }
    }

    // Chat

    [PunRPC]
    private void RPC_ChatText(string player, string chat)
    {
        if (chat != "")
        {
            GameObject newChat = Instantiate(ChatTextSample, Chats.transform);

            TextMeshProUGUI textMeshProUGUI = newChat.GetComponent<TextMeshProUGUI>();

            textMeshProUGUI.SetText(player + ": " + chat);
            newChat.transform.SetAsLastSibling();
            newChat.SetActive(true);

            if (player == PhotonNetwork.LocalPlayer.NickName)
            {
                textMeshProUGUI.color = Color.yellow;
            }

            if (!Chat_UI.activeSelf)
            {
                Notification.SetActive(true);
            }

            if (Chats.transform.childCount > 20)
            {
                Transform deleteChat = Chats.transform.GetChild(0);
                deleteChat.gameObject.SetActive(false);

                Destroy(deleteChat.gameObject);
            }
        }
    }

    public void ToggleIsChatting(bool value)
    {
        isChatting = value;
    }

    public void ToggleChatUI()
    {
        Chat_UI.SetActive(!Chat_UI.activeSelf);
        if (Chat_UI.activeSelf)
        {
            Notification.SetActive(false);
        }
    }

    public void Chat()
    {
        string text = "";

        TMP_InputField textMeshProUGUI = ChatText.GetComponent<TMP_InputField>();
        text = textMeshProUGUI.text;

        textMeshProUGUI.SetTextWithoutNotify("");
        if (text != "")
        {
            view.RPC("RPC_ChatText", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, text);
        }
    }

    // Inventory

    public void DropItem(int amount)
    {
        if (SelectedItem.ItemData != null && SelectedItem.ItemData.itemRef)
        {
            bool removed = false;
            Transform holder = SelectedItem.ItemData.holder;
            if (SelectedItem.ItemData.amount - amount <= 0)
            {
                if (holder != null)
                {
                    ItemHolder itemholder = holder.GetComponent<ItemHolder>();
                    itemholder.Unequip(true);
                }
                removed = true;
            }
            Inventory.DropItem(SelectedItem.ItemData, amount);
            UpdateInventory();

            if (removed)
            {
                SelectedItem.ItemData = null;
            }
        }
    }

    public void OpenShop(int shopIndex)
    {
        SelectedShopItem.ShopID = shopIndex;
        Iventory_UI.SetActive(false);
        Shop shop = Game.g_shops.transform.GetChild(shopIndex).GetComponent<Shop>();

        foreach (Transform item in ShopStorage.transform)
        {
            item.gameObject.SetActive(false);
        }

        foreach (Trade trade in shop.data.shop)
        {
            Transform item = ShopStorage.transform.Find(trade.item.itemID);
            if (item != null)
            {
                item.gameObject.SetActive(true);
            } else
            {
                item = Instantiate(ShopButton.transform, Vector3.zero, Quaternion.Euler(0f, 0f, 0f), ShopStorage.transform);
                item.SetAsLastSibling();

                item.name = trade.item.itemID;

                ItemInstanceButton itemButton = item.GetComponent<ItemInstanceButton>();
                itemButton.cost = trade.cost;
                itemButton.item = new ItemInstance(trade.item);
                itemButton.itemReference = trade.item;

                item.gameObject.SetActive(true);
            }
        }

        foreach (Transform item in ShopStorage.transform)
        {
            if (item != null && !item.gameObject.activeSelf)
            {
                Destroy(item.gameObject);
            } 
        }

        Shop_UI.gameObject.SetActive(true);
    }

    public void CloseShop()
    {
        if (Shop_UI.gameObject.activeSelf)
        {
            Shop_UI.gameObject.SetActive(false);
            SelectedShopItem.ItemData = null;
        }
    }

    public void HideSell()
    {
        if (SellUI.gameObject.activeSelf)
        {
            SellUI.gameObject.SetActive(false);
        }
    }

    public void WIN_Game()
    {
        Win_UI.gameObject.SetActive(true);
    }

    public void BuyItem(int amount)
    {
        Inventory.Purchase(SelectedShopItem.ItemData.itemRef.itemID, amount, SelectedShopItem.ShopID);
    }

    public void ToggleInventory()
    {
        Iventory_UI.SetActive(!Iventory_UI.activeSelf);
        SellUI.gameObject.SetActive(false);
        Shop_UI.gameObject.SetActive(false);
        if (Iventory_UI.activeSelf)
        {
            Admin_UI.SetActive(false);
        }
    }

    public void OpenSell()
    {
        Iventory_UI.SetActive(true);
        SellUI.gameObject.SetActive(true);
        Shop_UI.gameObject.SetActive(false);
        if (Iventory_UI.activeSelf)
        {
            Admin_UI.SetActive(false);
        }
    }

    public void UpdateInventory()
    {
        LoadInventory();
    }

    public void SellItem(ItemInstance item)
    {
        Inventory.SellItem(item);
    }

    public void UseSelectItem()
    {
        if (SelectedItem.ItemData != null && SelectedItem.ItemData.itemRef)
        {
            if (SelectedItem.action == "Use")
            {
                bool ranOut = false;
                Transform holder = SelectedItem.ItemData.holder;
                if (SelectedItem.ItemData.amount <= 1)
                {
                    ranOut = true;
                }
                Inventory.UseItem(SelectedItem.ItemData);
                if (ranOut)
                {
                    if (holder != null) holder.GetComponent<ItemHolder>().Unequip(true);
                }
            } else
            {
                Transform holder = SelectedItem.ItemData.holder;
                if (holder != null) holder.GetComponent<ItemHolder>().Unequip(true);
            }
        }
    }

    public void ToggleArmor(ItemHolder holder)
    {
        Inventory.Wearing(holder.item, holder.SlotType);
    }

    public void UseItem(ItemHolder holder)
    {
        if (holder.item != null && holder.item.itemRef)
        {
            bool ranOut = false;
            if (holder.item.amount <= 1)
            {
                ranOut = true;
            }
            Inventory.UseItem(holder.item);

            if (ranOut) holder.Unequip(true);
        }
    }

    void LoadInventory()
    {
        foreach (Transform item in Storage.transform)
        {
            item.gameObject.SetActive(false);
        }

        for (int index = 0; index < Inventory.Items.Count; index++)
        {
            ItemInstance item = Inventory.Items[index];

            if (item.amount>0)
            {
                Transform storageItem = null;
                if (index < Storage.transform.childCount)
                {
                    storageItem = Storage.transform.GetChild(index);
                }
                if (storageItem == null)
                {
                    AddInventoryButton(item);
                }
                else
                {
                    item.storage = storageItem;
                }

                ItemInstanceButton itemButton = item.storage.GetComponent<ItemInstanceButton>();
                itemButton.item = item;
                itemButton.itemReference = item.itemRef;

                item.storage.gameObject.SetActive(true);
            }
        }

        foreach (Transform item in Storage.transform)
        {
            if (item != null && !item.gameObject.activeSelf)
            {
                Destroy(item.gameObject);
            }
        }

        UpdateLoadOut();
    }

    public void AddInventoryButton(ItemInstance item)
    {
        if (item.storage == null)
        {
            item.storage = Instantiate(Button.transform, Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Storage.transform);
            item.storage.gameObject.transform.SetAsLastSibling();

            ItemInstanceButton itemButton = item.storage.GetComponent<ItemInstanceButton>();
            itemButton.item = item;
            itemButton.itemReference = item.itemRef;

            item.storage.gameObject.SetActive(true);
        }
    }

    public void UpdateWP()
    {
        try
        {
            ItemHolder wp = null;
            if (useMainWP) wp = Weapon_Slot[0].GetComponent<ItemHolder>();
            else wp = Weapon_Slot[1].GetComponent<ItemHolder>();

            if (wp.item != null && wp.item.itemRef)
            {
                usingWP = wp.item;
                wp_loadout.transform.GetComponent<UnityEngine.UI.Image>().sprite = wp.item.itemRef.icon;
                wp_loadout.SetActive(true);
            }
            else
            {
                usingWP = null;
                wp_loadout.SetActive(false);
            }

            Inventory.Holding(usingWP);
        }
        catch
        {

        }
    }

    public void UpdateLoadOut()
    {
        for (int i=0; i<Consumer_Slot.Count; i++)
        {
            try
            {
                ItemHolder dat = Consumer_Slot[i].GetComponent<ItemHolder>();
                Transform loadout = Loadout_Consumer[i].transform;
                GameObject icon = loadout.Find("Icon").gameObject;
                GameObject amount = loadout.Find("Amount").gameObject;
                if (dat.item == null)
                {
                    icon.SetActive(false);
                    amount.SetActive(false);
                }
                else
                {
                    icon.transform.GetComponent<UnityEngine.UI.Image>().sprite = dat.item.itemRef.icon;
                    amount.transform.GetComponent<TextMeshProUGUI>().SetText("x"+dat.item.amount);
                    icon.SetActive(true);
                    amount.SetActive(true);
                }
            } catch
            {

            }
        }

        UpdateWP();
    }

}
