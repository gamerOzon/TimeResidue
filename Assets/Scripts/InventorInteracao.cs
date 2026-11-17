using UnityEngine;

public class InventorInteracao : MonoBehaviour
{
    [Header("Duração do Áudio")]
    public float duracaoAudio = 4.6f;

    [Header("Deteção de Mão")]
    public float distanciaAtivacao = 0.3f;

    public System.Action onAnimacaoIniciou;
    public System.Action onInteracaoCompleta;

    private Transform maoEsquerda;
    private Transform maoDireita;
    private bool jaAtivou = false;
    private float tempoAnimacaoIniciou = 0f;

    void Start()
    {
        GameObject le = GameObject.Find("LeftHandAnchor");
        GameObject re = GameObject.Find("RightHandAnchor");
        if (le != null) maoEsquerda = le.transform;
        if (re != null) maoDireita = re.transform;
    }

    void Update()
    {
        if (!jaAtivou)
        {
            bool maoPertoEsq = maoEsquerda != null &&
                Vector3.Distance(maoEsquerda.position, transform.position) < distanciaAtivacao;
            bool maoPertoDir = maoDireita != null &&
                Vector3.Distance(maoDireita.position, transform.position) < distanciaAtivacao;

            if (maoPertoEsq || maoPertoDir)
            {
                jaAtivou = true;
                tempoAnimacaoIniciou = Time.time;
                onAnimacaoIniciou?.Invoke();
                StartCoroutine(Sequencia());
            }
        }
    }

    System.Collections.IEnumerator Sequencia()
    {
        yield return new WaitForSeconds(duracaoAudio);
        jaAtivou = false;
        onInteracaoCompleta?.Invoke();
    }
}