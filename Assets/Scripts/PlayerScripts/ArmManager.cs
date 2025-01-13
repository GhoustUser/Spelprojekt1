using System.Collections.Generic;
using UnityEngine;

public class ArmManager : MonoBehaviour
{
    /* -------- Settings --------*/
    [Header("Appearance")] [Tooltip("amount of arms")] [SerializeField] [Range(0, 100)]
    private int ArmCount = 8;

    [Tooltip("the length of each arm")] [SerializeField]
    private float Length = 1.8f;

    [Tooltip("makes the length of each arm slightly random")] [SerializeField] [Range(0f, 1f)]
    private float LengthRandomness = 0.1f;

    [Tooltip("the width of each arm")] [SerializeField] [Range(0.01f, 0.2f)]
    private float Width = 0.1f;

    [Tooltip("How thin the arm will get when stretched")] [SerializeField] [Range(0f, 1f)]
    private float Stretching = 0.5f;

    [Header("Behaviour")] [Tooltip("the amount of momentum for each arm")] [SerializeField] [Range(0f, 1f)]
    private float MomentumFactor = 0.9f;

    [Tooltip("when moving an arm forward, it will be placed within this angle in front of its origin")]
    [SerializeField]
    [Range(0f, 180f)]
    private float AngleRange = 90.0f;

    [Tooltip("How many attempts each arm will make to adjust")] [SerializeField]
    private int Attempts = 5;

    [Header("Debugging")]
    [Tooltip("when enabled, changing settings in the editor will apply immediately")]
    [SerializeField]
    private bool DoUpdateArmsRealtime = false;

    [Header("Material")] [SerializeField] private Material ArmMaterial;

    /* -------- Variables --------*/
    private List<armScript> armScripts;
    public LayerMask wallLayer;

    private int _armCount = 1;
    private float _length = 1.8f;
    private float _lengthRandomness = 0f;
    private float _width = 0.1f;
    private float _stretching = 0.5f;
    private float _momentumFactor = 0.9f;
    private int _attempts = 5;
    private float _angleRange = 90.0f;

    /* -------- Start --------*/
    private void Start()
    {
        //sync variables
        _armCount = ArmCount;
        _length = Length;
        _lengthRandomness = LengthRandomness;
        _width = Width;
        _stretching = Stretching;
        _momentumFactor = MomentumFactor;
        _attempts = Attempts;
        _angleRange = AngleRange;

        //initialize arms
        armScripts = new List<armScript>();
        for (int i = 0; i < _armCount; i++) AddArm();
    }

    /* -------- Update --------*/
    private void Update()
    {
        if (!DoUpdateArmsRealtime) return;

        /* -- update arm count -- */
        if (ArmCount != _armCount)
        {
            //reduce amount of arms
            if (ArmCount < _armCount)
            {
                for (int i = _armCount; i > ArmCount; i--)
                {
                    armScripts[i - 1].DeleteArm();
                    armScripts.RemoveAt(i - 1);
                }
            }
            else if (ArmCount > _armCount)
            {
                for (int i = _armCount; i < ArmCount; i++) AddArm();
            }

            _armCount = (int)ArmCount;
        }

        /* -- update arm length -- */
        if (!Mathf.Approximately(Length, _length))
        {
            foreach (armScript armScript in armScripts) armScript.totalLength = Length;
            _length = Length;
        }

        /* -- update arm length randomness -- */
        if (!Mathf.Approximately(LengthRandomness, _lengthRandomness))
        {
            foreach (armScript armScript in armScripts) armScript.lengthRandomness = LengthRandomness;
            _lengthRandomness = LengthRandomness;
        }

        /* -- update arm width -- */
        if (!Mathf.Approximately(Width, _width))
        {
            foreach (armScript armScript in armScripts) armScript.width = Width;
            _width = Width;
        }

        /* -- update arm stretching -- */
        if (!Mathf.Approximately(Stretching, _stretching))
        {
            foreach (armScript armScript in armScripts) armScript.stretching = Stretching;
            _stretching = Stretching;
        }

        /* -- update arm momentum factor -- */
        if (!Mathf.Approximately(MomentumFactor, _momentumFactor))
        {
            foreach (armScript armScript in armScripts) armScript.momentumFactor = MomentumFactor;
            _momentumFactor = MomentumFactor;
        }

        /* -- update arm angle range -- */
        if (!Mathf.Approximately(AngleRange, _angleRange))
        {
            foreach (armScript armScript in armScripts) armScript.angleRange = AngleRange;
            _angleRange = AngleRange;
        }

        /* -- update arm attempts -- */
        if (Attempts != _attempts)
        {
            foreach (armScript armScript in armScripts) armScript.attempts = Attempts;
            _attempts = Attempts;
        }
    }

    /* -------- Create Arm --------*/
    private void AddArm()
    {
        // Create a new arm
        GameObject arm = new GameObject($"Arm_{armScripts.Count}");

        // Set its parent to the current GameObject
        arm.transform.SetParent(transform);
        //set random position around parent
        Vector2 direction = Random.insideUnitCircle;
        arm.transform.localPosition = new Vector3(direction.x * Length, direction.y * Length, 0);

        // Add the armScript and set its variables
        armScript script = arm.AddComponent<armScript>();
        armScripts.Add(script);

        script.totalLength = _length;
        script.lengthRandomness = _lengthRandomness;
        script.width = _width;
        script.stretching = _stretching;
        script.momentumFactor = _momentumFactor;
        script.angleRange = _angleRange;
        script.attempts = _attempts;
        script.armMaterial = ArmMaterial;
    }
    
    /* -------- Override all arm targets --------*/
    public void OverrideArmTargets(Vector2 position, float armAmount = 1.0f)
    {
        foreach (var script in armScripts)
        {
            script.OverrideTarget(position);
        }
    }
}