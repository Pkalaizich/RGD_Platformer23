using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator anim;
    private SphereCollider sphCollider;

    private void Awake()
    {
        sphCollider = GetComponent<SphereCollider>();
        anim = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Debug.Log("Choco al Player!");
            anim.SetTrigger("Destroyed");
            sphCollider.enabled = false;
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
