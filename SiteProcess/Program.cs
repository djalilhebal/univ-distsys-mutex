using System;
using System.Diagnostics;
using Meikodayo;

namespace SiteProcess
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Log("Started");
            Log($"Checking I/O: IsInputRedirected? {Console.IsInputRedirected}; IsOutputRedirected? {Console.IsOutputRedirected}");

            AbstractAlgo algo = null;

            while (true)
            {
                var m = MeikoNetwork.GetNextMessage();
                var type = (string)m["type"];
                switch (type)
                {
                    case "init":
                        var id = (char)m["id"];
                        var sites = ((string)m["sites"]).Split(',');
                        // ...
                        break;
                    
                    default:
                        string handlerName = $"OnGot{m.Type}";
                        ((dynamic)algo)[handlerName](m);
                        break;
                }

            }
            
        }
        
        static void Log(string msg)
        {
            Console.Error.WriteLine(msg);
        }

    }
}