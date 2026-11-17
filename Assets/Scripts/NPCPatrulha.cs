using UnityEngine;
using UnityEngine.AI;

public class NPCPatrulha : MonoBehaviour
{
    private Vector3[] pontos = new Vector3[]
    {
        new Vector3(6.79f, 0.02f, -3.45f),
        new Vector3(-3.92f, 0.02f, -2.69f),
        new Vector3(-3.38f, 0.02f, 10.1f)
    };

    private int pontoAtual = 0;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        IrParaProximoPonto();
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            IrParaProximoPonto();
    }

    void IrParaProximoPonto()
    {
        agent.destination = pontos[pontoAtual];
        pontoAtual = (pontoAtual + 1) % pontos.Length;
    }
}