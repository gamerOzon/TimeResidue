using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTeleporte : MonoBehaviour
{
    public string nomeCena = "Passado";

    [Header("Seguranca")]
    [Tooltip("So teleporta depois de Armar() (narrador acabar de falar).")]
    public bool exigeArmar = true;
    [Tooltip("Depois de armado, exige que a cabeca esteja fora do portal uma vez antes de teleportar. Evita teleporte instantaneo se a pessoa ja estiver em cima.")]
    public bool exigeEntradaNova = true;

    private Transform cabeca;
    private bool jaTeleportou = false;
    private bool armado = false;
    private bool esteveFora = false;

    void Start()
    {
        GameObject cab = GameObject.Find("CenterEyeAnchor");
        if (cab != null) cabeca = cab.transform;

        armado = !exigeArmar; // se nao exige armar, ja nasce armado
    }

    /// <summary>Liberta o teleporte. Chamar quando o narrador parar de falar.</summary>
    public void Armar()
    {
        armado = true;
        esteveFora = false; // forca entrada nova a partir de agora
    }

    void Update()
    {
        if (jaTeleportou || cabeca == null) return;
        if (!armado) return;

        Collider col = GetComponent<Collider>();
        if (col == null) return;

        bool dentro = col.bounds.Contains(cabeca.position);

        // Exige que a cabeca saia uma vez antes de aceitar entrada
        if (exigeEntradaNova && !esteveFora)
        {
            if (!dentro) esteveFora = true;
            return;
        }

        if (dentro)
        {
            jaTeleportou = true;
            SceneManager.LoadScene(nomeCena);
        }
    }
}
