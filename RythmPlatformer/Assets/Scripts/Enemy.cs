using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Debug.Log("Choco al Player!");
            //this.gameObject.SetActive(false);
        }
    }

    public void DeactivateEnemy(float timeTodisappear)
    {
        StartCoroutine(DelayedDeactivate(timeTodisappear));        
    }

    private IEnumerator DelayedDeactivate(float dealy)
    {
        yield return new WaitForSeconds(dealy);
        this.gameObject.SetActive(false);
    }
}
