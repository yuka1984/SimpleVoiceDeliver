using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceModel;
using System.Threading;
using Microsoft.Practices.Composite.Wpf.Commands;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NSpeex;
using NSpeexTestServer;

namespace NSpeexTest
{
    public class VolumeUpdatedEventArgs : EventArgs
    {
        public int Volume { get; set; }
    }

    public class JitterBufferWaveProvider : WaveStream
    {
        private readonly SpeexDecoder decoder = new SpeexDecoder(BandMode.Narrow);
        private readonly SpeexJitterBuffer jitterBuffer;

        //private readonly NativeDecoder decoder = new NativeDecoder((EncodingMode)1);
        //private readonly NativeJitterBuffer jitterBuffer;

        private readonly WaveFormat waveFormat;
        private readonly object readWriteLock = new object();

        public JitterBufferWaveProvider()
        {
            waveFormat = new WaveFormat(decoder.FrameSize * 50, 16, 1);
            jitterBuffer = new SpeexJitterBuffer(decoder);
            //jitterBuffer = new NativeJitterBuffer(decoder);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int peakVolume = 0;
            int bytesRead = 0;
            lock (readWriteLock)
            {
                while (bytesRead < count)
                {
                    if (exceedingBytes.Count != 0)
                    {
                        buffer[bytesRead++] = exceedingBytes.Dequeue();
                    }
                    else
                    {
                        short[] decodedBuffer = new short[decoder.FrameSize * 2];
                        jitterBuffer.Get(decodedBuffer);
                        for (int i = 0; i < decodedBuffer.Length; ++i)
                        {
                            if (bytesRead < count)
                            {
                                short currentSample = decodedBuffer[i];
                                peakVolume = currentSample > peakVolume ? currentSample : peakVolume;
                                BitConverter.GetBytes(currentSample).CopyTo(buffer, offset + bytesRead);
                                bytesRead += 2;
                            }
                            else
                            {
                                var bytes = BitConverter.GetBytes(decodedBuffer[i]);
                                exceedingBytes.Enqueue(bytes[0]);
                                exceedingBytes.Enqueue(bytes[1]);
                            }
                        }
                    }
                }
            }

            OnVolumeUpdated(peakVolume);

            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (readWriteLock)
            {
                jitterBuffer.Put(buffer);
            }
        }

        public override long Length
        {
            get { return 1; }
        }

        public override long Position
        {
            get { return 0; }
            set { throw new NotImplementedException(); }
        }

        public override WaveFormat WaveFormat 
        { 
            get
            {
                return waveFormat;
            }
        }

        public EventHandler<VolumeUpdatedEventArgs> VolumeUpdated;

        private void OnVolumeUpdated(int volume)
        {
            var eventHandler = VolumeUpdated;
            if (eventHandler != null)
            {
                eventHandler.BeginInvoke(this, new VolumeUpdatedEventArgs { Volume = volume }, null, null);
            }
        }

        private readonly Queue<byte> exceedingBytes = new Queue<byte>();
    }

    //public class BufferedWaveProvider : WaveStream
    //{
    //    private readonly Decoder decoder = new Decoder(Mode.WideBand);
    //    private readonly WaveFormat waveFormat;
    //    public EventHandler<VolumeUpdatedEventArgs> VolumeUpdated;
    //    private readonly object readWriteLock = new object();
    //    private int maxBufferSize = 5; // buffer up to a maximum of framews
    //    private Queue<byte[]> bufferedFrames = new Queue<byte[]>();
    //    private Queue<byte> exceedingBytes = new Queue<byte>();

    //    public BufferedWaveProvider()
    //    {
    //        waveFormat = new WaveFormat(decoder.FrameSize * 50, 16, 1);
    //    }

    //    public override WaveFormat WaveFormat
    //    {
    //        get { return waveFormat; }
    //    }
        
    //    private void OnVolumeUpdated(int volume)
    //    {
    //        var eventHandler = VolumeUpdated;
    //        if (eventHandler != null)
    //        {
    //            eventHandler.BeginInvoke(this, new VolumeUpdatedEventArgs {Volume = volume}, null, null);
    //        }
    //    }
        
