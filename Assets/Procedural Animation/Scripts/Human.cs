using UnityEngine;

public class Human : MonoBehaviour
{
    [Header("Refrences")]
    public Rigidbody rb;

    [Header("Movement")]
    public Vector3 Velocity;

    [Header("Feet positioning")]
    public Transform lFoot;
    public Transform rFoot;

    public float FeetY;
    public float jointY;

    public float FeetSpeed;

    public float width;
    public float height;

    float time;

    [Header("Gizmos")]
    public bool DrawFeet;
    private void Update()
    {
        //Feet Positioning
        float VEL = Mathf.Abs(Velocity.magnitude);

        time += Time.deltaTime * -FeetSpeed * VEL;
        if (VEL == 0)
        {
           time = 0;
        }

        float w = width * VEL;
        float h = height* VEL;

        Vector3 rFootPostion = new Vector3(0, Mathf.Sin(time) * h, Mathf.Cos(time) * w);
        Vector3 lFootPostion = new Vector3(0, Mathf.Sin(time + Mathf.PI) * h, Mathf.Cos(time + Mathf.PI) * w);

        rFoot.localPosition = rFootPostion + new Vector3(rFoot.localPosition.x, Mathf.Lerp(FeetY, jointY, VEL), 0);
        lFoot.localPosition = lFootPostion + new Vector3(lFoot.localPosition.x, Mathf.Lerp(FeetY, jointY, VEL), 0);

    }
    private void OnDrawGizmos()
    {
        if (DrawFeet == true)
        {
            Gizmos.DrawSphere(rFoot.position, 0.2f);
            Gizmos.DrawSphere(lFoot.position, 0.2f);
        }

    }
}
