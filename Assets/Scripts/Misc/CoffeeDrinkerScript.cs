using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;

public class CoffeeDrinkerScript : MonoBehaviour
{

    private AudioSource audioSource;
    private bool canDrink;
    [SerializeField] 
    private bool consumable;

    private SpriteRenderer sr;

    private CircleCollider2D collider;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        sr = GetComponent<SpriteRenderer>();
        collider = GetComponent<CircleCollider2D>();
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
        audioSource.Play();
        if (consumable)
        {
            ScoreManager.coffeeConsumed++;
            sr.enabled = false;
            collider.enabled = false; 
        }
    }
}
