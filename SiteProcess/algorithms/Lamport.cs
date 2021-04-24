using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Meikodayo;

namespace SiteProcess
{
    
    /**
     * Lamport algorithm.
     *
     * Lamport a proposé un algorithme d’exclusion mutuelle distribuée équitable, les requêtes
     *  d’accès à la ressource critique sont exécutées selon un ordre total établi entre les messages de
     *  requêtes (les horloges logiques sont utilisées pour ordonner les requêtes).
     *
     * NOTES
     * ------
     * - Message complexity: `3(n - 1)`. PS: 3 messages = 1 request + 1 ACK + 1 liberation.
     * - Synchronization delay: TODO
     *
     * - Idea: A bit similar to R-A, but uses a priority queue.
     *         Unlike R-A that gives explicit permissions, Lamport's only _**Ack**nowledges_ requests and sends _Liberations_. It lets sites manage access themselves.
     *
     * - hi: DoEnter(h++), DoLeave(hi++), OnGotRequest(Max(hi, hj) + 1), OnGotAck(Max(hi, hj) + 1), OnGotFree(Max(hi, hj) + 1).
     *      DoSomething(hi++) or OnGotSomething(max(hi, hj) + 1).
     *
     * - The network MUST be FIFO.
     */
    public class Lamport : AbstractAlgo
    {

        // 1. variables locales d’un site i
        private char i;
        private int hi = 0;
        private SiteEtat etati = SiteEtat.Dehors;
        // requête-filei : file d’attente de requêtes (triée selon l’horloge logique)
        private PriorityQueue requestQueue = new();
        private HashSet<char> attendusi = new(); // COURS: "initialisé Ri"; KAITO: The fuck, we'll override it anyways when we "Do Enter"
        private HashSet<char> Ri;

        public Lamport(char siteId, char[] siteIds)
            : base(siteId, siteIds)
        {
            i = siteId;
            Ri = siteIds.Where(j => j != i).ToHashSet();
        }
        
        public void DoEnter()
        {
            hi = hi + 1;
            etati = SiteEtat.Demandeur;
            requestQueue.Enqueue((hi, i));
            attendusi = Ri.ToHashSet(); // clone(?)
            
            foreach (var j in Ri)
            {
               SendMessage(new MeikoMessage
                   {
                       Dst = j,
                       Type = "Request",
                       Src = i,
                       H = hi,
                       }
                   );
            }
            
            // Attendre ( attendusi = {} et requête(hi, i) est en tête de requête-filei );
            // TODO: refactor
            while (! (attendusi.Count == 0 && requestQueue.Peek() == (hi, i)) ) Thread.Sleep(50);

            etati = SiteEtat.Dedans;
        }

        public void DoLeave()
        {
            hi = hi + 1;
            etati = SiteEtat.Dehors;
            
            foreach (var j in Ri)
            {
                // Send Free(hi, i) to j.
                SendMessage(new MeikoMessage
                    {
                        Dst = j,
                        Type = "Free",
                        Src = i,
                        H = hi,
                    }
                    );
            }
            
            // Enlever la requête de i de requête-filei ; 
            requestQueue.Dequeue();
        }

        void OnGotRequest(MeikoMessage m)
        {
            int hj = m.H.Value;
            char j = m.Src;
            
            hi = Math.Max(hi, hj) + 1;
            requestQueue.Enqueue((hj, j));
            // Send ack(hi, i) to j
            SendMessage(new MeikoMessage
                {
                    Type = "Ack",
                    H = hi,
                    Src = i, 
                    Dst = j,
                }
                );
        }

        void OnGotAck(dynamic m)
        {
            int hj = m.h;
            char j = m.From;

            hi = Math.Max(hi, hj) + 1;
            // attendusi := attendusi – {j} ; 
            attendusi.RemoveWhere(x => x == j);
        }

        void OnGotFree(dynamic m)
        {
            int hj = m.h;
            char j = m.From;
            
            hi = Math.Max(hi, hj) + 1;
            requestQueue.Dequeue();
        }

    }

    // ---
    
    class PriorityQueue
    {
        private SortedList<int, (int, char)> _list = new();
        
        public void Enqueue((int, char) element)
        {
            _list.Add(element.Item1, element);
        }

        public (int, char) Dequeue()
        {
            var head = _list[0];
            _list.RemoveAt(0);
            return head;
        }

        public (int, char)? Peek()
        {
            if (_list.Count > 0)
            {
                return _list[0];
            }
            else
            {
                return null;
            }
        }
        
    }

}
