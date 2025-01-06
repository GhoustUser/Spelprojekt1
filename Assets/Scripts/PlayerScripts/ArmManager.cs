using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmManager : MonoBehaviour
{
    /* -------- Settings --------*/
    [SerializeField] private int ArmCount = 1;
    [SerializeField] private float Length = 1.6f;
    [SerializeField] private float MomentumFactor = 0.9f;
    [SerializeField] private int Attempts = 5;
    [SerializeField] private float AngleRange = 45.0f;
    [SerializeField] private Material ArmMaterial;

    /* -------- Start --------*/
    void Start()
    {
        for (int i = 0; i < ArmCount; i++)
        {
            // Create a new arm
            GameObject arm = new GameObject($"Arm_{i}");

            // Set its parent to the current GameObject
            arm.transform.SetParent(transform);
            //set random position around parent
            Vector2 direction = Random.insideUnitCircle;
            arm.transform.localPosition = new Vector3(direction.x * Length, direction.y * Length, 0);
            
            // Add the armScript and set its variables
            armScript script = arm.AddComponent<armScript>();
            
            script.TotalLength = Length;
            script.MomentumFactor = MomentumFactor;
            script.Attempts = Attempts;
            script.AngleRange = AngleRange;
            script.ArmMaterial = ArmMaterial;
        }
    }

    /* -------- Update --------*/
    void Update()
    {
    }
}