using UnityEngine;

public class MovingPlatformTriggerTile : MonoBehaviour
{
    private MovingPlatformGroup platformGroup; // Ссылка на родительский объект

    private void Start()
    {
        // Ищем родительский объект с компонентом MovingPlatformGroup
        platformGroup = GetComponentInParent<MovingPlatformGroup>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && platformGroup != null)
        {
            platformGroup.StartMoving(); // Запускаем движение платформы
        }
    }
}

