using System;
using System.Collections.Generic;
using System.Text;
using ESBasic.Security;
using ESBasic.ObjectManagement.Managers;
using System.Configuration;
using ESBasic;
using JustLib.Records;

namespace GG2014.Server
{
    public interface IDBPersister : IChatRecordPersister
    {
        void InsertUser(User t);
        void UpdateUserFriends(User t);
        void InsertGroup(Group t);       
        void UpdateUser(User t);
        void UpdateGroup(Group t);
        void DeleteGroup(string groupID);
        List<User> GetAllUser();
        List<Group> GetAllGroup();      

        void ChangeUserPassword(string userID, string newPasswordMD5);
        void ChangeUserGroups(string userID, string groups);        
    }    

    public class VirtualDB : IDBPersister
    {
        private ObjectManager<string, ObjectManager<string, List<ChatMessageRecord>>> chatRecordTable = new ObjectManager<string, ObjectManager<string, List<ChatMessageRecord>>>();//ownerID - guestID - msgRtf。 
        private ObjectManager<string, List<ChatMessageRecord>> groupChatRecordTable = new ObjectManager<string, List<ChatMessageRecord>>();//groupID - guestID - msgRtf。 

        #region InsertChatMessageRecord
        public void InsertChatMessageRecord(ChatMessageRecord chatMessage)
        {
            if (!chatMessage.IsGroupChat)
            {
                //owner 为Sender
                if (!this.chatRecordTable.Contains(chatMessage.SpeakerID))
                {
                    this.chatRecordTable.Add(chatMessage.SpeakerID, new ObjectManager<string, List<ChatMessageRecord>>());
                }
                ObjectManager<string, List<ChatMessageRecord>> guests = this.chatRecordTable.Get(chatMessage.SpeakerID);
                if (!guests.Contains(chatMessage.AudienceID))
                {
                    guests.Add(chatMessage.AudienceID, new List<ChatMessageRecord>());
                }
                List<ChatMessageRecord> records = guests.Get(chatMessage.AudienceID);
                records.Add(chatMessage);

                //owner 为chatMessage.AudienceID
                if (!this.chatRecordTable.Contains(chatMessage.AudienceID))
                {
                    this.chatRecordTable.Add(chatMessage.AudienceID, new ObjectManager<string, List<ChatMessageRecord>>());
                }
                ObjectManager<string, List<ChatMessageRecord>> guests2 = this.chatRecordTable.Get(chatMessage.AudienceID);
                if (!guests2.Contains(chatMessage.SpeakerID))
                {
                    guests2.Add(chatMessage.SpeakerID, new List<ChatMessageRecord>());
                }
                List<ChatMessageRecord> records2 = guests2.Get(chatMessage.SpeakerID);
                records2.Add(chatMessage);
            }
            else
            {
                if (!this.groupChatRecordTable.Contains(chatMessage.AudienceID))
                {
                    this.groupChatRecordTable.Add(chatMessage.AudienceID, new List<ChatMessageRecord>());
                }
                List<ChatMessageRecord> records = this.groupChatRecordTable.Get(chatMessage.AudienceID);
                records.Add(chatMessage);
            }
        } 
        #endregion

        #region GetAllUser
        public List<User> GetAllUser()
        {
            List<User> list = new List<User>();
            string pwdMD5 = SecurityHelper.MD5String("1");

            list.Add(new User("10000", pwdMD5, "张超", "我的好友:10001,10002,10003",  "每一天都是崭新的！", 0, "G001,G002"));
            list.Add(new User("10001", pwdMD5, "刘海", "我的好友:10000,10002,10003",  "加油，努力。", 1, "G001,G002"));
            list.Add(new User("10002", pwdMD5, "马小华", "我的好友:10001,10000,10003",  "随风而逝...", 2, "G001"));
            list.Add(new User("10003", pwdMD5, "李建平", "我的好友:10001,10002,10000", "有事请call我", 3, "G001"));
            list.Add(new User("10004", pwdMD5, "刘珊琪", "","岁月是把杀猪刀", 4, "G001"));
            list.Add(new User("10005", pwdMD5, "周晓新", "",  "每一天都是崭新的！", 0, "G001,G002"));
            list.Add(new User("10006", pwdMD5, "李文畅", "", "加油，努力。", 5, "G001,G002"));
            list.Add(new User("10007", pwdMD5, "王云", "", "每一天都是崭新的！", 6, "G001,G002"));
            list.Add(new User("10008", pwdMD5, "陈思思", "", "加油，努力。", 7, "G001,G002"));

            foreach (User user in list)
            {
                user.Version = 20;
            }

            return list;
        } 
        #endregion     

