using UnityEngine;
using UnityEngine.UI;

public class SloopJetpack : MonoBehaviour
{
    [Header("Jetpack Settings")]
    public float jetpackForce = 40f;
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

            // Retorna a força de aceleração
            return Vector3.up * jetpackForce;
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