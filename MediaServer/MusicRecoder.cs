using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.IO;

namespace MediaServer
{
    class MusicRecoder
    {

        private WaveFormatConversionStream audioStream;

        private byte[][] audioBuffers = new byte[2][];
        public PlaybackState state = PlaybackState.Stopped;
        private int bufferInUse = 0;
        public int bufferSize = 1024*256;

        Task currentBufferWriter;

        public int BufferSize { get { return bufferSize; } }

        public WaveFormat WaveFormat { get { return audioStream.WaveFormat; } }

        public MusicRecoder(string filePath)
        {
            audioBuffers[0] = new byte[bufferSize];
            audioBuffers[1] = new byte[bufferSize];

            string extension = Path.GetExtension(filePath);
            switch (extension)
            {
                case ".mp3": Mp3FileReader mp3 = new Mp3FileReader(filePath); 
                        audioStream = new WaveFormatConversionStream(mp3.WaveFormat, mp3);
                    break;
                case ".wma": MediaFoundationReader wma = new MediaFoundationReader(filePath);
                        audioStream = new WaveFormatConversionStream(wma.WaveFormat, wma);
                    break;
                case ".m4a": MediaFoundationReader m4a = new MediaFoundationReader(filePath);
                    audioStream = new WaveFormatConversionStream(m4a.WaveFormat, m4a); ;
                    break;
            }
        }
        
        public byte[] Play()
        {
            
                int prevBuffer = bufferInUse;
                long pos = audioStream.Position;
                if(currentBufferWriter != null)
                    currentBufferWriter.Wait();
                int remain = Convert.ToInt32(audioStream.Length - audioStream.Position);
                Console.WriteLine(remain + "<" + bufferSize);
                if (remain < bufferSize)
                {
                    state = PlaybackState.Stopped;
                    if (remain != 0)
                    {
                        int newBufferSize = bufferSize + remain;
                        byte[] tempB = new byte[newBufferSize];
                        Array.Copy(audioBuffers[bufferInUse], tempB, bufferSize);
                        audioStream.Read(tempB, bufferSize, newBufferSize);

                        bufferSize = newBufferSize;
                        return tempB;
                    }
                    else
                        return audioBuffers[bufferInUse];
                }
                if (state == PlaybackState.Paused || state == PlaybackState.Stopped)
                {
                    state = PlaybackState.Playing;
                    audioStream.Read(audioBuffers[bufferInUse], 0, bufferSize);
                    switchBuffer();
                    currentBufferWriter = audioStream.ReadAsync(audioBuffers[bufferInUse], 0, bufferSize);
                }
                else
                {
                    switchBuffer();
                    currentBufferWriter = audioStream.ReadAsync(audioBuffers[bufferInUse], 0, bufferSize);
                }
                return audioBuffers[prevBuffer];

                //switchBuffer();
                //int remain = Convert.ToInt32(audioStream.Length - audioStream.Position);
                //audioStream.Read(audioBuffers[bufferInUse], 0, remain);
                //return audioBuffers[bufferInUse];
         
            
               
        }


        private void switchBuffer()
        {
            if (bufferInUse == 0)
            {
                bufferInUse = 1;
            }
            else
                bufferInUse = 0;
        }

        public void clearBuffers() 
        {
            
            audioBuffers[0] = new byte[bufferSize];
            audioBuffers[1] = new byte[bufferSize];
        }
        
    }
}
