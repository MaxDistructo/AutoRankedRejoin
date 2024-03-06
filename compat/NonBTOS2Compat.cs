using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRequeue.compat
{
    class NonBTOS2Compat : IBTOS2Compat
    {
        public bool CheckEnabled()
        {
            return false;
        }

        public bool CheckModdedGamemode()
        {
            return false;
        }

        public void JoinCasualMode()
        {
            //STUB
            return;
        }
    }
}
