using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroHP : MonoBehaviour
{

    public static int HPCount;

    public GameObject[] HPs;

    public HeroLife heroLife;//HeroLife
    public int AddHPCount;
    // Start is called before the first frame update
    void Start()
    {
        HPCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (HPCount == 0)
        {
            HPs[0].SetActive(false); 
            HPs[1].SetActive(false);
            HPs[2].SetActive(false);
        }
        else if (HPCount == 1)
        {
            HPs[0].SetActive(true); 
            HPs[1].SetActive(false);
            HPs[2].SetActive(false);
        }
        else if (HPCount == 2)
        {
            HPs[0].SetActive(true);
            HPs[1].SetActive(true);
            HPs[2].SetActive(false);
        }
        else if (HPCount == 3)
        {
            HPs[0].SetActive(true);
            HPs[1].SetActive(true);
            HPs[2].SetActive(true);
        }




        if (Input.GetKeyDown(KeyCode.R))
        {
            if (HPCount <= 3 &&HPCount>0 && heroLife.currentHealth < heroLife.MaxHealth)
            {
                HPCount--;
                heroLife.currentHealth += AddHPCount;
                if (heroLife.currentHealth >= heroLife.MaxHealth)
                {
                    heroLife.currentHealth = heroLife.MaxHealth;
                }
            }
        }
    }
}
