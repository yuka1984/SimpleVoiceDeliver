namespace DeliverServer
{
    /// <summary>
    /// 送信データモデル
    /// </summary>
    public class SenderModel
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="channel">チャンネル</param>
        /// <param name="isBinary">バイナリデータフラグ</param>
        /// <param name="receiveData">受信データ</param>
        /// <param name="isClose"></param>
        public SenderModel(string channel, bool isBinary, byte[] receiveData, bool isClose = false)
        {
            Channel = channel;
            IsBinary = isBinary;
            ReceiveData = receiveData;
            IsClose = isClose;
        }

        /// <summary>
        /// 切断フラグ
        /// </summary>
        public bool IsClose { get; }

        /// <summary>
        /// バイナリデータフラグ
        /// </summary>
        public bool IsBinary { get; }

        /// <summary>
        /// チャンネル識別
        /// </summary>
        public string Channel { get;  }

        /// <summary>
        /// 受信データ
        /// </summary>
        public byte[] ReceiveData { get; }
    }
}