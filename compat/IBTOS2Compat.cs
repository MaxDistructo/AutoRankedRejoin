using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRequeue.compat
{
    public interface IBTOS2Compat
    {
        void JoinCasualMode();
        bool CheckEnabled();
        bool CheckModdedGamemode();
    }
}
