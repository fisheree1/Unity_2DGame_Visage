using UnityEngine;
using UnityEngine.UI;

public class StoneInteraction : MonoBehaviour
{
    public GameObject textPopup; // 提示文本框UI
    public string message = "A false face"; // 显示的文本内容
    public Transform popupPosition; // 文本框显示位置（石碑上方）

    private bool playerInRange = false;

    void Start()
    {
        if (textPopup != null)
            textPopup.SetActive(false);
    }

    void Update()
    {
        // 只有在范围内才响应E键
        if (playerInRange)
        {
            ShowTextPopup();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // 也可以在这里显示“按E查看信息”的小提示
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideTextPopup();
        }
    }

    void ShowTextPopup()
    {
        if (textPopup == null) return;

        // 设置文本内容
        Text textComponent = textPopup.GetComponentInChildren<Text>();
        if (textComponent != null)
            textComponent.text = message;

        // 设置文本框位置
        if (popupPosition != null)
            textPopup.transform.position = popupPosition.position;

        textPopup.SetActive(true);

        // 2秒后自动隐藏
        Invoke(nameof(HideTextPopup), 2f);
    }

    void HideTextPopup()
    {
        if (textPopup != null)
            textPopup.SetActive(false);
    }
}