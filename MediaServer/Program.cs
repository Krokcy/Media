using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Threading;
using System.IO;

namespace MediaServer
{
    class Program
    {
        static void Main(string[] args)
        {
            WaveOutEvent woe = new WaveOutEvent();

            woe.NumberOfBuffers = 2;

            //NAudio.Wave.Mp3FileReader mp3Reader = new NAudio.Wave.Mp3FileReader(@"G:\musik\DAD\Call Of The Wild\DAD - Call Of The Wild - 04 - Marlboro Man.mp3");
            //woe.Init(mp3Reader);
            
            //woe.Play();

            //while (true)
            //{
            //    Console.WriteLine(woe.PlaybackState);
            //    t
            //}
            string filePath = @"G:\musik\Eminem\[2005] Curtain Call - The Hits 2CD\";

            foreach (string f in Directory.EnumerateFiles(filePath, "*.mp3"))
            {
                string fileName = Path.GetFileNameWithoutExtension(f);
                MusicRecoder mr = new MusicRecoder(f);
                WaveOut windowsPlayer = new WaveOut();

                BufferedWaveProvider provider = new BufferedWaveProvider(mr.WaveFormat);
                double temp = provider.BufferLength / 3;
                mr.bufferSize = Convert.ToInt32(Math.Ceiling(temp));
                mr.clearBuffers();

                windowsPlayer.Init(provider);
                windowsPlayer.Play();

                provider.AddSamples(mr.Play(), 0, mr.BufferSize);
                TimeSpan elapsedTime = new TimeSpan();
                double buffDur = 0;

                while (mr.state != PlaybackState.Stopped)
                {

                    buffDur = provider.BufferedDuration.TotalMilliseconds;
                    elapsedTime = elapsedTime.Add(provider.BufferedDuration);
                    Console.WriteLine("PLaying: " + fileName);
                    Console.WriteLine(provider.BufferedDuration);
                    Console.WriteLine(elapsedTime);
                    Console.WriteLine(provider.BufferedBytes);
                    Console.WriteLine(mr.WaveFormat);


                    Thread.Sleep(Convert.ToInt32(Math.Floor(buffDur / (double)2)));
                    byte[] tempB = mr.Play();

                    provider.AddSamples(tempB, 0, mr.BufferSize);

                    if (mr.state == PlaybackState.Stopped)
                        break;
                    Thread.Sleep(Convert.ToInt32(Math.Ceiling(buffDur / (double)2)));
                    tempB = mr.Play();

                    provider.AddSamples(tempB, 0, mr.BufferSize);

                    Console.Clear();

                }

                buffDur = provider.BufferedDuration.TotalMilliseconds;
                elapsedTime = elapsedTime.Add(provider.BufferedDuration);
                Console.WriteLine("PLaying: " + fileName);
                Console.WriteLine(provider.BufferedDuration);
                Console.WriteLine(elapsedTime);
                Console.WriteLine(provider.BufferedBytes);

                Thread.Sleep(provider.BufferedDuration);
                windowsPlayer.Stop();
                Thread.Sleep(100);
            }


        }
    }
}
