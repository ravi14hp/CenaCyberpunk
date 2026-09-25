using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float speed = 5f;

    [Header("Free Look Camera")]
    [SerializeField] private GameObject Camera;

    [Header("Flutuação")]
    [SerializeField] private float alturaFlutuacao = 0.5f;
    [SerializeField] private float forcaFlutuacao = 40f;
    [SerializeField] private float amortecimento = 8f;
    [SerializeField] private LayerMask chaoLayer;

    [Header("Efeito de Flutuação")]
    [SerializeField] private float amplitudeFlutuacao = 0.15f;
    [SerializeField] private float velocidadeOscilacao = 1f;

    private Rigidbody rb;

    private float movex;
    private float movez;
    private float CurrentSpeed;

    private float initialRotationX;
    private float initialRotationZ;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Guarda a rotação original do Player
        initialRotationX = transform.eulerAngles.x;
        initialRotationZ = transform.eulerAngles.z;
    }

    private void Start()
    {
        CurrentSpeed = speed;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        Move();
        Flutuar();
    }

    private void Move()
    {
        // Sprint
        if (Input.GetKey(KeyCode.LeftShift))
        {
            CurrentSpeed = speed * 2f;
        }
        else
        {
            CurrentSpeed = speed;
        }

        // Inputs
        movex = Input.GetAxis("Horizontal");
        movez = Input.GetAxis("Vertical");

        // Direção baseada na câmera
        Vector3 CamF = Camera.transform.forward;
        Vector3 CamR = Camera.transform.right;

        CamF.y = 0;
        CamR.y = 0;

        CamF.Normalize();
        CamR.Normalize();

        Vector3 Dir = CamF * movez + CamR * movex;

        // Evita ficar mais rápido andando na diagonal
        if (Dir.magnitude > 1f)
        {
            Dir.Normalize();
        }

        // Movimento
        rb.linearVelocity = new Vector3(
            Dir.x * CurrentSpeed,
            rb.linearVelocity.y,
            Dir.z * CurrentSpeed
        );

        // Rotação do Player
        Quaternion finalRotation = Quaternion.Euler(
            initialRotationX,
            Camera.transform.eulerAngles.y,
            initialRotationZ
        );

        rb.MoveRotation(finalRotation);
    }

    private void Flutuar()
    {
        RaycastHit hit;

        // Faz a altura desejada subir e descer lentamente
        float alturaAtualDesejada =
            alturaFlutuacao +
            Mathf.Sin(Time.time * velocidadeOscilacao) * amplitudeFlutuacao;

        // Raio procurando o chão
        if (Physics.Raycast(
            transform.position,
            Vector3.down,
            out hit,
            alturaFlutuacao + amplitudeFlutuacao + 2f,
            chaoLayer))
        {
            // Diferença entre a altura atual e a altura desejada
            float diferencaAltura = alturaAtualDesejada - hit.distance;

            // Velocidade vertical atual
            float velocidadeVertical = rb.linearVelocity.y;

            // Força de flutuação
            float forca =
                (diferencaAltura * forcaFlutuacao) -
                (velocidadeVertical * amortecimento);

            rb.AddForce(
                Vector3.up * forca,
                ForceMode.Acceleration
            );
        }
    }
}
