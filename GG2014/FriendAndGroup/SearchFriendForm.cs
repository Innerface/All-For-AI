using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CCWin;
using CCWin.Win32;
using CCWin.Win32.Const;
using System.Diagnostics;
using System.Configuration;
using ESPlus.Rapid;
using CCWin.SkinControl;

namespace GG2014
{
    internal partial class SearchFriendForm : BaseForm
    {
        private IChatSupporter chatSupporter;

        public SearchFriendForm( IChatSupporter supporter)
        {
            InitializeComponent();  
            this.chatSupporter = supporter;
        }   

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.chatListBox.Items.Clear();
            List<ChatListSubItem> list = this.chatSupporter.SearchChatListSubItem(this.skinTextBox_id.SkinTxt.Text.Trim());
            bool hasResult = list.Count > 0;
            this.skinLabel_noResult.Visible = !hasResult ;
            if (hasResult)
            {
                this.chatListBox.Items.Add(new ChatListItem("查找结果"));
                this.chatListBox.Items[0].IsOpen = true;
                foreach (ChatListSubItem item in list)
                {                  
                    this.chatListBox.Items[0].SubItems.Add(item);
                }
            }
        }

        private void chatListBox_DoubleClickSubItem(object sender, ChatListEventArgs e)
        {
            ChatForm form = this.chatSupporter.GetChatForm(e.SelectSubItem.ID);            
            form.Show();
        }         
    }
}
