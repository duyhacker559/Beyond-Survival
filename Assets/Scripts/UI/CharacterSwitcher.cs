using UnityEngine;
using UnityEngine.UI;  // Để làm việc với UI

public class CharacterSwitcher : MonoBehaviour
{
    public Sprite[] characterSprites;  // Mảng chứa các sprite của các nhân vật
    public Image characterImage;       // Image UI để hiển thị nhân vật hiện tại

    private int currentCharacterIndex = 0;

    // Hàm chuyển sang nhân vật tiếp theo
    public void NextCharacter()
    {
        currentCharacterIndex++;
        if (currentCharacterIndex >= characterSprites.Length)
        {
            currentCharacterIndex = 0;  // Nếu hết các nhân vật, quay lại nhân vật đầu tiên
        }
        UpdateCharacter();
    }

    // Hàm chuyển về nhân vật trước
    public void PreviousCharacter()
    {
        currentCharacterIndex--;
        if (currentCharacterIndex < 0)
        {
            currentCharacterIndex = characterSprites.Length - 1;  // Nếu là nhân vật đầu tiên, chuyển sang cuối cùng
        }
        UpdateCharacter();
    }

    // Cập nhật sprite nhân vật
    private void UpdateCharacter()
    {
        characterImage.sprite = characterSprites[currentCharacterIndex];
    }
}
