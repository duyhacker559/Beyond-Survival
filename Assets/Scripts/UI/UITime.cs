using TMPro;
using UnityEngine;

public class UITime : MonoBehaviour
{
    private void Update()
    {
		try
		{
            string timeStr = DayNightCycle2D.hour.ToString("00") + ":" + DayNightCycle2D.minute.ToString("00");
			transform.GetComponent<TextMeshProUGUI>().SetText($"{timeStr} - Day {DayNightCycle2D.currentDay}");
		}
		catch (System.Exception)
		{
			throw;
		}
    }
}
