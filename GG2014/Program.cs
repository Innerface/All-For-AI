using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ESPlus.Rapid;
using ESPlus.Application.CustomizeInfo;
using CCWin;
using System.Drawing;
using JustLib.NetworkDisk.Passive;

namespace GG2014
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                GlobalResourceManager.Initialize();

                ESPlus.GlobalUtil.SetMaxLengthOfUserID(20);
                ESPlus.GlobalUtil.SetMaxLengthOfMessage(1024 * 1024 * 10);
                OMCS.GlobalUtil.SetMaxLengthOfUserID(20);
                MainForm mainForm = new MainForm();
                IRapidPassiveEngine passiveEngine = RapidEngineFactory.CreatePassiveEngine();

                NDiskPassiveHandler nDiskPassiveHandler = new NDiskPassiveHandler(); //V 2.0
                ComplexCustomizeHandler complexHandler = new ComplexCustomizeHandler(nDiskPassiveHandler, mainForm);//V 2.0
                LoginForm loginForm = new LoginForm(passiveEngine, complexHandler);

                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                nDiskPassiveHandler.Initialize(passiveEngine.FileOutter, null);
                mainForm.Initialize(passiveEngine, loginForm.LoginStatus, loginForm.StateImage);
                Application.Run(mainForm);
            }
            catch (Exception ee)
            {
                MessageBoxEx.Show(ee.Message);
                ee = ee;
            }
        }
    }

}
