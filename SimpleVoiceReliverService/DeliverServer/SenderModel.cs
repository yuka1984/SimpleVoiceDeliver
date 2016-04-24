namespace DeliverServer
{
    /// <summary>
    /// ���M�f�[�^���f��
    /// </summary>
    public class SenderModel
    {
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="channel">�`�����l��</param>
        /// <param name="isBinary">�o�C�i���f�[�^�t���O</param>
        /// <param name="receiveData">��M�f�[�^</param>
        /// <param name="isClose"></param>
        public SenderModel(string channel, bool isBinary, byte[] receiveData, bool isClose = false)
        {
            Channel = channel;
            IsBinary = isBinary;
            ReceiveData = receiveData;
            IsClose = isClose;
        }

        /// <summary>
        /// �ؒf�t���O
        /// </summary>
        public bool IsClose { get; }

        /// <summary>
        /// �o�C�i���f�[�^�t���O
        /// </summary>
        public bool IsBinary { get; }

        /// <summary>
        /// �`�����l������
        /// </summary>
        public string Channel { get;  }

        /// <summary>
        /// ��M�f�[�^
        /// </summary>
        public byte[] ReceiveData { get; }
    }
}