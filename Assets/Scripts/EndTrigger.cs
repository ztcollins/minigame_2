using UnityEngine;

public class EndTrigger : MonoBehaviour
{
    public GameManager gameManager;

    void OnTriggerEnter (Collider other) {
        Debug.Log(other.name);
        gameManager.CompleteLevel();
    }
}
