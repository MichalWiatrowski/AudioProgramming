using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleInteract : MonoBehaviour
{
    //Reference to the piano cube and UI
    public GameObject pianoObject;
    public GameObject pianoHUD;
    SimpleMovement simpleMovementReference;
    // Start is called before the first frame update
    void Start()
    {
       //Set the reference
       simpleMovementReference = GetComponent<SimpleMovement>();
        //Disable the UI by default
       pianoHUD.SetActive(false);
        //Lock the mouse to middle of the screen
       Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
       //If the player pressed interact
        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            //Run a raycast from the mouse pointer (middle of the screen)
            if (Physics.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                //if it the piano object
                if (hit.transform.gameObject == pianoObject)
                {
                    //free the cursor
                    Cursor.lockState = CursorLockMode.None;
                    //enable the UI
                    pianoHUD.SetActive(true);
                    //disable the movement and shooting
                    simpleMovementReference.enabled = false;
                    GetComponent<PlayerManager>().m_canShoot = false;
                }     
            }
        }

        //if the player pressed escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //re lock the cursor
            //disable the UI
            //unlock movement and shooting
            Cursor.lockState = CursorLockMode.Locked;
            pianoHUD.SetActive(false);
            simpleMovementReference.enabled = true;
            GetComponent<PlayerManager>().m_canShoot = true;
        }
    }
}
