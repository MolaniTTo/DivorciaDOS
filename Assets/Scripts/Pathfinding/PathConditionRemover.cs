using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PathConditionRemover : MonoBehaviour
{
    [Tooltip("�ndice de la PathCondition que deseas eliminar")]
    public int pathConditionIndex;

    [ContextMenu("Eliminar PathCondition")]

    public void OnTriggerEnter(Collider other) 
    {
        if ((other.CompareTag("Player1") || other.CompareTag("Player2")))
        {
            RemovePathCondition();
        }
    }
    public void RemovePathCondition()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager no encontrado.");
            return;
        }

        if (pathConditionIndex < 0 || pathConditionIndex >= GameManager.instance.pathConditions.Count)
        {
            Debug.LogError("�ndice fuera de rango. No se puede eliminar la PathCondition.");
            return;
        }

        var name = GameManager.instance.pathConditions[pathConditionIndex].pathConditionName;
        GameManager.instance.pathConditions.RemoveAt(pathConditionIndex);

        Debug.Log($"PathCondition '{name}' en posici�n {pathConditionIndex} ha sido eliminada.");
    }
}
