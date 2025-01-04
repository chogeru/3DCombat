using UnityEngine;
using AbubuResouse.Log;


namespace AbubuResouse.Singleton
{
    /// <summary>
    /// シングルトンパターンを実装するための抽象クラス
    /// </summary>
    /// <typeparam name="T">シングルトンとして使用するクラスの型</typeparam>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {

        private static T _instance;

        /// <summary>
        /// シングルトンのインスタンスを取得
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        DebugUtility.LogError($"{typeof(T).Name}のインスタンスがシーンに存在しない！");
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// オブジェクトの初期化時に呼び出される
        /// シングルトンのインスタンスを設定し、オブジェクトが破棄されないようにする
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = (T)this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                DebugUtility.Log($"{typeof(T).Name}のインスタンスがすでに存在している！");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// オブジェクトが破棄される際に呼び出す
        /// インスタンスをクリアする
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                DebugUtility.Log($"{typeof(T).Name}のインスタンスを破棄！");
            }
        }
    }
}