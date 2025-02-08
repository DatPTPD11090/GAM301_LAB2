using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawner : MonoBehaviour
{
    [SerializeField]
    private GameObject PlayerToSpawn;
    private Vector3 respawnPosition;
    private Transform HeartPanel;

    public AudioClip deathSound;
    public AudioSource deathAudioSource;

    private void Start()
    {

        HeartPanel = GameObject.Find("HeartPanel").transform;


        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }


    public void SetPosition(Vector3 newPosition)
    {
        respawnPosition = newPosition;
    }

    // Hàm hồi sinh nhân vật
    private void Respawn()
    {

        foreach (Transform child in HeartPanel)
        {
            Destroy(child.gameObject);
        }


        GameObject newPlayer = Instantiate(PlayerToSpawn, respawnPosition, Quaternion.identity);

        // Lấy các component từ nhân vật mới
        AnimationHandler newPlayerAnim = newPlayer.GetComponent<AnimationHandler>();
        PlayerHealthHandler newHealthHandler = newPlayer.GetComponent<PlayerHealthHandler>();

        // Reset âm thanh và các hiệu ứng nếu cần
        if (deathAudioSource != null && deathSound != null)
        {
            deathAudioSource.clip = deathSound;
            deathAudioSource.Play();
        }

        // Đảm bảo sau khi hồi sinh, nhân vật vẫn bị quay khi mất máu
        if (newHealthHandler != null)
        {
            newHealthHandler.RespawnReset();  // Gọi hàm reset trong PlayerHealthHandler để reset trạng thái
        }
    }

    // Hàm gọi khi nhân vật chết
    public void PlayerIsDead()
    {
        Invoke("Respawn", 1);  // Gọi hàm hồi sinh sau 1 giây
    }
}
