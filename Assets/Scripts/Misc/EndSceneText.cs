using TMPro;
using UnityEngine;

public class EndSceneText : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    private float counter;

    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        tmp.alpha = 0;
        
    }

    void FixedUpdate()
    {
        tmp.alpha += 0.005f;
    }
}
