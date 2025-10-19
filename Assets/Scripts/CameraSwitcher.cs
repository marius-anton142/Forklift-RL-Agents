using UnityEngine;
using UnityEngine.UI;

public class CameraSwitcher : MonoBehaviour
{
    public Camera[] cameras;
    public Button[] buttons;
    public Sprite[] buttonSprites;

    void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => SwitchCamera(index));
        }
    }

    void SwitchCamera(int index)
    {
        foreach (Camera cam in cameras)
        {
            cam.gameObject.SetActive(false);
        }

        cameras[index].gameObject.SetActive(true);
        SoundManager.instance.PlayCameraClickSound();
    }

    public void ChangeButtonSprite(int index)
    {
        buttons[10].GetComponent<Image>().sprite = buttonSprites[index];
    }
}
