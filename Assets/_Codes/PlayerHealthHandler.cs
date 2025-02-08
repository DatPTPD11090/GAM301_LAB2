using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthHandler : MonoBehaviour
{
    private Vector3 respawnPosition; // Vị trí respawn mặc định cho nhân vật

    [SerializeField]
    private GameObject playerPrefab;  // Gán Prefab của nhân vật trong Inspector
    [SerializeField]
    public int PlayerMaxHealth;
    [SerializeField]
    public int PlayerCurrentHealth;
    private PlayerController _PlayerController;

    public GameObject pickupParticle;
    public Transform topPlayerHead;

    public Image HeartIcon;
    Transform HeartPanel;
    public Color FillColor, EmptyColor;

    public GameObject DeathParticle;

    private ConstantForce constantForce;  // Thêm biến ConstantForce
    private Rigidbody rb;  // Thêm Rigidbody để đảm bảo nhân vật có thể quay

    Respawner spawner;

    private bool hasRespawned = false;

    void Start()
    {
        _PlayerController = GetComponent<PlayerController>();
        spawner = GameObject.FindGameObjectWithTag("Respawn").GetComponent<Respawner>();
        spawner.SetPosition(transform.position);

        HeartPanel = GameObject.Find("HeartPanel").transform;

        for (int i = 0; i < PlayerMaxHealth; i++)
        {
            Image Icon = Instantiate(HeartIcon, HeartPanel);
            Icon.color = FillColor;
        }

        PlayerCurrentHealth = PlayerMaxHealth;

        // Lấy lại component ConstantForce và Rigidbody khi game bắt đầu hoặc hồi sinh
        constantForce = GetComponent<ConstantForce>();
        if (constantForce == null)
        {
            constantForce = gameObject.AddComponent<ConstantForce>();  // Thêm lại nếu bị mất
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();  // Thêm lại nếu bị mất
            rb.useGravity = false;  // Tắt gravity nếu không muốn bị rơi
        }
    }

    void Update()
    {
        if (PlayerCurrentHealth <= 0)
        {
            print("Player is dead!");
            spawner.PlayerIsDead();
            Instantiate(DeathParticle, transform.position, transform.rotation);
            Destroy(gameObject);
        }

        // Check if the player's y coordinate is below -40
        if (transform.position.y < -40 && !hasRespawned)
        {
            PlayerCurrentHealth = 0;
            Debug.Log("Player position below -40!");
        }

        UpdateHearts();
    }

    public void GainMaxHealth()
    {
        GameObject clone = Instantiate(pickupParticle, topPlayerHead.position, topPlayerHead.rotation);
        clone.transform.SetParent(topPlayerHead);
        PlayerMaxHealth += 1;
        ResetHealth();
    }

    public void GainHealth(int amount)
    {
        PlayerCurrentHealth += amount;
        GameObject clone = Instantiate(pickupParticle, topPlayerHead.position, topPlayerHead.rotation);
        clone.transform.SetParent(topPlayerHead);
        if (PlayerCurrentHealth > PlayerMaxHealth)
        {
            PlayerCurrentHealth = PlayerMaxHealth;
        }
        SavePlayerHealth();
    }

    public void LoseHealth(int amount, Vector3 push)
    {
        PlayerCurrentHealth -= amount;
        if (PlayerCurrentHealth < 0)
        {
            PlayerCurrentHealth = 0;
        }
        _PlayerController.controller.Move(-push);
        _PlayerController.flashRed();

        // Thêm lực xoay khi mất máu
        if (constantForce != null)
        {
            constantForce.torque = new Vector3(0, 500f, 0);  // Nhân vật quay quanh trục Y
            StartCoroutine(StopSpinningAfterDelay(1f));   // Dừng quay sau 1 giây
        }
        else
        {
            Debug.LogError("ConstantForce không tồn tại!");
        }

        SavePlayerHealth();
    }
    IEnumerator StopSpinningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (constantForce != null)
        {
            constantForce.torque = Vector3.zero;  // Dừng quay
        }
    }

    public void ResetHealth()
    {
        PlayerCurrentHealth = PlayerMaxHealth;
        Image Icon = Instantiate(HeartIcon, HeartPanel);
        Icon.color = FillColor;
    }

    void UpdateHearts()
    {
        Image[] icons = HeartPanel.GetComponentsInChildren<Image>();
        int numIcons = Mathf.Min(PlayerMaxHealth, icons.Length - 1); // new code with gpt i dont give a shit
        for (int n = 0; n < PlayerMaxHealth; n++)
        {
            if (n < PlayerCurrentHealth)
            {
                icons[n + 1].color = FillColor;
            }
            else
            {
                icons[n + 1].color = EmptyColor;
            }
        }
    }

    private void SavePlayerHealth()
    {
        // Save player health to PlayerPrefs
        PlayerPrefs.SetInt("PlayerMaxHealth", PlayerMaxHealth);
        PlayerPrefs.SetInt("PlayerCurrentHealth", PlayerCurrentHealth);
        PlayerPrefs.Save();
    }

    private void LoadPlayerHealth()
    {
        // Load player health from PlayerPrefs
        PlayerMaxHealth = PlayerPrefs.GetInt("PlayerMaxHealth", PlayerMaxHealth);
        PlayerCurrentHealth = PlayerPrefs.GetInt("PlayerCurrentHealth", PlayerMaxHealth);
    }
    public void RespawnReset()
    {
        // Reset lại sức khỏe và các trạng thái quay của nhân vật
        PlayerCurrentHealth = PlayerMaxHealth;
        UpdateHearts();

        // Đảm bảo ConstantForce được reset sau khi hồi sinh
        ConstantForce constantForce = GetComponent<ConstantForce>();
        if (constantForce != null)
        {
            constantForce.torque = Vector3.zero;  // Reset quay
        }
    }
    public void playerIsDead()
    {
        // Hồi sinh nhân vật sau 3 giây
        StartCoroutine(RespawnPlayer());
    }

    IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(3f);

        // Tạo lại nhân vật tại vị trí hồi sinh
        GameObject player = Instantiate(playerPrefab, respawnPosition, Quaternion.identity);

        // Reset lại trạng thái nhân vật sau khi hồi sinh
        player.GetComponent<PlayerHealthHandler>().RespawnReset();
    }
}

