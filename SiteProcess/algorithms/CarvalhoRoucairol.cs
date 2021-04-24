using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meikodayo;

namespace SiteProcess
{
    /**
     * Carvalho-Roucairol
     * #Permission #PermissionIndividuelle
     *
     * NOTES
     * ------
     * - Message complexity: `[ 0 ; 2*(n-1) ]`
     * - Synchronization delay: `[ 0 ; 2t ]`
     * 
     * - TLDR:
     *     + "I'll give you my permission for now and for later use".
     *     + Improvement upon Ricart-Agrawala
     *
     * - hi: DoEnter(hi++), DoLeave(NOTHING), OnGotRequest(Max(hi, hj)), OnGotPermission(NOTHING)
     *      + Uses `lasti`
     *
     * REFERENCES
     * -----------
     * - https://fr.wikipedia.org/wiki/Algorithme_Carvalho_et_Roucairol
     */
    public class CarvalhoRoucairol : AbstractAlgo
    {
        
        private char i;
        private SiteEtat etati = SiteEtat.Dehors;
        private int hi = 0;
        private int lasti = 0;
        private HashSet<char> attendusi = new();
        private HashSet<char> differei = new();
        private HashSet<char> Ri;

        public CarvalhoRoucairol(char siteId, char[] siteIds)
            : base(siteId, siteIds)
        {
            i = siteId;
            Ri = siteIds.Where(j => j != i).ToHashSet();
        }

        // ---
        
        public void DoEnter()
        {
            etati = SiteEtat.Demandeur;
            hi = hi + 1;
            lasti = hi;

            foreach (var j in Ri)
            {
                // Send request(lasti, i) to j
                SendMessage(new MeikoMessage
                {
                    Type = "Request",
                    H = lasti,
                    Src = i,
                    Dst = j,
                });
            }

            while (! (Ri.Count == 0) )
                /* no-op */;
            
            etati = SiteEtat.Dedans;
        }
        
        public void DoLeave()
        {
            etati = SiteEtat.Dehors;

            foreach (var j in differei)
            {
                // Send Permission(i, j) to j
                SendMessage(new MeikoMessage
                {
                    Type = "Permission",
                    ["Pair"] = (i, j),
                    Src = i,
                    Dst = j,
                });
            }

            Ri = differei.ToHashSet();
            differei = new HashSet<char>();
        }

        public void OnGotRequest(MeikoMessage m)
        {
            int hj = m.H.Value;
            char j = m.Src;

            hi = Math.Max(hi, hj);
            bool prioritei = (
                (etati == SiteEtat.Demandeur) && (lasti < hj || lasti == hj && i < j) ||
                (etati == SiteEtat.Dedans)
            );

            if (prioritei)
            {
                // différéi := différéi ∪ {j} ;
                differei.Add(j);
            }
            else
            {
                // Send permission(i,j) to j
                SendMessage(new MeikoMessage
                {
                    Type = "Permission",
                    ["Pretty"] = $"permission({i},{j})",
                    ["Value"] = (i, j),
                    Src = i,
                    Dst = j,
                });
                
                // I'll need to ask j for our permission
                Ri.Add(j);
                
                // "Welp, I actually do need the permission myself, do request it from j"
                if (etati == SiteEtat.Demandeur)
                {
                    // Send request(lasti, i) to j
                    SendMessage(new MeikoMessage
                    {
                        Type = "Request",
                        ["Pretty"] = $"request(lasti={lasti}, {i})",
                        H = lasti,
                        Src = i,
                        Dst = j,
                    });
                }
            }

        }

        public void OnGotPermission(MeikoMessage m)
        {
            var j = m.Src;
            
            Ri.Remove(j);
        }
    }
}
