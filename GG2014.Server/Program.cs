using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ESPlus.Widgets;
using ESPlus.Rapid;
using ESPlus.Application.CustomizeInfo.Server;
using ESFramework;
using ESFramework.Server.UserManagement;
using ESPlus.Core;
using ESPlus.Application.Friends.Server;
using System.Configuration;
using OMCS;
using OMCS.Server;
using ESPlus.Application.CustomizeInfo;
using System.Runtime.Remoting;
using JustLib.NetworkDisk.Server;

/*
 * 本demo采用的是ESFramework的免费版本，不需要再次授权、也没有使用期限限制。若想获取ESFramework其它版本，请联系 www.oraycn.com 或 QQ：372841921。
 * 
 */
namespace GG2014.Server
{
    static class Program
    {
        private static IRapidServerEngine RapidServerEngine = RapidEngineFactory.CreateServerEngine();
        private static IMultimediaServer MultimediaServer;            

        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                GlobalCache globalCache = new GlobalCache();

                #region 初始化ESFramework服务端引擎                                
                ESPlus.GlobalUtil.SetAuthorizedUser("FreeUser", "");
                ESPlus.GlobalUtil.SetMaxLengthOfUserID(20);
                ESPlus.GlobalUtil.SetMaxLengthOfMessage(1024 * 1024 * 10);
                //自定义的好友管理器
                FriendsManager friendManager = new FriendsManager(globalCache);
                Program.RapidServerEngine.FriendsManager = friendManager;
                //自定义的组管理器
                GroupManager groupManager = new GroupManager(globalCache);
                Program.RapidServerEngine.GroupManager = groupManager;


                NDiskHandler nDiskHandler = new NDiskHandler(); //网盘处理器 V1.9
                CustomizeHandler handler = new CustomizeHandler();
                ComplexCustomizeHandler complexHandler = new ComplexCustomizeHandler(nDiskHandler, handler); 

                //初始化服务端引擎
                Program.RapidServerEngine.SecurityLogEnabled = false;
                Program.RapidServerEngine.Initialize(int.Parse(ConfigurationManager.AppSettings["Port"]), complexHandler, new BasicHandler(globalCache));
                //让IGroupManager的GetGroupmates方法返回所有联系人（组友，好友），则就可以关闭好友上显现通知了。          
                Program.RapidServerEngine.FriendsController.FriendNotifyEnabled = false;
                Program.RapidServerEngine.GroupController.GroupNotifyEnabled = true;
                Program.RapidServerEngine.GroupController.BroadcastBlobListened = true; //为群聊天记录

                //初始化网盘处理器 V1.9
                NetworkDiskPathManager networkDiskPathManager = new NetworkDiskPathManager() ;
                NetworkDisk networkDisk = new NetworkDisk(networkDiskPathManager, Program.RapidServerEngine.FileController);
                nDiskHandler.Initialize(Program.RapidServerEngine.FileController, networkDisk);

                //设置重登陆模式
                Program.RapidServerEngine.UserManager.RelogonMode = RelogonMode.ReplaceOld; 

                //离线消息控制器 V3.2
                OfflineFileController offlineFileController = new OfflineFileController(Program.RapidServerEngine, globalCache);

                handler.Initialize(globalCache, Program.RapidServerEngine, offlineFileController);
                #endregion            

                #region 初始化OMCS服务器
                OMCS.GlobalUtil.SetAuthorizedUser("FreeUser", "");
                OMCS.GlobalUtil.SetMaxLengthOfUserID(20);
                OMCSConfiguration config = new OMCSConfiguration(
                    int.Parse(ConfigurationManager.AppSettings["CameraFramerate"]),
                    int.Parse(ConfigurationManager.AppSettings["DesktopFramerate"]));

                //用于验证登录用户的帐密
                DefaultUserVerifier userVerifier = new DefaultUserVerifier();
                Program.MultimediaServer = MultimediaServerFactory.CreateMultimediaServer(int.Parse(ConfigurationManager.AppSettings["OmcsPort"]), userVerifier, config,false);                          
                
                #endregion

                #region 发布用于注册的Remoting服务
                RemotingConfiguration.Configure("GG2014.Server.exe.config", false);
                RemotingService registerService = new Server.RemotingService(globalCache ,Program.RapidServerEngine);
                RemotingServices.Marshal(registerService, "RemotingService");      
                #endregion       
 
                //如果不需要默认的UI显示，可以替换下面这句为自己的Form
                ESPlus.Widgets.MainServerForm mainForm = new ESPlus.Widgets.MainServerForm(Program.RapidServerEngine);
                mainForm.Text = ConfigurationManager.AppSettings["SoftwareName"] + " 服务器";
                Application.Run(mainForm);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }       
    }
}
