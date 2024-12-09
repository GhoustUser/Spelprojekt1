using UnityEngine;

public class BloodStain : MonoBehaviour
{
    [SerializeField] private float displayTime = 3;
    [SerializeField] private SpriteRenderer sr;

    private float timer;

    private void Start()
    {
        timer = displayTime;
    }

    void Update()
    {
        // Reduces the opacity of the bloodstain gradually, and deletes it when it's invisible.
        sr.color = new Color(1, 1, 1, timer / displayTime);
        timer -= Time.deltaTime;
        if (timer < 0) Destroy(gameObject);
    }
}
