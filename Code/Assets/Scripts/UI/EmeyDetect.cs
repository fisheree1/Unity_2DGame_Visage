using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmeyDetect : MonoBehaviour
{
    [Header("����Ի����")]
    public Image ChatPanel;
    [Header("����Ի������ʾʱ��")]
    public float ChatPanelShowTime;
    [Header("����Ի����Y��λ��")]
    public float DistanceY;
    [Header("����Ի�����")]
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
