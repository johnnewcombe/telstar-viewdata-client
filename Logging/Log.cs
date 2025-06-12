using System;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace TelstarClient.Logging;

public static class Log {

    public static TraceSwitch TraceSwitch = new TraceSwitch("General", "Entire Application");

    public static void Debug(string message) {

        if (TraceSwitch.TraceVerbose) {
            Trace.WriteLine(FormatMessage("DEBUG", message));
        }
    }
    public static void Information(string message) {

        if (TraceSwitch.TraceInfo) {
            Trace.WriteLine(FormatMessage("INFO", message));
        }
    }

    public static void Warning(string message) {
        if (TraceSwitch.TraceWarning) {
            Trace.TraceWarning(FormatMessage("WARNING", message));
        }
    }

    public static void Error(string message) {
        if (TraceSwitch.TraceError) {
            Trace.TraceError(FormatMessage("ERROR", message));
        }
    }

    public static void LogLevel(string logLevel) {
        switch (logLevel) {
            case "Debug":
                Logging.Log.TraceSwitch.Level = TraceLevel.Verbose;
                break;
            case "Information":
                Logging.Log.TraceSwitch.Level = TraceLevel.Info;
                break;
            case "Warning":
                Logging.Log.TraceSwitch.Level = TraceLevel.Warning;
                break;
            case "Error":
                Logging.Log.TraceSwitch.Level = TraceLevel.Error;
                break;
        }
    }

    private static string FormatMessage(string type, string message) {
        message = string.Format($"{DateTime.Now} [{type}]: {message}");
        return message;
    }


}