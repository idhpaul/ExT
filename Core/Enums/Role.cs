using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExT.Core.Enums
{
    public enum Role
    {
        None,
        [Description("리더⚡")]
        Leader,
        [Description("크루💪")]
        Crew,
        [Description("Developer🚀")]
        Developer,
        [Description("Bot🤖")]
        Bot
    }
}
