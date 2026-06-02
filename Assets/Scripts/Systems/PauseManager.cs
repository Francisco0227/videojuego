using UnityEngine;

public static class PauseManager
{
    private static int _count = 0;

    public static void Pause()
    {
        _count++;
        Time.timeScale = 0f;
    }

    public static void Resume()
    {
        _count = Mathf.Max(0, _count - 1);
        if (_count == 0)
            Time.timeScale = 1f;
    }

    // Llamar al volver al menú principal para limpiar el estado
    public static void ForceReset()
    {
        _count = 0;
        Time.timeScale = 1f;
    }
}
