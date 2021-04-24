using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Meikodayo;

namespace SiteProcess
{
    /**
     * Lann algorithm.
     *
     * NOTES
     * ------
     * - Message complexity: `[ 0 ; Infinity ]`
     * - Synchronizatin delay: `[ 0 ; n*t ]`
     *
     * - TLDR:
     *      + Un algorithme à base de mouvement perpétuel
     *
     */
    public class Lann : AbstractAlgo
    {
        
        private char i;
        private SiteEtat etati = SiteEtat.Dehors;
        private bool jetonDispi = false;
        private List<char> sortedSites;
        
        public Lann(char siteId, char[] siteIds)
            : base(siteId, siteIds)
        {
            i = siteId;
            sortedSites = siteIds.ToList();
            sortedSites.Sort();
        }
        
        // --

        public void DoEnter()
        {
            etati = SiteEtat.Demandeur;
            
            while (! (jetonDispi) ) 
                /* no-op */;
                
            etati = SiteEtat.Dedans;
        }

        public void DoLeave()
        {
            etati = SiteEtat.Dehors;
            jetonDispi = false;
            SendJetonToTheNextSite();
        }

        public void OnGotJeton(MeikoMessage m)
        {
            if (etati == SiteEtat.Demandeur)
            {
                jetonDispi = true;
            }
            else
            {
                SendJetonToTheNextSite();
            }
        }

        private void SendJetonToTheNextSite()
        {
            int nextIndex = (sortedSites.IndexOf(i) + 1) % sortedSites.Count;
            char j = sortedSites[nextIndex];
            
            SendMessage(new MeikoMessage
            {
                Type = "Jeton",
                Dst = j,
            });
        }

    }
}
