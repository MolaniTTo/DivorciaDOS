using UnityEngine;

public class FloatAndRotate : MonoBehaviour
{
    // Par�metros de flotaci�n
    public float floatAmplitude = 0.5f;  // Cu�nto se mueve arriba/abajo
    public float floatFrequency = 1f;    // Qu� tan r�pido flota

    // Par�metros de rotaci�n
    public Vector3 rotationSpeed; // Velocidad de rotaci�n por eje

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;

        // Asignar velocidad de rotaci�n aleatoria si no se configura manualmente
        if (rotationSpeed == Vector3.zero)
        {
            rotationSpeed = new Vector3(
                Random.Range(-30f, 30f),
                Random.Range(-30f, 30f),
                Random.Range(-30f, 30f)
            );
        }

        // Tambi�n puedes aleatorizar amplitud y frecuencia si quieres m�s variedad
        floatAmplitude *= Random.Range(0.8f, 1.2f);
        floatFrequency *= Random.Range(0.8f, 1.2f);
    }

    void Update()
    {
        // Movimiento de flotaci�n
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotaci�n
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
