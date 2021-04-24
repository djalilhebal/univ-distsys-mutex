using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Meikodayo;

namespace SiteProcess
{
    /**
     * Suzuki-Kasami (Version initiale).
     * #Jeton #JetonRequetes
     */
    public class SuzukiKasamiInitiale : AbstractAlgo
    {
        
        private char i;
        private SiteEtat etati = SiteEtat.Dehors;
        private HashSet<char> Ri;
        private bool jetonDispi = false;
        private int[] jeton = null;

        /**
         * Tout site i est doté d’un tableau nbreqi : [1..n] d’entiers croissants init à (0...0),
         * nbreqi[j] est le nombre de requêtes faites par j et connues par i ;
         */
        private int[] nbreqi;
            
        public SuzukiKasamiInitiale(char siteId, char[] siteIds)
            : base(siteId, siteIds)
        {
            i = siteId;
            Ri = siteIds.Where(j => j != i).ToHashSet();
            int n = siteIds.Length;
            nbreqi = new List<int>(n).Select(val => 0).ToArray();
        }

        // ---

        public void DoEnter()
        {
            etati = SiteEtat.Demandeur;
            
            if (!jetonDispi)
            {
                nbreqi[i] = nbreqi[i] + 1;
                foreach (var j in Ri)
                {
                    SendMessage(new MeikoMessage
                    {
                        ["Pretty"] = $"request({i}, n={nbreqi[i]})",
                        Type = "Request",
                        Src = i,
                        ["n"] = nbreqi[i],
                        Dst = j,
                    });
                }

                while (! (jetonDispi) )
                    /* no-op */;
            }

            etati = SiteEtat.Dedans;
        }

        public void DoLeave()
        {
            etati = SiteEtat.Dehors;
            
            jeton[i] = nbreqi[i];

            // jValues = "pour j de i+1 à n puis de 1 à i-1" (-- PROF)
            // or for j = i + 1 to n - 1 and then from 0 to i - 1 (-- KAITO)
            var n = Ri.Count + 1; // all sites, including me ("i")
            var iToEnd = Enumerable.Range(i + 1, n).Where(el => el < n);
            var startToI = Enumerable.Range(0, i).Where(el => el < i);
            var jValues = iToEnd.Concat(startToI);
            foreach (var j in jValues)
            {
                if (nbreqi[j] > jeton[j])
                {
                    jetonDispi = false;
                    SendMessage(new MeikoMessage
                        {
                            Type = "Jeton",
                            ["Jeton"] = jeton,
                            Dst = (char)j,
                        });
                    break;
                }
            }
        }

        public void OnGotJeton(MeikoMessage m)
        {
            jeton = (int[])m["Jeton"];
            jetonDispi = true;
        }

        public void OnGotRequest(MeikoMessage m)
        {
            var j = m.Src;
            var nj = (int)m["n"];
            
            nbreqi[j] = nbreqi[j] + 1;
            
            if (jetonDispi && etati == SiteEtat.Dehors && (nbreqi[j] > jeton[j]))
            {
                jetonDispi = false;
                SendMessage(new MeikoMessage
                {
                    Type = "Jeton",
                    ["Jeton"] = jeton,
                    Dst = j,
                });
            }
        }
        
        // ---

        public static MeikoMessage CreateNewJeton(int n)
        {
            // Le jeton est un tableau : jeton :[1..n] d’entiers init à 0, jeton[i] contient le nombre d’utilisation de la SC par le site i ;
            int[] jeton = new List<int>(n).Select(val => 0).ToArray();

            return new MeikoMessage
            {
                Type = "Jeton",
                ["Jeton"] = jeton,
                Src = '*'
            };
        }
        
    }
}
