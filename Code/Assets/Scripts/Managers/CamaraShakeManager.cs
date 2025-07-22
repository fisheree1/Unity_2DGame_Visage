using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class CamaraShakeManager : MonoBehaviour
{
    public static CamaraShakeManager Instance;
    [SerializeField] private float globalShakeForce =1f;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
           
        }
    }
    public void CamaraShake(CinemachineImpulseSource impulseSource)
    {
        impulseSource.GenerateImpulseWithForce(globalShakeForce);
    }
}
