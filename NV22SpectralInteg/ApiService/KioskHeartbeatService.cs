// KioskHeartbeatService.cs
using System;
using System.Windows.Forms;

namespace NV22SpectralInteg.Services
{
    public static class KioskHeartbeatService
    {
        private static System.Windows.Forms.Timer _availabilityTimer;
        private const int IntervalMinutes = 15;

        /// <summary>
        /// Initializes and starts the background timer. Call this once when the application starts.
        /// </summary>
        public static void Initialize()
        {
            _availabilityTimer = new System.Windows.Forms.Timer();
            _availabilityTimer.Interval = IntervalMinutes * 60 * 1000; // 15 minutes
            //_availabilityTimer.Interval = 10000; // 10 seconds for testing
            _availabilityTimer.Tick += OnTimerTick;
            _availabilityTimer.Start();

            // Optionally, trigger the first log immediately on startup
            // after a short delay to allow the main app to load.
            // OnTimerTick(null, EventArgs.Empty); 
            Logger.MachineLog($"🚀 Kiosk availability log service initialized. Will run every {IntervalMinutes} minutes.");
        }

        private static async void OnTimerTick(object sender, EventArgs e)
        {
            try
            {
                // ✅ This is the most important check.
                // It ensures we don't send a request until the Kiosk ID is set.
                if (string.IsNullOrEmpty(AppSession.KioskId))
                {
                    Logger.MachineLog("💓 Availability tick skipped: KioskId not yet set in AppSession.");
                    return; // Do nothing and wait for the next tick
                }

                Logger.MachineLog("💓 Availability timer ticked. Logging machine status...");
                await ApiService.LogMachineAvailabilityAsync();
            }
            catch (Exception ex)
            {
                // Catch all exceptions to prevent the application from crashing
                Logger.LogError("🚨 Unhandled exception in availability log tick.", ex);
            }
        }
    }
}