using UnityEngine;

public class coloringCar : MonoBehaviour
{
    [Header("colors")]
    public Gradient color1;
    public Gradient color2;

    [Header("coloring the car")]
    public Material carMat;
    public SpriteRenderer carSprite;

    private void Start()
    {
        carSprite.material = new Material(carMat);

        float time = Random.Range(0f, 1f);
        carSprite.material.SetColor("_output_color_1", color1.Evaluate(time));
        carSprite.material.SetColor("_output_color_2", color2.Evaluate(time));
    }
}
