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
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        tutorialDuration -= Time.deltaTime;
        print(tutorialDuration);
        if (tutorialDuration < 0)
        {
            text.gameObject.SetActive(false); 
        }
    }
}
