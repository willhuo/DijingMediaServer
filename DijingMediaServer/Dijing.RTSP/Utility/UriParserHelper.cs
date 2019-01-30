using System;
using System.Collections.Generic;
using System.Text;

namespace Dijing.RTSP.Utility
{
    public class UriParserHelper
    {
        public static void RegisterRtspUri()
        {
            if(UriParser.IsKnownScheme("rtsp")==false)
            {
                UriParser.Register(new HttpStyleUriParser(), "rtsp", 554);
            }
        }
    }
}
