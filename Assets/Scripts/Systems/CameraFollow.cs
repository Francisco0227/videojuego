using UnityEngine;

// Esta clase hace que la cámara siga al jugador suavemente
public class CameraFollow : MonoBehaviour
{
    // "public" para poder arrastrarlo desde el Inspector de Unity
    public Transform target;         // El objeto al que seguirá (el jugador)
    public float smoothSpeed = 5f;   // Qué tan suave es el seguimiento
    public Vector3 offset;           // Distancia entre la cámara y el jugador

    // LateUpdate se ejecuta DESPUÉS de Update
    // Así nos aseguramos que el jugador ya se movió antes de mover la cámara
    void LateUpdate()
    {
        // Si no hay target asignado, no hacemos nada (evita errores)
        if (target == null) return;

        // Calculamos dónde queremos que esté la cámara
        // La posición del jugador + el offset que configuremos
        Vector3 desiredPosition = target.position + offset;

        // Lerp = Linear Interpolation = movimiento suave entre dos puntos
        // Mueve la cámara desde donde está (transform.position)
        // hacia donde queremos (desiredPosition)
        // a una velocidad de smoothSpeed * Time.deltaTime
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        // Aplicamos la nueva posición a la cámara
        transform.position = smoothedPosition;
    }
}