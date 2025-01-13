using UnityEngine;

public class CoffeeDrinkerScript : MonoBehaviour
{

    private AudioSource audioSource;
    private bool canDrink;
    [SerializeField] 
    private bool consumable;

    private SpriteRenderer sr;

    private CircleCollider2D coll;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        sr = GetComponent<SpriteRenderer>();
        coll = GetComponent<CircleCollider2D>();
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
        ScoreManager.coffeeConsumed += 0.25f;
        if (consumable)
        {
            sr.enabled = false;
            coll.enabled = false; 
        }
    }
}
