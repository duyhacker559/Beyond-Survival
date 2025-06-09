using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyNetworkManager: MonoBehaviourPunCallbacks
{
    [SerializeField] public TMP_InputField playerNameInput;
    [SerializeField] public TMP_InputField roomNameInput;
    [SerializeField] public RoomEntryUI _roomEntryUIPrefab;
    [SerializeField] public Transform _roomListParent;
    [SerializeField] public RoomEntryUI _playerEntryUIPrefab;
    [SerializeField] public Transform _playerListParent;
    [SerializeField] public TMP_Text _currentLocationText;
    [SerializeField] public GameObject _roomListWindow;
    [SerializeField] public GameObject _playerListWindow;
    [SerializeField] public GameObject _createRoomWindow;
    [SerializeField] public GameObject LobbyPanel;
    [SerializeField] public GameObject RoomPanel;

    [SerializeField] public TMP_Text statusText;
    [SerializeField] public Button _leaveRoomBtn;
    [SerializeField] public Button _startGameBtn;

    private List<RoomEntryUI> _roomList = new List<RoomEntryUI>();
    private List<RoomEntryUI> _playerList = new List<RoomEntryUI>();

    private void Initialize()
    {
        _leaveRoomBtn.interactable = false;
        _startGameBtn.interactable = false;
        ShowWindow(true);
    }
    private void Start()
    {
        
        Initialize();
        Connect();
    }
    
    #region PhotonCallbacks

    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected to master server";
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateRoomList(roomList);
    }
    public override void OnJoinedLobby()
    {
        _currentLocationText.text = "Rooms";
        statusText.text = "Lobby";
        Debug.Log("Joined Lobby");
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (statusText == null) return;
        Debug.Log("Disconnected");
    }

    [PunRPC]
    public void SetClass()
    {

    }

    public override void OnJoinedRoom()
    {
        statusText.text = "Joined " + PhotonNetwork.CurrentRoom.Name;
        _currentLocationText.text = PhotonNetwork.CurrentRoom.Name;
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        _leaveRoomBtn.interactable = true;

        if (PhotonNetwork.IsMasterClient)
        {
            _startGameBtn.interactable = true;
        }

        ShowWindow(false);
        UpdatePlayerList();
 
    }


    public override void OnLeftRoom()
    {
        if (statusText != null)
        {
            statusText.text = "Lobby";
        }
        if(_currentLocationText != null)
        {
            _currentLocationText.text = "Rooms";
            
        }
        Debug.Log("Left room");
        _leaveRoomBtn.interactable = false;
        _startGameBtn.interactable = false;
        ShowWindow(true);
        UpdatePlayerList();

    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdatePlayerList();

    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdatePlayerList();

    }
    #endregion

    private void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void UpdateRoomList(List<RoomInfo> list) 
    {
        //clear current list of rooms
        for(int i = 0; i < _roomList.Count; i++)
        {
            Destroy(_roomList[i].gameObject);
        }
        _roomList.Clear();
        //Generate a new list with the updated info
        for(int i = 0; i < list.Count; i++)
        {
            if (list[i].PlayerCount == 0)
            {
                continue;
            }
            RoomEntryUI newRoomEntry = Instantiate(_roomEntryUIPrefab, _roomListParent);
            newRoomEntry.LobbyNetworkParent = this;
            newRoomEntry.setName(list[i].Name);
            newRoomEntry.SetPlayerCount(list[i].PlayerCount, list[i].MaxPlayers);

            _roomList.Add(newRoomEntry);

        }
    }

    private void UpdatePlayerList()
    {
        // Clear the current player list
        for (int i = 0; i < _playerList.Count; i++)
        {
            Destroy(_playerList[i].gameObject);
        }
        _playerList.Clear();

        if (PhotonNetwork.CurrentRoom == null) return;
         // Generate a new list of player
        foreach (KeyValuePair<int,Photon.Realtime.Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            RoomEntryUI newPlayerEntry = Instantiate(_playerEntryUIPrefab);
            newPlayerEntry.transform.SetParent(_playerListParent,false);
            newPlayerEntry.transform.SetAsFirstSibling();
            newPlayerEntry.setName(player.Value.NickName);
            newPlayerEntry.gameObject.name = player.Value.NickName;
            _playerList.Add(newPlayerEntry);
            newPlayerEntry.transform.GetComponent<PlayerEntryUI>().SetPlayerInfo(player.Value);
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _startGameBtn.interactable = true;
        }
    }

    private void ShowWindow(bool isRoomList)
    {
        LobbyPanel.SetActive(isRoomList);
        RoomPanel.SetActive(!isRoomList);
    }


    public void JoinRoom(string roomName)
    {
        PhotonNetwork.NickName = string.IsNullOrWhiteSpace(playerNameInput.text)
            ? "Player " + Random.Range(0, 5000)
            : playerNameInput.text;
        PhotonNetwork.JoinRoom(roomName);

    }

    public void CreateRoom()
    {
        if(string.IsNullOrEmpty(roomNameInput.text) == false)
        {
            PhotonNetwork.NickName = string.IsNullOrWhiteSpace(playerNameInput.text)
            ? "Player " + Random.Range(0, 5000)
            : playerNameInput.text;
            PhotonNetwork.CreateRoom("Room " + roomNameInput.text, new RoomOptions() { MaxPlayers = 4, BroadcastPropsChangeToAll = true });
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnStartGamePressed()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel("Game");
    }

    public void OnPressBackMenu()
    {
        SceneManager.LoadScene("Start");
    }
}
