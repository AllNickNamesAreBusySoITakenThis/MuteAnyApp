using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuteAnyApp.Core.Enums
{
    public enum ActionType
    {
        [Description("Замутить")]
        Mute,

        [Description("Размутить")]
        Unmute,

        [Description("Установить на")]
        SetTo
    }
}
