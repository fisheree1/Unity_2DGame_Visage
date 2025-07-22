using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    [SerializeField]
    private int damage = 1;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player entered trap");
            // Assuming the player has a method to take damage
            HeroLife heroLife = collision.gameObject.GetComponent<HeroLife>();
            if (heroLife != null && !heroLife.IsInvulnerable)
            {
                heroLife.TakeDamage(damage); // Adjust damage value as needed
            }
        }
    }
}
