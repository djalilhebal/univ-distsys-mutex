using System.Diagnostics;
using System.IO;

namespace Meikodayo
{
    /**
     * A node.
     */
    public class MeikoSite {
        public char Id { get; set; }
        public StreamWriter Stdin { get; set; }
        public StreamReader Stdout { get; set; }

        public StreamReader Stderr { get; set; }
        
        /**
         * The backing process.
         */
        public Process Process { get; set; }
    }
    
}