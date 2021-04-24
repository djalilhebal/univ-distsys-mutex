using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meikodayo;

namespace SiteProcess
{
    /**
     * Algorithme de [Ricart & Agrawala, 81]
     * #Permission #PermissionIndividuelle
     *
     * NOTES
     * ------
     * - Message complexity: `2(n - 1)`
     *   (n - 1 requests and n - 1 permission responses)
     * 
     * - Synchronization delay: `[t; 2t]`
     *      + In the "worst" case: **2t** (everyone is 'Dehors').
     *      + In the "best" case: **1t** (our permission request reaches a 'Dedans' site at the exact moment when a it becomes 'Dehors' / leaves the CS).
     *
     * - hi: DoEnter(hi++), DoLeave(NOTHING), OnGotRequest(Max(hi, hj)), OnGotPermission(NOTHING)
     *      + Uses `lasti`
     *
     * REFERENCES
     * -----------
     * - https://web.archive.org/web/20120227161726/http://www.pps.jussieu.fr/~rifflet/enseignements/AlgoProgSysRep/ricart.html
     */
    public class RicartAgrawala : AbstractAlgo
    {

        private char i;
        private SiteEtat etati = SiteEtat.Dehors;
        private int hi = 0;
        private int lasti = 0;
        private HashSet<char> attendusi = new();
        private HashSet<char> differei = new();
        private HashSet<char> Ri;

        public RicartAgrawala(char siteId, char[] siteIds)
            : base(siteId, siteIds)
        {
            i = siteId;
            Ri = siteIds.Where(j => j != i).ToHashSet();
        }
        
        public void DoEnter()
        {
            etati = SiteEtat.Demandeur;
            hi = hi + 1;
            lasti = hi;
            attendusi = Ri;

            foreach (var j in Ri)
            {
                SendMessage(new MeikoMessage
                    {
                        Src = i,
                        Dst = j,
                        Type = "Request",
                        H = lasti,
                        // ---
                        ["Contents"] = (lasti, i),
                    });
            }

            while (! (attendusi.Count == 0) ) /* no-op */;
            
            etati = SiteEtat.Dedans;
        }

        public void DoLeave()
        {
            etati = SiteEtat.Dehors;

            foreach (var j in differei)
            {
                // Send Permission(i) to j
                SendMessage(new MeikoMessage
                {
                    Src = i,
                    Dst = j,
                    Type = "Permission",
                    });
            }

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
                differei.Add(j);
            }
            else
            {
                // Send Permission(i) to j
                SendMessage(new MeikoMessage
                {
                    Type = "Permission",
                    Src = i,
                    Dst = j,
                });
            }
        }

        public void OnGotPermission(MeikoMessage m)
        {
            char j = m.Src;
            
            attendusi.RemoveWhere(x => x == j);
        }
        
    }
}
