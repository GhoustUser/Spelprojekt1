using UnityEngine;

public class Hunger : MonoBehaviour
{
    [SerializeField] private float maxHunger;
    public static float hungerLevel;
    [SerializeField] private float decayRate;

    private RectTransform rTransform;
    private float initSize;
    private Player player;

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

        if (hungerLevel <= maxHunger) return;

        player.GainHealth(1);
        hungerLevel = maxHunger * 0.5f;
    }
}
