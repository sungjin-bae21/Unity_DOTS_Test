using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientObjectActiveController : MonoBehaviour
{
    void Start()
    {
        if (NetworkUtility.GetServerWorld() != null)
        {
#if UNITY_EDITOR
#else
            gameObject.SetActive(false);
#endif
        }
    }
}
