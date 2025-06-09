using UnityEngine;
using TMPro;

public class RoomEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;

    public LobbyNetworkManager LobbyNetworkParent;

    private string roomName;

    public void setName(string name)
    {
        roomName = name;
        roomNameText.text = name;
    }

    public void SetPlayerCount(int current, int max)
    {
        playerCountText.text = $"{current}/{max}";
    }

    public void OnJoinClicked()
    {
        LobbyNetworkParent.JoinRoom(roomName);
    }
}
