using System;
using System.Collections.Generic;
using System.Text;

using ESPlus.Rapid;
using ESBasic;

using JustLib.Records;

namespace GG2014.Server
{
    internal class RemotingService :MarshalByRefObject, IRemotingService
    {
        private GlobalCache globalCache;
        private IRapidServerEngine rapidServerEngine;
        public RemotingService(GlobalCache db ,IRapidServerEngine engine)
        {
            this.globalCache = db;
            this.rapidServerEngine = engine;
        }

        public RegisterResult Register(User user)
        {
            try
            {
                if (this.globalCache.IsUserExist(user.UserID))
                {
                    return RegisterResult.Existed;
                }

                this.globalCache.InsertUser(user);
                return RegisterResult.Succeed;
            }
            catch (Exception ee)
            {
                return RegisterResult.Error;
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public ChatRecordPage GetChatRecordPage(ChatRecordTimeScope timeScope, string senderID, string accepterID, int pageSize, int pageIndex)
        {
            return this.globalCache.GetChatRecordPage(timeScope ,senderID, accepterID, pageSize, pageIndex);
        }

        public ChatRecordPage GetGroupChatRecordPage(ChatRecordTimeScope timeScope, string groupID, int pageSize, int pageIndex)
        {
            ChatRecordPage page = this.globalCache.GetGroupChatRecordPage(timeScope ,groupID, pageSize, pageIndex);
            return page;
        }       

        public void InsertChatMessageRecord(ChatMessageRecord record)
        {
            //目前没有通过remoting插入数据库
        }
    }
}
