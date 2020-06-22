using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class ZombieManager : MonoBehaviour
{
    //to track the growl timer
    float m_growlTimer = 0.0f;
    float m_growlMaxTimer = 10.0f; //when the zombie should growl
    bool m_attacked = false;
    //referencess to the player and destination objects
    GameObject m_playerObject;
    public GameObject m_Destination1;
    public GameObject m_Destination2;
    public GameObject m_Destination3;
    // Start is called before the first frame update
    void Start()
    {
        //init it with random value so not all zombies growl at the same time
        m_growlTimer = Random.Range(0.0f, 9.9f);
        //init a random destination
        int temp = Random.Range(1, 3);
        if (temp == 1) GetComponent<NavMeshAgent>().destination = m_Destination1.transform.position;
        else if (temp == 2) GetComponent<NavMeshAgent>().destination = m_Destination2.transform.position;
        else if (temp == 3) GetComponent<NavMeshAgent>().destination = m_Destination3.transform.position;
        //reference the player
        m_playerObject = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        //increment the growl timer
        m_growlTimer += Time.deltaTime;   
        if (m_growlTimer >= m_growlMaxTimer) //check if the zombie should growl
        {
          //Call the WWise event on that zombie
          AkSoundEngine.PostEvent("ZombieSound_Play", this.gameObject);
          //reset the growl
          m_growlTimer = Random.Range(0.0f, 4.0f);
        }
        //if the player is inside the safe zone, set attacked to false
        if (m_playerObject.GetComponent<PlayerManager>().m_insideSafeZone) m_attacked = false;
        Move();
    }

    void OnCollisionEnter(Collision collision)
    {
        //if hit with the ball
        if (collision.collider.gameObject.tag == "Ball")
        {
            //Stop playing the growl sound, and call the hurt sound event
            AkSoundEngine.PostEvent("ZombieSound_Stop", this.gameObject);
            AkSoundEngine.PostEvent("ZombieHurtSound_Play", this.gameObject);
            //set attacked to true
            m_attacked = true;
            //reset the growl timer and increment zombies hit
            m_growlTimer = 0.0f;
            m_playerObject.GetComponent<PlayerManager>().m_zombiesHit++;
        }
    }

    void Move()
    {
        //If the player isnt insside the safe zone and the zombie is attacked
        if (!m_playerObject.GetComponent<PlayerManager>().m_insideSafeZone && m_attacked)
        {
            //Set the nav mesh agent to follow the player
            GetComponent<NavMeshAgent>().SetDestination(m_playerObject.transform.position);
        }
        else
        {
            //Keep setting the 3 destinations to the zombie once it reaches its previouss one
            if (GetComponent<NavMeshAgent>().remainingDistance < 2)
            {
                int temp = Random.Range(1, 3);
                if (temp == 1) GetComponent<NavMeshAgent>().destination = m_Destination1.transform.position;
                else if (temp == 2) GetComponent<NavMeshAgent>().destination = m_Destination2.transform.position;
                else if (temp == 3) GetComponent<NavMeshAgent>().destination = m_Destination3.transform.position;
            }
        }
    }
}
