using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    // Ссылка на скрипт игрока (находим при входе)
    private PlayerInteractionController playerController;

    void OnTriggerEnter(Collider other)
    {
        // Проверяем, что в зону вошел именно игрок (по тегу "Player")
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerInteractionController>();

            if (playerController != null)
            {
                playerController.EnterZone(this.gameObject);
            }
            else
            {
                Debug.LogError("На игроке нет скрипта PlayerInteractionController!");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerController != null)
            {
                playerController.ExitZone();
                playerController = null;
            }
        }
    }

    // Если объект уничтожается, пока игрок внутри
    void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.ForceExitFromObject(this.gameObject);
        }
    }
}