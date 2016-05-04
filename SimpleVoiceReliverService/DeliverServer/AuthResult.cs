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
        /// <param name="type"></param>
        public AuthResult(bool result, string channel, ClientType type)
        {
            Result = result;
            Channel = channel;
            Type = type;
        }

        /// <summary>
        ///     認証結果
        /// </summary>
        public bool Result { get; }

        

        /// <summary>
        ///     接続チャンネル
        /// </summary>
        public string Channel { get; }

        /// <summary>
        /// クライアントタイプ
        /// </summary>
        public ClientType Type { get; }
    }

    /// <summary>
    /// クライアントタイプ
    /// </summary>
    public enum ClientType
    {
        Sender, Receiver
    }
}