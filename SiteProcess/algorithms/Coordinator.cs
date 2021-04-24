using System.Collections.Generic;
using Meikodayo;

namespace SiteProcess
{
    /**
     * Centralized coordinator algorithm.
     *
     * We assume Node Index 0 is the Coordinator.
     */
    public class Coordinator : AbstractAlgo
    {
        private const char WellKnownCoordinatorId = (char)0;
        
        private int i;
        bool hasCoordinatorRole;
        Queue<int> L = new();
        bool SCOccupied = false;
        
        public Coordinator(char siteId, char[] siteIds)
            : base(siteId, siteIds)
        {
            i = siteId;
            hasCoordinatorRole = siteId == WellKnownCoordinatorId;
        }
        public void DoMain() {
            if (hasCoordinatorRole)
            {
                DoCoordinatorMain();
            }
            else
            {
                // ...
                DoEnter();
                // Do some critical section-related work...
                DoLeave();
                // ...

            }
        }

        private void DoCoordinatorMain()
        {
            do {
                var m = ReceiveMessage(); // lire(message, j)
                var j = m.Src;
                if (m.Type == "Request")
                {
                    if (SCOccupied == false) {
                        SCOccupied = true;
                        SendMessage(new MeikoMessage
                        {
                            Type = "Authorization",
                            Dst = j,
                        });
                    } else {
                        L.Enqueue(j);
                    }

                }
                else if (m.Type == "Liberation")
                {
                    if (L.Count == 0) {
                        SCOccupied = false;
                    } else {
                        var nextSiteId = L.Dequeue();
                        SendMessage(new MeikoMessage
                        {
                            Type = "Authorization",
                            Dst = (char)nextSiteId,
                        });
                    }
                }

            } while (true);

        }
        
        public void DoEnter() {
            SendMessage(new MeikoMessage
            {
                Type = "Request",
                Dst = WellKnownCoordinatorId,
            });
            
            // WaitFor(message.Type == "Authorization");
            while (ReceiveMessage().Type != "Authorization") ;
            // TODO: Obviously we shouldn't throw away received messages that are != Authorization, but treat them.
            //      Also, we should ensure that the Authorization message's source is the coordinator.
        }

        public void DoLeave() {
            SendMessage(new MeikoMessage
            {
                Type = "Liberation",
                Dst = WellKnownCoordinatorId,
            });
        }

    }
}