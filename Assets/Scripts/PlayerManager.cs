using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    //Reference to audio clips and text showing ammo and zombies hit
    public AudioClip shootAudioClip;
    public AudioClip walkAudioClip;
    public AudioClip noAmmoAudioClip;
    public Text currentAmmoText;
    public Text zombiesHitText;

    //Reference to ball prefab and spawn location of the ball
    public GameObject m_ballPrefab;
    public GameObject m_spawnLocation;

    int m_maxAmmo = 5; //max ammo the player can hold
    int m_currentAmmo = 5; //current ammo


    float m_maxCharge = 3.5f; //how long the charge can last
    float m_currentCharge = 1.0f; //current charge
    bool m_charging = false; //is the player charging the gun
    bool m_reloading = false; //is the player reloading the gun

    public bool m_canShoot = true; //can the player shoot
    public bool m_insideSafeZone = false; //iss the player inside the "safe zone"
    public int m_zombiesHit = 0; //how many zombies hit

    //List containing all the balls in range around the player
    //used to track which balls to suck in when reloading
    List<GameObject> m_ballsInRange = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {    
    }

    // Update is called once per frame
    void Update()
    {
        //If the player can shoot
        if (m_canShoot)
        {
            Shoot();
            RestoreAmmo();
            Walk();
        }
        //Update the text
        currentAmmoText.text = "Current Ammo: " + m_currentAmmo.ToString();
        zombiesHitText.text = m_zombiesHit.ToString() + " Zombies Hit";
    }
    
    void Walk()
    {
        //If the player pressed any of the movement buttons
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            //check if the audio source is playing
            if (!GetComponent<AudioSource>().isPlaying)
            {
                //If not, load the audio clip and play
                GetComponent<AudioSource>().clip = walkAudioClip;
                GetComponent<AudioSource>().Play();
            }
        }
    }
    void Shoot()
    {
        //Check if the player pressed shoot and if there is enough ammo
        if (Input.GetKey(KeyCode.Mouse0) && (m_currentAmmo > 0))
        {
            //Increment the charge variables
            m_currentCharge += Time.deltaTime;
            //Max out the charge variable at max charge
            if (m_currentCharge >= m_maxCharge) m_currentCharge = m_maxCharge;
            //set charging to true
            m_charging = true;

        }
        //If the player lets go of the shoot button and was charging
        else if (Input.GetKeyUp(KeyCode.Mouse0) && m_charging)
        {
            //Instantiate a ball prefab at the spawn location
            GameObject temp = Instantiate(m_ballPrefab, m_spawnLocation.transform.position, m_spawnLocation.transform.rotation);
            //Set its forward velocity by 25 * current charge
            temp.GetComponent<Rigidbody>().velocity = transform.TransformDirection(Vector3.forward * m_currentCharge * 25);

            //Call the modify amplitude function
            //The longer the charge, the louder it is
            //Set it to the audio source and play
      
            temp.GetComponent<AudioSource>().clip = GetComponent<AudioEffects>().ModifyAmplitude(m_currentCharge, shootAudioClip);
            temp.GetComponent<AudioSource>().Play();


            //Reset the variables to default and decrement ammo
            m_currentCharge = 1.0f;
            m_charging = false;
            m_currentAmmo--;
        }
        //If the player pressed shoot and there is not enough ammo
        else if (Input.GetKey(KeyCode.Mouse0) && m_currentAmmo <= 0)
        {
            //Check if the audio source is playing
            if (!GetComponent<AudioSource>().isPlaying)
            {
                //If it isnt
                //Pass the audio clip into te loop and fade functions
                //Attach it to the audio source and play
                AudioClip temporaryAudioClip = GetComponent<AudioEffects>().Loop(noAmmoAudioClip, 3);
                GetComponent<AudioSource>().clip = GetComponent<AudioEffects>().Fade(temporaryAudioClip, 0.5f);
                GetComponent<AudioSource>().Play();
            }
        }
    }

    void RestoreAmmo()
    {
        //If reload is pressed, and the player is already not reloading
        if (Input.GetKeyDown(KeyCode.R) && (m_currentAmmo + 1 <= m_maxAmmo) && !m_reloading)
        {
            //set reloading to true
            m_reloading = true;
        }

        //if the player is reloading
        if (m_reloading)
        {
            //for every ball in range
            for (int i = 0; i < m_ballsInRange.Count; i++)
            {  
                //reset their velocity
                m_ballsInRange[i].GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                //Update their position by using the move towards function, with the speed of 4
                m_ballsInRange[i].transform.position = Vector3.MoveTowards(m_ballsInRange[i].transform.position, transform.position, 4.0f * Time.deltaTime);
            }
            //Once there are no balls in range, the player is no longer reloading
            if (m_ballsInRange.Count <= 0) m_reloading = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //If a ball is in range
        if (other.gameObject.name == "Ball(Clone)")
        {
            //if the ball is not in the list
            if (!m_ballsInRange.Contains(other.gameObject))
            {
                //add it to the list
                m_ballsInRange.Add(other.gameObject);
            }
            
        }
        //if the player entered the safe zone
        if (other.gameObject.tag == "SafeZone")
        {
            //update the safe zone variable
            m_insideSafeZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //If a ball left the range
        if (other.gameObject.name == "Ball(Clone)")
        {
            //if the ball is in the list
            if (m_ballsInRange.Contains(other.gameObject))
            {
                //remove it to the list
                m_ballsInRange.Remove(other.gameObject);
            }
         
        }
        //if the player left the safe zone
        if (other.gameObject.tag == "SafeZone")
        {
            //update the safe zone variable
            m_insideSafeZone = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //If the player is reloading
        if (m_reloading)
        {
            //If the player comes in contact with a ball
            if (collision.collider.gameObject.name == "Ball(Clone)")
            {
                //and the ball is in the list
                if (m_ballsInRange.Contains(collision.collider.gameObject))
                {
                    //start the coroutine for playing reload sound
                    StartCoroutine(PlaySoundAndRemove(collision.collider.gameObject));
                 
                }
            }
        }
      
    }
    IEnumerator PlaySoundAndRemove(GameObject go1)
    {
        //remove the ball from the list
        m_ballsInRange.Remove(go1);
        //replace its audio clip with the reversed version and then play it
        go1.GetComponent<AudioSource>().clip = GetComponent<AudioEffects>().ReverseAudio(shootAudioClip);
        go1.gameObject.GetComponent<AudioSource>().Play();
     
        //wait for the audio to stop playing
        yield return new WaitUntil(() => !go1.GetComponent<AudioSource>().isPlaying);
        //destroy the object
        Destroy(go1);

        //increment audio
        m_currentAmmo++;
        //Max out the ammo at max ammo
        if (m_currentAmmo > m_maxAmmo) m_currentAmmo = m_maxAmmo;
    }
}
