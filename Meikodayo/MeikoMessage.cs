using System.Collections;

namespace Meikodayo
{
    public class MeikoMessage : Hashtable
    {
        public string Id { get; set; }

        /**
         * Source. From.
         */
        public char Src { get; set; }

        /**
         * Destination. To.
         */
        public char Dst { get; set; }
        
        /**
         * "Init" or some algorithm-defined message type (e.g. "Request", "Permission")
         */
        public string Type { get; set; }
        
        // Used for init
        public char? SiteId { get; set; }
        public char[]? SiteIds { get; set; }
        public string? AlgoId { get; set; }
        
        // Others...

        // H is for hj
        // H is for horloge (French for _Clock_)
        // TODO: Rename to timestamp and make it `long`?
        public int? H { get; set; }
    }
    
}