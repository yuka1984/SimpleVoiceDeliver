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
        public AuthResult(bool result, string channel)
        {
            Result = result;
            Channel = channel;
        }

        /// <summary>
        ///     �F�،���
        /// </summary>
        public bool Result { get; }

        /// <summary>
        ///     �ڑ��`�����l��
        /// </summary>
        public string Channel { get; }
    }
}