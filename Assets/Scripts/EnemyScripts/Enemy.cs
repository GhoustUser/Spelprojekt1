using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected abstract void Movement();

    private void Update()
    {
        Movement();
    }
}
