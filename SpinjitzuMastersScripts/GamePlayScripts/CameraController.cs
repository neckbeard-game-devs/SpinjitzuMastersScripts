using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera[] vrCams;
    public CinemachineTargetGroup[] camTargetGroups;
    public Transform[] camTargetsTrans;
    public Animator cutSceneCamAnim, bossBattleCam;

    public void SwitchCamTargets(int deadNinja, int player)
    {
        for (int x = 0; x < camTargetGroups.Length; x++)
        {
            camTargetGroups[x].m_Targets[deadNinja].target = camTargetsTrans[player];
        }
    }
    public void ClearCams(int cam)
    {
        vrCams[cam].gameObject.SetActive(true);
        vrCams[cam].Priority = 1;
        for (int i = 0; i < vrCams.Length; i++)
        {
            if (i != cam)
            {
                vrCams[i].Priority = 0;
                vrCams[i].gameObject.SetActive(false);
            }
        }
    }
}
