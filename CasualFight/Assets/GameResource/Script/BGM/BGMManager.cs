using System.Collections.Generic;
using UnityEngine.SceneManagement;
using AbubuResouse.Log;
using UnityEngine;

namespace AbubuResouse.Singleton
{
    /// <summary>
    /// BGMの再生を管理するマネージャークラス
    /// </summary>
    public class BGMManager : AudioManagerBase<BGMManager>
    {
        /// <summary>
        /// BGM名とリソースパスのマッピング
        /// </summary>
        [System.Serializable]
        public class BGMEntry
        {
            public string BGMName;
            public string ResourcePath;
        }

        [Tooltip("BGMのマッピングリスト")]
        [SerializeField]
        private List<BGMEntry> bgmList = new List<BGMEntry>();

        private Dictionary<string, string> bgmDictionary;

        protected override void Awake()
        {
            base.Awake();
            InitializeBGMDictionary();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// BGMリストを辞書に初期化する
        /// </summary>
        private void InitializeBGMDictionary()
        {
            bgmDictionary = new Dictionary<string, string>();
            foreach (var bgm in bgmList)
            {
                if (!bgmDictionary.ContainsKey(bgm.BGMName))
                {
                    bgmDictionary.Add(bgm.BGMName, bgm.ResourcePath);
                }
                else
                {
                    DebugUtility.LogWarning($"BGM名が重複: {bgm.BGMName}");
                }
            }
        }

        /// <summary>
        /// シーンがロードされた際にBGMを停止する
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StopBGM();

        /// <summary>
        /// 指定されたBGM名に対応するサウンドを再生する
        /// </summary>
        /// <param name="bgmName">BGM名</param>
        /// <param name="volume">音量</param>
        public override void PlaySound(string bgmName, float volume)
        {
            if (bgmDictionary.TryGetValue(bgmName, out string resourcePath))
            {
                LoadAndPlayClip(resourcePath, volume);
            }
            else
            {
                DebugUtility.LogError($"指定されたBGM名が存在しない: {bgmName}");
            }
        }

        /// <summary>
        /// 現在再生中のBGMを停止
        /// </summary>
        public void StopBGM()
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                audioSource.clip = null;
                DebugUtility.Log("BGM停止");
            }
        }

        /// <summary>
        /// シーンロードイベントを解除
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
