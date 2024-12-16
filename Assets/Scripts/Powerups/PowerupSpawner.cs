using System.Collections.Generic;
using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> powerupList;

    private void Start()
    {
        Instantiate(powerupList[Random.Range(0, powerupList.Count)], transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}