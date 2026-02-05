using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class TitleChange : MonoBehaviour
{
    [Header("移動先のシーン名")]
    [SerializeField]
    string m_SceneName;


    [Header("非表示にするキャンバスグループ")]
    [SerializeField]
    CanvasGroup m_HideCanvasGroup;

    /// <summary>
    /// シーン変更処理（ボタン用）
    /// </summary>
    public void ChangeScene()
    {
        if (!string.IsNullOrEmpty(m_SceneName))
        {
            // 既存UIを非表示にする
            if (m_HideCanvasGroup != null)
            {
                m_HideCanvasGroup.alpha = 0f;
                m_HideCanvasGroup.blocksRaycasts = false;
            }

            // タイムライン(PlayableDirector)が動いていたら停止する
            var directors = FindObjectsOfType<UnityEngine.Playables.PlayableDirector>();
            foreach (var director in directors)
            {
                if (director.state == UnityEngine.Playables.PlayState.Playing)
                {
                    director.Stop();
                }
            }

            Resources.UnloadUnusedAssets();

            // LoadingManagerを使ってロード
            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.LoadSceneAsync(m_SceneName).Forget();
            }
            else
            {
                // LoadingManagerがない場合のフォールバック
                Debug.LogWarning("TitleChange: LoadingManager.Instance is null. 使用する際はシーンにLoadingManagerを配置してください。");
                SceneManager.LoadScene(m_SceneName);
            }
        }
        else
        {
            Debug.LogWarning("Scene Name is empty in TitleChange script.");
        }
    }
}
