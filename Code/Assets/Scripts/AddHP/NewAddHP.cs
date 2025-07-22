using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewAddHP : MonoBehaviour
{
    GameObject Player;
    public GameObject AddHPUI;
    public HeroLife heroLife;

    public int AddMaxHPCount;
    public GameObject BianKuang;
    public GameObject XueTiao;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.Find("Hero");
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(Player.transform.position, gameObject.transform.position) < 2)
        {
            AddHPUI.SetActive(true);
                
                if (heroLife.currentHealth >= heroLife.maxHealth - 1)
                {
                    Debug.Log(1);
                    heroLife.maxHealth += AddMaxHPCount;
                    heroLife.currentHealth = heroLife.maxHealth;
                }
                else
                {
                    heroLife.maxHealth += AddMaxHPCount;
                }
                Destroy(gameObject);
                AddHPUI.SetActive(false);
                float X = BianKuang.transform.localScale.x;
                BianKuang.transform.localScale = new Vector3(X += 0.1f, BianKuang.transform.localScale.y, BianKuang.transform.localScale.z);
                float X1 = XueTiao.transform.localScale.x;
                XueTiao.transform.localScale = new Vector3(X1 += 0.1f, XueTiao.transform.localScale.y, XueTiao.transform.localScale.z);
        }
        else
        {
            AddHPUI.SetActive(false);
        }
    }
}
