using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HP_Prop : MonoBehaviour
{

    GameObject Player;
    public GameObject HPUI;
    private HeroLife heroLife; // 添加HeroLife引用
    
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.Find("Hero");
        // 获取HeroLife组件
        if (Player != null)
        {
            heroLife = Player.GetComponent<HeroLife>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(Player.transform.position, gameObject.transform.position) < 2) 
        {
            HPUI.SetActive(true);
            if (HeroHP.HPCount < 3)
            {
                HPUI.SetActive(false);
                
                // 增加血瓶数量
                HeroHP.HPCount++;
                
                // 通知HeroLife记录额外收集的血瓶
                if (heroLife != null)
                {
                    heroLife.OnHPCollected();
                }
                
                Destroy(gameObject);
            }
        }
        else
        {
            HPUI.SetActive(false);
        }
    }
}
