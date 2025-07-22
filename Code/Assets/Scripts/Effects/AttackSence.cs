using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSence : MonoBehaviour
{
    private static AttackSence instance;
    public static AttackSence Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<AttackSence>();
            return instance;
        }
    }
    private bool isShake;
    public void HitPause(int duration)
    {
        StartCoroutine(Pause(duration));
    }
    IEnumerator Pause(int duration)
    {
        float pauseTime = duration / 60f;
        Time.timeScale = 0; // 暂停游戏
        yield return new WaitForSecondsRealtime(pauseTime); //
        Time.timeScale = 1;                                                   //等待指定时间
    }
    public void CameraShake(float duration, float strength)
    {
        if (!isShake)
        {
            StartCoroutine(Shake(duration, strength));
        }
    }
    IEnumerator Shake(float duration, float strength)
    {
        isShake = true;
        Transform camera = Camera.main.transform;
        Vector3 startPositon = camera.position;

        while (duration > 0)
        {
            camera.position = Random.insideUnitSphere * strength + startPositon;
            duration -= Time.deltaTime;
            yield return null; // 等待下一帧
        }
        camera.position = startPositon; // 恢复原位置
        isShake = false;
    }

}