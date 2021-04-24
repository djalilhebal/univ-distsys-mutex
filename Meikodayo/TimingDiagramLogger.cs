using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Meikodayo
{
    /**
     * PlantUML Timing Diagram logger.
     * 
     * - See (offline) PlantUML Language Reference Guide (Version 1.2019.9)
     *   + Section 9: Timing Diagram
     *   + > This is only a proposal and subject to change.
     * 
     *  - See (online): https://plantuml.com/en/timing-diagram
     * 
     * - Tested with PlantUML v1.2021.00
     */
    public class TimingDiagramLogger
    {
        private List<string> _header = new();
        private List<string> _variables = new();
        private List<string> _messages = new();
        private List<string> _states = new();

        public readonly char SiteId;
        public readonly string SiteName;
        
        public TimingDiagramLogger(char siteId, string siteName) {
            // TODO: Assert siteId is in [a-z]
            // TODO: Assert siteName is not null
            SiteId = siteId;
            SiteName = siteName;
        }

    /**
     * Return current time in UNIX time
     */
    long Now() {
      return DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
    
    public void LogInit() {
        _header.Add($"concise \"{SiteName}\" as {SiteId}");
        LogState("{-}");
    }

    /**
     * For simplicity, we favor Linux stuff over Windows and trying to adapt the output to them.
     * Like, `\n` vs `\r\n` and `/` path separators vs `\`.
     */
    public void LogComment(string msg) {
        bool isMultiline = msg.Contains("\n");
        if (isMultiline) {
            _states.Add($"/'\n{msg}\n'/");
        } else {
            _states.Add($"'{msg}'");
        }
    }

    public void LogState(string state)
    {
        var when = Now();
        _states.Add($"@{when}\r\n{SiteId} is {state}\r\n");
    }

    public void LogSent(MeikoMessage message)
    {
        var when = Now();
        var from = message.Src;
        var to = message.Dst;
        _variables.Add($"!$m{message.Id}Sent = {when}");
        _messages.Add($"{from}@$m{message.Id}Sent -> {to}@$m{message.Id}Received : {message.Type}");
    }

    public void LogReceived(MeikoMessage message)
    {
        var when = Now();
        _variables.Add($"!$m{message.Id}Received = {when}");
    }

    private void ExportLog(IList<string> lines, string filename) {
        File.WriteAllText(filename, String.Join("\r\n", lines));
    }

    // Call on exit?
    public void ExportTimingLogs() {
      ExportLog(_header,    $"{SiteId}__header.partial.puml");
      ExportLog(_variables, $"{SiteId}__variables.partial.puml");
      ExportLog(_messages,  $"{SiteId}__messages.partial.puml");
      ExportLog(_states,    $"{SiteId}__states.partial.puml");
    }

    public static void GenerateCompleteDiagram() {
        string partialFileRegex = @"[a-z]__(header|variables|messages|states)\.partial\.puml$";
        // Get all files that match partialFileRegex
        var files = Directory.GetFiles("./logs/").Where(path => Regex.IsMatch(path, partialFileRegex));
        
        string completeHeader = String.Join("\r\n", files.Where(f => f.Contains("__header")).Select(f => $"!include {f}"));
        string completeVariables = String.Join("\r\n", files.Where(f => f.Contains("__variables")).Select(f => $"!include {f}"));
        string completeMessages = String.Join("\r\n", files.Where(f => f.Contains("__messages")).Select(f => $"!include {f}"));
        string completeStates  = String.Join("\r\n", files.Where(f => f.Contains("__states")).Select(f => $"!include {f}"));
        
        string completePuml = String.Join("\r\n", new[] {
            "@startuml",
            "",
            "' étati ∈ {dehors, demandeur, dedans}",
            "!$dehors    = \"Dehors #yellowgreen\"",
            "!$demandeur = \"Demandeur #yellow\"",
            "!$dedans    =  \"Dedans #tomato\"",
            "",
            completeHeader,
            completeVariables,
            completeMessages,
            completeStates,
            "@enduml"
        });

        File.WriteAllText("logs/scenario.puml", completePuml);
    }
   
    }
}
