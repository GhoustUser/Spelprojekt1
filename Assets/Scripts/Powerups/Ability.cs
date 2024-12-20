using System.Collections;
using UnityEngine;

public interface Ability
{
    public IEnumerator Activate(Vector3 direction);
}
