using UnityEngine;

public class fruiteBox : MonoBehaviour
{
    [Header("cutting particals")]
    public ParticleSystem cutPrefab;
    public Material cutMat;
    public SpriteRenderer box;

    public void cut(float angle)
    {
        ParticleSystem rightCut = PoolManager.SpawnObject(cutPrefab, transform.position, Quaternion.Euler(0, 0, angle));
        ParticleSystem leftCut = rightCut.transform.GetChild(0).GetComponent<ParticleSystem>();
        ParticleSystem.MainModule rightMain = rightCut.main;
        ParticleSystem.MainModule leftMain = leftCut.main;

        rightMain.startColor = box.color;
        leftMain.startColor = box.color;

        Material mat = new Material(cutMat);
        mat.SetFloat("_angle", angle);
        mat.SetColor("_Color", box.color);

        ParticleSystemRenderer rightRenderer = rightCut.GetComponent<ParticleSystemRenderer>();
        ParticleSystemRenderer leftRenderer = leftCut.GetComponent<ParticleSystemRenderer>();
        rightRenderer.material = mat;
        leftRenderer.material = mat;

        PoolManager.ReturnToPool(gameObject);
    }
}
