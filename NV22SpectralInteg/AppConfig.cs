using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg
{
    public class AppConfig
    {
        public required string ComPort { get; set; }
        public bool IsDevelopment { get; set; }
    }
}
