using System.Collections;
using UnityEngine;

public class Robot : MonoBehaviour
{
    [Header("Refrences")]
    public Transform[] FootPlaces;
    public Transform[] Targets;

    [Header("Taking steps")]
    public float MaxDistance;
    public float MaxTime;

    public enum SteppingMode { Timer, Distance, Hybrid }
    public SteppingMode _SteppingMode;

    public enum StepMotion { Linear, Circler}
    public StepMotion StepPath;
    public float StepAmplitude;
    public float StepYOffset;

    float[] Timers;
    bool[] Palanced;

    public float FramesToStep;
    public AnimationCurve StepSpeedMultiplayer;

    [Header("Checking for placment")]
    public float AdditonalY;

    public LayerMask WalkableMask;

    RaycastHit[] hit;

    [Header("Model Changes")]
    public bool positionModel;

    public float ModelY;

    [Header("Gizmos")]
    public bool DrawFootPlaces;
    public bool DrawFeetPlacementRay;

    bool started = false;

    private void Start()
    {
        started = true;

        Palanced = new bool[Targets.Length];

        hit = new RaycastHit[Targets.Length];

        Timers = new float[Targets.Length];
        for (int i = 0; i < Targets.Length; i++)
        {
            Timers[i] = i * MaxTime/2;
        }
    }

    private void Update()
    {
        Vector3 FPSum = Vector3.zero;

        for (int i = 0; i < Targets.Length; i++)
        {
            //checking for palance
            if (_SteppingMode == SteppingMode.Distance)
            {
                if (DistanceCheck(i)) { Palanced[i] = false; }
            }
            else if (_SteppingMode == SteppingMode.Timer)
            {
                if (TimeCheck(i)) { Palanced[i] = false; }
            }
            else
            {
                if (TimeCheck(i) && DistanceCheck(i)) { Palanced[i] = false; }
            }

            //trying to step if not palanced
            if (Palanced[i] == false)
            {
                StartCoroutine(Step(i));
            }

            //placing the limb on the ground
            if (Physics.Raycast(FootPlaces[i].position + new Vector3(0,AdditonalY,0), Vector3.down, out hit[i], int.MaxValue, WalkableMask) == true)
            {
                FootPlaces[i].position = hit[i].point;
            }

            //positioning model
            if (positionModel == true)
            {
                FPSum += FootPlaces[i].position;
                if (i+1 == Targets.Length)
                {
                    transform.position += Vector3.up * (FPSum/FootPlaces.Length + Vector3.up*(ModelY - transform.position.y)).y;
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        for (int i = 0; i < FootPlaces.Length; i++)
        {
            if (DrawFootPlaces == true)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(FootPlaces[i].position, 0.4f);
            }
            if (DrawFeetPlacementRay == true && started == true)
            {
                Gizmos.color = Color.red;
                if (hit[i].collider != null)
                {
                    Vector3 org = FootPlaces[i].position + new Vector3(0, AdditonalY, 0);
                    Gizmos.DrawRay(org, Vector3.down * Vector3.Distance(org, hit[i].point));
                }
            }
        }
    }

    bool DistanceCheck(int i)
    {
        float distance = Vector3.Distance(Targets[i].position, FootPlaces[i].position);
        return distance > MaxDistance && Palanced[i] == true;
    }
    bool TimeCheck(int i)
    {
        Timers[i] += Time.deltaTime;
        return Timers[i] >= Mathf.Ceil((Timers[i] - Time.deltaTime) / MaxTime) * MaxTime && Palanced[i] == true;
    }
    Vector3 LerpStep(Vector3 start, Vector3 end, float t)
    {
        return Vector3.LerpUnclamped(start, end, t);
    }
    Vector3 SlerpStep(Vector3 start, Vector3 end, float t, float centerOffset, float amp)
    {
        Vector3 Center = Vector3.Lerp(start, end, 0.5f) + (centerOffset * Vector3.up);

        Vector3 slerp = Vector3.SlerpUnclamped(start - Center, end - Center, t) + Center;

        return new Vector3(slerp.x, slerp.y * amp, slerp.z);
    }
    IEnumerator Step(int i)
    {
        Palanced[i] = true;
        Vector3 Start = Targets[i].position;

        for (float j = 0; j <= 1; j += 1f / FramesToStep)
        {
            if (StepPath == StepMotion.Linear)
            {
                Targets[i].position = LerpStep(Start, FootPlaces[i].position, StepSpeedMultiplayer.Evaluate(j));
            }
            else
            {
                Targets[i].position = SlerpStep(Start, FootPlaces[i].position, StepSpeedMultiplayer.Evaluate(j), StepYOffset, StepAmplitude);
            } 
            yield return null;
        }
    }
}

