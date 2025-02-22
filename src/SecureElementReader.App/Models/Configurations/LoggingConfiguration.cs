﻿using Serilog.Events;

namespace SecureElementReader.App.Models.Configurations
{
    public class LoggingConfiguration
    {
        public string LogFileName { get; set; }

        public long LimitBytes { get; set; }

        public LogEventLevel DefaultLogLevel { get; set; }

        public LogEventLevel MicrosoftLogLevel { get; set; }
    }
}
