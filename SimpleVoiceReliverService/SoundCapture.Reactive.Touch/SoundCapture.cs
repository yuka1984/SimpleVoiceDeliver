using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using AudioToolbox;
using AVFoundation;
using Foundation;

namespace SoundCapture.Reactive
{
    public class SoundCapture : ISoundCapture, IDisposable
    {
        private bool _isrecording;
        private readonly Subject<byte[]> _subject = new Subject<byte[]>();
        private readonly InputAudioQueue audioQueue;
        private readonly List<IntPtr> bufferPtrs = new List<IntPtr>();
        private readonly AudioStreamBasicDescription description;
        private readonly int pushsize;

        public SoundCapture(int sampleSize, int buffermilliseconds)
        {
            if (buffermilliseconds > 1000) throw new ArgumentOutOfRangeException(nameof(buffermilliseconds));
            pushsize = sampleSize/(1000/buffermilliseconds);

            description = new AudioStreamBasicDescription
            {
                SampleRate = sampleSize,
                Format = AudioFormatType.LinearPCM,
                FormatFlags = AudioFormatFlags.LinearPCMIsPacked | AudioFormatFlags.IsSignedInteger,
                BitsPerChannel = 16,
                ChannelsPerFrame = 1,
                BytesPerFrame = 2,
                FramesPerPacket = 1,
                BytesPerPacket = 2,
                Reserved = 0
            };

            audioQueue = new InputAudioQueue(description);
            for (var i = 0; i < 3; i++)
            {
                var ptr = IntPtr.Zero;
                audioQueue.AllocateBufferWithPacketDescriptors(pushsize*description.BytesPerPacket, pushsize, out ptr);
                audioQueue.EnqueueBuffer(ptr, pushsize, null);
                bufferPtrs.Add(ptr);
            }
            audioQueue.InputCompleted += AudioQueueOnInputCompleted;
        }

        public void Dispose()
        {
            Stop();
            _subject.OnCompleted();
            foreach (var ptr in bufferPtrs)
            {
                audioQueue.FreeBuffer(ptr);
            }
            audioQueue.QueueDispose();
            audioQueue.Dispose();
            _subject.Dispose();
        }

        public IDisposable Subscribe(IObserver<byte[]> observer) => _subject.Subscribe(observer);

        public void Start()
        {
            if (_isrecording) return;
            var status = audioQueue.Start();
            if (status != AudioQueueStatus.Ok)
            {
                throw new Exception(status.ToString());
            }
            _isrecording = true;
        }

        public void Stop()
        {
            if (!_isrecording) return;
            audioQueue.Stop(true);
            _isrecording = false;
        }

        private void AudioQueueOnInputCompleted(object sender, InputCompletedEventArgs args)
        {
            var buffer = (AudioQueueBuffer)Marshal.PtrToStructure(args.IntPtrBuffer, typeof(AudioQueueBuffer));
            var send = new byte[buffer.AudioDataByteSize];
            Marshal.Copy(buffer.AudioData, send, 0, (int)buffer.AudioDataByteSize);
            _subject.OnNext(send);

            var status = audioQueue.EnqueueBuffer(args.IntPtrBuffer, this.pushsize, args.PacketDescriptions);

        }
    }
}