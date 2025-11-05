using UnityEngine;
using UnityEngine.UI;

public class SloopJetpack : MonoBehaviour
{
    [Header("Jetpack Settings")]
    public float jetpackForce = 25f; // REDUZIDO de 40 para 25
    public float maxVerticalSpeed = 12f; // NOVO - Limita velocidade vertical
    public KeyCode jetpackKey = KeyCode.Space;

    [Header("Fuel Settings")]
    public float maxFuel = 100f;
    public float fuelConsumeRate = 20f;
    public float fuelRefillRate = 15f;
    public float fuelRefillDelay = 2f;

    [Header("UI (Opcional)")]
    public Image fuelBar;

    private float currentFuel;
    private bool isUsingJetpack = false;
    private float lastUseTime;

    private bl_FirstPersonController m_controller;

    void Start()
    {
        currentFuel = maxFuel;
        m_controller = GetComponent<bl_FirstPersonController>();
    }

    void Update()
    {
        HandleInput();
        UpdateFuel();
        UpdateUI();
    }

    void HandleInput()
    {
        // JETPACK ATIVA COM SPACE MANTIDO PRESSIONADO
        // Ativa tanto no chão quanto no ar, desde que tenha combustível
        if (Input.GetKey(jetpackKey) && currentFuel > 0)
        {
            isUsingJetpack = true;
            lastUseTime = Time.time;
        }
        else
        {
            isUsingJetpack = false;
        }
    }

    // Esta função será chamada pelo SloopMovement
    public Vector3 CalculateJetpackForce()
    {
        if (isUsingJetpack && currentFuel > 0)
        {
            currentFuel -= fuelConsumeRate * Time.deltaTime;
            currentFuel = Mathf.Max(0, currentFuel);

            // NOVO: Verifica velocidade vertical atual
            float currentVerticalSpeed = m_controller.Velocity.y;

            // Se já está na velocidade máxima ou acima, não adiciona mais força
            if (currentVerticalSpeed >= maxVerticalSpeed)
            {
                return Vector3.zero;
            }

            // Calcula força proporcional - quanto mais perto do limite, menor a força
            float speedRatio = Mathf.Clamp01(currentVerticalSpeed / maxVerticalSpeed);
            float adjustedForce = jetpackForce * (1f - speedRatio * 0.5f);

            // Retorna a força de aceleração ajustada
            return Vector3.up * adjustedForce;
        }
        return Vector3.zero;
    }

    void UpdateFuel()
    {
        // Reabastece combustível quando não está usando
        if (!isUsingJetpack)
        {
            if (Time.time > lastUseTime + fuelRefillDelay)
            {
                currentFuel += fuelRefillRate * Time.deltaTime;
                currentFuel = Mathf.Min(currentFuel, maxFuel);
            }
        }
    }

    void UpdateUI()
    {
        if (fuelBar != null)
        {
            fuelBar.fillAmount = currentFuel / maxFuel;
        }
    }

    // Propriedade pública para verificar se o jetpack está ativo
    public bool IsActive => isUsingJetpack;
}