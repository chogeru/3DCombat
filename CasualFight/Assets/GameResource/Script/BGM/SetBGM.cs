using AbubuResouse.Singleton;
using UnityEngine;

namespace AbubuResouse
{
    /// <summary>
    /// BGMセット用クラス
    /// </summary>
    public class SetBGM : MonoBehaviour
    {
        [SerializeField, Header("開始時にセットするBGM")]
        private string m_BGMName;
        [SerializeField, Header("音量")]
        private float m_Volume;
        [SerializeField, Header("ランダムBGM")]
        private string[] m_RandomBGMNames;
        [SerializeField, Header("ランダム再生をオンにするかどうか")]
        private bool isRondomBGM;


        private void Start()
        {
            Initialization();
        }
        private void Update()
        {
            if (isRondomBGM)
            {
                RandomBGM();
            }
        }

        /// <summary>
        /// 初期設定
        /// </summary>
        private void Initialization()
        {
            if (BGMManager.Instance != null)
            {
                BGMManager.Instance.GetComponent<AudioSource>().clip = null;
                if (!string.IsNullOrEmpty(m_BGMName))
                {
                    BGMManager.Instance.PlaySound(m_BGMName, m_Volume);
                }
            }
        }

        /// <summary>
        /// ランダムBGMの再生処理を行う
        /// </summary>
        private void RandomBGM()
        {
            if (BGMManager.Instance != null)
            {
                AudioSource audioSource = BGMManager.Instance.GetComponent<AudioSource>();
                audioSource.loop = false;
                if (!audioSource.isPlaying)
                {
                    var index = Random.Range(0, m_RandomBGMNames.Length);
                    BGMManager.Instance.PlaySound(m_RandomBGMNames[index], m_Volume);
                }
            }
        }
    }
}