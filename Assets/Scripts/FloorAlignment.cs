using UnityEngine;

public class FloorAlignment : MonoBehaviour
{
    void Start()
    {
        // Força o player para o chão
        Vector3 pos = transform.position;
        pos.y = 0;
        transform.position = pos;
    }
}