    //    #region Overrides of Stream

    //    public override void Write(byte[] buffer, int offset, int count)
    //    {
    //        lock (readWriteLock)
    //        {
    //            if (bufferedFrames.Count != maxBufferSize)
    //            {
    //                bufferedFrames.Enqueue(buffer);
    //            }
    //        }
    //    }

    //    public override int Read(byte[] buffer, int offset, int count)
    //    {
    //        int peakVolume = 0;
    //        int bytesRead = 0;

    //        lock (readWriteLock)
    //        {
    //            while (bytesRead < count)
    //            {
    //                if (exceedingBytes.Count != 0)
    //                {
    //                    buffer[bytesRead++] = exceedingBytes.Dequeue();
    //                }
    //                else
    //                {
    //                    byte[] frame = Length == 0 ? new byte[decoder.FrameSize] : bufferedFrames.Dequeue();
    //                    short[] decodedBuffer = new short[decoder.FrameSize];
    //                    int decodingResult = decoder.Decode(frame, decodedBuffer);
    //                    if (decodingResult != 0)
    //                        return count;

    //                    for (int i = 0; i < decodedBuffer.Length; ++i)
    //                    {
    //                        if (bytesRead < count)
    //                        {
    //                            short currentSample = decodedBuffer[i];
    //                            peakVolume = currentSample > peakVolume ? currentSample : peakVolume;
    //                            BitConverter.GetBytes(currentSample).CopyTo(buffer, offset + bytesRead);
    //                            bytesRead += 2;
    //                        }
    //                        else
    //                        {
    //                            var bytes = BitConverter.GetBytes(decodedBuffer[i]);
    //                            exceedingBytes.Enqueue(bytes[0]);
    //                            exceedingBytes.Enqueue(bytes[1]);
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        OnVolumeUpdated(peakVolume);

    //        return bytesRead;
    //    }

    //    public override long Length
    //    {
    //        get { return bufferedFrames.Count; }
    //    }

