using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmeyDetect : MonoBehaviour
{
    [Header("怪物对话面板")]
    public Image ChatPanel;
    [Header("怪物对话面板显示时间")]
    public float ChatPanelShowTime;
    [Header("怪物对话面板Y轴位移")]
    public float DistanceY;
    [Header("怪物对话内容")]
    public string ChatContent;
    public bool _isTrigger;
    public bool _isShowChat;
    void Start()
    {
        
    }

    void Update()
    {
        DetectState();
        ChatPanel.transform.position = new Vector2(transform.position.x,transform.position.y + DistanceY);
        ChatPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = ChatContent;
    }
    public void DetectState()
    {
        if(!_isTrigger && !_isShowChat)
        {
            Invoke("ShowChat", ChatPanelShowTime);
        }
        if(_isTrigger)
        {
            CloseChat();
        }
    }
    public void ShowChat()
    {
        ChatPanel.gameObject.SetActive(true);
        _isShowChat = true;
    }
    public void CloseChat()
    {
        ChatPanel.gameObject.SetActive(false);
        _isShowChat = false;
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
         if(other.tag == "Player")
         {
            _isTrigger = true;
         }
    }
    public void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            _isTrigger = true;
        }
    }
    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            _isTrigger = false;
        }
    }
}
