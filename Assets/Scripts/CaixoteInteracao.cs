using UnityEngine;

public class CaixoteInteracao : MonoBehaviour
{
    [Header("Objetos do Caixote")]
    public Transform objeto1;
    public Transform objeto2;
    public Transform objeto3;

    [Header("Posições Iniciais")]
    public Vector3 inicialObj1 = new Vector3(2.093f, 0.025f, -1.479f);
    public Vector3 inicialObj2 = new Vector3(2.068f, 0.118f, -1.423f);
    public Vector3 inicialObj3 = new Vector3(2.068f, 0.028f, -1.465f);

    [Header("Posições Intermédias")]
    public Vector3 intermediaObj1 = new Vector3(2.09f, 0.896f, -1.48f);
    public Vector3 intermediaObj2 = new Vector3(2.068f, 1.129f, -1.437f);
    public Vector3 intermediaObj3 = new Vector3(2.068f, 0.781f, -1.465f);

    [Header("Posições Finais")]
    public Vector3 posicaoFinal1 = new Vector3(2.149f, 1.41f, -1.024f);
    public Vector3 posicaoFinal2 = new Vector3(2.149f, 1.41f, -1.47f);
    public Vector3 posicaoFinal3 = new Vector3(2.149f, 1.41f, -1.943f);

    [Header("Wobble")]
    public float wobbleVelocidade = 2.0f;
    public float wobbleIntensidade = 15f;

    [Header("Duração do Áudio")]
    public float duracaoAudio = 8.83f;

    [Header("Deteção de Mão")]
    public float distanciaAtivacao = 0.3f;

    // Eventos
    public System.Action onAnimacaoIniciou;
    public System.Action onInteracaoCompleta;

    private Transform maoEsquerda;
    private Transform maoDireita;
    private bool jaAtivou = false;
    private float tempoAnimacaoIniciou = 0f;

    private Quaternion rotacaoInicialObj1;
    private Quaternion rotacaoInicialObj2;
    private Quaternion rotacaoInicialObj3;

    private bool wobbleObj1 = false;
    private bool wobbleObj2 = false;
    private bool wobbleObj3 = false;

    void Start()
    {
        if (objeto1 != null) { objeto1.position = inicialObj1; rotacaoInicialObj1 = objeto1.rotation; }
        if (objeto2 != null) { objeto2.position = inicialObj2; rotacaoInicialObj2 = objeto2.rotation; }
        if (objeto3 != null) { objeto3.position = inicialObj3; rotacaoInicialObj3 = objeto3.rotation; }

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
                StartCoroutine(SequenciaObjetos());
            }
        }

        if (wobbleObj1) FazerWobble(objeto1, rotacaoInicialObj1);
        if (wobbleObj2) FazerWobble(objeto2, rotacaoInicialObj2);
        if (wobbleObj3) FazerWobble(objeto3, rotacaoInicialObj3);
    }

    void FazerWobble(Transform obj, Quaternion rotacaoBase)
    {
        if (obj == null) return;
        float angulo = Mathf.Sin(Time.time * wobbleVelocidade) * wobbleIntensidade;
        obj.rotation = rotacaoBase * Quaternion.Euler(0f, 0f, angulo);
    }

    void PararWobble(Transform obj, Quaternion rotacaoBase)
    {
        if (obj != null) obj.rotation = rotacaoBase;
    }

    System.Collections.IEnumerator SequenciaObjetos()
    {
        float velocidade = 1.5f;

        yield return StartCoroutine(MoverPara(objeto1, intermediaObj1, velocidade));
        wobbleObj1 = true;
        StartCoroutine(MoverPara(objeto1, posicaoFinal1, velocidade));
        yield return StartCoroutine(MoverPara(objeto2, intermediaObj2, velocidade));

        wobbleObj2 = true;
        StartCoroutine(MoverPara(objeto2, posicaoFinal2, velocidade));
        yield return StartCoroutine(MoverPara(objeto3, intermediaObj3, velocidade));

        wobbleObj3 = true;
        yield return StartCoroutine(MoverPara(objeto3, posicaoFinal3, velocidade));

        // Espera até o áudio terminar contando desde o início da interação
        float tempoDecorrido = Time.time - tempoAnimacaoIniciou;
        float tempoRestante = duracaoAudio - tempoDecorrido;
        if (tempoRestante > 0f)
            yield return new WaitForSeconds(tempoRestante);

        StartCoroutine(MoverPara(objeto3, intermediaObj3, velocidade));
        yield return StartCoroutine(MoverPara(objeto3, intermediaObj3, velocidade));
        wobbleObj3 = false;
        PararWobble(objeto3, rotacaoInicialObj3);
        StartCoroutine(MoverPara(objeto3, inicialObj3, velocidade));

        yield return StartCoroutine(MoverPara(objeto2, intermediaObj2, velocidade));
        wobbleObj2 = false;
        PararWobble(objeto2, rotacaoInicialObj2);
        StartCoroutine(MoverPara(objeto2, inicialObj2, velocidade));

        yield return StartCoroutine(MoverPara(objeto1, intermediaObj1, velocidade));
        wobbleObj1 = false;
        PararWobble(objeto1, rotacaoInicialObj1);
        yield return StartCoroutine(MoverPara(objeto1, inicialObj1, velocidade));

        jaAtivou = false;
        onInteracaoCompleta?.Invoke();
    }

    System.Collections.IEnumerator MoverPara(Transform obj, Vector3 destino, float velocidade)
    {
        while (Vector3.Distance(obj.position, destino) > 0.02f)
        {
            obj.position = Vector3.Lerp(obj.position, destino, Time.deltaTime * velocidade);
            yield return null;
        }
        obj.position = destino;
    }
}