using UnityEngine;

public class CanTriggerScript : MonoBehaviour
{
    public static bool canTrigger;

    public static void EnableTrigger()
    {
        canTrigger = true;
    }
    
    public static void DisableTrigger()
    {
        canTrigger = false;
    }
}
