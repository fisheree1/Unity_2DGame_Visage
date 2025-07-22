using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class BrideManage : MonoBehaviour
{
    public static BrideManage instance;
    [Header("������ʾ����")]
    public ShowType type;
    [Header("ʧ��������ʾ�ŵ�����")]
    public int FailShowCount;
    [Header("��Ҫ��ʾ������ͼƬ")]
    public List<GameObject> BrideList;
    [Header("�Զ��������ɹ���ʾ���루������")]
    public List<int> KeyList = new List<int>();
    [HideInInspector] public int TriggerIndex;
    [Header("�Ѽ�¼����")]
    public List<int> RecordList = new List<int>();
    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    void Update()
    {
        if(TriggerIndex >= 3)
        {
            JudgemengKey();
            ShowBride();
            TriggerIndex = 0;
        }
    }
    /// <summary>
    /// ��ʾ����
    /// </summary>
    public void ShowBride()
    {
        switch(type)
        {
            case ShowType.Fail:
                for(int i = 0; i < FailShowCount; i++)
                {
                    int num = Random.Range(0, BrideList.Count);
                    BrideList[num].SetActive(true);
                    BrideList.RemoveAt(num);
                }
                break;
            case ShowType.Success:
                for (int j = 0; j < BrideList.Count; j++)
                {
                    BrideList[j].SetActive(true);
                }
                break;
        }
    }
    public void JudgemengKey()
    {
        for(int i = 0;i < RecordList.Count;i++)
        {
            if (RecordList[i] != KeyList[i])
            {
                type = ShowType.Fail;
                return;
            }
        }
        type = ShowType.Success;
    }
}
public enum ShowType
{
    Fail,
    Success
}
