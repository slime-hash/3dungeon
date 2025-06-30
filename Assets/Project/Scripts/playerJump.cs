using System.Collections;
using UnityEngine;

public class playerJump2 : MonoBehaviour
{
    public Transform playerTransform;
    private float moveSpeed = 10f;
    public float jumpHeight = 1.5f;

    private Vector3 currentDirection = Vector3.forward;
    private bool isMoving = false;
    private bool hasFallen = false;
    private int jumpStage = 1;
    private Transform targetTile;
    private Transform currentTile;

    private Coroutine jumpCoroutine; // ссылка на текущую корутину

void Update()
{
    if (!isMoving && !hasFallen)
    {
        if (Input.GetKey(KeyCode.W)) currentDirection = Vector3.forward;
				else if (Input.GetKey(KeyCode.S)) currentDirection = Vector3.back;
        else if (Input.GetKey(KeyCode.A)) currentDirection = Vector3.left;
        else if (Input.GetKey(KeyCode.D)) currentDirection = Vector3.right;
        else return;

        playerTransform.forward = currentDirection;

				// Ищем текущую плитку
        Vector3 currentTileRayStart = playerTransform.position;
				Vector3 tileRayDir = Vector3.down;
        RaycastHit hit;
				if (Physics.Raycast(currentTileRayStart, tileRayDir, out hit, 1f) && (hit.collider.CompareTag("tile") || hit.collider.CompareTag("ice")))
        {
            currentTile = hit.transform;
						Debug.Log(currentTile);
        }

				Vector3 rayStart = playerTransform.position;
        float rayLength = 1.5f;
        Vector3 forwardRayDir = playerTransform.forward;
				
				// Vase1 check 
        if (Physics.Raycast(rayStart, forwardRayDir, out hit, rayLength))
        {	
						targetTile = hit.transform;
						if (hit.collider.CompareTag("vase"))
						{
            		jumpCoroutine = StartCoroutine(BumpIntoVaseAndPause(hit.collider.gameObject, currentDirection));
								return;
						}
						else if (hit.collider.CompareTag("wall"))
						{
            		jumpCoroutine = StartCoroutine(BumpIntoWallLoop(currentDirection, true));
            		return;
						}
				}
				
        Vector3 tileRayStartCenter = playerTransform.position + currentDirection;
        Vector3 tileRayStartLeft = tileRayStartCenter - Vector3.Cross(Vector3.up, currentDirection) * 0.4f; // 0.2f - смещение влево
        Vector3 tileRayStartRight = tileRayStartCenter + Vector3.Cross(Vector3.up, currentDirection) * 0.4f; // 0.2f - смещение вправо

        //if (Physics.Raycast(tileRayStartCenter, tileRayDir, out hit, 1f) || Physics.Raycast(tileRayStartLeft, tileRayDir, out hit, 1f) || Physics.Raycast(tileRayStartRight, tileRayDir, out hit, 1f))
        if (Physics.Raycast(tileRayStartCenter, tileRayDir, out hit, 1f))
        {
            targetTile = hit.transform;
            if (hit.collider.CompareTag("tile"))
            {
                // >>>> Запускаем корутину прыжка
                jumpCoroutine = StartCoroutine(StepByStepJumpLoop(currentDirection));
								return;
            }
						if (hit.collider.CompareTag("ice"))
						{
                // >>>> Запускаем корутину скольжения
								jumpCoroutine = StartCoroutine(SlideOnIce(currentDirection));
								return;
						}

        }
				//добавить дополнительно очень короткую паузу чтобы было сложнее случайно упасть в бездну 
				//после паузы если убрал кнопку - return, если не убрал кнопку:
        hasFallen = true;
				Debug.Log("Fall");
				//StartCoroutine(FallIntoAbyss(currentDirection));
				return;
    }
}
    private IEnumerator JumpProcess()
    {
				moveSpeed = 15f;
      	while (jumpStage != 3)
        {
            Vector3 updatedTargetPos = Vector3.zero;

            switch (jumpStage)
            {
                case 1: // ДВИЖЕНИЕ В СТОРОНУ И ВВЕРХ
                    {
                      	if (playerTransform.parent != targetTile.parent)
                      	{
                      	    playerTransform.SetParent(targetTile.parent);
                      	}												
                        updatedTargetPos = targetTile.position + Vector3.up * jumpHeight;
                        playerTransform.position = Vector3.MoveTowards(playerTransform.position, updatedTargetPos, moveSpeed * Time.deltaTime);

                        if (Vector3.Distance(playerTransform.position, updatedTargetPos) < 0.01f)
                        {
                            jumpStage = 2;
                        }
                    }
                    break;

                case 2: // СПУСК ВНИЗ
                    {
                        updatedTargetPos = targetTile.position + Vector3.up;
                        playerTransform.position = Vector3.MoveTowards(playerTransform.position, updatedTargetPos, moveSpeed * Time.deltaTime);

                        if (Vector3.Distance(playerTransform.position, updatedTargetPos) < 0.01f)
                        {
                            // Жестко фиксируем финальную позицию
                            playerTransform.position = updatedTargetPos;
                            //isMoving = false; -- обнулим когда будем выходить в update()
														if (Physics.Raycast(playerTransform.position, Vector3.down, out RaycastHit hit, 1f) && (hit.collider.CompareTag("tile") || hit.collider.CompareTag("ice")))
		        								{
		        								    currentTile = hit.transform;
		        								}
                            jumpStage = 3;
                        }
                    }
                    break;
            }
            yield return null; // Ждем следующий кадр
        }
    }

private IEnumerator BumpProcess()
{
		moveSpeed = 10f;
    while (jumpStage != 3)
    {
        Vector3 updatedTargetPos = Vector3.zero;

        switch (jumpStage)
        {
            case 1: // Подпрыгиваем к препятствию

                updatedTargetPos = targetTile.position + Vector3.up * 0.9f;
                playerTransform.position = Vector3.MoveTowards(playerTransform.position, updatedTargetPos, moveSpeed * Time.deltaTime);

                if (Vector3.Distance(playerTransform.position, updatedTargetPos) < 0.8f)
                {
                    jumpStage = 2;
                }
                break;

            case 2: // Возвращаемся на исходную плитку

                updatedTargetPos = currentTile.position + Vector3.up;
                playerTransform.position = Vector3.MoveTowards(playerTransform.position, updatedTargetPos, moveSpeed * Time.deltaTime);

                if (Vector3.Distance(playerTransform.position, updatedTargetPos) < 0.01f)
                {
                    playerTransform.position = updatedTargetPos;
                    jumpStage = 3;
                }
                break;
        }

        yield return null;
    }
}

