using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadLock.Classes
{
    internal sealed class HandleLockerDetails
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ProcessId { get; set; }
    }
}
