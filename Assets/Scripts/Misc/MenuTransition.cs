using UnityEngine;

public class MenuTransition : MonoBehaviour
{
    void Start()
    {
        GetComponent<Animator>().SetBool("open", true);   
    }
}
