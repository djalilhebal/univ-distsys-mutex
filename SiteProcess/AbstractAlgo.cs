using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Meikodayo;

namespace SiteProcess
{
    
    public enum SiteEtat
    {
        Dehors,
        Demandeur,
        Dedans,
    }

    public abstract class AbstractAlgo
    {
        public AbstractAlgo(char siteId, char[] siteIds)
        {
            CurrentSiteId = siteId;
        }

        // ---

        protected bool Less((int, char) pairI, (int, char) pairJ)
        {
            var (hi, i) = pairI;
            var (hj, j) = pairJ;
            
            return (hi < hj || hi == hj && i < j);
        }

        protected MeikoMessage ReceiveMessage()
        {
            return MeikoNetwork.GetNextMessage();
        }
        
        protected bool SendMessage(MeikoMessage message)
        {
            message.Id = GetNextId();
            MeikoNetwork.SendMessage(message);
            return true;
        }
        
        // ---
        
        private static char CurrentSiteId { get; set; }
        private static long _idGenerationCounter = 0;
        // TODO: Use Snowflake-like IDS
        private static string GetNextId()
        {
            var currentMessageNumber = ++_idGenerationCounter;
            return $"{CurrentSiteId}{currentMessageNumber}";
        }
        
    }
}