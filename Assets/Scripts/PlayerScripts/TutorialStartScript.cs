using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStartScript : MonoBehaviour
{
    public GameObject player;
    public GameObject camera;
    
    // Start is called before the first frame update
    void Start()
    {
        PlayerMovement.controlEnabled = false;
        PlayerAttack.controlEnabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnPlayer()
    {
        player.SetActive(true);
        camera.SetActive(false);
        PlayerMovement.controlEnabled = true;
        PlayerAttack.controlEnabled = true;
    }
}
