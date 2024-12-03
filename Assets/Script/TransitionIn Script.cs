using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TransitionIn : MonoBehaviour
{
    [SerializeField]
    private Material _transitionIn;
    [SerializeField]
    private float transitionTime;

    void Start()
    {
        StartCoroutine( BeginTransition() );
    }

    IEnumerator BeginTransition()
    {
        yield return Animate( _transitionIn, transitionTime );
    }

    /// <summary>
    /// time秒かけてトランジションを行う
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    
    IEnumerator Animate(Material material, float time)
    {
        GetComponent<Image>().material = material;
        float current = 0;
        while (current < time) {
            material.SetFloat( "_Alpha", current / time );
            yield return new WaitForEndOfFrame();
            current += Time.deltaTime;
        }
        material.SetFloat( "_Alpha", 1 );
    }
}
