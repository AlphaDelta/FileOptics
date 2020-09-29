using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileOptics.TESV
{
    public class ChangeForm
    {
        public uint RefID, ChangeFlags;
        public ChangeFormType Type;

        public bool HasFlag(ChangeFormFlagACHR flag)
        {
            return (ChangeFlags & (uint)flag) == (uint)flag;
        }
    }
}
