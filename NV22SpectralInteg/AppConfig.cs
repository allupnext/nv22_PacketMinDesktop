﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg
{
    public class AppConfig
    {
        public string ComPort { get; set; }
        public bool IsDevelopment { get; set; }
        public Dictionary<string, int> ScreenTimeouts { get; set; }
        public int CounterTime { get; set; }

    }
}
