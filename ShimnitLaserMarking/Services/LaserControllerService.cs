using Serilog;
using ShimnitLaserMarking.Models;
using ShimnitLaserMarking.Services.LaserControllers;
using System;
using System.Threading.Tasks;

namespace ShimnitLaserMarking.Services
{
    public class LaserControllerService : IDisposable
    {
        private ILaserController? _currentController;
        private bool _disposed;

        public bool IsConnected => _currentController?.IsConnected ?? false;
        public bool IsReady => _currentController?.IsReady ?? false;
        public LaserControllerType? CurrentControllerType => _currentController?.ControllerType;

        public event EventHandler<MarkingProgressEventArgs>? MarkingProgress;
        public event EventHandler<MarkingCompletedEventArgs>? MarkingCompleted;
        public event EventHandler<LaserErrorEventArgs>? ErrorOccurred;
        public event EventHandler<StatusChangedEventArgs>? StatusChanged;

        public async Task<bool> ConnectAsync(LaserControllerType controllerType)
        {
            try
            {
                // Disconnect existing controller if any
                if (_currentController != null)
                {
                    await DisconnectAsync();
                }

                // Create new controller instance
                _currentController = CreateController(controllerType);
                
                if (_currentController == null)
                {
                    Log.Error("Failed to create controller instance for type {ControllerType}", controllerType);
                    return false;
                }

                // Subscribe to events
                SubscribeToControllerEvents();

                // Connect and initialize
                var connected = await _currentController.ConnectAsync();
                if (connected)
                {
                    var initialized = await _currentController.InitializeAsync();
                    if (initialized)
                    {
                        Log.Information("Successfully connected and initialized {ControllerType} controller", controllerType);
                        return true;
                    }
                    else
                    {
                        Log.Error("Failed to initialize {ControllerType} controller", controllerType);
                        await _currentController.DisconnectAsync();
                        return false;
                    }
                }
                else
                {
                    Log.Error("Failed to connect to {ControllerType} controller", controllerType);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error connecting to {ControllerType} controller", controllerType);
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_currentController != null)
                {
                    UnsubscribeFromControllerEvents();
                    await _currentController.DisconnectAsync();
                    _currentController.Dispose();
                    _currentController = null;
                    
                    Log.Information("Controller disconnected successfully");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error disconnecting controller");
                throw;
            }
        }

        public async Task<bool> StartMarkingAsync(MarkingJob job)
        {
            try
            {
                if (_currentController == null || !_currentController.IsReady)
                {
                    Log.Warning("Controller not ready for marking");
                    return false;
                }

                // Set parameters based on job and controller type
                var parameters = CreateParametersFromJob(job);
                var parametersSet = await _currentController.SetParametersAsync(parameters);
                
                if (!parametersSet)
                {
                    Log.Error("Failed to set controller parameters for job {JobNumber}", job.JobNumber);
                    return false;
                }

                // Update job status
                job.Status = MarkingJobStatus.InProgress;
                job.StartedDate = DateTime.Now;
                await DatabaseService.UpdateMarkingJobAsync(job);

                // Start marking
                var success = await _currentController.StartMarkingAsync(
                    job.MarkingText, 
                    job.PositionX, 
                    job.PositionY, 
                    job.Rotation);

                if (success)
                {
                    Log.Information("Started marking job {JobNumber} for vehicle {VehicleNumber}", 
                        job.JobNumber, job.VehicleNumberPlate);
                }
                else
                {
                    // Update job status to failed
                    job.Status = MarkingJobStatus.Failed;
                    job.ErrorMessage = _currentController.LastError;
                    await DatabaseService.UpdateMarkingJobAsync(job);
                    
                    Log.Error("Failed to start marking job {JobNumber}: {Error}", 
                        job.JobNumber, _currentController.LastError);
                }

                return success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error starting marking for job {JobNumber}", job.JobNumber);
                
                // Update job status to failed
                job.Status = MarkingJobStatus.Failed;
                job.ErrorMessage = ex.Message;
                await DatabaseService.UpdateMarkingJobAsync(job);
                
                return false;
            }
        }

        public async Task<bool> StopMarkingAsync()
        {
            try
            {
                if (_currentController == null)
                    return false;

                return await _currentController.StopMarkingAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error stopping marking");
                return false;
            }
        }

        public async Task<bool> PauseMarkingAsync()
        {
            try
            {
                if (_currentController == null)
                    return false;

                return await _currentController.PauseMarkingAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error pausing marking");
                return false;
            }
        }

        public async Task<bool> ResumeMarkingAsync()
        {
            try
            {
                if (_currentController == null)
                    return false;

                return await _currentController.ResumeMarkingAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error resuming marking");
                return false;
            }
        }

        public async Task<bool> TestLaserAsync()
        {
            try
            {
                if (_currentController == null || !_currentController.IsReady)
                    return false;

                return await _currentController.TestLaserAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error testing laser");
                return false;
            }
        }

        public async Task<bool> CalibrateAsync()
        {
            try
            {
                if (_currentController == null || !_currentController.IsConnected)
                    return false;

                return await _currentController.CalibrateAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error calibrating controller");
                return false;
            }
        }

        public async Task<ControllerStatus?> GetStatusAsync()
        {
            try
            {
                if (_currentController == null)
                    return null;

                return await _currentController.GetStatusAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting controller status");
                return null;
            }
        }

        public async Task<(double X, double Y, double Z)> GetPositionAsync()
        {
            try
            {
                if (_currentController == null)
                    return (0, 0, 0);

                return await _currentController.GetPositionAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting controller position");
                return (0, 0, 0);
            }
        }

        public async Task<bool> MoveToPositionAsync(double x, double y, double z = 0)
        {
            try
            {
                if (_currentController == null || !_currentController.IsReady)
                    return false;

                return await _currentController.MoveToPositionAsync(x, y, z);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error moving to position");
                return false;
            }
        }

        private ILaserController? CreateController(LaserControllerType controllerType)
        {
            try
            {
                var config = Program.Configuration.GetSection($"LaserSettings:{controllerType}");
                var port = config["CommunicationPort"] ?? "COM1";
                var name = $"{controllerType} Controller";

                return controllerType switch
                {
                    LaserControllerType.RTC3 => new RTC3Controller(name, port),
                    LaserControllerType.RTC4 => new RTC4Controller(name, port),
                    _ => null
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating controller instance for type {ControllerType}", controllerType);
                return null;
            }
        }

        private LaserParameters CreateParametersFromJob(MarkingJob job)
        {
            var parameters = job.ControllerType == LaserControllerType.RTC4 
                ? LaserParameters.GetDefaultRTC4Parameters() 
                : LaserParameters.GetDefaultRTC3Parameters();

            // Override with job-specific parameters
            parameters.Power = job.LaserPower;
            parameters.Speed = job.LaserSpeed;
            parameters.PulseFrequency = job.PulseFrequency;
            parameters.Passes = job.Passes;
            parameters.MarkingDepth = job.MarkingDepth;
            parameters.StartPositionX = job.PositionX;
            parameters.StartPositionY = job.PositionY;
            parameters.ZAxisPosition = job.PositionZ;
            parameters.Rotation = job.Rotation;

            return parameters;
        }

        private void SubscribeToControllerEvents()
        {
            if (_currentController == null) return;

            _currentController.MarkingProgress += OnMarkingProgress;
            _currentController.MarkingCompleted += OnMarkingCompleted;
            _currentController.ErrorOccurred += OnErrorOccurred;
            _currentController.StatusChanged += OnStatusChanged;
        }

        private void UnsubscribeFromControllerEvents()
        {
            if (_currentController == null) return;

            _currentController.MarkingProgress -= OnMarkingProgress;
            _currentController.MarkingCompleted -= OnMarkingCompleted;
            _currentController.ErrorOccurred -= OnErrorOccurred;
            _currentController.StatusChanged -= OnStatusChanged;
        }

        private void OnMarkingProgress(object? sender, MarkingProgressEventArgs e)
        {
            MarkingProgress?.Invoke(this, e);
        }

        private async void OnMarkingCompleted(object? sender, MarkingCompletedEventArgs e)
        {
            try
            {
                MarkingCompleted?.Invoke(this, e);
                
                // Log completion to database
                await DatabaseService.LogSystemEventAsync(
                    e.Success ? "Information" : "Error",
                    $"Marking operation completed. Success: {e.Success}, Time: {e.TotalTime:mm\\:ss}, Quality: {e.QualityScore}/10",
                    "LaserController",
                    e.Success ? null : new Exception(e.ErrorMessage ?? "Unknown error"));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling marking completed event");
            }
        }

        private async void OnErrorOccurred(object? sender, LaserErrorEventArgs e)
        {
            try
            {
                ErrorOccurred?.Invoke(this, e);
                
                // Log error to database
                await DatabaseService.LogSystemEventAsync(
                    e.Severity.ToString(),
                    e.ErrorMessage,
                    "LaserController",
                    e.Exception);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling laser error event");
            }
        }

        private void OnStatusChanged(object? sender, StatusChangedEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
            Log.Debug("Controller status changed from {OldStatus} to {NewStatus}", e.OldStatus, e.NewStatus);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    DisconnectAsync().Wait(5000); // Wait up to 5 seconds for graceful disconnect
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during service disposal");
                }

                _disposed = true;
            }
        }
    }
}