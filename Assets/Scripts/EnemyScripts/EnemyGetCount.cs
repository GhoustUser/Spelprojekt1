using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyGetCount : MonoBehaviour
{
    public static int enemyCount;

    public static bool gameWin = false;
    
    [SerializeField]
    private TextMeshProUGUI text; 
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        print(enemyCount);
        if(enemyCount <= 0 && gameWin)
        {
            PlayerMovement.controlEnabled = false;
            PlayerAttack.controlEnabled = false;
            text.gameObject.SetActive(true);
        }
    }
}
