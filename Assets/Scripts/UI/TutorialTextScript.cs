using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class TutorialTextScript : MonoBehaviour
{
    private float tutorialDuration = 10.0f;
    
    [SerializeField]
    private TextMeshProUGUI text;

    void FixedUpdate()
    {
        tutorialDuration -= Time.deltaTime;
        if (text == null) return;
        if (tutorialDuration < 0)
        {
            text.gameObject.SetActive(false); 
        }
    }
}
