using MuteAnyApp.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuteAnyApp.Core.Types
{
    public class ProcessModel
    {
        public ProcessModel(int processId, string processName, SoundChangeAction processAction)
        {
            ProcessId = processId;
            ProcessName = processName;
            ProcessAction = processAction;
        }

        public ProcessModel(int processId, string processName)
        {
            ProcessId = processId;
            ProcessName = processName;
        }

        public int ProcessId { get; set; }

        public string ProcessName { get; set; }

        public SoundChangeAction? ProcessAction { get; set; }
    }
}
