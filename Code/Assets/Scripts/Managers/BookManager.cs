using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookManager : MonoBehaviour
{
    public GameObject BookTip;
    public GameObject BookUI;
    public GameObject Player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(Player.transform.position, gameObject.transform.position) < 2)
        {
            BookTip.SetActive(true);
            if (Input.GetKeyDown(KeyCode.E))
            {
                BookUI.SetActive(true);
            }
        }
        else
        {
            BookTip.SetActive(false);
        }
    }
}
