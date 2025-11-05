using UnityEngine;

public class SloopMovement : MonoBehaviour
{
    [Header("Skiing Physics (Tribes Style)")]
    [Tooltip("Velocidade mínima para começar a esquiar.")]
    public float minSpeedToSki = 8f;
    [Tooltip("Velocidade máxima absoluta (0 = sem limite).")]
    public float maxSpeed = 35f; // REDUZIDO de 50 para 35
    [Tooltip("Gravidade aplicada quando está no ar ou descendo.")]
    public float gravity = 18f; // REDUZIDO de 25 para 18
    [Tooltip("Fricção aplicada no chão plano.")]
    public float groundFriction = 4f; // AUMENTADO de 2 para 4
    [Tooltip("Fricção aplicada durante skiing (muito baixa).")]
    public float skiFriction = 0.3f; // AUMENTADO de 0.1 para 0.3
    [Tooltip("Controle do jogador no chão (WASD).")]
    public float groundControl = 25f;
    [Tooltip("Controle do jogador durante skiing (WASD).")]
    public float skiControl = 20f; // AUMENTADO de 15 para 20
    [Tooltip("Controle do jogador no ar (WASD).")]
    public float airControl = 1.2f; // AUMENTADO de 0.8 para 1.2
    [Tooltip("Força para manter grudado no chão em alta velocidade.")]
    public float groundStickForce = 20f; // AUMENTADO de 15 para 20
    [Tooltip("Quanto da velocidade é mantida ao subir rampas (0-1). 1 = sem perda.")]
    [Range(0f, 1f)]
    public float uphillMomentumRetention = 0.75f; // REDUZIDO de 0.85 para 0.75

    [Header("Skiing Balance")]
    [Tooltip("Multiplicador de ganho de velocidade em descidas")]
    [Range(0.1f, 2f)]
    public float downhillSpeedGain = 0.6f; // NOVO - controla ganho em descidas
    [Tooltip("Ângulo mínimo de rampa para ganhar velocidade")]
    public float minSlopeAngleForBoost = 15f; // NOVO
    [Tooltip("Fricção extra ao virar durante skiing")]
    [Range(0f, 5f)]
    public float turningFriction = 1.5f; // NOVO

    public bool IsSkiing { get; private set; } = false;
    public float CurrentSpeed { get; private set; } = 0f;

    // Referências
    private bl_FirstPersonController m_controller;
    private SloopJetpack m_jetpack;

    // Estado interno
    private Vector3 velocity = Vector3.zero;
    private Vector3 groundNormal = Vector3.up;
    private bool wasGrounded = false;
    private Vector3 lastMoveDirection = Vector3.zero;

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
            }
            else if (!jumpPressed && velocity.y < 0)
            {
                // Aplica força para grudar no chão
                velocity.y = -groundStickForce;
            }

            // Calcula o ângulo da rampa
            float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
            bool isOnSlope = slopeAngle > 5f;

            if (IsSkiing)
            {
                // ===== MODO SKIING (ALTA VELOCIDADE) =====

                // NOVO: Aplica velocidade máxima durante skiing
                if (maxSpeed > 0 && CurrentSpeed > maxSpeed)
                {
                    horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
                    velocity.x = horizontalVelocity.x;
                    velocity.z = horizontalVelocity.z;
                }

                // Em rampas, adiciona ou remove velocidade baseado na inclinação
                if (isOnSlope && slopeAngle > minSlopeAngleForBoost)
                {
                    // Projeta a gravidade na direção da rampa
                    Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
                    float slopeFactor = Mathf.Clamp01((slopeAngle - minSlopeAngleForBoost) / 45f);

                    // Verifica se está descendo ou subindo
                    float verticalDirection = Vector3.Dot(velocity.normalized, Vector3.up);

                    if (verticalDirection < 0) // Descendo
                    {
                        // GANHA velocidade descendo (REDUZIDO com multiplicador)
                        velocity += slopeDirection * gravity * slopeFactor * downhillSpeedGain * Time.deltaTime;
                    }
                    else // Subindo
                    {
                        // PERDE velocidade subindo
                        velocity += slopeDirection * gravity * slopeFactor * uphillMomentumRetention * Time.deltaTime;
                    }
                }

                // NOVO: Fricção baseada em mudança de direção
                if (input.magnitude > 0.1f)
                {
                    float directionChange = Vector3.Angle(lastMoveDirection, inputDirection);
                    float turnFriction = Mathf.Clamp01(directionChange / 90f) * turningFriction;
                    velocity = Vector3.Lerp(velocity, velocity * (1f - turnFriction * Time.deltaTime), Time.deltaTime * 5f);
                }

                // Fricção base durante skiing (AUMENTADA)
                float effectiveSkiFriction = skiFriction;
                // Aumenta fricção em terreno plano
                if (!isOnSlope) effectiveSkiFriction *= 2f;

                velocity.x = Mathf.Lerp(velocity.x, velocity.x * (1f - effectiveSkiFriction), Time.deltaTime);
                velocity.z = Mathf.Lerp(velocity.z, velocity.z * (1f - effectiveSkiFriction), Time.deltaTime);

                // ===== CONTROLE DIRECIONAL APRIMORADO =====
                if (input.magnitude > 0.1f)
                {
                    // Força direcional (MELHORADA)
                    Vector3 controlForce = inputDirection * skiControl * Time.deltaTime;

                    // Reduz controle se estiver em velocidade muito alta
                    float speedRatio = Mathf.Clamp01(CurrentSpeed / maxSpeed);
                    controlForce *= Mathf.Lerp(1f, 0.6f, speedRatio);

                    velocity += controlForce;

                    // Freio ao pressionar S
                    if (input.y < -0.1f)
                    {
                        Vector3 forwardVelocity = m_controller.transform.forward * Vector3.Dot(velocity, m_controller.transform.forward);
                        velocity -= forwardVelocity * groundFriction * Time.deltaTime;
                    }

                    lastMoveDirection = inputDirection;
                }
            }
            else
            {
                // ===== MOVIMENTO NORMAL (BAIXA VELOCIDADE) =====
                Vector3 targetVelocity = inputDirection * baseSpeed;

                // Transição suave para velocidade desejada
                velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x, groundFriction * Time.deltaTime);
                velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z, groundFriction * Time.deltaTime);

                // LIMITA a velocidade máxima no movimento normal
                Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
                if (horizontalVel.magnitude > baseSpeed * 1.2f)
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
            if (maxSpeed > 0)
            {
                Vector3 totalVelocity = new Vector3(velocity.x, 0, velocity.z);
                if (totalVelocity.magnitude > maxSpeed)
                {
                    totalVelocity = totalVelocity.normalized * maxSpeed;
                    velocity.x = totalVelocity.x;
                    velocity.z = totalVelocity.z;
                }
            }
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
            float airControlMultiplier = wasGrounded ? airControl * 2.5f : airControl * 1.8f;

            if (input.magnitude > 0.1f)
            {
                velocity += inputDirection * groundControl * airControlMultiplier * Time.deltaTime;
            }

            wasGrounded = false;
        }

        return velocity;
    }
}