        public void DeleteGroup(string groupID)
        {
           
        }

        #region GetAllGroup
        public List<Group> GetAllGroup()
        {
            List<Group> list = new List<Group>();
            list.Add(new Group("G001", "测试群1", "10000", "本周周末安排加班！", "10000,10001,10002,10003,10004"));
            list.Add(new Group("G002", "测试群2", "10000", "春节长假快到了，请大家做好收尾工作！", "10000,10001"));
            return list;
        } 
        #endregion

        #region GetGroupChatRecordPage
        public ChatRecordPage GetGroupChatRecordPage(ChatRecordTimeScope timeScope, string groupID, int pageSize, int pageIndex)
        {
            int totalCount = 0;
            if (pageSize <= 0 || pageIndex < 0)
            {
                return new ChatRecordPage(totalCount,pageIndex, new List<ChatMessageRecord>());
            }

            if (!this.groupChatRecordTable.Contains(groupID))
            {
                return new ChatRecordPage(totalCount, pageIndex, new List<ChatMessageRecord>());
            }

            List<ChatMessageRecord> records = this.groupChatRecordTable.Get(groupID);
            totalCount = records.Count;
            int pageCount = records.Count / pageSize;
            if (records.Count % pageSize > 0)
            {
                ++pageCount;
            }

            if (pageIndex == int.MaxValue)
            {
                pageIndex = pageCount - 1;
            }

            if (pageIndex >= pageCount)
            {
                return new ChatRecordPage(totalCount, pageIndex, new List<ChatMessageRecord>());
            }

            List<ChatMessageRecord> page = new List<ChatMessageRecord>();
            for (int i = pageIndex * pageSize; i < records.Count && page.Count <= pageSize; i++)
            {
                page.Add(records[i]);
            }

            return new ChatRecordPage(totalCount, pageIndex, page); ;
        }
        #endregion

        #region GetChatRecordPage
        public ChatRecordPage GetChatRecordPage(ChatRecordTimeScope timeScope, string senderID, string accepterID, int pageSize, int pageIndex)
        {
            int totalCount = 0;
            if (pageSize <= 0 || pageIndex < 0)
            {
                return new ChatRecordPage(totalCount, pageIndex, new List<ChatMessageRecord>());
            }

            if (!this.chatRecordTable.Contains(senderID))
            {
                return new ChatRecordPage(totalCount, pageIndex, new List<ChatMessageRecord>());
            }

            ObjectManager<string, List<ChatMessageRecord>> friends = this.chatRecordTable.Get(senderID);
            if (!friends.Contains(accepterID))
            {
                return new ChatRecordPage(totalCount, pageIndex, new List<ChatMessageRecord>());
            }

            List<ChatMessageRecord> records = friends.Get(accepterID);
            totalCount = records.Count;
            int pageCount = records.Count / pageSize;
            if (records.Count % pageSize > 0)
            {
                ++pageCount;
            }

            if (pageIndex == int.MaxValue)
            {
                pageIndex = pageCount - 1;
            }

            if (pageIndex >= pageCount)
            {
                return new ChatRecordPage(totalCount, pageIndex, new List<ChatMessageRecord>());
            }

            List<ChatMessageRecord> page = new List<ChatMessageRecord>();
            for (int i = pageIndex * pageSize; i < records.Count && page.Count <= pageSize; i++)
            {
                page.Add(records[i]);
            }

            return new ChatRecordPage(totalCount, pageIndex, page);
        }
        #endregion

        public void ChangeUserPassword(string userID, string newPasswordMD5)
        {
            
        }

        public void ChangeUserGroups(string userID, string groups)
        {
            
        }

        public void UpdateUserFriends(User t)
        {
            
        }

        public void InsertUser(User t)
        {
            
        }

        public void InsertGroup(Group t)
        {
            
        }

        public void UpdateUser(User t)
        {
            
        }

        public void UpdateGroup(Group t)
        {
            
        }
    }

}
