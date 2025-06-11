
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class PlayerEntryUI : MonoBehaviourPunCallbacks
{
    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
    public Image playerAvatar;
    public GameObject leftArrow;
    public GameObject rightArrow;

    public Transform classText;

    public static PlayerClass[] playerClass;

    private PhotonView view;

    void Awake()
    {
       view = GetComponent<PhotonView>();
       playerClass = Resources.LoadAll<PlayerClass>("Add/PlayerClass");
    }

    Photon.Realtime.Player player;
    public void SetPlayerInfo(Photon.Realtime.Player _player)
    {
        player = _player;
        UpdatePlayerItem(player);
    }

    private void ChangeAvatar(int direction)
    {
        if (playerClass.Length == 0)
        {
            Debug.LogError("Mảng avatars không có phần tử.");
            return;
        }

        int currentAvatar = playerProperties.ContainsKey("playerAvatar") ? (int)playerProperties["playerAvatar"] : 0;

        currentAvatar = (currentAvatar + direction + playerClass.Length) % playerClass.Length;

        playerProperties["playerAvatar"] = currentAvatar;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);

        Debug.Log($"Avatar đã thay đổi sang {currentAvatar}.");
        playerAvatar.sprite = playerClass[currentAvatar].icon;

        transform.Find("PlayerClass").GetComponent<TextMeshProUGUI>().SetText(playerClass[currentAvatar].name);

        Debug.Log($"Cập nhật avatar: {playerAvatar.sprite.name}");
    }
    public void OnclickLeftArrow()
    {
        if (PhotonNetwork.LocalPlayer.NickName == gameObject.name)
        {
            Debug.Log("Nhấn nút mũi tên trái");
            ChangeAvatar(-1);
        }
    }
    public void OnClickRightArrow()
    {
        if (PhotonNetwork.LocalPlayer.NickName == gameObject.name)
        {
            Debug.Log("Nhấn nút mũi tên phải");
            ChangeAvatar(1);
        }
    }

    // Cập nhật thông tin người chơi khi có sự thay đổi thuộc tính
    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log("UPdate");
        if (player == targetPlayer)
        {
            UpdatePlayerItem(targetPlayer);
        }
    }

    // Cập nhật avatar người chơi
    private void UpdatePlayerItem(Photon.Realtime.Player targetPlayer)
    {
        if (targetPlayer.CustomProperties.ContainsKey("playerAvatar"))
        {
            int avatarIndex = (int)targetPlayer.CustomProperties["playerAvatar"];
            playerAvatar.sprite = playerClass[avatarIndex].icon;
            transform.Find("PlayerClass").GetComponent<TextMeshProUGUI>().SetText(playerClass[avatarIndex].name);
            Debug.Log($"Cập nhật avatar từ Photon: {playerAvatar.sprite.name}");
            playerProperties["playerAvatar"] = avatarIndex;
        }
        else
        {
            playerProperties["playerAvatar"] = 0;
        }
    }
}