using Photon.Pun;
using UnityEngine;

public class SpeedUp : MonoBehaviourPunCallbacks
{
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            this.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
