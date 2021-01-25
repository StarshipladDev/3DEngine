using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2
{
    public class Corpse:Entity
    {

        public Corpse(String baseFile, int sec, bool animated = true) : base(baseFile, sec, animated)
        {
            dying = true;
        }
    }
}
