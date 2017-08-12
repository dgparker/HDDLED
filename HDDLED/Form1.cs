using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.Collections.Specialized;
using System.Threading;

namespace HDDLED
{
    public partial class Form1 : Form
    {
        NotifyIcon hddLedIcon;
        Icon activeIcon;
        Icon idleIcon;
        Thread hddLedWorker;

        public Form1()
        {
            InitializeComponent();
            activeIcon = new Icon("HDD_Busy.ico");
            idleIcon = new Icon("HDD_Idle.ico");

            hddLedIcon = new NotifyIcon();
            hddLedIcon.Icon = idleIcon;
            hddLedIcon.Visible = true;

            //Create all Context Menu items and add them to notification tray items
            MenuItem progNameMenuItem = new MenuItem("HDDLED");
            MenuItem quitMenuItem = new MenuItem("Quit");
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(progNameMenuItem);
            contextMenu.MenuItems.Add(quitMenuItem);
            hddLedIcon.ContextMenu = contextMenu;

            //Wire up quit button to close app
            quitMenuItem.Click += QuitMenuItem_Click;


            //Hides the form
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            //start worker thread
            hddLedWorker = new Thread(new ThreadStart(HddActivityThread));
            hddLedWorker.Start();
        }



        //close the app on click on quit
        private void QuitMenuItem_Click(object sender, EventArgs e)
        {
            hddLedWorker.Abort();
            hddLedIcon.Dispose();
            this.Close();
        }
        //This is the thread that pulls HDD for activity notification icon
        public void HddActivityThread()
        {
            ManagementClass driveDataClass = new ManagementClass("Win32_PerfFormattedData_PerfDisk_PhysicalDisk");
            try
            {

                //main loop
                while (true)
                {
                    ManagementObjectCollection driveDataClassCollection = driveDataClass.GetInstances();
                    foreach( ManagementObject obj in driveDataClassCollection)
                    {
                        if ( obj["Name"].ToString() == "_Total" )
                        {
                            if(Convert.ToUInt64(obj["DiskBytesPersec"]) > 0)
                            {
                                //show busy icon
                                hddLedIcon.Icon = activeIcon;

                            }
                            else{
                                //show idle icon
                                hddLedIcon.Icon = idleIcon;
                            }
                        }
                    }


                    Thread.Sleep(100);
                }
            } catch(ThreadAbortException)
            {
                driveDataClass.Dispose();
                //thread was aborted

            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
