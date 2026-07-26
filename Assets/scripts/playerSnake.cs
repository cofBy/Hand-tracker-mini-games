using UnityEngine;

public class playerSnake : MonoBehaviour
{
    [Header("movement")]
    public sentisHandTracker handTracker;
    public Rigidbody2D rb;
    public float speed;

    [Header("camera movement")]
    public Camera cam;
    public float followTime;
    Vector2 vel;

    public float lookAhead;

    private void Update()
    {
        handleCamera();
    }
    private void FixedUpdate()
    {
        Vector2 dir = handTracker.palmCenter() - rb.position;
        rb.linearVelocity = dir.normalized * speed;
    }

    void handleCamera()
    {
        Vector2 target = (Vector2)transform.position + (rb.linearVelocity.normalized * lookAhead);
        cam.transform.position = (Vector3)Vector2.SmoothDamp(cam.transform.position, target, ref vel, followTime) + new Vector3(0, 0, -10f);
    }
}
