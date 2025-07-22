using UnityEngine;

public class BookProKeyboardInput : MonoBehaviour
{
    public BookPro bookPro; // 拖拽赋值或自动获取
    public float flipDuration = 0.5f;
    
    void Start()
    {
        if (bookPro == null)
        {
            bookPro = GetComponent<BookPro>();
        }
    }

    void Update()
    {
        if (bookPro == null) return;
        if (!bookPro.interactable) return;
        // 防止动画中重复翻页
        var flipper = bookPro.GetComponent<PageFlipper>();
        if (flipper != null && flipper.enabled) return;

        // 右箭头：向右翻页
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (bookPro.CurrentPaper <= bookPro.EndFlippingPaper)
            {
                PageFlipper.FlipPage(bookPro, flipDuration, FlipMode.RightToLeft, null);
            }
        }
        // 左箭头：向左翻页
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (bookPro.CurrentPaper > bookPro.StartFlippingPaper)
            {
                PageFlipper.FlipPage(bookPro, flipDuration, FlipMode.LeftToRight, null);
            }
        }
    }
} 