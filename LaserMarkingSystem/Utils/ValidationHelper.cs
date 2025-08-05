using LaserMarkingSystem.Models;

namespace LaserMarkingSystem.Utils
{
    public static class ValidationHelper
    {
        public static class ParameterRanges
        {
            public static class RTC3
            {
                public const double MinPower = 0;
                public const double MaxPower = 1000;
                public const double MinSpeed = 0;
                public const double MaxSpeed = 10000;
                public const double MinFrequency = 0;
                public const double MaxFrequency = 100000;
                public const double MinPulseWidth = 0;
                public const double MaxPulseWidth = 1000;
                public const double MinFocusPosition = -100;
                public const double MaxFocusPosition = 100;
                public const double MinMarkingDepth = 0;
                public const double MaxMarkingDepth = 10;
                public const int MinRepetitions = 1;
                public const int MaxRepetitions = 100;
                public const double MinSpotSize = 0.01;
                public const double MaxSpotSize = 10;
                public const double MinGalvoDelay = 0;
                public const double MaxGalvoDelay = 1000;
            }

            public static class RTC4
            {
                public const double MinPower = 0;
                public const double MaxPower = 1000;
                public const double MinSpeed = 0;
                public const double MaxSpeed = 10000;
                public const double MinFrequency = 0;
                public const double MaxFrequency = 100000;
                public const double MinPulseWidth = 0;
                public const double MaxPulseWidth = 1000;
                public const double MinFocusPosition = -100;
                public const double MaxFocusPosition = 100;
                public const double MinMarkingDepth = 0;
                public const double MaxMarkingDepth = 10;
                public const int MinRepetitions = 1;
                public const int MaxRepetitions = 100;
                public const double MinSpotSize = 0.01;
                public const double MaxSpotSize = 10;
                public const double MinGalvoDelay = 0;
                public const double MaxGalvoDelay = 1000;
                public const double MinAcceleration = 0;
                public const double MaxAcceleration = 10000;
                public const double MinJerk = 0;
                public const double MaxJerk = 10000;
                public const double MinCornerSpeed = 0;
                public const double MaxCornerSpeed = 10000;
                public const double MinJumpSpeed = 0;
                public const double MaxJumpSpeed = 10000;
                public const double MinJumpDelay = 0;
                public const double MaxJumpDelay = 1000;
                public const double MinMarkDelay = 0;
                public const double MaxMarkDelay = 1000;
                public const double MinPolyDelay = 0;
                public const double MaxPolyDelay = 1000;
                public const double MinLaserDelay = 0;
                public const double MaxLaserDelay = 1000;
                public const double MinSkipDelay = 0;
                public const double MaxSkipDelay = 1000;
                public const double MinArcDelay = 0;
                public const double MaxArcDelay = 1000;
                public const double MinArcAngle = 0;
                public const double MaxArcAngle = 360;
                public const double MinArcCorner = 0;
                public const double MaxArcCorner = 1000;
            }
        }

        public static ValidationResult ValidateLaserParameters(LaserParameters parameters, ControllerType controllerType)
        {
            var result = new ValidationResult { IsValid = true };

            if (controllerType == ControllerType.RTC3)
            {
                if (parameters.Power < ParameterRanges.RTC3.MinPower || parameters.Power > ParameterRanges.RTC3.MaxPower)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Power must be between {ParameterRanges.RTC3.MinPower} and {ParameterRanges.RTC3.MaxPower} Watts");
                }

                if (parameters.Speed < ParameterRanges.RTC3.MinSpeed || parameters.Speed > ParameterRanges.RTC3.MaxSpeed)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Speed must be between {ParameterRanges.RTC3.MinSpeed} and {ParameterRanges.RTC3.MaxSpeed} mm/s");
                }

                if (parameters.Frequency < ParameterRanges.RTC3.MinFrequency || parameters.Frequency > ParameterRanges.RTC3.MaxFrequency)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Frequency must be between {ParameterRanges.RTC3.MinFrequency} and {ParameterRanges.RTC3.MaxFrequency} Hz");
                }

