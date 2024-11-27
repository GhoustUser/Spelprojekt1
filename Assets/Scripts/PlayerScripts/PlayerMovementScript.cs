using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
/////////////// INFORMATION ///////////////
// This script automatically adds a Rigidbody2D and a CapsuleCollider2D componentin the inspector.
// The following components are needed: Player Input
public class TopDownMovement : MonoBehaviour
{
    public float maxSpeed = 7;
    public bool controlEnabled { get; set; } = true; // You can edit this variable from Unity Events
    public bool isAttacking;
    public bool isDashing;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Set gravity scale to 0 so player won't "fall"
        rb.gravityScale = 0;
    }
    private void Update()
    {
    }
    private void FixedUpdate()
    {
        // Set velocity based on direction of input and maxSpeed
        if (controlEnabled)
        {
            rb.velocity = moveInput.normalized * maxSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
        // Write code for walking animation here. (Suggestion: send your current velocity into the Animator for both the x - and y - axis.)
    }
    // Handle Move-input
    // This method can be triggered through the UnityEvent in PlayerInput
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>().normalized;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        isAttacking = !isAttacking;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        isDashing = !isDashing;
    }
}