using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace Windows_Volume
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon ni;
        public MainWindow()
        {
            InitializeComponent();

            // Set Tray icon
            ni = new System.Windows.Forms.NotifyIcon();
            // Path iconPath = 
            ni.Icon = Windows_Volume.Properties.Resources.pngegg_CL4_icon;
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = System.Windows.WindowState.Normal;
                };
            
            ni.ShowBalloonTip(3000, 
                "Windows Microphone Volume is now running", 
                "Microphone volume will be permanently set to 100%", 
                ToolTipIcon.Info);

            CoreAudioDevice defaultPlaybackDevice = new CoreAudioController().DefaultCaptureDevice;
            defaultPlaybackDevice.Volume = 100;

            defaultPlaybackDevice.VolumeChanged.Subscribe(new VolumeReporter());
        }

        // Minimize to system tray when application is minimized.
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.Hide();

                ni.ShowBalloonTip(2000,
                    "Windows Microphone Volume is still running",
                    "Microphone volume will be permanently set to 100%",
                    ToolTipIcon.Info);
            }

            base.OnStateChanged(e);
        }
    }

    public class VolumeReporter : IObserver<DeviceVolumeChangedArgs>
    {
        private IDisposable unsubscriber;
        private bool updating = false;


        public virtual void Subscribe(IObservable<DeviceVolumeChangedArgs> provider)
        {
            unsubscriber = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }

        public virtual void OnCompleted()
        {
            Console.WriteLine("Additional temperature data will not be transmitted.");
        }

        public virtual void OnError(Exception error)
        {
            // Do nothing.
        }

        public virtual void OnNext(DeviceVolumeChangedArgs value)
        {
            if (!updating)
            {
                updating = true;

                Thread.Sleep(200);
                value.Device.Volume = 100;
                updating = false;
            }
        }
    }
}
