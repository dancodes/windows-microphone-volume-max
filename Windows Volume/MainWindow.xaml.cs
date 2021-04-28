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
using System.Windows.Threading;

namespace Windows_Volume
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon ni;
        private CoreAudioDevice defaultPlaybackDevice;
        private IDisposable volumeChangedSub;
        private Thread workerThread;
        public MainWindow()
        {
            InitializeComponent();

            this.WindowState = WindowState.Minimized;
            Hide();

            SetTrayIcon();
            setMaxVolume();

            workerThread = new Thread(() =>
            {
                while (true)
                {
                    setMaxVolume();

                    Thread.Sleep(1000 * 30);
                }
            });
            workerThread.IsBackground = true;
            workerThread.Start();

            volumeChangedSub = defaultPlaybackDevice.VolumeChanged.Subscribe(new VolumeReporter(this));

            Microsoft.Win32.SystemEvents.PowerModeChanged += OnPowerChange;
        }

        private void SetTrayIcon()
        {
            ni = new NotifyIcon();
            // Path iconPath = 
            ni.Icon = Properties.Resources.pngegg_CL4_icon;
            ni.Visible = true;
            ni.ContextMenuStrip = CreateContextMenuStrip();
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };

            ni.ShowBalloonTip(1000,
                "Microphone Levels is now running",
                "Microphone level will be permanently set to 100%",
                ToolTipIcon.Info);
        }

        private ContextMenuStrip CreateContextMenuStrip()
        {
            var contextMenuStrip = new ContextMenuStrip();
            var closeMenuItem = new ToolStripMenuItem("Close");
            closeMenuItem.Click += CloseMenuItem_Click;
            contextMenuStrip.Items.Add(closeMenuItem);

            return contextMenuStrip;
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void OnPowerChange(object s, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case Microsoft.Win32.PowerModes.Resume:
                    volumeChangedSub = defaultPlaybackDevice.VolumeChanged.Subscribe(new VolumeReporter(this));
                    break;
                case Microsoft.Win32.PowerModes.Suspend:
                    volumeChangedSub.Dispose();
                    break;
            }
        }

        public void setMaxVolume()
        {

            if (defaultPlaybackDevice == null)
            {
                defaultPlaybackDevice = new CoreAudioController().DefaultCaptureDevice;
            }

            defaultPlaybackDevice.Volume = 100;

            Action action = delegate ()
            {
                mainText.Text = String.Format("Level is set to 100%");
            };

            Dispatcher.Invoke(DispatcherPriority.Normal, action);
        }

        // Minimize to system tray when application is minimized.
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.Hide();

                ni.ShowBalloonTip(1000,
                    "Microphone Levels is still running",
                    "Microphone level will be permanently set to 100%",
                    ToolTipIcon.Info);
            }

            base.OnStateChanged(e);
        }
    }

    public class VolumeReporter : IObserver<DeviceVolumeChangedArgs>
    {
        private IDisposable unsubscriber;
        private bool updating = false;
        private MainWindow w;

        public VolumeReporter(MainWindow w)
        {
            this.w = w;
        }


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
                w.setMaxVolume();
                updating = false;
            }
        }
    }
}
