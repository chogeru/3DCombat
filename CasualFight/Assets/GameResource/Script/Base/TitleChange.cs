using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class TitleChange : MonoBehaviour
{
    [Header("移動先のシーン名")]
    [SerializeField]
    string m_SceneName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// シーン変更処理（ボタン用）
    /// </summary>
    public void ChangeScene()
    {
        if (!string.IsNullOrEmpty(m_SceneName))
        {
            SceneManager.LoadScene(m_SceneName);
        }
        else
        {
            Debug.LogWarning("Scene Name is empty in TitleChange script.");
        }
    }
}
