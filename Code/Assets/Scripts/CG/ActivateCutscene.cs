using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ActivateCutscene : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Start the cutscene
            playableDirector.Play();
            // Optionally, disable the collider to prevent retriggering
            GetComponent<Collider2D>().enabled = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
