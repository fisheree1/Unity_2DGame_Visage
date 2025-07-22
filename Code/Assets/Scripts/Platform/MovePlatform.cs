using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlatform : MonoBehaviour
{

    public float speed;
    public float waitTime;
    public Transform[] moveSpots;

    private int i;
    private Transform playerDefTransform;

    // Start is called before the first frame update
    void Start()
    {
        i = 1;
        playerDefTransform = GameObject.FindGameObjectWithTag("Player").transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, moveSpots[i].position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, moveSpots[i].position) < 0.1f)
        {
            if (waitTime < 0.0f)
            {
                if (i == 0)
                {
                    i = 1;
                }
                else if (i == 1)
                {
                    i = 0;
                }

                waitTime = 0.5f; // Reset wait time
            }
            else
            {
                waitTime -= Time.deltaTime; // Decrease wait time
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.transform.parent = gameObject.transform;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.transform.parent = playerDefTransform;
        }
    }

}
