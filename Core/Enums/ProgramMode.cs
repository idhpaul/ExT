using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExT.Core.Enums
{
    public enum ProgramMode
    {
        [Description("Dev 모드 입니다.")]
        Dev,
        [Description("Live 모드 입니다.")]
        Live,
    }
}
