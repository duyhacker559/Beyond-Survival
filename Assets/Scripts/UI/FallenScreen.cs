using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FallenScreen : MonoBehaviour
{

    public Transform currentCamera;
    public Transform Spectate;

    public Transform displayName;

    private bool canSpectate = false;

    private int playerIndex = 0;

    private void Awake()
    {
        
    }

    public void Next()
    {
        playerIndex++;
        if (playerIndex >= Game.g_players.transform.childCount) playerIndex = 0;
        Spectate = Game.g_players.transform.GetChild(playerIndex);
    }

    public void Back()
    {
        playerIndex--;
        if (playerIndex < 0 ) playerIndex = Game.g_players.transform.childCount - 1;
        Spectate = Game.g_players.transform.GetChild(playerIndex);
    }

    private void Update()
    {
        try
        {
            if (canSpectate)
            {
                if (Spectate != null)
                {
                    Vector3 targetCamPos = new Vector3(Spectate.position.x, Spectate.position.y, -10);
                    currentCamera.transform.position = Vector3.Lerp(currentCamera.transform.position, targetCamPos, Time.deltaTime * 3f);

                    if (PhotonNetwork.LocalPlayer.NickName == Spectate.name)
                    {
                        displayName.GetComponent<TextMeshProUGUI>().SetText("Yourself");
                    }
                    else
                    {
                        displayName.GetComponent<TextMeshProUGUI>().SetText(Spectate.name);
                    }
                }
                else
                {
                    Spectate = Game.g_players.transform.GetChild(0);
                    playerIndex = 0;
                }
            }
        } catch
        {

        }
    }

    private void OnEnable()
    {
        Spectate = Game.g_players.transform.GetChild(0);
        playerIndex = 0;
        canSpectate = true;
    }

    private void OnDisable()
    {
        canSpectate = false;
        Spectate = null;
    }
}
