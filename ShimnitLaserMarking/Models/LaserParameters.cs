using System;
using System.ComponentModel.DataAnnotations;

namespace ShimnitLaserMarking.Models
{
    public class LaserParameters
    {
        // Basic Parameters
        [Range(0, 100)]
        public double Power { get; set; } = 50.0;

        [Range(1, 10000)]
        public int Speed { get; set; } = 1000;

        [Range(1000, 100000)]
        public int PulseFrequency { get; set; } = 20000;

        [Range(1, 20)]
        public int Passes { get; set; } = 1;

        // Advanced Parameters
        [Range(0.001, 10.0)]
        public double MarkingDepth { get; set; } = 0.1;

        [Range(0, 1000)]
        public int PulseWidth { get; set; } = 100; // microseconds

        [Range(0, 100)]
        public double DutyCycle { get; set; } = 50.0;

        // Position and Movement
        [Range(-1000, 1000)]
        public double StartPositionX { get; set; } = 0.0;

        [Range(-1000, 1000)]
        public double StartPositionY { get; set; } = 0.0;

        [Range(-100, 100)]
        public double FocusPosition { get; set; } = 0.0;

        [Range(0, 360)]
        public double Rotation { get; set; } = 0.0;

        // Jump and Mark Delays
        [Range(0, 10000)]
        public int JumpDelay { get; set; } = 100; // microseconds

        [Range(0, 10000)]
        public int MarkDelay { get; set; } = 50; // microseconds

        [Range(0, 10000)]
        public int PolygonDelay { get; set; } = 20; // microseconds

        // Scanner Settings (RTC Specific)
        [Range(1, 1000)]
        public int ScannerSpeed { get; set; } = 500;

        [Range(1, 1000)]
        public int ScannerAcceleration { get; set; } = 100;

        // Wobble Parameters (for certain applications)
        public bool WobbleEnabled { get; set; } = false;
        
        [Range(0, 100)]
        public double WobbleAmplitude { get; set; } = 0.0;

        [Range(1, 1000)]
        public int WobbleFrequency { get; set; } = 100;

        // Quality Settings
        [Range(0.001, 1.0)]
        public double LineSpacing { get; set; } = 0.1;

        [Range(1, 10)]
        public int QualityFactor { get; set; } = 5;

        // Safety Parameters
        [Range(0, 30)]
        public int LaserOnDelay { get; set; } = 1000; // milliseconds

        [Range(0, 30)]
        public int LaserOffDelay { get; set; } = 500; // milliseconds

        // Temperature and Environmental
        public bool TemperatureCompensation { get; set; } = true;
        public bool VibrationCompensation { get; set; } = false;

        // RTC4 Specific Parameters
        public bool Is3DMode { get; set; } = false;
        
        [Range(-100, 100)]
        public double ZAxisPosition { get; set; } = 0.0;

        public bool OpticalPowerControl { get; set; } = false;
        public bool DigitalPowerControl { get; set; } = true;

        // Correction Parameters
        public bool GeometricCorrection { get; set; } = true;
        public string CorrectionFile { get; set; } = "";

        // Validation
        public bool IsValid()
        {
            return Power >= 0 && Power <= 100 &&
                   Speed >= 1 && Speed <= 10000 &&
                   PulseFrequency >= 1000 && PulseFrequency <= 100000 &&
                   Passes >= 1 && Passes <= 20;
        }

        public static LaserParameters GetDefaultRTC3Parameters()
        {
            return new LaserParameters
            {
                Power = 50.0,
                Speed = 1000,
                PulseFrequency = 20000,
                Passes = 1,
                ScannerSpeed = 500,
                ScannerAcceleration = 100,
                JumpDelay = 100,
                MarkDelay = 50,
                PolygonDelay = 20,
                QualityFactor = 5,
                TemperatureCompensation = true,
                GeometricCorrection = true,
                DigitalPowerControl = true
            };
        }

        public static LaserParameters GetDefaultRTC4Parameters()
        {
            return new LaserParameters
            {
                Power = 50.0,
                Speed = 1200,
                PulseFrequency = 25000,
                Passes = 1,
                ScannerSpeed = 600,
                ScannerAcceleration = 120,
                JumpDelay = 80,
                MarkDelay = 40,
                PolygonDelay = 15,
                QualityFactor = 6,
                TemperatureCompensation = true,
                GeometricCorrection = true,
                DigitalPowerControl = true,
                OpticalPowerControl = true,
                Is3DMode = false
            };
        }
    }
}