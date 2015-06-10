using System;
using System.Collections.Generic;
using System.Text;


namespace GG2014.Server
{
    internal class GroupManager : ESPlus.Application.Group.Server.IGroupManager
    {
        private GlobalCache globalCache;
        public GroupManager(GlobalCache db)
        {
            this.globalCache = db;
        }

        public List<string> GetGroupMembers(string groupID)
        {
            Group group =  this.globalCache.GetGroup(groupID);
            if (group == null)
            {
                return new List<string>();
            }

            return group.MemberList;
        }

        public List<string> GetGroupmates(string userID)
        {
            return this.globalCache.GetAllContacts(userID);
        }
    }
}
