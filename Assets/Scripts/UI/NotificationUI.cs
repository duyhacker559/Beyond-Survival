using UnityEngine;
//using Photon.Pun;
using TMPro;

public class NotificationUI : MonoBehaviour
{
    public TMP_Text notificationText;
    private float displayTime = 3f;
    private float timer;

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0) notificationText.text = "";
        }
    }

    public void ShowNotification(string message)
    {
        notificationText.text = message;
        timer = displayTime;
    }

   /*
    * public override void OnJoinedRoom()
    {
        ShowNotification("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
    }
   */
}