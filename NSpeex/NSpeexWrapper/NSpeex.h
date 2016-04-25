// NSpeex.h

#pragma once

#include <speex/speex.h>
#include <speex/speex_preprocess.h>
#include <speex/speex_jitter.h>

using namespace System;
using namespace System::Runtime::InteropServices;

namespace NSpeex {

	public enum EncodingMode
	{
		NarrowBand = 0,
		WideBand = 1,
		UltraWideBand = 2
	};

	const SpeexMode* ConvertToSpeexMode(EncodingMode mode)
	{
		const SpeexMode *speexMode = &speex_nb_mode;
		switch (mode)
		{
		case NarrowBand:
			speexMode = &speex_nb_mode;
			break;
		case WideBand:
			speexMode = &speex_wb_mode;
			break;
		case UltraWideBand:
			speexMode = &speex_uwb_mode;
			break;
		default:
			speexMode = &speex_nb_mode;
		}

		return speexMode;
	}

	public ref class NativeEncoder
	{
	public:
		NativeEncoder(EncodingMode mode);
		~NativeEncoder();

		int Encode(array<short>^ inData, int inOffset, int inCount, array<Byte>^ outData, int outOffset, int outCount);

		property int FrameSize
		{
			int get()
			{
				return samplesPerFrame;
			}
		}

		property int Quality
		{
			void set(int quality)
			{
				speex_encoder_ctl(enc_state, SPEEX_SET_QUALITY, &quality);
			}
		}

		property bool VBR
		{
			void set(bool value)
			{
				int vbr = value ? 1: 0;
				speex_encoder_ctl(enc_state, SPEEX_SET_VBR, &vbr);
			}
		}

		property bool DTX
		{
			void set(bool value)
			{
				int dtx = value ? 1: 0;
				speex_encoder_ctl(enc_state, SPEEX_SET_DTX, &dtx);
			}
		}

	private:
		SpeexBits* bits;
		void* enc_state;
		short samplesPerFrame;
	};

	public ref class NativeDecoder
	{
	public:
		NativeDecoder(EncodingMode mode);
		~NativeDecoder();

		short Decode(array<Byte>^ inData, int inOffset, int inCount, array<short>^ outData, int outOffset, bool lostFrame);

		property int FrameSize
		{
			int get()
			{
				return samplesPerFrame;
			}
		}

	internal:
		void* dec_state;
		short Decode(Byte* inData, int inOffset, int inCount, short* outData, int outOffset);

	private:
		SpeexBits* bits;
		short samplesPerFrame;
	};

	public ref class NativePreprocessor
	{
	public:
		NativePreprocessor(int frameSize, int samplingRate);
		~NativePreprocessor();

		bool Process(array<short>^ frame);

		property bool AGC 
		{ 
			void set(bool on)
			{
				speex_preprocess_ctl(state, SPEEX_PREPROCESS_SET_AGC, &on);
			}
		}
		
		property int AGCIncrement
		{
			int get ()
			{
				int level;
				speex_preprocess_ctl(state, SPEEX_PREPROCESS_GET_AGC_INCREMENT, &level);
				return level;
			}

			void set (int dBpS)
			{
				speex_preprocess_ctl(state, SPEEX_PREPROCESS_SET_AGC_INCREMENT , &dBpS);
			}
		}

		property int MaxAGCGain
		{
			void set(int dB)
			{
				speex_preprocess_ctl(state, SPEEX_PREPROCESS_SET_AGC_MAX_GAIN , &dB);
			}
			int get()
			{
				int level;
				speex_preprocess_ctl(state, SPEEX_PREPROCESS_GET_AGC_MAX_GAIN, &level);
				return level;
			}
		}
		
		property bool Denoise
		{
			void set(bool on)
			{
				speex_preprocess_ctl(state, SPEEX_PREPROCESS_SET_DENOISE, &on);
			}
		}

		property bool VAD
		{
			void set(bool on)
			{
				speex_preprocess_ctl(state, SPEEX_PREPROCESS_SET_VAD, &on);
			}
		}

	private:
		SpeexPreprocessState* state;
	};

	public ref class NativeJitterBuffer
	{
	public:
		NativeJitterBuffer(NativeDecoder^ decoder);
		~NativeJitterBuffer();

		void Put(array<Byte>^ data);
		void Get(array<short>^ decodedData);

	private:
		JitterBuffer *jitter;
		SpeexBits* bits;
		NativeDecoder^ decoder;
		int currentPutTimestamp;
		char *outBuffer;
	};
}
