using System;
using System.ServiceModel;

namespace NSpeexTest.Silverlight.ServiceContract
{
    [ServiceContract]
    public interface ISpeexStreamerUp
    {
        [OperationContract(AsyncPattern = true, IsOneWay = true)]
        IAsyncResult Begin_Publish(byte[] data, AsyncCallback callback, object state);

        void End_Publish(IAsyncResult result);
    }

    public interface ISpeexStreamerUpChannel : ISpeexStreamerUp, IClientChannel
    { }
}
