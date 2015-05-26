using UnityEngine;
using System.Collections;

public class DamageDealer : MonoBehaviour {

    public float Damage = 5;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Player>() != null)
        {
            other.gameObject.GetComponent<Player>().DoDamage(Damage);
        }

    }
}
