using UnityEngine;
using System.Collections;

public class PapeisVoar : MonoBehaviour
{
    [Header("Papéis")]
    public Transform papel1;
    public Transform papel2;

    [Header("Destino")]
    public Vector3 posicaoLareira = new Vector3(20.5732002f, 0.324999988f, -67.2110291f);

    [Header("Velocidade")]
    public float velocidade = 2f;
    public float velocidadeRotacao = 270f;

    public void IniciarVoo()
    {
        if (papel1 != null) StartCoroutine(VoarPapel(papel1));
        if (papel2 != null) StartCoroutine(VoarPapel(papel2));
    }

    IEnumerator VoarPapel(Transform papel)
    {
        Vector3 origem = papel.position;
        Vector3 destino = posicaoLareira;
        Vector3 pontoMedio = (origem + destino) / 2f + Vector3.up * 1.5f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidade;

            Vector3 pos = Mathf.Pow(1 - t, 2) * origem
                        + 2 * (1 - t) * t * pontoMedio
                        + Mathf.Pow(t, 2) * destino;

            papel.position = pos;
            papel.Rotate(velocidadeRotacao * Time.deltaTime, 0f, 0f, Space.World);

            yield return null;
        }

        papel.position = destino;
        papel.gameObject.SetActive(false);
    }
}