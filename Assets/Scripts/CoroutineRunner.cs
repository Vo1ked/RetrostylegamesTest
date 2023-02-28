using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    public Coroutine RunCoroutine(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }

    public void StopRunningCoroutine(Coroutine coroutine)
    {
        StopCoroutine(coroutine);
    }
}
