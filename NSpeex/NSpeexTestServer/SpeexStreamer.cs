using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

namespace NSpeexTestServer
{
    public class SynchronizedQueue<TItem>
    {
        Queue<TItem> queue;
        object lockObject;
        ManualResetEvent itemAvailableEvent;
        ManualResetEvent spaceAvailableEvent;

        int maxLength;
        public SynchronizedQueue(int maxLength)
        {
            this.lockObject = new object();
            this.queue = new Queue<TItem>();
            this.maxLength = maxLength;
        }

        public virtual TItem Dequeue()
        {
            TItem item;
            lock (lockObject)
            {
                while (queue.Count == 0)
                {
                    Monitor.Wait(lockObject);
                }

                item = queue.Dequeue();
                Monitor.PulseAll(lockObject);
            }

            return item;
        }

        public virtual void Enqueue(TItem item)
        {
            lock (lockObject)
            {
                if (queue.Count == maxLength)
                {
                    Monitor.Wait(lockObject);
                }

                queue.Enqueue(item);
                Monitor.PulseAll(lockObject);
            }
        }
        public virtual int MaxLength
        {
            get { return this.maxLength; }
        }
        public virtual int Count
        {
            get { return queue.Count; }
        }
        public virtual bool MaxLengthReached
        {
            get { return Count >= maxLength; }
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant, IncludeExceptionDetailInFaults = true)]
    public class SpeexStreamer : ISpeexStreamer
    {
        private static List<ISpeexStreamerCallback> subscribers = new List<ISpeexStreamerCallback>();

        private static SynchronizedQueue<byte[]> queue = new SynchronizedQueue<byte[]>(1000);

        public SpeexStreamer()
        {
            ThreadPool.QueueUserWorkItem(DoPublish);
        }

        private void DoPublish(object state)
        {
            while (true)
            {
                var frame = queue.Dequeue();
                foreach (var subscriber in subscribers)
                {
                    subscriber.OnPublish(frame);
                }
            }
        }

        #region Implementation of ISpeexStreamer

        public void Publish(byte[] data)
        {
            //queue.Enqueue(data);

            foreach (var subscriber in subscribers)
            {
                subscriber.OnPublish(data);
            }
        }

        public void Subscribe()
        {
            subscribers.Add(OperationContext.Current.GetCallbackChannel<ISpeexStreamerCallback>());
        }

        #endregion
    }
}
