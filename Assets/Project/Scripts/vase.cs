using UnityEngine;
using System.Collections;

public class Vase : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer mesh;
    [SerializeField] int crackedBlend = 0;
		[SerializeField] float tiltAngle = 10f;      // градусов
    [SerializeField] float tiltTime  = 1f;    // секунд

    [SerializeField] bool broken = false;

    public IEnumerator Break(Vector3 direction)
    {
        broken = true;

				 // ---------- 1. короткий наклон ----------
        Quaternion startRot  = transform.localRotation;
        Vector3 tiltAxis     = Vector3.Cross(Vector3.up, direction).normalized;  // ось, перпендикулярная удару
        Quaternion targetRot = Quaternion.AngleAxis(tiltAngle, tiltAxis) * startRot;

        // вперёд
        for (float t = 0; t < 1f; t += Time.deltaTime / tiltTime)
        {
            transform.localRotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }
        transform.localRotation = targetRot;

				// Мгновенно включаем трещину
				mesh.SetBlendShapeWeight(crackedBlend, 100);

        // назад
        for (float t = 0; t < 1f; t += Time.deltaTime / tiltTime)
        {
            transform.localRotation = Quaternion.Lerp(targetRot, startRot, t);
            yield return null;
        }
        transform.localRotation = startRot;


        gameObject.tag = "Untagged";
    }

    void OnTriggerEnter(Collider other)
    {
        if (!broken) return;
        if (other.CompareTag("Player"))
            Destroy(gameObject);
    }
}
