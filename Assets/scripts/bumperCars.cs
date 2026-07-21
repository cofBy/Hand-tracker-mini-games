using System.Collections.Generic;
using UnityEngine;

public class bumperCars : MonoBehaviour
{
    [Header("spawning enemy cars")]
    public Rigidbody2D enemyCar;
    public int enemyCarsAmount;

    public Rigidbody2D playerCar;
    List<Rigidbody2D> cars = new List<Rigidbody2D>();

    [Header("moving camera")]
    public Camera cam;
    public float followTime;
    Vector2 vel;

    public float lookAhead;

    [Header("score")]
    public scoreManager scoringSystem;
    private void Start()
    {
        cars.Add(playerCar);
        for (int i = 0; i < enemyCarsAmount; i++)
        {
            spawnCars();
        }
        foreach (Rigidbody2D car in cars)
        {
            if (car == playerCar) continue;
            enemyCarMovement carMovement = car.gameObject.GetComponent<enemyCarMovement>();
            carMovement.cars = cars;
            carMovement.scoringSystem = scoringSystem;
        }
    }
    private void Update()
    {
        if (scoringSystem.timer > 0)
        {
            handleCamera();
        }
        else
        {
            for (int i = 0; i < cars.Count; i++)
            {
                Destroy(cars[i].gameObject);
            }
            cars.Clear();
        }
    }

    void spawnCars()
    {
        float x = Random.Range(-9f, 9f);
        float y = Random.Range(-9f, 9f);

        Rigidbody2D carInstance = PoolManager.SpawnObject(enemyCar, new Vector2(x, y), Quaternion.identity);
        cars.Add(carInstance);
    }

    void handleCamera()
    {
        Vector2 target = playerCar.transform.position + (playerCar.transform.right.normalized * lookAhead);
        cam.transform.position = (Vector3)Vector2.SmoothDamp(cam.transform.position, target, ref vel, followTime) + new Vector3(0, 0, -10f);
    }
}