                if (parameters.PulseWidth < ParameterRanges.RTC3.MinPulseWidth || parameters.PulseWidth > ParameterRanges.RTC3.MaxPulseWidth)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Pulse Width must be between {ParameterRanges.RTC3.MinPulseWidth} and {ParameterRanges.RTC3.MaxPulseWidth} microseconds");
                }

                if (parameters.FocusPosition < ParameterRanges.RTC3.MinFocusPosition || parameters.FocusPosition > ParameterRanges.RTC3.MaxFocusPosition)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Focus Position must be between {ParameterRanges.RTC3.MinFocusPosition} and {ParameterRanges.RTC3.MaxFocusPosition} mm");
                }

                if (parameters.MarkingDepth < ParameterRanges.RTC3.MinMarkingDepth || parameters.MarkingDepth > ParameterRanges.RTC3.MaxMarkingDepth)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Marking Depth must be between {ParameterRanges.RTC3.MinMarkingDepth} and {ParameterRanges.RTC3.MaxMarkingDepth} mm");
                }

                if (parameters.Repetitions < ParameterRanges.RTC3.MinRepetitions || parameters.Repetitions > ParameterRanges.RTC3.MaxRepetitions)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Repetitions must be between {ParameterRanges.RTC3.MinRepetitions} and {ParameterRanges.RTC3.MaxRepetitions}");
                }

                if (parameters.SpotSize < ParameterRanges.RTC3.MinSpotSize || parameters.SpotSize > ParameterRanges.RTC3.MaxSpotSize)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Spot Size must be between {ParameterRanges.RTC3.MinSpotSize} and {ParameterRanges.RTC3.MaxSpotSize} mm");
                }

                if (parameters.GalvoDelay < ParameterRanges.RTC3.MinGalvoDelay || parameters.GalvoDelay > ParameterRanges.RTC3.MaxGalvoDelay)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Galvo Delay must be between {ParameterRanges.RTC3.MinGalvoDelay} and {ParameterRanges.RTC3.MaxGalvoDelay} ms");
                }
            }
            else if (controllerType == ControllerType.RTC4)
            {
                // Validate RTC3 parameters first
                var rtc3Result = ValidateLaserParameters(parameters, ControllerType.RTC3);
                if (!rtc3Result.IsValid)
                {
                    result.IsValid = false;
                    result.Errors.AddRange(rtc3Result.Errors);
                }

                // Additional RTC4 specific validations would go here
                // For now, we'll use the same ranges as RTC3 for basic parameters
            }

            return result;
        }

        public static ValidationResult ValidateJob(MarkingJob job)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(job.JobName))
            {
                result.IsValid = false;
                result.Errors.Add("Job Name is required");
            }

            if (string.IsNullOrWhiteSpace(job.NumberPlate))
            {
                result.IsValid = false;
                result.Errors.Add("Number Plate is required");
            }

            if (string.IsNullOrWhiteSpace(job.CustomerName))
            {
                result.IsValid = false;
                result.Errors.Add("Customer Name is required");
            }

            if (string.IsNullOrWhiteSpace(job.MarkingText))
            {
                result.IsValid = false;
                result.Errors.Add("Marking Text is required");
            }

            if (string.IsNullOrWhiteSpace(job.OperatorName))
            {
                result.IsValid = false;
                result.Errors.Add("Operator Name is required");
            }

            if (job.Priority < 1 || job.Priority > 10)
            {
                result.IsValid = false;
                result.Errors.Add("Priority must be between 1 and 10");
            }

            if (job.ScheduledTime < DateTime.Now.AddDays(-30))
            {
                result.IsValid = false;
                result.Errors.Add("Scheduled Time cannot be more than 30 days in the past");
            }

            return result;
        }

        public static ValidationResult ValidateController(LaserController controller)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(controller.Name))
            {
                result.IsValid = false;
                result.Errors.Add("Controller Name is required");
            }

            if (string.IsNullOrWhiteSpace(controller.IpAddress))
            {
                result.IsValid = false;
                result.Errors.Add("IP Address is required");
            }

            if (!IsValidIpAddress(controller.IpAddress))
            {
                result.IsValid = false;
                result.Errors.Add("Invalid IP Address format");
            }

            if (controller.Port < 1 || controller.Port > 65535)
            {
                result.IsValid = false;
                result.Errors.Add("Port must be between 1 and 65535");
            }

            return result;
        }

        private static bool IsValidIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            var parts = ipAddress.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (!int.TryParse(part, out int value) || value < 0 || value > 255)
                    return false;
            }

            return true;
        }

        public static bool IsValidNumberPlate(string numberPlate)
        {
            if (string.IsNullOrWhiteSpace(numberPlate))
                return false;

            // Basic validation for Indian number plate format
            // Format: AA-12-BB-1234 or AA-12-1234
            var pattern = @"^[A-Z]{2}-\d{2}-[A-Z]{2}-\d{4}$|^[A-Z]{2}-\d{2}-\d{4}$";
            return System.Text.RegularExpressions.Regex.IsMatch(numberPlate, pattern);
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public string GetErrorMessage()
        {
            return string.Join("\n", Errors);
        }
    }
}