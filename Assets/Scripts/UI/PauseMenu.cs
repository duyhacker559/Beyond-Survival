using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;  // Đừng quên import SceneManager

public class PauseMenu : MonoBehaviourPunCallbacks
{
    public GameObject pausePanel;        
    public GameObject confirmExitPanel;  
    public Button continueButton;        
    public Button exitButton;           
    public Button yesButton;            
    public Button noButton;             

    void Start()
    {
        // Ẩn các panel khi game bắt đầu
        pausePanel.SetActive(false);
        confirmExitPanel.SetActive(false);

        // Gắn sự kiện cho các nút
        continueButton.onClick.AddListener(ContinueGame);
        exitButton.onClick.AddListener(ShowConfirmExitPanel);
        noButton.onClick.AddListener(HideConfirmExitPanel);
    }

    void Update()
    {
        // Kiểm tra xem người chơi có nhấn ESC không
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Nếu panel đang ẩn thì hiển thị, ngược lại thì ẩn đi
            if (!pausePanel.activeInHierarchy)
            {
                pausePanel.SetActive(true);  // Hiển thị panel
            }
            else
            {
                pausePanel.SetActive(false); // Ẩn panel
            }
        }
    }

    void ContinueGame()
    {
        pausePanel.SetActive(false); 
    }

    // Hiển thị panel xác nhận thoát game
    void ShowConfirmExitPanel()
    {
        confirmExitPanel.SetActive(true);       
    }

    // Ẩn panel xác nhận thoát game
    void HideConfirmExitPanel()
    {
        confirmExitPanel.SetActive(false);  // Ẩn panel xác nhận thoát
        pausePanel.SetActive(true);         // Hiển thị lại panel pause
    }

    public void BackToMenu()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Start");
    }
}
