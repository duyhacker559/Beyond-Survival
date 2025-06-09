using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    public GameObject movementPanel;
    public GameObject minimapPanel;
    public GameObject notificationLayer;

    public Button nextButton;
    public Button gotItButton;

    void Start()
    {
        notificationLayer.SetActive(true);
        nextButton.onClick.AddListener(ShowMinimapPanel);
        gotItButton.onClick.AddListener(CloseTutorial);
    }

    public void ShowMinimapPanel()
    {
        movementPanel.SetActive(false);
        minimapPanel.SetActive(true);
    }

    public void CloseTutorial()
    {
        notificationLayer.SetActive(false);
    }

}
