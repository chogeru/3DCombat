using UnityEngine;
using System;
using AbubuResouse.Log;

namespace AbubuResouse.Singleton
{
    /// <summary>
    /// サウンド関連の基本機能クラス
    /// </summary>
    public abstract class AudioManagerBase<T> : SingletonMonoBehaviour<T> where T : SingletonMonoBehaviour<T>
    {
        protected AudioSource audioSource;

        protected override void Awake()
        {
            base.Awake();
            audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// 指定されたサウンドクリップを再生する抽象関数
        /// </summary>
        /// <param name="clipName">サウンドクリップ名</param>
        /// <param name="volume">音量</param>
        public abstract void PlaySound(string clipName, float volume);

        /// <summary>
        /// 指定されたリソースパスのサウンドクリップをロードし、再生する
        /// </summary>
        /// <param name="resourcePath">リソースパス</param>
        /// <param name="volume">音量</param>
        protected virtual void LoadAndPlayClip(string resourcePath, float volume)
        {
            try
            {
                AudioClip clip = Resources.Load<AudioClip>(resourcePath);
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.volume = volume;
                    audioSource.Play();
                    DebugUtility.Log($"サウンドクリップを再生: {resourcePath}");
                }
                else
                {
                    DebugUtility.LogError($"サウンドファイルが見つからない: {resourcePath}");
                }
            }
            catch (Exception ex)
            {
                DebugUtility.LogError($"サウンドファイルのロード時にエラー発生: {ex.Message}");
            }
        }
    }
}
