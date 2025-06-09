using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviourPunCallbacks
{
    public void BackToMenu()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Start");
    }
}
