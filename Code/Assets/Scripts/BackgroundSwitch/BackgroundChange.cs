using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundChange : MonoBehaviour
{
    // Start is called before the first frame update

    private Animator anim;

    [SerializeField] private bool xChange = false;
    [SerializeField] private float xChangeAmount = 5f;
    [SerializeField] private bool yChange = false;
    [SerializeField] private float yChangeAmount = 5f;

    [SerializeField] private GameObject background1;
    [SerializeField] private GameObject background2;
    [SerializeField] private GameObject middleGround1;
    [SerializeField] private GameObject middleGround2;

    [SerializeField] private GameObject middleGround3;
    [SerializeField] private GameObject middleGround4;


    void Start()
    {
        anim = GetComponent<Animator>();
        Debug.Log(anim == null ? "Animator is null" : "Animator found");
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("BackgroundChange Triggered");
        if (other.CompareTag("Player"))
        {
            Debug.Log(anim == null ? "Animator is null" : "Animator found");
            Debug.Log("Player entered BackgroundChange trigger");
            anim.SetTrigger("trigger");
            if (xChange) other.transform.position = new Vector3(other.transform.position.x + xChangeAmount, other.transform.position.y, other.transform.position.z);
            if (yChange) other.transform.position = new Vector3(other.transform.position.x, other.transform.position.y + yChangeAmount, other.transform.position.z);

            if (background1 != null)
            {
                background1.SetActive(false);
            }

            if (background2 != null)
            {
                background2.SetActive(true);
            }

            if (middleGround1 != null)
            {
                middleGround1.SetActive(false);
            }
            
            if (middleGround2 != null)
            {
                middleGround2.SetActive(true);
            }

            if (middleGround3 != null)
            {
                middleGround3.SetActive(false);
            }

            if (middleGround4 != null)
            {
                middleGround4.SetActive(true);
            }

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            anim.SetTrigger("trigger");
        }
    }
}
