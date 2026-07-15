using UnityEngine;

public class fruiteBox : MonoBehaviour
{
    [Header("cutting particals")]
    public ParticleSystemRenderer cutPrefab;
    public Material cutMat;
    public SpriteRenderer box;

    public void cut(float angle)
    {
        ParticleSystemRenderer rightRenderer = PoolManager.SpawnObject(cutPrefab, transform.position, Quaternion.Euler(0, 0, angle));
        ParticleSystemRenderer leftRenderer = rightRenderer.transform.GetChild(0).GetComponent<ParticleSystemRenderer>();

        Material rightMat = new Material(cutMat);
        rightMat.SetFloat("_angle", angle);
        rightMat.SetTexture("_MainTex", box.sprite.texture);
        Material leftMat = new Material(rightMat);
        leftMat.SetFloat("_angle", -angle);

        rightRenderer.material = rightMat;
        leftRenderer.material = leftMat;

        PoolManager.ReturnToPool(gameObject);
    }
}
