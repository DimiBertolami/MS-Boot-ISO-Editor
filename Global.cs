﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimsISOTweaker
{
    internal class Global
    {
        public static int PID { get; set; } = 0;
        public static Process ps { get; set; }
        public static string Args { get; set; } = string.Empty;
        public static bool RedirectStandardInput { get; set; } = true;
        public static bool RedirectStandardOutput { get; set; } = false;
        public static bool RedirectStandardError { get; set; } = false;
<<<<<<< HEAD
        //public string ps { get; set; } = string.Empty;
=======
>>>>>>> 42ec80438a994506d6e8abded9889aec8778b1c1
    }
}
