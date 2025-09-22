using Photon.Pun;
using UnityEngine;

public class PowerUp : MonoBehaviourPunCallbacks
{
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            this.gameObject.SetActive(false);
        }
    }
}
