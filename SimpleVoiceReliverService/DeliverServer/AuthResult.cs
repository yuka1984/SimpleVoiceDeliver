namespace DeliverServer
{
    /// <summary>
    ///     認証結果モデル
    /// </summary>
    public class AuthResult
    {
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="result"></param>
        /// <param name="channel"></param>
        public AuthResult(bool result, string channel)
        {
            Result = result;
            Channel = channel;
        }

        /// <summary>
        ///     認証結果
        /// </summary>
        public bool Result { get; }

        /// <summary>
        ///     接続チャンネル
        /// </summary>
        public string Channel { get; }
    }
}