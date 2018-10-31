using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevelCombiner
{
    public struct CombinerCommand
    {
        public bool fog;
        public bool alpha;
        public bool solid;
        public bool opaque;

        public CombinerCommand(Int64 FCcmd)
        {
            fog = false;
            alpha = false;
            solid = false;
            opaque = false;

            if ((UInt64)FCcmd == 0xFC127E24FFFFF9FC)
                solid = true;

            if ((UInt64)FCcmd == 0xFC121824FF33FFFF)
                opaque = true;

            if ((UInt64)FCcmd == 0xFC122E24FFFFFBFD || (UInt64)FCcmd == 0xFC127E24FFFFFBFD)
                alpha = true;

            if ((UInt64)FCcmd == 0xFC127FFFFFFFF838)
            {
                fog = true;
                solid = true;
            }

            if ((UInt64)FCcmd == 0xFCFFFFFFFFFCF238 || (UInt64)FCcmd == 0xFC127FFFFFFFF238)
            {
                fog = true;
                opaque = true;
            }

            if ((UInt64)FCcmd == 0xFC127FFFFFFFFA38)
            {
                fog = true;
                alpha = true;
            }
        }

        public CombinerCommand(string name)
        {
            fog = name.Contains("fog");
            alpha = name.Contains("alpha");
            opaque = name.Contains("opaque");
            solid = name.Contains("solid");
        }

        public CombinerCommand(bool fog = false, bool alpha = false, bool opaque = false, bool solid = false)
        {
            this.fog = fog;
            this.alpha = alpha;
            this.opaque = opaque;
            this.solid = solid;
        }

        public UInt64 GetFCcmd()
        {
            if (((solid ? 1 : 0) + (opaque ? 1 : 0) + (alpha ? 1 : 0)) > 1)
                return 0;

            if (!fog)
            {
                if (solid)
                    return 0xFC127E24FFFFF9FC;

                if (opaque)
                    return 0xFC121824FF33FFFF;

                if (alpha)
                    return 0xFC122E24FFFFFBFD;
            }
            else
            {
                if (solid)
                    return 0xFC127FFFFFFFF838;

                if (opaque)
                    return 0xFCFFFFFFFFFCF238;

                if (alpha)
                    return 0xFC127FFFFFFFFA38;
            }

            return 0;
        }

        public override string ToString()
        {
            string name = "";
            if (fog)
                name += "fog ";
            if (alpha)
                name += "alpha ";
            if (opaque)
                name += "opaque ";
            if (solid)
                name += "solid ";

            return name.Trim();
        }

        public static CombinerCommand GetNewCombiner(DisplayListRegion dlRegion)
        {
            CombinerCommand oldCmd = new CombinerCommand(dlRegion.FCcmdfirst);

            if (dlRegion.isEnvcolorEnabled && dlRegion.isFogEnabled)
                return new CombinerCommand(fog: true, alpha: true);

            if (dlRegion.isEnvcolorEnabled && !dlRegion.isFogEnabled)
                return new CombinerCommand(alpha: true);

            if (dlRegion.isFogEnabled)
            {
                if (oldCmd.solid)
                    return new CombinerCommand(fog: true, solid: true);

                if (oldCmd.opaque)
                    return new CombinerCommand(fog: true, opaque: true);
            }
            else
            {
                if (oldCmd.solid)
                    return new CombinerCommand(solid: true);

                if (oldCmd.opaque)
                    return new CombinerCommand(opaque: true);
            }

            return oldCmd;
        }
    }

    static class OtherMode
    {
        public static UInt64 GetB9Cmd(int layer)
        {
            if (layer == 0)
                return 0xB900031D00552230;
            if (layer == 1) 
                return 0xB900031D00552078;
            if (layer == 2)
                return 0xB900031D00552D58;
            if (layer == 3)
                return 0xB900031D00552478;
            if (layer == 4)
                return 0xB900031D00553078;
            if (layer == 5)
                return 0xB900031D005049D8;
            if (layer == 6)
                return 0xB900031D00504DD8;
            if (layer == 7)
                return 0xB900031D005045D8;

            return 0;
        }
    }
}
