using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Meikodayo
{
    /**
     * The simulation network...
     * 
     * Imagine we're on a wireless LAN (Wi-Fi n) that is FIFO for whatever reason.
     */
    public class MeikoNetwork
    {
        public static void SendMessage(MeikoMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            Console.WriteLine(json);
        }
        
        public static MeikoMessage GetNextMessage()
        {
            var incomingMessage = Console.ReadLine();
            var parsed = JsonSerializer.Deserialize<MeikoMessage>(incomingMessage);
            return parsed;
        }

        // ---
        
        private BlockingCollection<MeikoMessage> _queue = new();
        public IDictionary<char, MeikoSite> Sites { get; set; }

        private CancellationTokenSource _tokenSource;

        public void Add(MeikoMessage packet)
        {
            _queue.Add(packet);
        }
        
        public void Start()
        {
            _tokenSource = new CancellationTokenSource();
            foreach (var (_, site) in Sites)
            {
                Task.Run(() =>
                    {
                        while (!_tokenSource.IsCancellationRequested)
                        {
                            var jsonData = site.Stdout.ReadLine();
                            Console.Error.WriteLine($"Read from {site.Id}: {jsonData}");
                            var parsedMessage = JsonSerializer.Deserialize<MeikoMessage>(jsonData);
                            _queue.Add(parsedMessage);
                        }
                    });
            }
            
            Task.Run(() =>
            {
                while (!_tokenSource.IsCancellationRequested)
                {
                    var message = _queue.Take(_tokenSource.Token);
                    var messageJson = JsonSerializer.Serialize(message);
                    Console.Error.WriteLine($"Forwarding to {message.Dst}...");
                    Sites[message.Dst].Stdin.WriteLine(messageJson);
                }
            });
        }
        
        public void Stop()
        {
            _tokenSource.Cancel();
            foreach (var site in Sites)
            {
                site.Value.Process.Close();
                site.Value.Process.WaitForExit(100);
                site.Value.Process.Kill();
            }
        }

    }
    
}