using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Threading;
using System.IO;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using Microsoft.CSharp;
using System.Net.Sockets;
using System.Web;
using System.Net;

namespace MediaServer
{

    class Program
    {
        static void Main(string[] args)
        {
            MMDeviceEnumerator mmDeviceEnum = new MMDeviceEnumerator();
            bool run = true;
            Task.Factory.StartNew(() =>
            {
                Console.ReadLine();
                run = false;
            });
            Task.Factory.StartNew(async () => 
            {

                UdpClient udpHost = new UdpClient(8080);
                BufferedWaveProvider MDWaveProvider = new BufferedWaveProvider(new WaveFormat());
                MDWaveProvider.DiscardOnBufferOverflow = true;
                WaveOut windowsPlayerMic = new WaveOut();
                Console.WriteLine("Connected");
                windowsPlayerMic.Init(MDWaveProvider);
                windowsPlayerMic.Play();
                bool runClient = true;
                TimeSpan timeCool = new TimeSpan(0, 0, 2);
                TimeSpan? timeEnd = null;
                DateTime? time = null;
                while (runClient)
                {
                    if (udpHost.Available > 0) {
                        Console.WriteLine($"Awaiting network, {udpHost.Available}");
                        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        byte[] buffer = udpHost.Receive(ref RemoteIpEndPoint);
                        MDWaveProvider.AddSamples(buffer, 0, buffer.Length);
                        Console.WriteLine($"Recieved bytes {buffer.Length} from network");
                    }
                    else
                    {

                    }
                    if(!run)
                    {
                        if (time == null)
                            time = DateTime.Now;
                        timeEnd = (DateTime.Now - time);
                        runClient = (timeEnd ?? new TimeSpan(0,0,5)) < timeCool;
                    }
                }
                Thread.Sleep(55);
                Console.WriteLine($"stopped client after {(timeEnd ?? new TimeSpan(1, 0, 0)).TotalSeconds} seconds");
            });

            UdpClient udpClient = new UdpClient();
            udpClient.Connect("localhost", 8080);

            foreach (MMDevice d in mmDeviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                Console.WriteLine(d.DeviceFriendlyName);

                WasapiCapture ws = new WasapiCapture(d);
                ws.WaveFormat = new WaveFormat();
                ws.ShareMode = AudioClientShareMode.Exclusive;
                try
                {
                    ws.DataAvailable += (s, a) =>
                    {
                        udpClient.SendAsync(a.Buffer, a.BytesRecorded);
                        Console.WriteLine($"Send bytes {a.BytesRecorded} to network");
                    };
                    Console.WriteLine(ws.CaptureState);
                    ws.StartRecording();
                    while ((ws.CaptureState != NAudio.CoreAudioApi.CaptureState.Stopped || udpClient.Client.Connected) && run)
                    {
                        Thread.Sleep(500);
                    }
                }
                finally { ws.Dispose(); d.Dispose();
                }
                TimeSpan timeCool = new TimeSpan(0, 0, 2);
                TimeSpan? timeEnd = null;
                DateTime? time = null;
                bool runHost = true;
                while (runHost)
                {
                    if (time == null)
                        time = DateTime.Now;
                    timeEnd = (DateTime.Now - time);
                    runHost = (timeEnd ?? new TimeSpan(0, 0, 5)) < timeCool;
                }
                udpClient.Close();
                Console.WriteLine($"stopped host after {(timeEnd ?? new TimeSpan(1, 0, 0)).TotalSeconds} seconds");
            }
            Console.ReadLine();
            /*
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
                provider.BufferLength = 1024 * 512;
                double temp = provider.BufferLength / 3;
                mr.BufferSize = Convert.ToInt32(Math.Ceiling(temp));
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
            */

        }
    }
}
