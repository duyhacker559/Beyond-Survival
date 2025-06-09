using UnityEngine;
using UnityEngine.UI;

public class NotificationController : MonoBehaviour
{
    public GameObject notificationEntryPlayer; // Tham chiếu đến NotificationEntryPlayer
    public Button startButton;                 // Tham chiếu đến nút Start
    public Button gotItButton;                 // Tham chiếu đến nút Got it!

    void Start()
    {
        // Gắn sự kiện cho nút Start và Got it!
        startButton.onClick.AddListener(HideNotificationPanel);
        gotItButton.onClick.AddListener(HideNotificationPanel);
    }

    // Hàm để ẩn NotificationEntryPlayer
    void HideNotificationPanel()
    {
        notificationEntryPlayer.SetActive(false);
    }
}
