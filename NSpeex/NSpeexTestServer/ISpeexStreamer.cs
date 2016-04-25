using System.ServiceModel;

namespace NSpeexTestServer
{
    [ServiceContract]
    public interface ISpeexStreamerUp
    {
        [OperationContract(IsOneWay = true)]
        void Publish(byte[] data);
    }

    [ServiceContract(CallbackContract = typeof(ISpeexStreamerCallback))]
    public interface ISpeexStreamer : ISpeexStreamerUp
    {
        [OperationContract]
        void Subscribe();
    }

    public interface ISpeexStreamerCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnPublish(byte[] data);
    }

    public interface ISpeexStreamerUpChannel : ISpeexStreamerUp, IClientChannel
    { }

    public interface ISpeexStreamerChannel : ISpeexStreamer, IClientChannel
    {}
}
