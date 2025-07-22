using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Prop : MonoBehaviour
{
    [Header("绑定的提示组件")]
    public Image Key_Image;
    [Header("道具触发音频")]
    public AudioSource _Source;
    [Header("道具触发按键")]
    public KeyCode _KeyCode;
    [Header("道具触发钥匙")]
    public int _Key;
    private bool isTrigger;
    private bool hasTrigger;
    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(_KeyCode) && !hasTrigger)
        {
            Trigger();
            hasTrigger = true;
        }
    }
    public void Trigger()
    {
        _Source.Play();
        BrideManage.instance.RecordList.Add(_Key);
        BrideManage.instance.TriggerIndex += 1;
    }
    public void OnTriggerStay2D(Collider2D other)
    {
        if(other.tag == "Player" && !hasTrigger)
        {
            isTrigger = true;
            Key_Image.gameObject.SetActive(true);
            Key_Image.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = _KeyCode.ToString();
        }
    }
    public void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag == "Player")
        {
            isTrigger = false;
            Key_Image.gameObject.SetActive(false);
            Key_Image.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = _KeyCode.ToString();
        }
    }
}
