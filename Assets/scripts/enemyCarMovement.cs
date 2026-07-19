using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;

public class enemyCarMovement : MonoBehaviour
{
    [Header("moving")]
    public Rigidbody2D rb;
    public float linearSpeed;
    public float angularSpeed;

    public float maxSpeed;

    [Header("ticks")]
    public float secondsPerTick;
    float timer;

    [Header("getting closest car")]
    public List<Rigidbody2D> cars;
    Vector2 target;
    
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= secondsPerTick)
        {
            timer -= secondsPerTick;
            tick();
        }

        Vector2 dir = (Vector2)transform.position - target;
        rb.linearVelocity += (Vector2)transform.right * linearSpeed;
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
        rb.angularVelocity = Vector2.SignedAngle(dir.normalized, transform.right) * angularSpeed;
    }

    void tick()
    {
        Rigidbody2D closestCar = cars[Random.Range(0, cars.Count)];
        foreach (Rigidbody2D car in cars)
        {
            if (car == rb) continue;

            float distance = Vector2.Distance(car.transform.position, transform.position);
            float minDistance = Vector2.Distance(closestCar.transform.position, transform.position);
            if (distance < minDistance) closestCar = car;
        }

        target = closestCar.transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target);
    }
}
