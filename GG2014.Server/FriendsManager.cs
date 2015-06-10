using System;
using System.Collections.Generic;
using System.Text;


namespace GG2014.Server
{
    internal class FriendsManager : ESPlus.Application.Friends.Server.IFriendsManager
    {
        private GlobalCache globalCache;
        public FriendsManager(GlobalCache db)
        {
            this.globalCache = db;
        }

        public List<string> GetFriendsList(string ownerID, string tag)
        {
            return this.globalCache.GetFriends(ownerID);
        }
    }
}
