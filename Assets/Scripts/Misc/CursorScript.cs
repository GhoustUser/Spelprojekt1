using UnityEngine;

public class CursorScript : MonoBehaviour
{
    void Update()
    {
        Cursor.visible = false;
        transform.position = Input.mousePosition;
    }
}
