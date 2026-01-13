using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachineAI
{
    /// <summary>
    /// ステートの受け皿
    /// ステート(状態)の基礎骨子で、これを基にステートが運営される
    /// ステート内にある、Enter、Execute、Exitは、実装されたステートでオーバーライド(上書き)されて実行される。
    /// Enter   ステートに移行した際に最初に起動する関数。
    ///         Unityでいうなれば、Start()と同じものと考えればよい。
    /// Stay    ステートが継続している間常に実行される関数。
    ///         Unityでいうなれば、Update()と同じものと考えればよい。
    /// Exit    ステートが終了する際に実行される関数。
    ///         まぁ、C++でいうデストラクタと同じ
    /// </summary>
    public class State<T>
    {
        ///このステートを利用するインスタンス
        public T owner;

        ///コンストラクタで、ステート登録された場合、登録先のAIをオーナーとして認定する。
        ///認定されたオーナー(owner)を使って、様々な行動を処理させる事になる。
        public State(T owner)
        {
            ///コンストラクタで獲得したオーナー(owner)対象を代入し確定させる。
            this.owner = owner;
        }
    
        ///このステートに遷移する時に一度だけ呼ばれる
        ///UnityでのStart()関数と同じもの
        public virtual void Enter()
        {
        }

        ///このステートである間、毎フレーム呼ばれる
        ///UnityでのUpdate()関数と同じもの
        public virtual void Stay()
        {
        }

        ///このステートから他のステートに遷移するときに一度だけ呼ばれる
        ///C++でのディストラクタと同じもの
        public virtual void Exit()
        {
        }
    }
}
