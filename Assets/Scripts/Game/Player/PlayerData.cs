using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerRoomStats
{
    int id = 0;
    int user_class = 0;
}

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    public List<PlayerRoomStats> items = new List<PlayerRoomStats>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


}