    IEnumerator MoveToPosition(Vector3 target, float duration)
    {
        Vector3 start = playerTransform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            playerTransform.position = Vector3.Lerp(start, target, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerTransform.position = target;
    }

		IEnumerator BumpIntoWallLoop(Vector3 direction, bool withPause)
		{
				isMoving = true;
				jumpStage = 1;
		    yield return StartCoroutine(BumpProcess());

				if (withPause) yield return StartCoroutine(WaitWithCancel(direction));
				
				while (IsDirectionKeyHeld(direction))
    		{
						Vector3 rayStart = playerTransform.position;
        		Vector3 forwardRayDir = playerTransform.forward;

						if (Physics.Raycast(rayStart, forwardRayDir, out RaycastHit hit, 1.5f) && (hit.collider.CompareTag("wall")))
        		{	
								targetTile = hit.transform;												
								jumpStage = 1;
		    				yield return StartCoroutine(BumpProcess());
								yield return StartCoroutine(WaitWithCancel(direction, 4));
								continue;
						}		
						else 
						{
								isMoving = false;
								yield break;
						}
				}

				isMoving = false;
		}

		IEnumerator StepByStepJumpLoop(Vector3 direction)
		{
				isMoving = true;
				jumpStage = 1;
				yield return StartCoroutine(JumpProcess());

				yield return StartCoroutine(WaitWithCancel(direction, 5));

				while (IsDirectionKeyHeld(direction))
    		{
						Vector3 rayStart = playerTransform.position;
        		Vector3 forwardRayDir = playerTransform.forward;
						Vector3 tileRayStart = playerTransform.position + currentDirection;
						Vector3 tileRayDir = Vector3.down;
						if (Physics.Raycast(rayStart, forwardRayDir, out RaycastHit hit, 1.5f) && (hit.collider.CompareTag("wall")))
						{
								jumpStage = 1;
		    				yield return StartCoroutine(BumpIntoWallLoop(direction, false));
								//yield return StartCoroutine(WaitWithCancel(direction, 40)); почему то не работает
						}
						else if (Physics.Raycast(rayStart, forwardRayDir, out hit, 1.5f) && (hit.collider.CompareTag("vase")))
						{
								/*
		    				yield return StartCoroutine(BumpIntoVaseAndPause(hit.collider.gameObject, currentDirection));
								*/
								isMoving = false;
								yield break;
						}
            else if (Physics.Raycast(tileRayStart, tileRayDir, out hit, 1f))
            {
                if (hit.collider.CompareTag("tile"))
                {
										// Ищем следующую плитку
                    targetTile = hit.transform;
								}
								else if (hit.collider.CompareTag("ice"))
								{
										yield return StartCoroutine(SlideOnIce(direction));
										yield break;
								}
                tileRayStart = playerTransform.position;
                if (Physics.Raycast(tileRayStart, tileRayDir, out hit, 1f) && hit.collider.CompareTag("tile"))
                {
                		// Ищем текущую плитку
                    currentTile = hit.transform;
										jumpStage = 1;
										yield return StartCoroutine(JumpProcess());
                }
						}
						else
						{
								hasFallen = true; // персонаж упал в бездну
								isMoving = false;
								Debug.Log("Fall");
								yield break;
						}
				}
				isMoving = false;
		}


		bool IsDirectionKeyHeld(Vector3 dir)
		{
		    if (dir == Vector3.forward) return Input.GetKey(KeyCode.W);
		    if (dir == Vector3.back) return Input.GetKey(KeyCode.S);
		    if (dir == Vector3.left) return Input.GetKey(KeyCode.A);
		    if (dir == Vector3.right) return Input.GetKey(KeyCode.D);
		    return false;
		}

		IEnumerator WaitWithCancel(Vector3 direction, int steps = 10, float interval = 0.02f)
		{
		    while (steps > 0)
		    {
		        yield return new WaitForSeconds(interval);
		        if (!IsDirectionKeyHeld(direction))
		        {
		            isMoving = false;
		            yield break;
		        }
						//Debug.Log(steps);
		        steps--;
		    }
		}
		
		private IEnumerator BumpIntoVaseAndPause(GameObject vase, Vector3 direction, bool doBump = true)
		{
		    isMoving = true;
				jumpStage = 1;

				if (doBump)
				{
		    		yield return StartCoroutine(BumpProcess());
				}
    		// Запускаем анимацию трещины у вазы
    		Vase vaseScript = vase.GetComponent<Vase>();
    		if (vaseScript != null)
    		{
    		    yield return StartCoroutine(vaseScript.Break(direction));
    		}

		    int steps = 10;
		    while (steps > 0)
		    {
		        yield return new WaitForSeconds(0.02f);
		        if (!IsDirectionKeyHeld(direction))
		        {
		            isMoving = false;
		            yield break;
		        }
		        steps--;
		    }
		
		    isMoving = false;
		}

		IEnumerator SlideOnIce(Vector3 direction)
		{
				isMoving = true;

				Vector3 nextPos = playerTransform.position + direction;
				//скользим только на первую плитку льда и останавливаемся на ее середине
        yield return StartCoroutine(MoveToPosition(nextPos, 0.08f));
				RaycastHit hit;
				//если, стоя на середине первой плитки ice, оказалась стена/ваза то делаем bumpintowall
				
				// Ищем текущую плитку
				if (Physics.Raycast(playerTransform.position, Vector3.down, out hit, 1f) && (hit.collider.CompareTag("tile") || hit.collider.CompareTag("ice")))
        {
            currentTile = hit.transform;
        }
				
				if (Physics.Raycast(playerTransform.position, playerTransform.forward, out hit, 1f))
				{
				    if (hit.collider.CompareTag("wall"))
				    {
				        Debug.Log("WallOnIce");
				        yield return StartCoroutine(WaitWithCancel(direction, 8));
				        isMoving = false;
				        yield break;
				    }
				    if (hit.collider.CompareTag("vase"))
				    {
				        Debug.Log("VaseOnIce");

				        yield return StartCoroutine(BumpIntoVaseAndPause(hit.collider.gameObject, direction, false));
				        yield break;
				    }
				}

				//если после первой плитки ice нету стены, то там или tile или ice, значит скользим туда
				while (Physics.Raycast(playerTransform.position, Vector3.down, out hit, 1f) && (hit.collider.CompareTag("ice")))
				{
						nextPos = playerTransform.position + direction;
						yield return StartCoroutine(MoveToPosition(nextPos, 0.08f));
						if (Physics.Raycast(playerTransform.position, Vector3.down, out hit, 1f) && (hit.collider.CompareTag("tile") || hit.collider.CompareTag("ice")))
        		{
        		    currentTile = hit.transform;
        		}
						if (Physics.Raycast(playerTransform.position, playerTransform.forward, out hit, 1f))
						{
						    if (hit.collider.CompareTag("wall"))
						    {
						        Debug.Log("WallOnIce");
						        yield return StartCoroutine(WaitWithCancel(direction, 8));
						        isMoving = false;
						        yield break;
						    }
						    if (hit.collider.CompareTag("vase"))
						    {
						        Debug.Log("VaseOnIce");
						        yield return StartCoroutine(BumpIntoVaseAndPause(hit.collider.gameObject, direction, false));
						        yield break;
						    }
						}
						if (Physics.Raycast(playerTransform.position, Vector3.down, out hit, 1f) && (hit.collider.CompareTag("tile")))
						{
								isMoving = false;
								yield break;
						}
						else if (!Physics.Raycast(playerTransform.position, Vector3.down, out hit, 1f))
						{
								Debug.Log("Fall");
								hasFallen = true;
								yield break;
						}
				}
		}
}
