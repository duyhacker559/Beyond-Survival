using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class PlayerInventory : MonoBehaviour
{

    [SerializeField] public List<ItemInstance> Items;

    [SerializeField] public float pickUpRange = 2f;

    [SerializeField] public ItemInstance holdingItem;

    [SerializeField] public List<ItemInstance> Armor;
    [SerializeField] public List<string> currentWearing;

    private Transform nearestItem = null;
    private Transform nearestFallen = null;
    private Transform nearestShop = null;
    private Transform nearestBlackSmith = null;

    private float revivePower = 0f;
    private float shopPower = 0f;
    private float blackSmithPower = 0f;
    private int shopOwner = 0;
    private int blackSmithOwner = 0;

    private float scanTime = 0.2f;
    private float scanTimer = -2f;

    private string currentWeaponModel = "";
    private Transform weaponMuzzle;
    private Transform particleEmitter;

    private HealthSystem healthSystem;
    private Player player;
    private PhotonView view;

    // Attack

    private int _itemIndex = 0;

    private float timer = 0f;
    private float atkCooldown = 0f;

    private float reloadCheckTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        player = GetComponent<Player>();
        view = GetComponent<PhotonView>();
        healthSystem = GetComponent<HealthSystem>();

        for (int i=0; i<3; i++)
        {
            Armor.Add(null);
            currentWearing.Add("");
        }

        for  (int i=0; i<Items.Count; i++)
        {
            Items[i].itemID = i;
            _itemIndex = i + 1;
        }
    }

    void Start()
    {
        
    }

    [PunRPC]
    public void RPC_Holding(int itemIndex)
    {
        if (itemIndex == -1)
        {
            holdingItem = null;
        } else
        {
            ItemInstance item = GetItemInstanceFromID(itemIndex);
            if (item != null)
            {
                holdingItem = item;
            }
        }
        healthSystem.calculateArmor(Armor, holdingItem);
    }

    public void Holding(ItemInstance item)
    {
        holdingItem = item;
        if (holdingItem != null)
        {
            if (Items.Contains(item))
            {
                view.RPC("RPC_Holding", RpcTarget.Others, item.itemID);
            }
        } else
        {
            view.RPC("RPC_Holding", RpcTarget.Others, -1);
        }
        healthSystem.calculateArmor(Armor, holdingItem);
    }

    [PunRPC]
    public void RPC_Wearing(int itemIndex, int slot)
    {
        if (itemIndex == -1)
        {
            if (Armor[slot] != null)
            {
                Armor[slot] = null;
            }
        }
        else
        {
            ItemInstance item = GetItemInstanceFromID(itemIndex);
            if (item != null)
            {
                Armor[slot] = item;
            }
        }
        healthSystem.calculateArmor(Armor, holdingItem);
    }

    public void Wearing(ItemInstance item, int slot)
    {
        Armor[slot] = item;
        if (Armor[slot] != null)
        {
            if (Items.Contains(item))
            {
                view.RPC("RPC_Wearing", RpcTarget.Others, item.itemID, slot);
            }
        }
        else
        {
            view.RPC("RPC_Wearing", RpcTarget.Others, -1, slot);
        }
        healthSystem.calculateArmor(Armor, holdingItem);
    }

    // Game function

    public int FindItem(ItemInstance item)
    {
        int index = 0;
        for (index = 0; index < Items.Count; index++)
        {
            if (Items[index] != null)
            {
                if (item.itemRef.itemName == Items[index].itemRef.itemName)
                {
                    return index;
                }
            }
        }
        return -1;
    }

    public ItemInstance GetItemInstanceFromID(int id)
    {
        int index = 0;
        for (index = 0; index < Items.Count; index++)
        {
            if (Items[index] != null)
            {
                if (Items[index].itemID == id)
                {
                    return Items[index];
                }
            }
        }
        return null;
    }

    public int GetItemIndexFromID(int id)
    {
        int index = 0;
        for (index = 0; index < Items.Count; index++)
        {
            if (Items[index] != null)
            {
                if (Items[index].itemID == id)
                {
                    return index;
                }
            }
        }
        return -1;
    }

    // Update Item

    [PunRPC]
    private void RPC_UpdateItem(int id, string json)
    {
        ItemInstance data = new ItemInstance(JsonUtility.FromJson<ItemInstanceSender>(json));
       // Debug.Log(data.itemID);
       // Debug.Log(data.amount);
        int index = GetItemIndexFromID((int)id);

        //Debug.Log(index);

        Items[index].amount = data.amount;

        if (Items[index].amount <= 0)
        {
            Items[index].storage.gameObject.SetActive(false);
            Items.RemoveAt(index);
        }
    }

    // Drop Item

    [PunRPC]
    private void Master_DropItem(int id, int amount)
    {
        int index = GetItemIndexFromID (id);
        if (Items[index] != null)
        {
            if (Items[index].itemRef.isConsumable || Items[index].itemRef._itemType == ItemType.QuestItem)
            {
                int DropAmount = (int)Mathf.Min(amount, Items[index].amount);
                GameManager.Instance.SpawnItem(Items[index].itemRef, DropAmount, transform.position, new Quaternion(0, 0, 0, 0));

                Items[index].amount -= DropAmount;
                view.RPC("RPC_UpdateItem", RpcTarget.Others, id, (new ItemInstanceSender(Items[index])).ToJson());
                if (Items[index].amount <= 0)
                {
                    Items.RemoveAt(index);
                }
            }
            else
            {
                GameManager.SpawnItem(Items[index], transform.position, new Quaternion(0, 0, 0, 0));
                Items[index].amount = 0;
                view.RPC("RPC_UpdateItem", RpcTarget.Others, id, (new ItemInstanceSender(Items[index])).ToJson());
                Items.RemoveAt(index);
            }
        }
    }

    public void DropItem(ItemInstance item, int amount)
    {
        if (Items.Contains(item)) { }
        else return;
            int index = Items.IndexOf(item);
        if (Items[index] != null)
        {
            if (Items[index].itemRef.isConsumable || Items[index].itemRef._itemType == ItemType.QuestItem)
            {
                int DropAmount = (int)Mathf.Min(amount, Items[index].amount);
                if (!PhotonNetwork.IsMasterClient)
                {
                    Items[index].amount -= DropAmount;
                }
                view.RPC("Master_DropItem", RpcTarget.MasterClient, Items[index].itemID, DropAmount);
            }
            else
            {
                view.RPC("Master_DropItem", RpcTarget.MasterClient, Items[index].itemID, Items[index].amount);
            }
        }
    }

    // Use Item

    [PunRPC]
    private void Master_UseItem(int id, Vector3 pos, float look)
    {
        int index = GetItemIndexFromID(id);
        if (Items[index] != null)
        {
            if (Items[index].itemRef.isConsumable && Items[index].amount > 0)
            {

                if (Items[index].itemRef.canShoot)
                {
                    Vector3 CurrentPos = pos;

                    for (int i = 0; i < Items[index].itemRef.fireAmount; i++)
                    {
                        GameManager.SummonProjectile(transform.gameObject,
                            CurrentPos,
                            Quaternion.Euler(0, 0, look + UnityEngine.Random.Range(-Items[index].itemRef.spread, Items[index].itemRef.spread)),
                            new ProjectileData(
                                Items[index].itemRef.bulletSpeed,
                                Items[index].itemRef.damage,
                                Items[index].itemRef.knockBack,
                                Items[index].itemRef.knockBack_Duration,
                                Items[index].itemRef.bulletLifetime,
                                Items[index].itemRef.effects,
                                Items[index].itemRef.projectileDat,
                                Game.g_enemies.transform
                            ),
                            Items[index].itemRef.projectile
                        );
                    }
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    foreach (Modify mod in Items[index].itemRef.consumeEffect)
                    {
                        switch (mod.modify_ID)
                        {
                            case "HP":
                                player.health.Heal(mod.modify_IntValue);
                                break;
                            case "MP":
                                player.GainMana((float)mod.modify_IntValue);
                                break;
                            case "AP":
                                player.health.AddArmor((int)mod.modify_IntValue);
                                break;
                            default:
                                break;
                        }
                    }

                    foreach (DamageEffect effect in Items[index].itemRef.effects.ToList<DamageEffect>())
                    {
                        player.health.addEffect(effect);
                    }

                    Items[index].amount -= 1;
                    view.RPC("RPC_UpdateItem", RpcTarget.Others, id, (new ItemInstanceSender(Items[index])).ToJson());
                    if (Items[index].amount <= 0)
                    {
                        Items.RemoveAt(index);
                    }

                    if (PhotonNetwork.IsMasterClient)
                    {
                        PlayerUI.UI.transform.GetComponent<PlayerUI>().UpdateInventory();
                    }
                }
            }
        }
    }

    public void UseItem(ItemInstance item)
    {
        if (Items.Contains(item)) { }
        else return;
        int index = Items.IndexOf(item);
        if (Items[index] != null)
        {
            if (Items[index].itemRef.isConsumable && Items[index].amount>0)
            {
                Vector2 fireDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
                float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

                Vector3 CurrentPos = transform.position;

                view.RPC("Master_UseItem", RpcTarget.All, item.itemID, CurrentPos, angle);

                if (!PhotonNetwork.IsMasterClient)
                {
                    Items[index].amount -= 1;
                    PlayerUI.UI.transform.GetComponent<PlayerUI>().UpdateInventory();
                }
            }
        }
    }

    // Add Item
    [PunRPC]
    private void WIN()
    {
        PlayerUI.UI.transform.GetComponent<PlayerUI>().WIN_Game();
    }

    [PunRPC]
    private void Master_AddItem(string id)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (Transform item in Game.g_items.transform)
            {
                if ((item.position - transform.position).magnitude < pickUpRange)
                {
                    ItemObject itemObject = item.GetComponent<ItemObject>();
                    ItemInstance itemData = itemObject.Data;
                    if (item.name == id)
                    {
                        if (itemData.itemRef.useOnDelete)
                        {
                            int amount = itemData.amount;
                            foreach (Modify mod in itemData.itemRef.consumeEffect)
                            {
                                switch (mod.modify_ID)
                                {
                                    case "HP":
                                        player.health.Heal(mod.modify_IntValue * amount);
                                        break;
                                    case "MP":
                                        player.GainMana((float)mod.modify_IntValue * amount);
                                        break;
                                    case "AP":
                                        player.health.AddArmor((int)mod.modify_IntValue * amount);
                                        break;
                                    case "Cash":
                                        player.cash += (int)mod.modify_IntValue * amount;
                                        break;
                                    default:
                                        break;
                                }
                            }

                            foreach (DamageEffect effect in itemData.itemRef.effects.ToList<DamageEffect>())
                            {
                                player.health.addEffect(effect);
                            }

                            PhotonNetwork.Destroy(item.gameObject);
                            healthSystem.UpdateStats();
                            player.UpdateCash(player.cash);
                        } else
                        {

                            if (itemData.itemRef.itemID == "TheCure")
                            {
                                view.RPC("WIN", RpcTarget.All);
                                DayNightCycle2D.timeScale = 0f;
                                foreach (Transform enemy in Game.g_enemies.transform)
                                {
                                    try
                                    {
                                        HealthSystem healthSystem = enemy.GetComponent<HealthSystem>();
                                        healthSystem.TakeDamage(999999999);
                                    } catch
                                    {

                                    }
                                }
                            }

                            AddItem(itemData);
                            PhotonNetwork.Destroy(item.gameObject);
                            view.RPC("Client_AddItem", RpcTarget.Others, new ItemInstanceSender(itemData).ToJson());
                        }
                        return;
                    }
                }
            }
        }
    }

    [PunRPC]
    private void Client_AddItem(string json)
    {
        AddItem(new ItemInstance(JsonUtility.FromJson<ItemInstanceSender>(json)));
    }

    public void AddItem(ItemInstance item)
    {
        int index = FindItem(item);
        if (index >= 0 && (item.itemRef.isConsumable || item.itemRef._itemType == ItemType.QuestItem))
        {
            Items[index].amount += item.amount;
        }
        else
        {
            Items.Add(new ItemInstance(_itemIndex++, item));
        }
        if (view.IsMine)
        {
            if (PlayerUI.UI != null)
            {
                PlayerUI.UI.GetComponent<PlayerUI>().UpdateInventory();
            }
        }
    }

    //---------------------------------------------------------

    [PunRPC]
    private void RPC_Cash(int cash)
    {
        player.cash = cash;
    }

    [PunRPC]
    private void Master_Sell(int id)
    {
        int index = GetItemIndexFromID(id);
        if (Items[index] != null)
        {
            if (Items[index].itemRef.canSell)
            {
                int earn = (int)(Items[index].itemRef.value * (Items[index].amount / Items[index].itemRef.amountThreshold));
                player.cash += earn;

                Items[index].amount = 0;
                view.RPC("RPC_UpdateItem", RpcTarget.Others, id, (new ItemInstanceSender(Items[index])).ToJson());
                view.RPC("RPC_Cash", RpcTarget.Others, player.cash);
                Items.RemoveAt(index);
            }
        }
    }

    public void SellItem(ItemInstance item)
    {
        int index = FindItem(item);
        if (index >= 0 && item.itemRef.canSell)
        {
            view.RPC("Master_Sell", RpcTarget.MasterClient, item.itemID);
        }
    }

    //---------------------------------------------------------

    private Transform FindNearItem()
    {
        Transform nearest = null;
        float nearestDis = pickUpRange;
        float dis;
        if (Game.items != null)
        {
            foreach (Transform item in Game.g_items.transform)
            {
                dis = (item.position - transform.position).sqrMagnitude;
                if (dis < pickUpRange)
                {
                    if (nearest == null || (dis < nearestDis)) 
                    {
                        nearest = item;
                        nearestDis = dis;
                    }
                }
            }
        }

        return nearest;
    }

    private Transform FindFallenAlly()
    {
        Transform nearest = null;
        float nearestDis = pickUpRange;
        float dis;
        if (Game.items != null)
        {
            foreach (Transform item in Game.g_players.transform)
            {
                dis = (item.position - transform.position).sqrMagnitude;
                if (dis < pickUpRange && item.GetComponent<Player>().fallen && item.name != PhotonNetwork.LocalPlayer.NickName)
                {
                    if (nearest == null || (dis < nearestDis))
                    {
                        nearest = item;
                        nearestDis = dis;
                    }
                }
            }
        }

        return nearest;
    }

    private Transform FindNearShop()
    {
        Transform nearest = null;
        float nearestDis = pickUpRange;
        float dis;
        if (Game.items != null)
        {
            for (int i=0;i< Game.g_shops.transform.childCount; i++)
            {
                Transform item = Game.g_shops.transform.GetChild(i);
                dis = (item.position - transform.position).sqrMagnitude;
                if (dis < pickUpRange)
                {
                    if (nearest == null || (dis < nearestDis))
                    {
                        shopOwner = i;
                        nearest = item;
                        nearestDis = dis;
                    }
                }
            }
        }

        return nearest;
    }

    private Transform FindNearBlackSmith()
    {
        Transform nearest = null;
        float nearestDis = pickUpRange;
        float dis;
        if (Game.items != null)
        {
            for (int i = 0; i < Game.g_blackSmiths.transform.childCount; i++)
            {
                Transform item = Game.g_blackSmiths.transform.GetChild(i);
                dis = (item.position - transform.position).sqrMagnitude;
                if (dis < pickUpRange)
                {
                    if (nearest == null || (dis < nearestDis))
                    {
                        blackSmithOwner = i;
                        nearest = item;
                        nearestDis = dis;
                    }
                }
            }
        }

        return nearest;
    }

    void GetWeaponModel(string model)
    {
        if (model != currentWeaponModel)
        {
            currentWeaponModel = model;

            if (holdingItem != null && holdingItem.itemRef != null)
            {

                foreach (Transform item in player.HandItem.transform)
                {
                    Destroy(item.gameObject);
                }

                GameObject wp_model = Instantiate(holdingItem.itemRef.model, player.HandItem.transform);
                wp_model.name = "Model";

                wp_model.GetComponent<SpriteRenderer>().sortingOrder = player.HandItem.parent.GetComponent<SpriteRenderer>().sortingOrder - 1;

                weaponMuzzle = wp_model.transform.Find("Muzzle");
                particleEmitter = wp_model.transform.Find("Particle");
            }
        }
    }

    private void GetWearModel(ItemInstance wearing, Transform model, Sprite oldSprite)
    {
        foreach (Transform item in model)
        {
            Destroy(item.gameObject);
        }

        if (wearing != null)
        {
            if (wearing.itemRef.armor_modelType == ArmorModelType.Model)
            {
                GameObject wp_model = Instantiate(wearing.itemRef.armor_Model, model);
                wp_model.transform.localPosition = new Vector3(0, 0, 0);

                Vector3 scale = wp_model.transform.localScale;
                scale.x = Mathf.Abs(scale.x);
                scale.y = Mathf.Abs(scale.y);
                wp_model.transform.localScale = scale;
            }
            else
            {
                model.GetComponent<SpriteRenderer>().sprite = wearing.itemRef.armor_Sprite;
            }
        } else
        {
            model.GetComponent<SpriteRenderer>().sprite = oldSprite;
        }
    }

    int CheckForMedkit()
    {
        int index = -1;

        foreach (ItemInstance item in Items)
        {
            if (item.itemRef.itemID == "Medkit")
            {
                return Items.IndexOf(item);
            }
        }

        return index;
    }

    void GetArmorModel(int slot, string model)
    {
        if (currentWearing[slot] != model)
        {
            currentWearing[slot] = model;

            if (Armor[slot] != null && Armor[slot].itemRef != null)
            {

                switch (slot)
                {
                    case 0:
                        player.model_Hair.gameObject.SetActive(!Armor[slot].itemRef.hide_Hair);
                        GetWearModel(Armor[slot], player.model_Hat, null);
                        break;
                    case 1:
                        GetWearModel(Armor[slot], player.model_Body, player._class.Body);
                        break;
                    case 2:
                        GetWearModel(Armor[slot], player.model_Pant_L, player._class.Leg);
                        GetWearModel(Armor[slot], player.model_Pant_R, player._class.Leg);
                        break;
                    default:
                        break;
                }
            } else
            {
                switch (slot)
                {
                    case 0:
                        player.model_Hair.gameObject.SetActive(true);
                        GetWearModel(null, player.model_Hat, null);
                        break;
                    case 1:
                        GetWearModel(null, player.model_Body, player._class.Body);
                        break;
                    case 2:
                        GetWearModel(null, player.model_Pant_L, player._class.Leg);
                        GetWearModel(null, player.model_Pant_R, player._class.Leg);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    [PunRPC]
    private void Master_RevivePlayer(string player)
    {
        Transform revivePlayer = null;

        foreach (Transform ally in Game.g_players.transform)
        {
            if (ally.name == player)
            {
                revivePlayer = ally;
                break;
            }
        }

        if (revivePlayer != null)
        {
            int index = CheckForMedkit();
            if (index >= 0)
            {
                ItemInstance medkit = Items[index];

                if (medkit != null)
                {
                    if (medkit.amount>0)
                    {
                        medkit.amount--;
                        revivePlayer.GetComponent<Player>().Revive();

                        view.RPC("RPC_UpdateItem", RpcTarget.Others, medkit.itemID, (new ItemInstanceSender(Items[index])).ToJson());
                        if (Items[index].amount <= 0)
                        {
                            Items.RemoveAt(index);
                        }
                    }
                }
            }
        }
    }

    void Update()
    {
        if (view.IsMine)
        {
            scanTimer += Time.fixedDeltaTime;
            reloadCheckTimer += Time.fixedDeltaTime;

            if (!PlayerUI.isChatting)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    PlayerUI.useMainWP = !(PlayerUI.useMainWP);
                    PlayerUI.UI.transform.GetComponent<PlayerUI>().UpdateWP();
                }

                if (Input.GetKeyDown(KeyCode.B))
                {
                    PlayerUI.UI.transform.GetComponent<PlayerUI>().ToggleInventory();
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    Reload();
                }
            }

            if (scanTimer > 0f)
            {
                scanTimer -= scanTime;

                if (nearestItem != null)
                {
                    GameObject canvas = nearestItem.transform.Find("Canvas").gameObject;
                    canvas.SetActive(false);
                }

                nearestItem = FindNearItem();

                //

                if (nearestFallen != null)
                {
                    GameObject canvas = nearestFallen.transform.Find("Canvas").Find("Revive").gameObject;
                    canvas.SetActive(false);
                }

                nearestFallen = FindFallenAlly();

                //

                if (nearestShop != null)
                {
                    GameObject canvas = nearestShop.transform.Find("Canvas").Find("Interact").gameObject;
                    canvas.SetActive(false);
                }

                nearestShop = FindNearShop();

                if (nearestShop == null)
                {
                    PlayerUI.UI.transform.GetComponent<PlayerUI>().CloseShop();
                    PlayerUI.UI.transform.GetComponent<PlayerUI>().HideSell();
                }

                //

                if (nearestBlackSmith != null)
                {
                    GameObject canvas = nearestBlackSmith.transform.Find("Canvas").Find("Interact").gameObject;
                    canvas.SetActive(false);
                }
                nearestBlackSmith = FindNearBlackSmith();
            }

            if (nearestItem != null)
            {
                ItemObject item = nearestItem.GetComponent<ItemObject>();
                GameObject canvas = nearestItem.transform.Find("Canvas").gameObject;
                canvas.SetActive(true);
                canvas.transform.Find("Image").Find("Input").GetComponent<TextMeshProUGUI>().SetText(item.Data.itemRef.inputKey.ToString());
                canvas.transform.localPosition = new Vector3(0, 0.6f + 0.05f * Mathf.Cos(Time.time * 2), 0);

                if (!PlayerUI.isChatting && Input.GetKeyDown(item.Data.itemRef.inputKey))
                {
                    view.RPC("Master_AddItem", RpcTarget.MasterClient, nearestItem.name);
                    nearestItem = null;
                }
            }

            if (nearestFallen != null)
            {
                try
                {
                    GameObject canvas = nearestFallen.transform.Find("Canvas").Find("Revive").gameObject;
                    canvas.SetActive(true);

                    if (!PlayerUI.isChatting && Input.GetKey(KeyCode.E))
                    {
                        revivePower += Time.fixedDeltaTime;
                        if (revivePower >= 5f)
                        {
                            int itemIndex = CheckForMedkit();
                            if (itemIndex >= 0)
                            {
                                view.RPC("Master_RevivePlayer", RpcTarget.MasterClient, nearestFallen.name);
                            }
                            revivePower = 0f;
                            canvas.SetActive(false);
                            nearestFallen = null;
                        }
                    }
                    else
                    {
                        revivePower = 0f;
                    }

                    canvas.GetComponent<Image>().fillAmount = Mathf.Clamp01(revivePower / 5f);
                } catch
                {

                }
            }

            if (nearestShop != null)
            {
                GameObject canvas = nearestShop.transform.Find("Canvas").Find("Interact").gameObject;
                canvas.SetActive(true);

                if (!PlayerUI.isChatting && Input.GetKey(KeyCode.E))
                {
                    shopPower += Time.fixedDeltaTime;
                    if (shopPower >= 1f)
                    {
                        PlayerUI playerUI = PlayerUI.UI.transform.GetComponent<PlayerUI>();
                        Shop shop = Game.g_shops.transform.GetChild(shopOwner).GetComponent<Shop>();
                        if (shop.data.canSell)
                        {
                            if (!playerUI.Iventory_UI.activeSelf)
                            {
                                playerUI.OpenSell();
                            }
                        } else
                        {
                            if (!playerUI.Shop_UI.gameObject.activeSelf)
                            {
                                playerUI.OpenShop(shopOwner);
                            }
                        }

                        shopPower = 0f;
                        nearestShop = null;
                    }
                }
                else
                {
                    shopPower = 0f;
                }

                canvas.GetComponent<Image>().fillAmount = Mathf.Clamp01(shopPower / 1f);
            }

            if (nearestBlackSmith != null)
            {
                GameObject canvas = nearestBlackSmith.transform.Find("Canvas").Find("Interact").gameObject;
                canvas.SetActive(true);

                if (!PlayerUI.isChatting && Input.GetKey(KeyCode.E))
                {
                    blackSmithPower += Time.fixedDeltaTime;
                    if (blackSmithPower >= 1f)
                    {

                        BlackSmith blackSmith = nearestBlackSmith.GetComponent<BlackSmith>();

                        if (blackSmith != null)
                        {
                            if (player.cash >= blackSmith.data.cost)
                            {
                                if (blackSmith.data.addHP)
                                {
                                    view.RPC("Master_Upgrade", RpcTarget.MasterClient, blackSmithOwner);
                                } else
                                {
                                    if (holdingItem != null && holdingItem.itemRef)
                                    {
                                        if (holdingItem.itemRef.canAttack)
                                        {
                                            view.RPC("Master_Upgrade", RpcTarget.MasterClient, blackSmithOwner);
                                        }
                                    }
                                }
                            }
                        }

                        blackSmithPower = 0f;
                        nearestBlackSmith = null;
                    }
                }
                else
                {
                    blackSmithPower = 0f;
                }

                canvas.GetComponent<Image>().fillAmount = Mathf.Clamp01(blackSmithPower / 1f);
            }
        }

        timer += Time.deltaTime;
        atkCooldown -= Time.deltaTime;

        if (holdingItem != null && holdingItem.itemRef)
        {
            player.swingOffset = holdingItem.itemRef.swingOffset;
            GetWeaponModel(holdingItem.itemRef.itemID);
            player.HandItem.gameObject.SetActive(true);
            if (holdingItem.itemRef.canShoot && holdingItem.reloading && reloadCheckTimer>=0)
            {
                if (view.IsMine)
                {
                    CheckReload();
                }
                reloadCheckTimer -= 0.5f;
            }
        }
        else
        {
            player.swingOffset = 0f;
            GetWeaponModel("");
            player.HandItem.gameObject.SetActive(false);
        }

        for (int i=0; i<3; i++)
        {
            if (Armor[i] != null && Armor[i].itemRef)
            {
                GetArmorModel(i, Armor[i].itemRef.itemID);
            }
            else
            {
                GetArmorModel(i, "");
            }
        }
    }

    [PunRPC]
    public void Master_Upgrade(int shopID)
    {
        Transform shopKeeper = Game.g_blackSmiths.transform.GetChild(shopID);

        BlackSmith blackSmith = shopKeeper.GetComponent<BlackSmith>();

        if (blackSmith != null)
        {
            if (player.cash >= blackSmith.data.cost)
            {
                if (blackSmith.data.addHP)
                {
                    player.cash -= blackSmith.data.cost;

                    healthSystem.AddMaxHP((float)blackSmith.data.health);
                    return;
                }

                if (holdingItem != null && holdingItem.itemRef)
                {
                    if (holdingItem.itemRef.canAttack)
                    {
                        if (blackSmith.data.canUpgrade)
                        {
                            holdingItem.attachments.Add(blackSmith.data.upgrade);
                        }

                        if (blackSmith.data.canRepair && holdingItem.itemRef.canShoot)
                        {
                            holdingItem.amount += (int)(holdingItem.itemRef.durability * blackSmith.data.repartAmount);
                        }

                        player.cash -= blackSmith.data.cost;

                        view.RPC("RPC_UpdateItem", RpcTarget.Others, holdingItem.itemID, (new ItemInstanceSender(holdingItem)).ToJson());
                    }
                }
            }
        }
    }

    [PunRPC]
    public void Master_Purchase(string item, int amount, int shopID)
    {
        Transform shopKeeper = Game.g_shops.transform.GetChild(shopID);

        Shop shop = shopKeeper.GetComponent<Shop>();

        foreach (Trade trade in shop.data.shop)
        {
            if (trade.item.itemID == item)
            {
                if (player.cash >= amount * trade.cost)
                {
                    player.cash -= amount * trade.cost;

                    ItemInstance newItem = new ItemInstance(trade.item);

                    if (trade.item.isWeapon)
                    {
                        for(int i =0; i<amount; i++)
                        {
                            if (trade.item.weaponType == WeaponType.Melee)
                            {
                                newItem = new ItemInstance(trade.item, 0, 1);
                            }
                            else
                            {
                                newItem = new ItemInstance(trade.item, trade.item.clipSize, trade.item.durability);
                            }
                            AddItem(newItem);
                            view.RPC("Client_AddItem", RpcTarget.Others, new ItemInstanceSender(newItem).ToJson());
                        }
                    }
                    else
                    {
                        newItem = new ItemInstance(trade.item, 0, amount);
                        AddItem(newItem);
                        view.RPC("Client_AddItem", RpcTarget.Others, new ItemInstanceSender(newItem).ToJson());
                    }
                }
                return;
            }
        }
    }

    public void Purchase(string item, int amount, int shopID)
    {
        Transform shopKeeper = Game.g_shops.transform.GetChild(shopID);

        Shop shop = shopKeeper.GetComponent<Shop>();

        foreach (Trade trade in shop.data.shop)
        {
            if (trade.item.itemID == item)
            {
                if (player.cash >= amount*trade.cost)
                {
                    if (!PhotonNetwork.IsMasterClient)
                    {
                        player.cash -= amount * trade.cost;
                    }

                    view.RPC("Master_Purchase", RpcTarget.MasterClient, item, amount, shopID);
                }
                return;
            }
        }
    }

    [PunRPC]
    private void RPC_Attack(Vector3 pos, float look)
    {
        if (holdingItem != null && holdingItem.itemRef)
        {
            if (atkCooldown <= 0f && (holdingItem.ammo > 0 && holdingItem.itemRef.canShoot || holdingItem.itemRef.canMelee) && holdingItem.amount>0)
            {
                if (holdingItem.itemRef.canShoot)
                {
                    player.TriggerRecoil(holdingItem.itemRef.recoil);
                    player.TriggerSwing(holdingItem.itemRef.swing);

                    Vector3 CurrentPos = pos;

                    if (holdingItem.ammo > 0)
                    {
                        AudioSource audioSource = weaponMuzzle.GetComponent<AudioSource>();
                        if (audioSource != null) audioSource.Play();
                    }

                    float damageDealt = holdingItem.itemRef.damage;

                    if (holdingItem.attachments != null && holdingItem.attachments.Count>0)
                    {
                        foreach (string modify in holdingItem.attachments)
                        {
                            switch (modify)
                            {
                                case "Damage":
                                    damageDealt += holdingItem.itemRef.damage * 0.25f;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    for (int i = 0; i < holdingItem.itemRef.fireAmount; i++)
                    {
                        GameManager.SummonProjectile(transform.gameObject,
                            CurrentPos,
                            Quaternion.Euler(0, 0, look + UnityEngine.Random.Range(-holdingItem.itemRef.spread, holdingItem.itemRef.spread)),
                            new ProjectileData(
                                holdingItem.itemRef.bulletSpeed,
                                damageDealt,
                                holdingItem.itemRef.knockBack,
                                holdingItem.itemRef.knockBack_Duration,
                                holdingItem.itemRef.bulletLifetime,
                                holdingItem.itemRef.effects,
                                holdingItem.itemRef.projectileDat,
                                Game.g_enemies.transform
                            ),
                            holdingItem.itemRef.projectile
                        );
                    }

                    if (particleEmitter != null)
                    {
                        ParticleSystem particle = particleEmitter.GetComponent<ParticleSystem>();
                        particle.Emit(holdingItem.itemRef.emit);
                    }

                    holdingItem.amount -= 1;
                    holdingItem.ammo = (holdingItem.ammo > 0) ? holdingItem.ammo - 1 : 0;
                    player.ConsumeMana(holdingItem.itemRef.shooting_manaConsume);

                    if (holdingItem.ammo == 0)
                    {
                        atkCooldown = holdingItem.itemRef.reload;
                        holdingItem.reloading = true;
                    }
                }
                else
                {
                    player.ConsumeMana(holdingItem.itemRef.mele_manaConsume);
                }

                if (holdingItem.itemRef.canMelee)
                {
                    player.TriggerRecoil(holdingItem.itemRef.recoil);
                    player.TriggerSwing(holdingItem.itemRef.swing);

                    Vector3 CurrentPos = pos;

                    if (weaponMuzzle != null) CurrentPos = weaponMuzzle.transform.position;

                    float damageDealt = holdingItem.itemRef.damage;

                    if (holdingItem.attachments != null && holdingItem.attachments.Count > 0)
                    {
                        foreach (string modify in holdingItem.attachments)
                        {
                            switch (modify)
                            {
                                case "Damage":
                                    damageDealt += holdingItem.itemRef.damage * 0.25f;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    GameManager.SummonAttackArea(
                         CurrentPos,
                         Quaternion.Euler(0, 0, look),
                         new AreaInstance(
                             damageDealt,
                             holdingItem.itemRef.knockBack,
                             holdingItem.itemRef.knockBack_Duration,
                             holdingItem.itemRef.effects,
                             holdingItem.itemRef.hitbox, 
                             Game.g_enemies.transform,
                             holdingItem.itemRef.hitboxData
                             )
                        );
                }

                atkCooldown += holdingItem.itemRef.cooldown;
            }

            if (atkCooldown <= 0f && holdingItem.reloading)
            {
                if (player.ConsumeMana(holdingItem.itemRef.reload_manaConsume))
                {
                    holdingItem.reloading = false;
                    holdingItem.ammo = holdingItem.itemRef.clipSize;
                }
            }

            if (holdingItem.amount == 0)
            {
                int index = GetItemIndexFromID(holdingItem.itemID);
                view.RPC("RPC_UpdateItem", RpcTarget.Others, holdingItem.itemID, (new ItemInstanceSender(Items[index])).ToJson());
                Items.RemoveAt(index);
            }
        }
    }

    public void Attack()
    {
        if (holdingItem != null && holdingItem.itemRef)
        {
            Vector2 fireDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
            float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

            if (atkCooldown <= 0f && (holdingItem.itemRef.canShoot || holdingItem.itemRef.canMelee) && holdingItem.amount>0)
            {
                Vector3 CurrentPos = transform.position;
                if (weaponMuzzle != null)
                {
                    CurrentPos = weaponMuzzle.transform.position;
                }

                view.RPC("RPC_Attack", RpcTarget.All, CurrentPos, angle);
            }
        }
    }

    [PunRPC]
    private void RPC_Reload(bool toggle)
    {
        if (holdingItem != null && holdingItem.itemRef)
        {
            if (holdingItem.itemRef.canShoot && holdingItem.amount>0 && !holdingItem.reloading)
            {
                holdingItem.ammo = 0;
                atkCooldown = holdingItem.itemRef.reload;
                holdingItem.reloading = true;
            }
        }
    }

    [PunRPC]
    private void RPC_CheckReload(bool toggle)
    {
        if (holdingItem != null && holdingItem.itemRef)
        {
            if (atkCooldown <= 0f && holdingItem.reloading)
            {
                if (player.ConsumeMana(holdingItem.itemRef.reload_manaConsume))
                {
                    holdingItem.reloading = false;
                    holdingItem.ammo = holdingItem.itemRef.clipSize;
                }
            }
        }
    }

    public void CheckReload()
    {
        if (holdingItem != null && holdingItem.itemRef)
        {
            if (atkCooldown <= 0f && holdingItem.reloading)
            {
                if (player.CurrentMana >= holdingItem.itemRef.reload_manaConsume)
                {
                    if (holdingItem.itemRef.canShoot && holdingItem.amount > 0 && holdingItem.ammo < holdingItem.itemRef.clipSize)
                    {
                        view.RPC("RPC_CheckReload", RpcTarget.All, true);
                    }
                }
            }
        }
    }

    public void Reload()
    {
        if (holdingItem != null && holdingItem.itemRef)
        {
            if (holdingItem.itemRef.canShoot && holdingItem.amount>0 && holdingItem.ammo<holdingItem.itemRef.clipSize)
            {
                view.RPC("RPC_Reload", RpcTarget.All, true);
            }
        }
    }
}
