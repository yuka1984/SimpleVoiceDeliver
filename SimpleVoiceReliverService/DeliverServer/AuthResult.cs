namespace DeliverServer
{
    /// <summary>
    ///     �F�،��ʃ��f��
    /// </summary>
    public class AuthResult
    {
        /// <summary>
        ///     �R���X�g���N�^
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
        ///     �F�،���
        /// </summary>
        public bool Result { get; }

        

        /// <summary>
        ///     �ڑ��`�����l��
        /// </summary>
        public string Channel { get; }

        /// <summary>
        /// �N���C�A���g�^�C�v
        /// </summary>
        public ClientType Type { get; }
    }

    /// <summary>
    /// �N���C�A���g�^�C�v
    /// </summary>
    public enum ClientType
    {
        Sender, Receiver
    }
}