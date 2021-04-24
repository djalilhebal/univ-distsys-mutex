using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Meikodayo
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;

            string sitesArg = "i,j,k";
            char[] siteIds = sitesArg.Split(',').Select(c => c[0]).ToArray();

            Dictionary<char, MeikoSite> siteMap = new();

            foreach (var siteId in siteIds)
            {
                var x = CreateMikuSite(siteId);
                siteMap.Add(siteId, x);
            }

            var networkSim = new MeikoNetwork
            {
                Sites = siteMap
            };

            networkSim.Start();
            
            Task.Delay(TimeSpan.FromSeconds(5)).Wait();

            foreach (var siteId in siteIds)
            {
                networkSim.Add(new MeikoMessage
                    {
                        Type = "Init",
                        SiteId = siteId,
                        SiteIds = siteIds,
                    });
            }

            Process.GetCurrentProcess().Exited += (sender, args) =>
            {
                networkSim.Stop();
                OnExit();
            };

            Task.Delay(-1).Wait();
        }

        private static MeikoSite CreateMikuSite(char id)
        {
            var siteProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    /*
                    FileName = "xterm",
                    Arguments = $"-T \"Site {id}\" -Smx03 -e dotnet run --project SiteProcess",
                    */
                    FileName = "dotnet",
                    Arguments = $"run --project SiteProcess",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = "/home/djalil/RiderProjects/DistMutexViz/",
                },
            };
            siteProcess.Start();
            
            MeikoSite meikoSite = new MeikoSite
            {
                Id = id,
                Stdin = siteProcess.StandardInput,
                Stdout = siteProcess.StandardOutput,
                Process = siteProcess,
            };

            return meikoSite;
        }
        
        static void OnExit()
        {
            string cmd = "java -jar plantuml.1.2021.0.jar scenario.puml";
            Console.Error.WriteLine("Running: " + cmd);
        }

    }
    
}
