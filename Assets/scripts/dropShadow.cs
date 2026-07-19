using UnityEngine;

public class dropShadow : MonoBehaviour
{
    [Header("spawning shadow")]
    public Vector2 offset;
    GameObject shadow;

    [Header("solid color")]
    public Material mat;

    private void Start()
    {
        shadow = new GameObject("drop shadow");

        shadow.transform.parent = transform;
        shadow.transform.localScale = Vector3.one;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        SpriteRenderer shadowRenderer = shadow.AddComponent<SpriteRenderer>();

        shadowRenderer.sprite = renderer.sprite;
        shadowRenderer.material = mat;
        shadowRenderer.sortingLayerName = renderer.sortingLayerName;
        shadowRenderer.sortingOrder = renderer.sortingOrder - 1;

        shadowRenderer.drawMode = renderer.drawMode;
        shadowRenderer.size = renderer.size;
    }
    private void Update()
    {
        shadow.transform.position = (Vector2)transform.position + offset;
        shadow.transform.rotation = transform.rotation;
    }
}
