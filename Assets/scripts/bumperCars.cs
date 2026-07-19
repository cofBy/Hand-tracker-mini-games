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

    private void Start()
    {
        cars.Add(playerCar);
        for (int i = 0; i < enemyCarsAmount; i++)
        {
            spawnCars();
        }
        foreach (Rigidbody2D car in cars)
        {
            if (car ==  playerCar) continue;
            car.gameObject.GetComponent<enemyCarMovement>().cars = cars;
        }
    }
    private void Update()
    {
        handleCamera();
    }

    void spawnCars()
    {
        float x = Random.Range(-10f, 10f);
        float y = Random.Range(-10f, 10f);

        Rigidbody2D carInstance = PoolManager.SpawnObject(enemyCar, new Vector2(x, y), Quaternion.identity);
        cars.Add(carInstance);
    }

    void handleCamera()
    {
        Vector2 target = playerCar.transform.position + (playerCar.transform.right.normalized * lookAhead);
        cam.transform.position = (Vector3)Vector2.SmoothDamp(cam.transform.position, target, ref vel, followTime) + new Vector3(0, 0, -10f);
    }
}
