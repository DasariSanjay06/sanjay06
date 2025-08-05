using System.ComponentModel;

namespace LaserMarkingSystem.Models
{
    public enum ControllerType
    {
        RTC3,
        RTC4
    }

    public enum LaserStatus
    {
        Ready,
        Busy,
        Error,
        Disconnected
    }

    public class LaserController
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ControllerType Type { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; }
        public LaserStatus Status { get; set; }
        public bool IsConnected { get; set; }
        public DateTime LastConnected { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string FirmwareVersion { get; set; } = string.Empty;
    }

    public class LaserParameters
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Power { get; set; } // Watts
        public double Speed { get; set; } // mm/s
        public double Frequency { get; set; } // Hz
        public double PulseWidth { get; set; } // microseconds
        public double FocusPosition { get; set; } // mm
        public double MarkingDepth { get; set; } // mm
        public int Repetitions { get; set; }
        public double SpotSize { get; set; } // mm
        public bool EnableGalvo { get; set; }
        public double GalvoDelay { get; set; } // ms
        public string Material { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    public class MarkingJob
    {
        public int Id { get; set; }
        public string JobName { get; set; } = string.Empty;
        public string NumberPlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string MarkingText { get; set; } = string.Empty;
        public string MarkingPattern { get; set; } = string.Empty; // JSON pattern data
        public LaserParameters Parameters { get; set; } = new();
        public DateTime ScheduledTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        public JobStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public int Priority { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public enum JobStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }

    public class MarkingPattern
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<MarkingElement> Elements { get; set; } = new();
        public double Width { get; set; } // mm
        public double Height { get; set; } // mm
        public string PatternData { get; set; } = string.Empty; // JSON serialized pattern
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    public class MarkingElement
    {
        public ElementType Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Font { get; set; } = "Arial";
        public double FontSize { get; set; } = 12;
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public double Rotation { get; set; } // degrees
        public string Color { get; set; } = "#000000";
    }

    public enum ElementType
    {
        Text,
        Rectangle,
        Circle,
        Line,
        Image,
        Barcode,
        QRCode
    }
}