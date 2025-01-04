namespace AbubuResouse.Singleton
{
    /// <summary>
    /// ゲーム全体の動作停止状態を管理するクラス
    /// </summary>
    public class StopManager : SingletonMonoBehaviour<StopManager>
    {
        private bool isStopped;
        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }
    }
}
