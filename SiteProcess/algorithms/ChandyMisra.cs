using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Meikodayo;

namespace SiteProcess
{
    /**
     * Chandy-Misra
     * #Permission #PermissionIndividuelle
     *
     * NOTES
     * ------
     * - Message complexity: `[ 0 ; 2*(n-1) ]`
     * - Synchronization delay: `[ 0 ; 2t ]`
     *
     * - TLDR:
     *      + Remember: Chandy = **Chan**nels bi**di**rectional
     *      + `Ri` is used as both of R-A's `Ri` and the `attendusi`
     *      + Ta3 utilise w non-utilise
     *      + Permissions for node pairs
     *      + Makanch horloge, kayn graphe
     *
     * ---
     *
     * - Communication channels are bidirectional.
     * - The graph is acyclic.
     *
     * <b>Vivacité</b>:
     * Graphe de priorité (G) : Un arc de i vers j indique
     *  que i n’est pas prioritaire sur j ssi permission(i,j) est :
     *  - sur le site i dans l’état util,
     *  - ou en transit de i vers j,
     *  - ou sur le site j et dans l’état non-util.
     *
     * `(i) -> (j)` means `(!prioritei /* than j *\/)` if permission(i, j)
     *  - is on sitei with etat == U,
     *  - is in transit from i to j, or
     *  - is on sitej with etat == NonU.
     *
     * ---
     *
     * END.
     *
     */
    public class ChandyMisra : AbstractAlgo
    {
        enum PermissionEtat
        {
            Utilise,
            NonUtilise
        }

        IDictionary<char, PermissionEtat> permissions;
        
        private char i;
        private SiteEtat etati = SiteEtat.Dehors;
        private bool prioritei;
        private HashSet<char> differei = new();
        private HashSet<char> Ri;
        
        public ChandyMisra(char siteId, char[] siteIds)
            : base(siteId, siteIds)
        {
            i = siteId;
            Ri = siteIds.Where(j => j != i).ToHashSet();

            // ∀i, j : permission(i,j).état = U;
            permissions = new Dictionary<char, PermissionEtat>();
            foreach (var j in siteIds.Where(j => j != siteId))
            {
                permissions.Add(j, PermissionEtat.Utilise);
            }
            
            // ∀i, j : permission(i,j) est placée sur l’un des deux sites est l’on a :
            //  permission(i,j) sur le site i ⇔ j 6∈ Ri, et i ∈ Rj
            //TODO
        }

        // ---
    
        public void DoEnter()
        {
            etati = SiteEtat.Demandeur;
            foreach (var j in Ri)
            {
                SendMessage(new MeikoMessage
                {
                    Type = "Request",
                    Src = i,
                    Dst = j,
                });
            }

            while (! (Ri.Count == 0) )
                /* no-op */;
            
            etati = SiteEtat.Dedans;
            
            // pour tout j tel que : 1 <= j != i <= n : permission(i,j).état := U ;
            foreach (var (j, _) in permissions)
            {
                permissions[j] = PermissionEtat.Utilise;
            }
            
        }

        public void DoLeave()
        {
            etati = SiteEtat.Dehors;

            // ∀j ∈ différéi : envoyer permission(i,j) à j ;
            foreach (var j in differei)
            {
                SendMessage(new MeikoMessage
                {
                    ["Pretty"] = $"permission({i},{j})",
                    Type = "Permission",
                    Dst = j,
                });
            }

            Ri = differei.ToHashSet();
            differei = new HashSet<char>();
        }

        public void OnGotRequest(MeikoMessage m)
        {
            var j = m.Src;
            
            // `j ∈ Ri` means that `permission(i,j)` est alors en transit vers sitei
            if (Ri.Contains(j))
            {
                prioritei = true;
            }
            else
            {
                prioritei = (etati == SiteEtat.Dedans) ||
                            (etati == SiteEtat.Demandeur && permissions[j] == PermissionEtat.NonUtilise);
            }

            if (prioritei)
            {
                differei.Add(j);
            }
            else
            {
                SendMessage(new MeikoMessage
                {
                    ["Pretty"] = $"permission({i},{j})",
                    Type = "Permission",
                    Src = i,
                    Dst = j,
                });

                Ri.Add(j);

                if (etati == SiteEtat.Demandeur)
                {
                    SendMessage(new MeikoMessage
                    {
                        Type = "Request",
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
            permissions[j] = PermissionEtat.NonUtilise;
        }
        
    }
}
