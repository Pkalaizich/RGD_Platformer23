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
            StartCoroutine(DelayedDeactivate(0));
            other.GetComponent<CharacterMovementRb>().EnemyCollision();
        }
    }

    public void DeactivateEnemy(float timeTodisappear)
    {
        StartCoroutine(DelayedDeactivate(timeTodisappear));        
    }

    private IEnumerator DelayedDeactivate(float dealy)
    {
        sphCollider.enabled = false;
        yield return new WaitForSeconds(dealy);
        anim.SetTrigger("Destroyed");
        yield return new WaitForSeconds(2f);
        this.gameObject.SetActive(false);
    }
}
