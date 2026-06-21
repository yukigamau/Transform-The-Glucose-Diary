using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGame : MonoBehaviour
{
    public string openingScene;
    public AudioSource audioSource;
    public UIMover uiMover;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Onclick()
    {
        StartCoroutine(PlaySoundAndLoadScene());
    }

    IEnumerator PlaySoundAndLoadScene()
    {
        // 1. 播放音效
        audioSource.Play();

        // 2. 等待音效播放完毕（clip.length 是音频的长度，单位秒）
        yield return new WaitForSeconds(audioSource.clip.length);

        // 3. 执行转场
        uiMover.ifMove = true;
    }
}
