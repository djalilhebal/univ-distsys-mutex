using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Meikodayo;

namespace SiteProcess
{
    /**
     * Maekawa.
     * #Permission #PermissionArbitre
     *
     * NOTES
     * ------
     * - Message complexity: `3 * card(n)` (PS: in our algorithm, `card(Ri)` ~ `root(n)`).
     * - Synchronization delay: `[ 2t ; Infinity [`
     * 
     * - TLDR:
     *      + One permission at a time (`permissionDisp`)
     *      + Uses a queue for deferred requests
     *      + When we received our permission(i), we send it to a queued requester.
     */
    public class Maekawa : AbstractAlgo
    {

        private char i;
        private SiteEtat etati = SiteEtat.Dehors;
        private bool permissionDisp = true;
        /**
         * filei est la liste des processus qui ont demandé la permission mais à
         *  qui on ne peut pas donner la permission
         */
        private Queue<char> requestFilei = new();
        private HashSet<char> attendusi;
        private HashSet<char> Ri;

        public Maekawa(char siteId, char[] siteIds)
            : base(siteId, siteIds)
        {
            i = siteId;
            Ri = siteIds.Where(j => j != i).ToHashSet();
        }
        
        // ---

        public void DoEnter()
        {
            etati = SiteEtat.Demandeur;
            attendusi = Ri.ToHashSet();
            
            // ∀j ∈ Ri : envoyer requête à j ;
            foreach (var j in Ri)
            {
                SendMessage(new MeikoMessage
                {
                    Type = "Request",
                    Dst = j,
                    Src = i,
                });
            }
            
            while (! (attendusi.Count == 0) )
                /* no-op */;
            
            etati = SiteEtat.Dedans;
        }

        public void DoLeave()
        {
            etati = SiteEtat.Dehors;

            foreach (var j in Ri)
            {
                SendMessage(new MeikoMessage
                {
                    ["Pretty"] = $"permission({i})",
                    Type = "Permission",
                    // it will be converted to JSON string either way, might as well be explicit
                    ["Of"] = i.ToString(),
                    Dst = j,
                    Src = i,
                });
            }
        }

        public void OnGotRequest(MeikoMessage m)
        {
            var j = m.Src;

            if (permissionDisp)
            {
                SendMessage(new MeikoMessage
                {
                    ["Pretty"] = $"permission({i})",
                    Type = "Permission",
                    Dst = j,
                });
                
                permissionDisp = false;
            }
            else
            {
                requestFilei.Enqueue(j);
            }
        }
        
        public void OnGotPermissionJ(MeikoMessage m)
        {
            var j = m.Src;

            attendusi.Remove(j);
        }

        public void OnGotPermissionI(MeikoMessage m)
        {
            if (requestFilei.Count == 0)
            {
                permissionDisp = true;
            }
            else
            {
                var k = requestFilei.Dequeue();
                SendMessage(new MeikoMessage
                    {
                        ["Pretty"] = $"permission({i})",
                        Type = "Permission",
                        ["Of"] = i.ToString(),
                        Dst = k,
                    });
            }
        }
        
        public void OnGotPermission(MeikoMessage m)
        {
            var whose = ((string?)m["Of"])[0];
            var isMine = whose == i;
            if (isMine)
            {
                OnGotPermissionI(m);
            }
            else
            {
                OnGotPermissionJ(m);
            }
        }

    }
}
