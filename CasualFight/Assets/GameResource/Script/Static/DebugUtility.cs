using UnityEngine;
namespace AbubuResouse.Log
{
    public static class DebugUtility
    {
        //エディタ上でのみデバッグログを表示するメソッド
        public static void Log(string message)
        {
#if UNITY_EDITOR
            Debug.Log(message);
#endif
        }
        // エディタ上でのみエラーログを表示するメソッド
        public static void LogError(string message)
        {
#if UNITY_EDITOR
            Debug.LogError(message);
#endif
        }
        public static void LogWarning(string message)
        {
#if UNITY_EDITOR
            Debug.LogWarning(message);
#endif
        }
    }
}