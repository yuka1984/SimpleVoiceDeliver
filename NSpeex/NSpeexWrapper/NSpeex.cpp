// This is the main DLL file.

#include "stdafx.h"

#include "NSpeex.h"

NSpeex::NativeEncoder::NativeEncoder(EncodingMode mode)
{
	bits = new SpeexBits();
	speex_bits_init(bits);
	enc_state = speex_encoder_init(ConvertToSpeexMode(mode));

	short samplesPerFrameTemp;
	if (speex_encoder_ctl(this->enc_state, SPEEX_GET_FRAME_SIZE, &samplesPerFrameTemp))
		throw gcnew InvalidOperationException();
	this->samplesPerFrame = samplesPerFrameTemp;
}

NSpeex::NativeEncoder::~NativeEncoder()
{
	speex_bits_destroy(bits);
	speex_encoder_destroy(enc_state); 
}

int NSpeex::NativeEncoder::Encode(array<short>^ inData, int inOffset, int inCount, array<Byte>^ outData, int outOffset, int outCount)
{
	pin_ptr<short> inDataPtr = &inData[inOffset];
	pin_ptr<Byte> outDataPtr = &outData[outOffset];
	int samplesProcessed = 0;
	int dtxResult = 0;
	speex_bits_reset(this->bits);
	while (samplesProcessed < inCount)
	{
		dtxResult += speex_encode_int(this->enc_state, inDataPtr + samplesProcessed, this->bits);

		samplesProcessed += samplesPerFrame;
	}
	if (dtxResult == 0)
		return 0;

	return speex_bits_write(this->bits, (char*)outDataPtr, outCount);
}

NSpeex::NativeDecoder::NativeDecoder(EncodingMode mode)
{
	bits = new SpeexBits();
	speex_bits_init(bits);
	dec_state = speex_decoder_init(ConvertToSpeexMode(mode));
	short samplesPerFrameTemp;
	if (speex_decoder_ctl(this->dec_state, SPEEX_GET_FRAME_SIZE, &samplesPerFrameTemp))
		throw gcnew InvalidOperationException();

	this->samplesPerFrame = samplesPerFrameTemp;
}

NSpeex::NativeDecoder::~NativeDecoder()
{
	speex_bits_destroy(bits);
	speex_decoder_destroy(dec_state); 
}

short NSpeex::NativeDecoder::Decode(array<Byte>^ inData, int inOffset, int inCount, array<short>^ outData, int outOffset, bool lostFrame)
{	
	pin_ptr<short> outDataPtr = &outData[outOffset];
	if (lostFrame)	
		return this->Decode(0, inOffset, inCount, outDataPtr, outOffset);

	pin_ptr<Byte> inDataPtr = &inData[inOffset];
	return this->Decode(inDataPtr, inOffset, inCount, outDataPtr, outOffset);
}

short NSpeex::NativeDecoder::Decode(Byte* inDataPtr, int inOffset, int inCount, short* outDataPtr, int outOffset)
{	
	// null pointer for inBits means lost frame
	if (inDataPtr == 0)
	{
		speex_decode_int(this->dec_state, 0, outDataPtr);
		return this->samplesPerFrame;
	}

	speex_bits_read_from(this->bits, (char*)inDataPtr, inCount);
	int samplesDecoded = 0;
	while(speex_decode_int(this->dec_state, this->bits, outDataPtr + samplesDecoded) == 0)
		samplesDecoded += this->samplesPerFrame;

	return samplesDecoded;
}

NSpeex::NativePreprocessor::NativePreprocessor(int frameSize, int samplingRate)
{
	state = speex_preprocess_state_init(frameSize, samplingRate);
}

NSpeex::NativePreprocessor::~NativePreprocessor()
{
	speex_preprocess_state_destroy(state);
}

bool NSpeex::NativePreprocessor::Process(array<short>^ frame)
{
	pin_ptr<short> pFrame = &frame[0];
	int result = speex_preprocess_run(state, pFrame);

	return result != 0;
}

NSpeex::NativeJitterBuffer::NativeJitterBuffer(NSpeex::NativeDecoder^ decoder)
{
	this->decoder = decoder;
	jitter = jitter_buffer_init(1);
	bits = new SpeexBits();
	speex_bits_init(bits);
	currentPutTimestamp = 0;
}

NSpeex::NativeJitterBuffer::~NativeJitterBuffer()
{
	jitter_buffer_destroy(jitter);
	speex_bits_destroy(bits);
	if (outBuffer != 0)
		delete outBuffer;
}

void NSpeex::NativeJitterBuffer::Put(array<Byte>^ data)
{
	pin_ptr<Byte> pData = &data[0];

	JitterBufferPacket p;
	p.len = data->Length;
	p.sequence = 0;
	p.span = 1;
	p.timestamp = this->currentPutTimestamp++;
	p.data = (char*)pData;
	jitter_buffer_put(jitter, &p);
}

void NSpeex::NativeJitterBuffer::Get(array<short>^ decodedData)
{
	if (outBuffer == 0)
		outBuffer = new char[decodedData->Length];
	
	JitterBufferPacket p;
	p.len = decodedData->Length;
	p.data = outBuffer;

	memset(p.data, 0, p.len);

	int ret = jitter_buffer_get(jitter, &p, 1, 0);
	pin_ptr<short> pinDecodedData = &decodedData[0];
	if (ret != JITTER_BUFFER_OK)
	{
		// no packet found
		decoder->Decode(0, 0, 0, pinDecodedData, 0);
	}
	else
	{
		decoder->Decode((Byte*)p.data, 0, p.len, pinDecodedData, 0);
	}

	jitter_buffer_tick(jitter);
}