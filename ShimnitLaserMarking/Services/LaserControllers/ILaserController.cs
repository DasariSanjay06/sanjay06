using ShimnitLaserMarking.Models;
using System;
using System.Threading.Tasks;

namespace ShimnitLaserMarking.Services.LaserControllers
{
    public interface ILaserController : IDisposable
    {
        /// <summary>
        /// Controller type (RTC3 or RTC4)
        /// </summary>
        LaserControllerType ControllerType { get; }

        /// <summary>
        /// Controller name/identifier
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Communication port (e.g., COM1)
        /// </summary>
        string CommunicationPort { get; set; }

        /// <summary>
        /// Connection status
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Controller ready for operations
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Current laser status
        /// </summary>
        LaserStatus LaserStatus { get; }

        /// <summary>
        /// Last error message
        /// </summary>
        string? LastError { get; }

        /// <summary>
        /// Connect to the laser controller
        /// </summary>
        Task<bool> ConnectAsync();

        /// <summary>
        /// Disconnect from the laser controller
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Initialize the controller with default parameters
        /// </summary>
        Task<bool> InitializeAsync();

        /// <summary>
        /// Set laser parameters
        /// </summary>
        Task<bool> SetParametersAsync(LaserParameters parameters);

        /// <summary>
        /// Start marking operation
        /// </summary>
        Task<bool> StartMarkingAsync(string text, double x, double y, double rotation = 0);

        /// <summary>
        /// Stop marking operation
        /// </summary>
        Task<bool> StopMarkingAsync();

        /// <summary>
        /// Pause marking operation
        /// </summary>
        Task<bool> PauseMarkingAsync();

        /// <summary>
        /// Resume marking operation
        /// </summary>
        Task<bool> ResumeMarkingAsync();

        /// <summary>
        /// Move to position without marking
        /// </summary>
        Task<bool> MoveToPositionAsync(double x, double y, double z = 0);

        /// <summary>
        /// Get current position
        /// </summary>
        Task<(double X, double Y, double Z)> GetPositionAsync();

        /// <summary>
        /// Test laser with low power pulse
        /// </summary>
        Task<bool> TestLaserAsync();

        /// <summary>
        /// Calibrate the controller
        /// </summary>
        Task<bool> CalibrateAsync();

        /// <summary>
        /// Get controller status information
        /// </summary>
        Task<ControllerStatus> GetStatusAsync();

        /// <summary>
        /// Reset controller to default state
        /// </summary>
        Task<bool> ResetAsync();

        /// <summary>
        /// Event fired when marking progress changes
        /// </summary>
        event EventHandler<MarkingProgressEventArgs>? MarkingProgress;

        /// <summary>
        /// Event fired when marking is completed
        /// </summary>
        event EventHandler<MarkingCompletedEventArgs>? MarkingCompleted;

        /// <summary>
        /// Event fired when an error occurs
        /// </summary>
        event EventHandler<LaserErrorEventArgs>? ErrorOccurred;

        /// <summary>
        /// Event fired when controller status changes
        /// </summary>
        event EventHandler<StatusChangedEventArgs>? StatusChanged;
    }

    public enum LaserStatus
    {
        Disconnected,
        Connected,
        Initializing,
        Ready,
        Marking,
        Paused,
        Error,
        Calibrating
    }

    public class ControllerStatus
    {
        public LaserStatus Status { get; set; }
        public double Temperature { get; set; }
        public double PowerLevel { get; set; }
        public int CurrentSpeed { get; set; }
        public bool LaserEnabled { get; set; }
        public bool SafetySystemActive { get; set; }
        public double OperatingHours { get; set; }
        public int MarkingJobsCompleted { get; set; }
        public string FirmwareVersion { get; set; } = string.Empty;
        public DateTime LastUpdate { get; set; }
    }

    public class MarkingProgressEventArgs : EventArgs
    {
        public int PercentComplete { get; set; }
        public string CurrentOperation { get; set; } = string.Empty;
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan? EstimatedTimeRemaining { get; set; }
    }

    public class MarkingCompletedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan TotalTime { get; set; }
        public int QualityScore { get; set; } // 1-10
    }

    public class LaserErrorEventArgs : EventArgs
    {
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
        public LaserErrorSeverity Severity { get; set; }
    }

    public class StatusChangedEventArgs : EventArgs
    {
        public LaserStatus OldStatus { get; set; }
        public LaserStatus NewStatus { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum LaserErrorSeverity
    {
        Information,
        Warning,
        Error,
        Critical
    }
}