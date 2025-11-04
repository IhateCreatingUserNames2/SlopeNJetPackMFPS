using UnityEngine;

public class SloopMovement : MonoBehaviour
{
    [Header("Skiing Physics (Tribes Style)")]
    [Tooltip("Velocidade mínima para começar a esquiar.")]
    public float minSpeedToSki = 8f;
    [Tooltip("Velocidade máxima absoluta (0 = sem limite).")]
    public float maxSpeed = 50f;
    [Tooltip("Gravidade aplicada quando está no ar ou descendo.")]
    public float gravity = 25f;
    [Tooltip("Fricção aplicada no chão plano.")]
    public float groundFriction = 2f;
    [Tooltip("Fricção aplicada durante skiing (muito baixa).")]
    public float skiFriction = 0.1f;
    [Tooltip("Controle do jogador no chão (WASD).")]
    public float groundControl = 25f;
    [Tooltip("Controle do jogador durante skiing (WASD).")]
    public float skiControl = 15f;
    [Tooltip("Controle do jogador no ar (WASD).")]
    public float airControl = 0.8f;
    [Tooltip("Força para manter grudado no chão em alta velocidade.")]
    public float groundStickForce = 15f;
    [Tooltip("Quanto da velocidade é mantida ao subir rampas (0-1). 1 = sem perda.")]
    [Range(0f, 1f)]
    public float uphillMomentumRetention = 0.85f;

    public bool IsSkiing { get; private set; } = false;
    public float CurrentSpeed { get; private set; } = 0f;

    // Referências
    private bl_FirstPersonController m_controller;
    private SloopJetpack m_jetpack;

    // Estado interno
    private Vector3 velocity = Vector3.zero;
    private Vector3 groundNormal = Vector3.up;
    private bool wasGrounded = false;

    void Start()
    {
        m_controller = GetComponent<bl_FirstPersonController>();
        m_jetpack = GetComponent<SloopJetpack>();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        groundNormal = hit.normal;
    }

    public Vector3 CalculateMovement(Vector2 input, float baseSpeed, bool jumpPressed, bool jumpHeld)
    {
        bool isGrounded = m_controller.m_CharacterController.isGrounded;
        Vector3 inputDirection = (m_controller.transform.forward * input.y + m_controller.transform.right * input.x).normalized;

        // Calcula velocidade horizontal atual
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        CurrentSpeed = horizontalVelocity.magnitude;

        // ===== SKIING: Ativa quando velocidade é alta E está no chão =====
        IsSkiing = isGrounded && CurrentSpeed > minSpeedToSki;

        if (isGrounded)
        {
            // ===== NO CHÃO =====

            // ===== PULO: Verifica ANTES de aplicar física de skiing =====
            if (jumpPressed && !jumpHeld)
            {
                // Pulo normal - APENAS velocidade vertical
                velocity.y = m_controller.jumpSpeed;
                // NÃO modifica velocidade horizontal (x e z)
            }
            else if (!jumpPressed && velocity.y < 0)
            {
                // Aplica força para grudar no chão (evita "pular")
                // Só aplica se NÃO está pulando
                velocity.y = -groundStickForce;
            }

            // Calcula o ângulo da rampa
            float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
            bool isOnSlope = slopeAngle > 5f;
            if (IsSkiing)
            {
                // ===== MODO SKIING (ALTA VELOCIDADE) =====

                // Em rampas, adiciona ou remove velocidade baseado na inclinação
                if (isOnSlope)
                {
                    // Projeta a gravidade na direção da rampa
                    Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
                    float slopeFactor = Mathf.Clamp01((slopeAngle - 5f) / 45f); // 0 a 1 baseado na inclinação

                    // Verifica se está descendo ou subindo
                    float verticalDirection = Vector3.Dot(velocity.normalized, Vector3.up);

                    if (verticalDirection < 0) // Descendo
                    {
                        // GANHA velocidade descendo
                        velocity += slopeDirection * gravity * slopeFactor * Time.deltaTime;
                    }
                    else // Subindo
                    {
                        // PERDE velocidade subindo (mas retém momentum)
                        velocity += slopeDirection * gravity * slopeFactor * uphillMomentumRetention * Time.deltaTime;
                    }
                }

                // Fricção mínima durante skiing
                velocity = Vector3.Lerp(velocity, new Vector3(velocity.x, velocity.y, velocity.z), skiFriction * Time.deltaTime);

                // ===== CONTROLE DIRECIONAL APRIMORADO DURANTE SKIING =====
                // WASD tem controle TOTAL da direção, independente do momentum
                if (input.magnitude > 0.1f)
                {
                    // Aplica força direcional forte
                    velocity += inputDirection * skiControl * Time.deltaTime;

                    // Se está tentando ir para trás (S), aplica freio adicional
                    if (input.y < -0.1f)
                    {
                        // Freio: reduz velocidade frontal
                        Vector3 forwardVelocity = m_controller.transform.forward * Vector3.Dot(velocity, m_controller.transform.forward);
                        velocity -= forwardVelocity * groundFriction * 0.5f * Time.deltaTime;
                    }
                }
            }
       
        else
        {
            // ===== MOVIMENTO NORMAL (BAIXA VELOCIDADE) =====

            // Controle total com WASD
            Vector3 targetVelocity = inputDirection * baseSpeed;

            // Aplica fricção para alcançar a velocidade desejada rapidamente
            velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x, groundFriction * Time.deltaTime);
            velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z, groundFriction * Time.deltaTime);

            // LIMITA a velocidade máxima no movimento normal
            Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
            if (horizontalVel.magnitude > baseSpeed * 1.2f) // Máximo 20% acima da velocidade base
            {
                horizontalVel = horizontalVel.normalized * baseSpeed * 1.2f;
                velocity.x = horizontalVel.x;
                velocity.z = horizontalVel.z;
            }
        }

            wasGrounded = true;
        }
        else
        {
            // ===== NO AR =====

            IsSkiing = false;

            // Jetpack: adiciona força vertical
            if (m_jetpack != null && m_jetpack.IsActive)
            {
                velocity += m_jetpack.CalculateJetpackForce() * Time.deltaTime;
            }
            else
            {
                // Gravidade normal
                velocity.y += Physics.gravity.y * m_controller.m_GravityMultiplier * Time.deltaTime;
            }

            // ===== CONTROLE AÉREO APRIMORADO =====
            // Mais controle logo após sair do chão, controle médio durante voo
            float airControlMultiplier = wasGrounded ? airControl * 3f : airControl * 1.5f;

            if (input.magnitude > 0.1f)
            {
                velocity += inputDirection * groundControl * airControlMultiplier * Time.deltaTime;
            }

            wasGrounded = false;
        }

        return velocity;
    }
}