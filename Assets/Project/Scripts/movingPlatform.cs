using UnityEngine;
using System.Collections.Generic;

public class MovingPlatformGroup : MonoBehaviour
{
    public enum MovingPlatformType { Normal, Triggered }
    public MovingPlatformType platformType = MovingPlatformType.Normal;

    [System.Serializable]
    public class MovementPoint
    {
        public float xOffset;
        public float zOffset;
        public float moveSpeed = 1f;
        public float waitTime = 0f;
    }

    public List<MovementPoint> points = new List<MovementPoint>();

    private int currentPointIndex = 0;
    private Vector3 startPosition;
    private bool isMoving = false;
    private bool isWaiting = false;
    private float waitTimer = 0f;

		private bool poka_ne_bylo_triggera = true; // Флаг, который контролирует, был ли триггер

    private void Start()
    {
        startPosition = transform.position;

        if (platformType == MovingPlatformType.Normal)
        {
            isMoving = true;
        }
    }

    private void Update()
    {
        if (!isMoving || points.Count == 0)
            return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
            }
            else
            {
                return;
            }
        }

        MovementPoint targetPoint = points[currentPointIndex];
        Vector3 targetPosition = startPosition + new Vector3(targetPoint.xOffset, 0f, targetPoint.zOffset);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, targetPoint.moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            // Если есть пауза — ждем
            if (targetPoint.waitTime > 0f)
            {
                isWaiting = true;
                waitTimer = targetPoint.waitTime;
            }

            // Переход к следующей точке
            currentPointIndex++;

            if (currentPointIndex >= points.Count)
            {
                if (platformType == MovingPlatformType.Normal)
                {
                    // Зацикливаемся
                    currentPointIndex = 0;
                }
                else if (platformType == MovingPlatformType.Triggered)
                {
                    // Останавливаемся
                    isMoving = false;
                }
            }
        }
    }

    public void StartMoving()
    {
				if (poka_ne_bylo_triggera)
				{
						poka_ne_bylo_triggera = false;
						if (!isMoving)
        		{
        		    isMoving = true;
        		    startPosition = transform.position; // важно обновить, если платформа стояла где-то
        		    currentPointIndex = 0; // начинаем путь сначала
        		}
				}
    }
}
