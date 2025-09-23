using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NV22SpectralInteg.InactivityManager
{
    /// <summary>
    /// Manages user inactivity detection and triggers a logout action.
    /// This is a static class, so it can be accessed from anywhere in the application.
    /// </summary>
    /// <summary>
    /// Manages user inactivity detection and triggers a logout action.
    /// This is a static class, so it can be accessed from anywhere in the application.
    /// </summary>
    public static class KioskIdleManager
    {
        private static System.Windows.Forms.Timer _inactivityTimer;
        private static System.Windows.Forms.Timer _countdownTimer;

        private static CountdownForm _countdownForm;

        private static int _maxInactivitySeconds;
        private const int CountdownDurationSeconds = 10;
        private static int _currentCountdownValue;

        private static Action _logoutAction;
        private static ActivityMessageFilter _messageFilter;

        /// <summary>
        /// Initializes the InactivityManager. Must be called once when the application starts.
        /// </summary>
        /// <param name="logoutAction">The method to call to perform the logout.</param>
        public static void Initialize(Action logoutAction)
        {
            _logoutAction = logoutAction ?? throw new ArgumentNullException(nameof(logoutAction));

            _inactivityTimer = new System.Windows.Forms.Timer();
            _inactivityTimer.Tick += InactivityTimer_Tick;

            _countdownTimer = new System.Windows.Forms.Timer { Interval = 1000 }; // Ticks every second
            _countdownTimer.Tick += CountdownTimer_Tick;

            _messageFilter = new ActivityMessageFilter();
            Application.AddMessageFilter(_messageFilter);
        }

        /// <summary>
        /// Starts the inactivity timer for the current screen.
        /// Call this in the Load or Activated event of each form that requires a timeout.
        /// </summary>
        /// <param name="timeoutSeconds">The number of seconds of inactivity before showing the warning.</param>
        public static void Start(int timeoutSeconds)
        {
            _maxInactivitySeconds = timeoutSeconds;
            ResetTimer();
        }

        /// <summary>
        /// Stops the timers completely. Call this when no user is logged in (e.g., on the login screen).
        /// </summary>
        public static void Stop()
        {
            _inactivityTimer.Stop();
            _countdownTimer.Stop();
            CloseCountdownForm();
        }

        /// <summary>
        /// Resets the inactivity timer. This is called automatically when user activity is detected.
        /// </summary>
        public static void ResetTimer()
        {
            _countdownTimer.Stop();
            CloseCountdownForm();

            _inactivityTimer.Interval = _maxInactivitySeconds * 1000;
            _inactivityTimer.Start();
        }

        /// <summary>
        /// This event fires when the main inactivity period has elapsed.
        /// </summary>
        private static void InactivityTimer_Tick(object sender, EventArgs e)
        {
            _inactivityTimer.Stop();

            _currentCountdownValue = CountdownDurationSeconds;
            ShowCountdownForm();
            _countdownTimer.Start();
        }

        /// <summary>
        /// This event fires every second during the visible countdown.
        /// </summary>
        private static void CountdownTimer_Tick(object sender, EventArgs e)
        {
            _currentCountdownValue--;

            if (_currentCountdownValue > 0)
            {
                _countdownForm?.UpdateMessage($"Logging out in {_currentCountdownValue} seconds...");
            }
            else
            {
                _countdownTimer.Stop();
                CloseCountdownForm();
                _logoutAction?.Invoke();
            }
        }

        private static void ShowCountdownForm()
        {
            if (_countdownForm == null || _countdownForm.IsDisposed)
            {
                _countdownForm = new CountdownForm();
            }
            _countdownForm.UpdateMessage($"Logging out in {CountdownDurationSeconds} seconds...");
            _countdownForm.Show();
        }

        private static void CloseCountdownForm()
        {
            if (_countdownForm != null && !_countdownForm.IsDisposed)
            {
                _countdownForm.Hide();
            }
        }

        /// <summary>
        /// A message filter class to capture touch, pen, or mouse events globally.
        /// </summary>
        private class ActivityMessageFilter : IMessageFilter
        {
            // --- MODIFIED: Use WM_POINTERDOWN for robust touch/pen/mouse detection ---
            // This is the modern, unified message for any "down" action on the screen.
            private const int WM_POINTERDOWN = 0x0246;
            //private const int WM_KEYDOWN = 0x0100;
            //private const int WM_MOUSEMOVE = 0x0200;

            public bool PreFilterMessage(ref Message m)
            {
                // --- MODIFIED: The condition now checks for the pointer down message ---
                if (m.Msg == WM_POINTERDOWN)
                {
                    KioskIdleManager.ResetTimer();
                }
                return false;
            }
        }
    }

}
