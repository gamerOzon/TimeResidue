using UnityEngine;

public class LeiteInteracao : MonoBehaviour
{
    [Header("Pacotes de Leite")]
    public Transform leite1;
    public Transform leite2;
    public Transform leite3;

    [Header("Órbita")]
    public float raioDaOrbita = 0.5f;
    public float velocidadeOrbita = 60f;
    public float alturaOrbita = 0.1f;

    [Header("Deteção de Mão")]
    public float distanciaAtivacao = 0.4f;

    // Eventos
    public System.Action onAnimacaoIniciou;
    public System.Action onInteracaoCompleta;

    private Transform maoEsquerda;
    private Transform maoDireita;
    private Transform cabeca;

    private bool jaAtivou = false;
    private bool emOrbita = false;
    private float anguloOrbita1 = 0f;
    private float anguloOrbita2 = 120f;
    private float anguloOrbita3 = 240f;

    private readonly Vector3 posLeite1 = new Vector3(-1.754f, 0.464f, -1.244f);
    private readonly Vector3 posLeite2 = new Vector3(-1.582f, 0.464f, -1.234f);
    private readonly Vector3 posLeite3Deitado = new Vector3(-1.573f, 0.517f, -1.079f);
    private readonly Vector3 posLeite3Empe = new Vector3(-1.573f, 0.464f, -1.079f);

    private readonly Vector3 rotLeite1 = new Vector3(0f, 0f, 0f);
    private readonly Vector3 rotLeite2 = new Vector3(0f, -20.992f, 0f);
    private readonly Vector3 rotLeite3Deitado = new Vector3(0f, 11.071f, 90f);
    private readonly Vector3 rotLeite3Empe = new Vector3(0f, 11.071f, 0f);

    // Duração do áudio de info — controlada pelo NarratorManager
    public float duracaoAudio = 15f;

    void Start()
    {
        GameObject le = GameObject.Find("LeftHandAnchor");
        GameObject re = GameObject.Find("RightHandAnchor");
        if (le != null) maoEsquerda = le.transform;
        if (re != null) maoDireita = re.transform;

        GameObject cab = GameObject.Find("CenterEyeAnchor");
        if (cab != null) cabeca = cab.transform;
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

                // NOVO — avisa que a animação começou
                onAnimacaoIniciou?.Invoke();

                StartCoroutine(SequenciaLeite());
            }
        }

        if (emOrbita && cabeca != null)
        {
            anguloOrbita1 += velocidadeOrbita * Time.deltaTime;
            anguloOrbita2 += velocidadeOrbita * Time.deltaTime;
            anguloOrbita3 += velocidadeOrbita * Time.deltaTime;

            leite1.position = PosicaoOrbita(anguloOrbita1);
            leite2.position = PosicaoOrbita(anguloOrbita2);
            leite3.position = PosicaoOrbita(anguloOrbita3);
        }
    }

    Vector3 PosicaoOrbita(float angulo)
    {
        float rad = angulo * Mathf.Deg2Rad;
        Vector3 centro = cabeca.position + Vector3.up * alturaOrbita;
        return new Vector3(
            centro.x + Mathf.Cos(rad) * raioDaOrbita,
            centro.y,
            centro.z + Mathf.Sin(rad) * raioDaOrbita);
    }

    System.Collections.IEnumerator SequenciaLeite()
    {
        float velocidade = 1.5f;

        yield return StartCoroutine(RotacionarPara(leite3, rotLeite3Empe, 2f));
        leite3.position = posLeite3Empe;

        Vector3 centro = cabeca.position + Vector3.up * alturaOrbita;
        Vector3 alvo1 = new Vector3(centro.x + Mathf.Cos(0f) * raioDaOrbita, centro.y, centro.z + Mathf.Sin(0f) * raioDaOrbita);
        Vector3 alvo2 = new Vector3(centro.x + Mathf.Cos(120f * Mathf.Deg2Rad) * raioDaOrbita, centro.y, centro.z + Mathf.Sin(120f * Mathf.Deg2Rad) * raioDaOrbita);
        Vector3 alvo3 = new Vector3(centro.x + Mathf.Cos(240f * Mathf.Deg2Rad) * raioDaOrbita, centro.y, centro.z + Mathf.Sin(240f * Mathf.Deg2Rad) * raioDaOrbita);

        StartCoroutine(MoverPara(leite1, alvo1, velocidade));
        StartCoroutine(MoverPara(leite2, alvo2, velocidade));
        yield return StartCoroutine(MoverPara(leite3, alvo3, velocidade));

        emOrbita = true;

        // Espera a duração do áudio
        yield return new WaitForSeconds(duracaoAudio);

        emOrbita = false;
        yield return null;

        StartCoroutine(MoverPara(leite1, posLeite1, velocidade));
        StartCoroutine(MoverPara(leite2, posLeite2, velocidade));
        yield return StartCoroutine(MoverPara(leite3, posLeite3Empe, velocidade));

        yield return StartCoroutine(RotacionarPara(leite3, rotLeite3Deitado, 2f));
        leite3.position = posLeite3Deitado;

        jaAtivou = false;

        // Avisa que terminou
        onInteracaoCompleta?.Invoke();
    }

    System.Collections.IEnumerator MoverPara(Transform obj, Vector3 destino, float velocidade)
    {
        while (Vector3.Distance(obj.position, destino) > 0.01f)
        {
            obj.position = Vector3.MoveTowards(obj.position, destino, velocidade * Time.deltaTime);
            yield return null;
        }
        obj.position = destino;
    }

    System.Collections.IEnumerator RotacionarPara(Transform obj, Vector3 rotacaoAlvo, float velocidade)
    {
        Quaternion inicio = obj.rotation;
        Quaternion alvo = Quaternion.Euler(rotacaoAlvo);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidade;
            obj.rotation = Quaternion.Lerp(inicio, alvo, t);
            yield return null;
        }
        obj.rotation = alvo;
    }
}