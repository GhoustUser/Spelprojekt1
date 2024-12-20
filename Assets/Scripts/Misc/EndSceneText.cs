using TMPro;
using UnityEngine;

public class EndSceneText : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    private float counter;

    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        tmp.color = Color.clear;
    }

    void FixedUpdate()
    {
        tmp.color += new Color(0.005f, 0.005f, 0.005f, 0.005f);
    }
}
