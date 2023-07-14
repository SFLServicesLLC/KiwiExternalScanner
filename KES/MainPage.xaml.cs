using FtpClientSample;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409
//https://stackoverflow.com/questions/40126286/windows-store-app-test-certificate-expired
//https://stackoverflow.com/questions/34539016/append-to-a-text-file-not-overwrite
//https://stackoverflow.com/questions/40131220/uwp-write-and-read-file-in-a-from-folder-from-a-string
//https://stackoverflow.com/questions/61858220/in-uwp-app-read-command-line-arguments-and-pass-it-from-app-xaml-cs-file-to-the

//Using
//https://stackoverflow.com/questions/50931099/how-to-access-local-network-smb-in-uwp

namespace RangeTrainerTutor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        // Setup a timer
        System.Timers.Timer myTimer = new System.Timers.Timer();

        #region Main Load
        public MainPage()
        {
            //to pass args from app.axml to mainpage.xaml
            //https://stackoverflow.com/questions/61858220/in-uwp-app-read-command-line-arguments-and-pass-it-from-app-xaml-cs-file-to-the

            //Publish the app
            //https://learn.microsoft.com/en-us/windows/msix/package/packaging-uwp-apps


            this.InitializeComponent();

            var hostNames = NetworkInformation.GetHostNames();

            //Get teh clock going
            DispatcherTimer Timer = new DispatcherTimer();

            Timer.Tick += Timer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();

        }
        #endregion

        #region Maximized

        #endregion 

        #region Command Line
        //*************************************************
        //*************************************************
        //********** EXECUTE COMMAND LINE STRING **********
        //*************************************************
        //*************************************************
        //
        //net localgroup Administrators DefaultAccount /add
        //Run first
        //http://www.iot-developer.net/windows-iot/uwp-programming-in-c/command-line-uwp-programming-in-c/executing-command-line-commands
        // Enable
        //reg ADD "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\EmbeddedMode\ProcessLauncher" /v AllowedExecutableFilesList /t REG_MULTI_SZ /d "c:\windows\system32\cmd.exe\0"
        // Disable
        //reg QUERY "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\EmbeddedMode\ProcessLauncher" /v AllowedExecutableFilesList
        //http://www.iot-developer.net/windows-iot/command-line/enabling-command-line

        private async Task<string> ExecuteCommandLineString(string CommandString)
        {
            const string CommandLineProcesserExe = "c:\\windows\\system32\\cmd.exe";
            const uint CommandStringResponseBufferSize = 8192;
            string currentDirectory = "C:\\";

            StringBuilder textOutput = new StringBuilder((int)CommandStringResponseBufferSize);
            uint bytesLoaded = 0;

            if (string.IsNullOrWhiteSpace(CommandString))
                return ("");

            var commandLineText = CommandString.Trim();

            var standardOutput = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            var standardError = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            var options = new Windows.System.ProcessLauncherOptions
            {
                StandardOutput = standardOutput,
                StandardError = standardError
            };

            try
            {
                var args = "/C \"cd \"" + currentDirectory + "\" & " + commandLineText + "\"";
                var result = await Windows.System.ProcessLauncher.RunToCompletionAsync(CommandLineProcesserExe, args, options);

                //First write std out
                using (var outStreamRedirect = standardOutput.GetInputStreamAt(0))
                {
                    using (var dataReader = new Windows.Storage.Streams.DataReader(outStreamRedirect))
                    {
                        while ((bytesLoaded = await dataReader.LoadAsync(CommandStringResponseBufferSize)) > 0)
                            textOutput.Append(dataReader.ReadString(bytesLoaded));

                        new System.Threading.ManualResetEvent(false).WaitOne(10);
                        if ((bytesLoaded = await dataReader.LoadAsync(CommandStringResponseBufferSize)) > 0)
                            textOutput.Append(dataReader.ReadString(bytesLoaded));
                    }
                }

                //Then write std err
                using (var errStreamRedirect = standardError.GetInputStreamAt(0))
                {
                    using (var dataReader = new Windows.Storage.Streams.DataReader(errStreamRedirect))
                    {
                        while ((bytesLoaded = await dataReader.LoadAsync(CommandStringResponseBufferSize)) > 0)
                            textOutput.Append(dataReader.ReadString(bytesLoaded));

                        new System.Threading.ManualResetEvent(false).WaitOne(10);
                        if ((bytesLoaded = await dataReader.LoadAsync(CommandStringResponseBufferSize)) > 0)
                            textOutput.Append(dataReader.ReadString(bytesLoaded));
                    }
                }

                return (textOutput.ToString());
            }
            catch (UnauthorizedAccessException uex)
            {
                return ("ERROR - " + uex.Message + "\n\nCmdNotEnabled");
            }
            catch (Exception ex)
            {
                return ("ERROR - " + ex.Message + "\n");
            }
        }
        #endregion

        #region Ping CommandLine

        #endregion

        #region PingServer
        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send("", 10);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException ex)
            {
                Console.WriteLine(ex.Message);
                // Discard PingExceptions and return false;
                return pingable;
            }
            return pingable;
        }
        #endregion

        #region Converting Hex Color
        public static Color GetColorFromHex(string hexString)
        {
            //add default transparency to ignore exception
            if (!string.IsNullOrEmpty(hexString) && hexString.Length > 6)
            {
                if (hexString.Length == 7)
                {
                    hexString = "FF" + hexString;
                }

                hexString = hexString.Replace("#", string.Empty);
                byte a = (byte)(Convert.ToUInt32(hexString.Substring(0, 2), 16));
                byte r = (byte)(Convert.ToUInt32(hexString.Substring(2, 2), 16));
                byte g = (byte)(Convert.ToUInt32(hexString.Substring(4, 2), 16));
                byte b = (byte)(Convert.ToUInt32(hexString.Substring(6, 2), 16));
                Color color = Color.FromArgb(a, r, g, b);
                return color;
            }

            //return black if hex is null or invalid
            return Color.FromArgb(255, 0, 0, 0);
        }
        #endregion

        #region Clock
        //https://stackoverflow.com/questions/38562704/make-clock-uwp-c
        private void Timer_Tick(object sender, object e)
        {
            Time.Text = DateTime.Now.ToString("MM/dd/yyyy h:mm:ss tt");
        }
        #endregion

        #region KeyPadClicks
        private void Onebutton_Click(object sender, RoutedEventArgs e)
        {
            //var value = int.Parse(DocketNumberTextbox.Text);
            //Numbertextbox.Text = (value + 1.ToString();
            //StatustextBox.Text = "Moved yard Up...";
            DocketNumberTextbox.Text += "1";
        }

        private void Twobutton_Click(object sender, RoutedEventArgs e)
        {
            //var value = int.Parse(DocketNumberTextbox.Text);
            //Numbertextbox.Text = (value + 1.ToString();
            //StatustextBox.Text = "Moved yard Up...";
            DocketNumberTextbox.Text += "2";
        }

        private void Threebutton_Click(object sender, RoutedEventArgs e)
        {
            //var value = int.Parse(DocketNumberTextbox.Text);
            //Numbertextbox.Text = (value + 1.ToString();
            //StatustextBox.Text = "Moved yard Up...";
            DocketNumberTextbox.Text += "3";
        }

        private void Fourbutton_Click(object sender, RoutedEventArgs e)
        {
            //var value = int.Parse(DocketNumberTextbox.Text);
            //Numbertextbox.Text = (value + 1.ToString();
            //StatustextBox.Text = "Moved yard Up...";
            DocketNumberTextbox.Text += "4";
        }

        private void Fivebutton_Click(object sender, RoutedEventArgs e)
        {
            //var value = int.Parse(DocketNumberTextbox.Text);
            //Numbertextbox.Text = (value + 1.ToString();
            //StatustextBox.Text = "Moved yard Up...";
            DocketNumberTextbox.Text += "5";
        }

        private void Sixbutton_Click(object sender, RoutedEventArgs e)
        {
            //var value = int.Parse(DocketNumberTextbox.Text);
            //Numbertextbox.Text = (value + 1.ToString();
            //StatustextBox.Text = "Moved yard Up...";
            DocketNumberTextbox.Text += "6";
        }

        private void Sevenbutton_Click(object sender, RoutedEventArgs e)
        {
            //var value = int.Parse(DocketNumberTextbox.Text);
            //Numbertextbox.Text = (value + 1.ToString();
            //StatustextBox.Text = "Moved yard Up...";
            DocketNumberTextbox.Text += "7";
        }

        private void Eightbutton_Click(object sender, RoutedEventArgs e)
        {
            //var value = int.Parse(DocketNumberTextbox.Text);
            //Numbertextbox.Text = (value + 1.ToString();
            //StatustextBox.Text = "Moved yard Up...";
            DocketNumberTextbox.Text += "8";
        }

        private void Ninebutton_Click(object sender, RoutedEventArgs e)
        {
            //var value = int.Parse(DocketNumberTextbox.Text);
            //Numbertextbox.Text = (value + 1.ToString();
            //StatustextBox.Text = "Moved yard Up...";
            DocketNumberTextbox.Text += "9";
        }

        private void Zerobutton_Click(object sender, RoutedEventArgs e)
        {
            //var value = int.Parse(DocketNumberTextbox.Text);
            //Numbertextbox.Text = (value + 1.ToString();
            //StatustextBox.Text = "Moved yard Up...";
            DocketNumberTextbox.Text += "0";

        }
        #endregion

        #region Hit Clear
        private void Clearbutton_Click(object sender, RoutedEventArgs e)
        {
            DocketNumberTextbox.Text = "";
            MessageTextBox.Text += "Clear button was hit!";
        }
        #endregion 

        #region Hit Enter
        private void Enterbutton_Click(object sender, RoutedEventArgs e)
        {
            //https://stackoverflow.com/questions/1019793/how-can-i-convert-string-to-int
            MessageTextBox.Text += "Starting to process a new docket" + "\n";

            MessageTextBox.Text += "Verifying if docket is vaild" + "\n";

            int x = 0;
            int.TryParse(DocketNumberTextbox.Text, out x);
            if (x > 0)
            {
                MessageTextBox.Text += "Docket is a vaild number" + "\n";
                //var value = int.Parse(DocketNumberTextbox.Text);
                //Numbertextbox.Text = (value + 1.ToString();
                //StatustextBox.Text = "Moved yard Up...";
                //DocketNumberTextbox.Text += "0";
                MessageTextBox.Text += "You entered " + x + " to ship a load." + "\n";
                //DoUpload(DocketNumberTextbox.Text);
                UNCCreateFile(DocketNumberTextbox.Text);
                //MessageTextBox.Text = "Ready for next docket number.";
            }
            else
            {
                MessageTextBox.Text += "The last number entered " + DocketNumberTextbox.Text + " is not a valid docket number, please try again!" + "\n";
            }

        }
        #endregion

        #region Scanner Enter Hit
        private void DocketNumberTextbox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            Enterbutton_Click(sender, e);
        }
        #endregion

        #region Create File on Linux Server
        private async void UNCCreateFile(string docnumber)
        {
            //https://stackoverflow.com/questions/34803648/configurationmanager-and-appsettings-in-universal-uwp-app

            //var packageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            //var sampleFile = await packageFolder.GetFileAsync("connect.txt");
            //var contents = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);

            //var resources = new Windows.ApplicationModel.Resources.ResourceLoader("resourcesFile");
            //string path = resources.GetString("FOLDER");

            var packageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var connectFile = await packageFolder.GetFileAsync("connect.txt");
            string pathFile = await Windows.Storage.FileIO.ReadTextAsync(connectFile);

            string path = @"\\linux04.onling.com\Public\SHPTRK";

            // This text is added only once to the file.
            if (!File.Exists(path))
            {
               try
                {

                    // Create a file to write to.
                    string createText = docnumber + Environment.NewLine;
                    File.WriteAllText(path, createText);
                    DocketNumberTextbox.Text = "";

                }
                catch (Exception ex)     
                {
                    MessageTextBox.Text += ex.Message + "\n";
                }
                
            }
            else
            {
                try
                {
                    MessageTextBox.Text += "More loads are waiting to be processed" + "\n";
                    string appendText = docnumber + Environment.NewLine;
                    File.AppendAllText(path, appendText);
                    DocketNumberTextbox.Text = "";
                    MessageTextBox.Text += "You're docket was added to the list to be shipped" + "\n";
                }
                catch(Exception ex) 
                {
                    MessageTextBox.Text += ex.Message;
                }
            }

            // Open the file to read from.
            //string readText = File.ReadAllText(path);
        }
        #endregion 

        #region Upload
        private async void DoUpload(string docnumber)
        {
            try
            {
                MessageTextBox.Text += "Setting the connection..." + "\n";

                Uri uri = new Uri("ftp://linux04.onling.com/imptruck.txt");

                MessageTextBox.Text += "Connecting..." + "\n";

                FtpClient client = new FtpClient();
                try
                {
                    await client.ConnectAsync(
                        new HostName(uri.Host),
                        uri.Port.ToString(),
                        "remuser",
                        "tucan4");
                }
                catch (Exception ex)
                {
                    MessageTextBox.Text += ex.Message;
                }
                MessageTextBox.Text += "Uploading..." + "\n";

                byte[] data = Encoding.UTF8.GetBytes(docnumber);

                MessageTextBox.Text += "..." + "\n";
                MessageTextBox.Text += "..." + "\n";

                await client.UploadAsync(uri.AbsolutePath, data);

                MessageTextBox.Text += "..." + "\n";
                MessageTextBox.Text += "..." + "\n";
                MessageTextBox.Text += "Done!" + "\n";

            }
            catch (Exception ex)
            {
                MessageTextBox.Text += ex.Message;
            }
        }
        #endregion

        #region Message Text Change
        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //https://stackoverflow.com/questions/40114620/uwp-c-sharp-scroll-to-the-bottom-of-textbox
            var grid = (Grid)VisualTreeHelper.GetChild(MessageTextBox, 0);
            for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
            {
                object obj = VisualTreeHelper.GetChild(grid, i);
                if (!(obj is ScrollViewer)) continue;
                ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
                break;
            }
        }
        #endregion 

    }

}
