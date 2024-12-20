using UnityEngine;
using UnityEngine.Events;

public class Albin_Events : MonoBehaviour
{    
    public string tagCondition = "Player";
    
    public UnityEvent onAreaCollision, onAreaNoCollision;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(tagCondition))
        {
            onAreaCollision.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(tagCondition))
        {
            onAreaNoCollision.Invoke();
        }
    }
}
