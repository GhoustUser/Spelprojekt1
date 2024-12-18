using UnityEngine;

public class Hunger : MonoBehaviour
{
    [SerializeField] private float maxHunger;
    public static float hungerLevel;
    [SerializeField] private float decayRate;

    private RectTransform rTransform;
    private float initSize;
    private Player player;
    private bool loseHealth;
    [Tooltip("After healing, the health becomes max hunger multiplied by healCost")]
    [Range(0, 1)] [SerializeField] private float healCost;

    void Start()
    {
        hungerLevel = maxHunger;
        rTransform = GetComponent<RectTransform>();
        initSize = rTransform.sizeDelta.x;
        player = FindObjectOfType<Player>();
    }

    void Update()
    {
        hungerLevel = Mathf.Max(0, hungerLevel - Time.deltaTime * decayRate);
        rTransform.sizeDelta = new Vector2(initSize * hungerLevel / maxHunger, rTransform.sizeDelta.y);

        if (hungerLevel == 0 &! loseHealth)
        {
            loseHealth = true;
            Invoke(nameof(TakeDamage), 5);
        }

        if (hungerLevel <= maxHunger) return;

        if (!player.GainHealth(1))
        {
            hungerLevel = maxHunger;
            return;
        }

        hungerLevel = maxHunger * healCost;
    }

    private void TakeDamage()
    {
        if (hungerLevel != 0) return;

        player.TakeDamage(1);
        loseHealth = false;
    }
}
