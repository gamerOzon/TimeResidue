using UnityEngine;

public class PosicaoInicial : MonoBehaviour
{
    void Start()
    {
        Invoke("DefinirPosicao", 0.5f);
    }

    void DefinirPosicao()
    {
        transform.position = new Vector3(-1.625f, 0f, 0.068f);
    }
}