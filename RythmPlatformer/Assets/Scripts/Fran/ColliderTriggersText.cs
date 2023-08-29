using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ColliderTriggersText : MonoBehaviour
{
    [SerializeField] TextMeshPro _TextToTrigger;
    [SerializeField] bool _TextNewState;
    [SerializeField] bool _OnceOnly;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            return;
        }
        _TextToTrigger.enabled = _TextNewState;
        if (_OnceOnly ) { Destroy(this.gameObject); }
    }
    private void OnDrawGizmos()
    {
        if (_TextNewState) { Gizmos.color = Color.green; } else { Gizmos.color = Color.red; }
        Gizmos.DrawWireCube(this.transform.position, Vector3.one);        
    }
}
