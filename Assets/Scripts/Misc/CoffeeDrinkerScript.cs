using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeDrinkerScript : MonoBehaviour
{

    private AudioSource audio;
    private bool canDrink;
    
    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canDrink == true)
        {
            DrinkCoffee();
        }
    }

    public void CanDrink()
    {
        canDrink = true;
    }

    public void CanNotDrink()
    {
        canDrink = false; 
    }
    public void DrinkCoffee()
    {
        audio.Play();
    }
}
