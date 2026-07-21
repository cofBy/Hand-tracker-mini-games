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

    [Header("bumping")]
    public CapsuleCollider2D col;
    public float checkLength;
    public float strength;

    [Header("scoring points")]
    public float resetTime;
    float resetTimer;

    [HideInInspector] public scoreManager scoringSystem;

    [Header("getting closest car")]
    public List<Rigidbody2D> cars;
    Vector2 target;

    public float maxRange;
    
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

        resetTimer = Mathf.Max(resetTimer - Time.deltaTime, 0);

        handlBumping();
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
        if (Vector2.Distance(closestCar.transform.position, transform.position) < maxRange)
        {
            target = closestCar.transform.position;
        }
        else
        {
            float x = Random.Range(-9f, 9f);
            float y = Random.Range(-9f, 9f);
            target = new Vector2(x, y);
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target);
    }

    void handlBumping()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + col.size.x * transform.right, rb.linearVelocity.normalized, checkLength);

        if (hit)
        {
            if (resetTimer <= 0 && hit.collider.gameObject.CompareTag("Player"))
            {
                resetTimer = resetTime;
             
                scoringSystem.score -= 1;
            }

            Vector2 normal = new Vector2(-hit.normal.y, hit.normal.x).normalized;
            Vector2 bounceDir = Vector2.Reflect(rb.linearVelocity, normal);

            Debug.DrawRay(hit.point, normal * 50, Color.blue);
            Debug.DrawRay(transform.position, rb.linearVelocity.normalized * 50, Color.yellow);
            Debug.DrawRay(transform.position, bounceDir * 50, Color.green);

            rb.linearVelocity = bounceDir.normalized * -strength;
            hit.collider.attachedRigidbody.linearVelocity = Vector2.zero;
        }
    }
}