    //    public override long Position
    //    {
    //        get
    //        {
    //            return 0;
    //        }
    //        set
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }
        
    //    #endregion
    //}

    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public class MainViewModel : INotifyPropertyChanged, ISpeexStreamerCallback
    {
        private long bufferDepth;
        private readonly SpeexEncoder encoder;
        //private readonly NativeEncoder encoder;

        //todo: private readonly NativePreprocessor preProcessor;
        private bool isRecording;
        private ISpeexStreamer sender;
        private ISpeexStreamer listener;
        private int volume;
        private IWaveIn waveIn;
        private IWavePlayer waveOut;
        private bool isPlaying;
        private JitterBufferWaveProvider waveProvider;

        public MainViewModel()
        {
            listenAddress = "net.tcp://localhost:8001/SpeexStreamer";

            encoder = new SpeexEncoder(BandMode.Narrow);
            //encoder.VBR = true;
            //encoder.DTX = true;
            //todo: preProcessor = new NativePreprocessor(encoder.FrameSize, 16000);// { AGC = true, AGCIncrement = 1, MaxAGCGain = 10, Denoise = true, VAD = true };

            //encoder = new NativeEncoder((EncodingMode) 1);
            //encoder.VBR = false;
            //encoder.DTX = false;

            waveFormat = new WaveFormat(encoder.FrameSize*50, 16, 1);
            waveIn = new WaveIn {WaveFormat = waveFormat, BufferMillisconds = 40, NumberOfBuffers = 2};
            //waveIn = new WasapiCapture();
            
            waveIn.DataAvailable += waveIn_DataAvailable;
            waveIn.RecordingStopped += waveIn_RecordingStopped;
            
            sender = new DuplexChannelFactory<ISpeexStreamer>(this, "NetTcpBinding_ISpeexStreamer").CreateChannel();

            waveProvider = new JitterBufferWaveProvider();
            waveProvider.VolumeUpdated += HandleVolumeUpdated;

            //waveOut = new DirectSoundOut(40);
            //waveOut = new WaveOut();
            waveOut = new WasapiOut(AudioClientShareMode.Shared, 20);

            waveOut.Init(waveProvider);
            waveOut.PlaybackStopped += waveOut_PlaybackStopped;
            waveOut.Volume = 1.0f;

            StartCommand = new DelegateCommand<object>(DoStartRecording, obj => !IsRecording);
            ListenCommand = new DelegateCommand<object>(DoListen, obj => !IsPlaying);
        }
        
        public long BufferDepth
        {
            get { return bufferDepth; }
            set
            {
                if (value != bufferDepth)
                {
                    bufferDepth = value;
                    OnPropertyChanged("BufferDepth");
                }
            }
        }

        public long BufferSize
        {
            get
            {
                return waveProvider.Length;
            }
        }

        public bool IsRecording
        {
            get { return isRecording; }
            set 
            { 
                if (value != isRecording)
                {
                    isRecording = value;
                    OnPropertyChanged("IsRecording");
                } 
            }
        }

        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                if (value != isPlaying)
                {
                    isPlaying = value;
                    OnPropertyChanged("IsPlaying");
                }
            }
        }

        public float SelectedVolume
        {
            get
            {
                return waveOut.Volume;
            }

            set
            {
                waveOut.Volume = value;
            }
        }

        private string listenAddress;
        private readonly WaveFormat waveFormat;

        public string ListenAddress
        {
            get
            {
                return listenAddress;
            }
            set
            {
                if (value != listenAddress)
                {
                    listenAddress = value;
                    OnPropertyChanged("ListenAddress");
                }
            }
        }

        public DelegateCommand<object> ListenCommand { get; private set; }

        public DelegateCommand<object> StartCommand { get; private set; }

        public MainView View { get; set; }

        public int Volume
        {
            get { return volume; }
            set
            {
                if (value != volume)
                {
                    volume = value;
                    OnPropertyChanged("Volume");
                }
            }
        }
        
        private void DoListen(object obj)
        {
            waveOut.Play();
            listener =
                new DuplexChannelFactory<ISpeexStreamer>(new InstanceContext(this), "NetTcpBinding_ISpeexStreamer",
                                                              new EndpointAddress(ListenAddress)).CreateChannel();
            listener.Subscribe();
            IsPlaying = true;
            RaiseCommands();
        }

        private void DoStartRecording(object obj)
        {
            waveIn.StartRecording();
            IsRecording = true;
            RaiseCommands();
        }

        private void HandleBufferDepthUpdated(object sender, EventArgs e)
        {
            if (!View.CheckAccess())
            {
                View.Dispatcher.BeginInvoke(new ThreadStart(() =>
                                                                {
                                                                    BufferDepth = waveProvider.Length;
                                                                }));

            }
        }

        private void HandleVolumeUpdated(object sender, VolumeUpdatedEventArgs e)
        {
            if (!View.CheckAccess())
            {
                View.Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    Volume = e.Volume;
                }));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void RaiseCommands()
        {
            StartCommand.RaiseCanExecuteChanged();
            ListenCommand.RaiseCanExecuteChanged();
        }

        private void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            // convert to short
            short[] data = new short[e.BytesRecorded / 2];
            Buffer.BlockCopy(e.Buffer, 0, data, 0, e.BytesRecorded);
            var encodedData = new byte[e.BytesRecorded];
            var encodedBytes = encoder.Encode(data, 0, data.Length, encodedData, 0, encodedData.Length);
            if (encodedBytes != 0)
            {
                var upstreamFrame = new byte[encodedBytes];
                Array.Copy(encodedData, upstreamFrame, encodedBytes);

                //Debug.WriteLine("Publishing: " + encodedBytesSample + " bytes");));)
                this.sender.Publish(upstreamFrame);
            }
        }

        private void waveIn_RecordingStopped(object sender, EventArgs e)
        {
            IsRecording = false;
            RaiseCommands();
        }

        private void waveOut_PlaybackStopped(object sender, EventArgs e)
        {
            RaiseCommands();
        }
        
        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of ISpeexStreamerCallback

        public void OnPublish(byte[] data)
        {
            waveProvider.Write(data, 0, data.Length);
        }

        #endregion
    }

}
