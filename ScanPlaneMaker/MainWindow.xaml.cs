//using AForge.Video;
//using AForge.Video.DirectShow;

using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;

namespace ScanPlaneMaker
{
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        #region Variables
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string _title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }
        string title = "Scan plane Maker";

        public string _scan_progress
        {
            get => scan_progress;
            set
            {
                scan_progress = value;
                OnPropertyChanged();
            }
        }
        string scan_progress = "  -  ";

        public float _x
        {
            get => x;
            set
            {
                x = value;
                OnPropertyChanged();
            }
        }
        float x;
        public float _y
        {
            get => y;
            set
            {
                y = value;
                OnPropertyChanged();
            }
        }
        float y;

        public double _goto_x_value
        {
            get => goto_x_value;
            set
            {
                goto_x_value = value;
                OnPropertyChanged();
            }
        }
        double goto_x_value = 10.0;

        public double _goto_y_value
        {
            get => goto_y_value;
            set
            {
                goto_y_value = value;
                OnPropertyChanged();
            }
        }
        double goto_y_value = 20.0;

        public SolidColorBrush _brd_image_borderbrush
        {
            get => brd_image_borderbrush;
            set
            {
                brd_image_borderbrush = value;
                OnPropertyChanged();
            }
        }
        SolidColorBrush brd_image_borderbrush;

        SolidColorBrush rouge = new SolidColorBrush(Colors.Red);
        SolidColorBrush vert = new SolidColorBrush(Colors.GreenYellow);

        public double _xy_move_value
        {
            get => xy_move_value;
            set
            {
                xy_move_value = value;
                OnPropertyChanged();
            }
        }
        double xy_move_value = 1;

        public double _x_range_value
        {
            get => x_range_value;
            set
            {
                x_range_value = value;
                OnPropertyChanged();
                Properties.Settings.Default.x_range_value = value;
                Properties.Settings.Default.Save();
            }
        }
        double x_range_value = Properties.Settings.Default.x_range_value;

        public double _y_range_value
        {
            get => y_range_value;
            set
            {
                y_range_value = value;
                OnPropertyChanged();
                Properties.Settings.Default.y_range_value = value;
                Properties.Settings.Default.Save();
            }
        }
        double y_range_value = Properties.Settings.Default.y_range_value;

        public double _scan_start_x_value
        {
            get => scan_start_x_value;
            set
            {
                scan_start_x_value = value;
                OnPropertyChanged();
                Properties.Settings.Default.scan_start_x_value = value;
                Properties.Settings.Default.Save();
            }
        }
        double scan_start_x_value = Properties.Settings.Default.scan_start_x_value;
        public double _scan_start_y_value
        {
            get => scan_start_y_value;
            set
            {
                scan_start_y_value = value;
                OnPropertyChanged();
                Properties.Settings.Default.scan_start_y_value = value;
                Properties.Settings.Default.Save();
            }
        }
        double scan_start_y_value = Properties.Settings.Default.scan_start_y_value;
        public double _scan_end_x_value
        {
            get => scan_end_x_value;
            set
            {
                scan_end_x_value = value;
                OnPropertyChanged();
                Properties.Settings.Default.scan_end_x_value = value;
                Properties.Settings.Default.Save();
            }
        }
        double scan_end_x_value = Properties.Settings.Default.scan_end_x_value;
        public double _scan_end_y_value
        {
            get => scan_end_y_value;
            set
            {
                scan_end_y_value = value;
                OnPropertyChanged();
                Properties.Settings.Default.scan_end_y_value = value;
                Properties.Settings.Default.Save();
            }
        }
        double scan_end_y_value = Properties.Settings.Default.scan_end_y_value;

        public double _measure_pix
        {
            get => measure_pix;
            set
            {
                measure_pix = value;
                OnPropertyChanged();
                Properties.Settings.Default.measure_pix = value;
                Properties.Settings.Default.Save();
                MeasureCompute();
            }
        }
        double measure_pix = Properties.Settings.Default.measure_pix;

        public double _measure_mm
        {
            get => measure_mm;
            set
            {
                measure_mm = value;
                OnPropertyChanged();
                Properties.Settings.Default.measure_mm = value;
                Properties.Settings.Default.Save();
                MeasureCompute();
            }
        }
        double measure_mm = Properties.Settings.Default.measure_mm;

        public double _range_computed
        {
            get => range_computed;
            set
            {
                range_computed = value;
                OnPropertyChanged();
            }
        }
        double range_computed;


        public string _saveFolderPath
        {
            get => saveFolderPath;
            set
            {
                saveFolderPath = value;
                OnPropertyChanged();
                Properties.Settings.Default.saveFolderPath = value;
                Properties.Settings.Default.Save();
            }
        }
        string saveFolderPath = Properties.Settings.Default.saveFolderPath;

        string scanfolder;

        public double _image_stability
        {
            get => image_stability;
            set
            {
                image_stability = value;
                OnPropertyChanged();
            }
        }
        double image_stability;

        public BitmapSource _map_path
        {
            get => mat_path.ToBitmapSource();
        }
        Mat mat_path;

        public BitmapSource _webcam
        {
            set
            {
                if (value == null) return;
                webcamImage = value;
                OnPropertyChanged();
            }
            get => webcamImage;
        }
        BitmapSource webcamImage;

        SerialPort _serialPort;

        //VideoSource videoSource;
        bool videoSource_isRunning;
        DeviceInformationCollection _videoDevices;
        DeviceInformation _videoDevice;
        Dictionary<string, List<VideoEncodingProperties>> _deviceCapabilities = new Dictionary<string, List<VideoEncodingProperties>>();
        VideoEncodingProperties _devicecap;

        MediaCapture _mediaCapture;
        MediaFrameReader _frameReader;

        StabilityDetector detector = new StabilityDetector(windowSize: 5, tolerance: 0.1);
        bool stability = false;

        System.Drawing.Rectangle? roi = null;
        OpenCvSharp.Rect? cvRoi = null;
        object lock_frame = new object();
        Mat originalframe;
        Mat frame;
        Mat frame_prev = new Mat();

        bool position_max_wait = false;
        Point2f position_max = new Point2f(-2, -2);

        bool scan_abord = false;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Charger les ports série disponibles
            LoadSerialPorts();

            // Charger les webcams disponibles
            LoadWebcams();

            // Désactiver les contrôles
            UpdateControlState(false);

            //Test_SuperResolution();
        }

        #region OLD SuperResolution (commenté)
        //void Test_SuperResolution()
        //{
        //    string folderPath = @"D:\PYTHON\ScanPlaneMaker\source\";
        //    DirectoryInfo dir = new DirectoryInfo(folderPath);
        //    var images = ChargerImagesDepuisDossier(folderPath);

        //    Mat moyenne = SuperResolution.MakeSuperResolutionFrom(images, SuperResolution.SuperResolutionType.moyenne);
        //    // Mat mediane = SuperResolution.MakeSuperResolutionFrom(images, SuperResolution.SuperResolutionType.mediane);

        //    moyenne.SaveImage(dir.Parent.FullName + "\\moyenne.jpg");
        //    // mediane.SaveImage(dir.Parent.FullName + "\\mediane.jpg");
        //}

        //public static List<Mat> ChargerImagesDepuisDossier(string folderPath)
        //{
        //    string[] imageFiles = Directory.GetFiles(folderPath);
        //List<Mat> images = new List<Mat>();
        //    // Charger chaque image
        //    foreach (var imagePath in imageFiles)
        //    {
        //        // Charger l'image avec OpenCV
        //        var img = Cv2.ImRead(imagePath);

        //        // Vérifier si l'image a bien été chargée
        //        if (!img.Empty())
        //            images.Add(img);
        //    }
        //    return images;
        //}        
        #endregion

        #region IHM
        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Fermer proprement les connexions
            CloseSerialPort();
            StopCamera();
        }

        void comboBoxCameras_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboBoxCameras.SelectedItem == null)
                return;

            string selection = comboBoxCameras.SelectedItem.ToString();
            foreach (DeviceInformation? device in _videoDevices)
            {
                if (device.Name == selection)
                {
                    _videoDevice = device;
                    LoadResolutions();
                    break;
                }
            }
        }
        void comboBoxResolutions_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboBoxResolutions.SelectedItem == null)
                return;

            string selection = comboBoxResolutions.SelectedItem.ToString();
            foreach (VideoEncodingProperties cap in _deviceCapabilities[_videoDevice.Name])
            {
                string resolution = $"[{cap.Subtype}] {cap.Width}x{cap.Height} {cap.FrameRate.Numerator} fps";
                if (resolution == selection)
                {
                    _devicecap = cap;
                    return;
                }
            }
        }

        void _btn_Arduino_Connect_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                string selectedPort = comboBoxPorts.SelectedItem as string;
                if (string.IsNullOrEmpty(selectedPort))
                {
                    System.Windows.MessageBox.Show("Please select a serial port.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _serialPort = new SerialPort(selectedPort, 9600, Parity.None, 8, StopBits.One);
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                _img_arduino_disconnect.Visibility = Visibility.Collapsed;
                _img_arduino_connect.Visibility = Visibility.Visible;
                UpdateControlState(true);

                AppendLogMessage($"Connected to {selectedPort}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error when connecting : {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        void _btn_Arduino_Disconnect_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                CloseSerialPort();
                UpdateControlState(false);
                _img_arduino_disconnect.Visibility = Visibility.Visible;
                _img_arduino_connect.Visibility = Visibility.Collapsed;
            }
        }


        void _btn_Send_Click(object sender, MouseButtonEventArgs e)
        {
            Send();
        }
        void _txt_Send_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Send();
            }
        }

        void _btn_Camera_Connect_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!videoSource_isRunning)
            {
                StartCamera();
                _img_camera_connect.Visibility = Visibility.Collapsed;
                _img_camera_disconnect.Visibility = Visibility.Visible;
            }
        }
        void _btn_Camera_Disconnect_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (videoSource_isRunning)
            {
                StopCamera();
                _img_camera_connect.Visibility = Visibility.Visible;
                _img_camera_disconnect.Visibility = Visibility.Collapsed;
            }
        }


        void _btn_Capture_click(object sender, MouseButtonEventArgs e)
        {
            ScreenShot();
        }


        void _btn_SelectFolder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (System.IO.Directory.Exists(_saveFolderPath))
                    OpenFolderInExplorer(_saveFolderPath);
                return;
            }
            var folderDialog = new OpenFolderDialog
            {
                Title = "Select Folder",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            if (System.IO.Directory.Exists(_saveFolderPath))
                folderDialog.InitialDirectory = _saveFolderPath;

            if (folderDialog.ShowDialog() == true)
            {
                _saveFolderPath = folderDialog.FolderName;
                txtSavePath.Text = ShortenPath(_saveFolderPath, 50);
                txtSavePath.ToolTip = _saveFolderPath;
                AppendLogMessage($"Save folder : {_saveFolderPath}");
            }

        }

        void _btn_COM_refresh_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LoadSerialPorts();
        }

        async void _btn_webcam_refresh_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            await LoadWebcams();
        }

        void _serial_text_right_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            txtReceivedData.Clear();
        }

        void _btn_move_left_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Move_X_moins();
        }
        void _btn_move_right_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Move_X_plus();
        }
        void _btn_move_down_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Move_Y_moins();
        }
        void _btn_move_up_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Move_Y_plus();
        }

        void _btn_scan_set_start_click(object sender, MouseButtonEventArgs e)
        {
            Send("p");
            Thread.Sleep(200);
            _scan_start_x_value = _x;
            _scan_start_y_value = _y;
        }
        void _btn_scan_set_end_click(object sender, MouseButtonEventArgs e)
        {
            Send("p");
            Thread.Sleep(200);
            _scan_end_x_value = _x;
            _scan_end_y_value = _y;
        }


        void _btn_move_X_max_y_max_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Send("j");
            position_max_wait = true;
        }
        void _btn_move_X_min_y_min_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Send("o");
        }
        void _btn_move_X_min_y_max_click(object sender, MouseButtonEventArgs e)
        {
            Send("7");
        }
        void _btn_move_X_max_y_min_click(object sender, MouseButtonEventArgs e)
        {
            Send("3");
        }

        void _btn_move_Zplus_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Move_Z_plus(1);
        }
        void _btn_move_Zmoins_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Move_Z_moins(1);
        }

        void _btn_move_Zplus2_click(object sender, MouseButtonEventArgs e)
        {
            Move_Z_plus(5);

        }

        void _btn_move_Zmoins2_click(object sender, MouseButtonEventArgs e)
        {
            Move_Z_moins(5);

        }

        void KeyDown_Pressed(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Space:
                    break;

                case System.Windows.Input.Key.PageUp:
                    Move_Z_plus(1);
                    break;

                case System.Windows.Input.Key.PageDown:
                    Move_Z_moins(1);
                    break;


                case System.Windows.Input.Key.Left:
                    Move_X_moins();
                    break;
                case System.Windows.Input.Key.Up:
                    Move_Y_plus();
                    break;
                case System.Windows.Input.Key.Right:
                    Move_X_plus();
                    break;
                case System.Windows.Input.Key.Down:
                    Move_Y_moins();
                    break;

            }
        }

        void _btn_goto_click(object sender, MouseButtonEventArgs e)
        {
            Send_GoTo(_goto_x_value, _goto_y_value);
        }

        void _btn_scan_start_click(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (System.IO.Directory.Exists(scanfolder))
                    OpenFolderInExplorer(scanfolder);
                else if (System.IO.Directory.Exists(_saveFolderPath))
                    OpenFolderInExplorer(_saveFolderPath);
                return;
            }

            //test Camera ?
            if (!videoSource_isRunning)
            {
                //start camera
                MessageBox.Show($"Camera need to be started.");
                return;
            }

            //test Arduino ?
            if (_serialPort == null || !_serialPort.IsOpen)
            {
                //start arduino
                MessageBox.Show($"Connect to Arduino board first.");
                return;
            }

            Scan();
            _img_scan_start.Visibility = Visibility.Collapsed;
            _img_scan_stop.Visibility = Visibility.Visible;
        }
        void _btn_scan_stop_click(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (System.IO.Directory.Exists(scanfolder))
                    OpenFolderInExplorer(scanfolder);
                return;
            }
            scan_abord = true;
            _img_scan_start.Visibility = Visibility.Visible;
            _img_scan_stop.Visibility = Visibility.Collapsed;
        }

        void _btn_goto_start_click(object sender, MouseButtonEventArgs e)
        {
            Send_GoTo(scan_start_x_value, scan_start_y_value);
        }
        void _btn_goto_end_click(object sender, MouseButtonEventArgs e)
        {
            Send_GoTo(scan_end_x_value, scan_end_y_value);
        }

        void _btn_get_position_click(object sender, MouseButtonEventArgs e)
        {
            Send("p");
        }

        void _btn_measure_on_image_click(object sender, MouseButtonEventArgs e)
        {
            if (frame == null || frame.Empty())
                return;
            _measure_pix = ImageMeasure._Display(frame);
        }

        void _btn_range_copy_compute_to_range_value_click(object sender, MouseButtonEventArgs e)
        {
            _y_range_value = _range_computed;
            _x_range_value = _range_computed;
        }



        void _btn_roi_click(object sender, MouseButtonEventArgs e)
        {
            // Récupérer les ROIs sélectionnées
            List<OpenCvSharp.Rect> selectedROIs = ROISelector.CreateROIs(originalframe);
            if (selectedROIs.Count > 0)
            {
                var drawing_roi = selectedROIs[0];
                roi = new Rectangle(drawing_roi.X, drawing_roi.Y, drawing_roi.Width, drawing_roi.Height);
                cvRoi = null;
                ROI_Display();
            }
        }
        #endregion

        void ROI_Display()
        {
            Dispatcher.Invoke(() =>
            {
                if (roi == null)
                    _lbl_roi.Content = "?";
                else
                    _lbl_roi.Content = ((Rectangle)roi).Width + "x" + ((Rectangle)roi).Height;
            });
        }

        #region Arduino
        void LoadSerialPorts()
        {
            comboBoxPorts.Items.Clear();
            foreach (string port in SerialPort.GetPortNames())
            {
                comboBoxPorts.Items.Add(port);
            }

            if (comboBoxPorts.Items.Count > 0)
                comboBoxPorts.SelectedIndex = comboBoxPorts.Items.Count - 1;
        }
        void CloseSerialPort()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.DataReceived -= SerialPort_DataReceived;
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;

                AppendLogMessage("Port série fermé");
            }
        }

        void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                return;

            try
            {
                string data = _serialPort.ReadLine();

                if (data.Contains("Position = "))
                {
                    string xystring = data.Replace("Position = ", "");
                    xystring = xystring.Replace("\r", "");
                    string[] splitter = new string[1] { ", " };
                    string[] xy = xystring.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
                    float x = float.Parse(xy[0].Replace('.', ','));
                    float y = float.Parse(xy[1].Replace('.', ','));
                    _x = x;
                    _y = y;
                }

                if (data.Contains("PositionMax = "))
                {
                    string xystring = data.Replace("PositionMax = ", "");
                    xystring = xystring.Replace("\r", "");
                    string[] splitter = new string[1] { ", " };
                    string[] xy = xystring.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
                    float x = float.Parse(xy[0].Replace('.', ','));
                    float y = float.Parse(xy[1].Replace('.', ','));
                    position_max = new Point2f(x, y);
                }

                data = System.Text.RegularExpressions.Regex.Replace(data, @"\r+$", "");

                // Dispatcher pour mettre à jour l'interface depuis un thread différent
                Dispatcher.Invoke(() =>
                {
                    AppendLogMessage($"Received : {data}");
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    AppendLogMessage($"Receive ERROR : {ex.Message}");
                });
            }
        }

        void Send()
        {
            string dataToSend = txtSendData.Text;
            Send(dataToSend);
        }

        void Send_GoTo(Point2d p) { Send_GoTo(p.X, p.Y); }
        void Send_GoTo(double x, double y)
        {
            Send("g"
                + x.ToString("0.00").Replace(",", ".")
                + ";"
                + y.ToString("0.00").Replace(",", "."));
        }
        void Send_get_positionMax()
        {
            Send("k");
        }

        void Send(string dataToSend)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
            {
                System.Windows.MessageBox.Show("Serial port not connected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(dataToSend))
            {
                System.Windows.MessageBox.Show("Please input some text to send.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                _serialPort.WriteLine(dataToSend);
                AppendLogMessage($"Sended: {dataToSend}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error when sending : {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Camera
        async Task LoadWebcams()
        {
            comboBoxCameras.Items.Clear();
            // Liste les caméras vidéo
            _videoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            foreach (var device in _videoDevices)
            {
                comboBoxCameras.Items.Add(device.Name);
                try
                {
                    var capture = new MediaCapture();
                    var settings = new MediaCaptureInitializationSettings
                    {
                        VideoDeviceId = device.Id,
                        StreamingCaptureMode = StreamingCaptureMode.Video
                    };

                    await capture.InitializeAsync(settings);

                    var controller = capture.VideoDeviceController;
                    var mediaStreamProperties = controller.GetAvailableMediaStreamProperties(MediaStreamType.VideoRecord);
                    _deviceCapabilities.Add(device.Name, new List<VideoEncodingProperties>());

                    foreach (var prop in mediaStreamProperties)
                        if (prop is VideoEncodingProperties v)
                            _deviceCapabilities[device.Name].Add(v);

                    capture.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  [Erreur lors de la récupération des formats] : {ex.Message}");
                }
            }

            if (_videoDevices.Count == 0)
            {
                System.Windows.MessageBox.Show("Aucune webcam détectée.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (comboBoxCameras.Items.Count > 0)
                comboBoxCameras.SelectedIndex = 0;
        }
        void LoadResolutions()
        {
            comboBoxResolutions.Items.Clear();

            string selectedDevice = comboBoxCameras.SelectedItem as string;
            if (selectedDevice == null || !_deviceCapabilities.ContainsKey(selectedDevice))
                return;

            List<VideoEncodingProperties> capabilities = _deviceCapabilities[selectedDevice];

            // Tri par résolution (la plus haute en premier)
            capabilities = capabilities
                .OrderBy(c => c.Subtype)                      // d'abord tri par Subtype (ordre alphabétique)
                .ThenByDescending(c => c.Width * c.Height)    // ensuite par résolution décroissante
                .ToList();
            foreach (VideoEncodingProperties cap in capabilities)
            {
                string resolution = $"[{cap.Subtype}] {cap.Width}x{cap.Height} {cap.FrameRate.Numerator} fps";
                comboBoxResolutions.Items.Add(resolution);
            }

            if (comboBoxResolutions.Items.Count > 0)
                comboBoxResolutions.SelectedIndex = 0;  // Sélectionner la résolution la plus élevée
        }

        async void StartCamera()
        {
            // Initialiser MediaCapture
            _mediaCapture = new MediaCapture();

            var settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = _videoDevice.Id,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu // Important pour l'accès CPU
            };

            await _mediaCapture.InitializeAsync(settings);

            // Configurer les propriétés d'encodage vidéo
            var encodingProperties = VideoEncodingProperties.CreateUncompressed(
                _devicecap.Subtype,
                (uint)_devicecap.Width,
                (uint)_devicecap.Height
            );
            encodingProperties.FrameRate.Numerator = _devicecap.FrameRate.Numerator;
            encodingProperties.FrameRate.Denominator = 1;

            // 4. Trouver la source de frame qui correspond le mieux
            MediaFrameSource frameSource = _mediaCapture.FrameSources.FirstOrDefault(source =>
                source.Value.Info.MediaStreamType == MediaStreamType.VideoRecord).Value;

            if (frameSource == null)
                throw new Exception("Aucune source vidéo trouvée");

            // Sélectionner le format le plus proche de nos besoins
            MediaFrameFormat preferredFormat = frameSource.SupportedFormats
                .OrderBy(format => Math.Abs((int)format.VideoFormat.Width - _devicecap.Width))
                .ThenBy(format => Math.Abs((int)format.VideoFormat.Height - _devicecap.Height))
                .FirstOrDefault();

            if (preferredFormat != null)
            {
                await frameSource.SetFormatAsync(preferredFormat);
                Console.WriteLine($"Format sélectionné: {preferredFormat.VideoFormat.Width}x{preferredFormat.VideoFormat.Height}");
            }

            // Créer le FrameReader
            _frameReader = await _mediaCapture.CreateFrameReaderAsync(frameSource);
            _frameReader.FrameArrived += FrameReader_FrameArrived;


            //réinit ROI si pas exploitable
            if (roi != null)
            {
                if (roi.Value.X + roi.Value.Width > _devicecap.Width ||
                    roi.Value.Y + roi.Value.Height > _devicecap.Height
                    )
                {
                    roi = null;
                    cvRoi = null;
                }
            }

            // Démarrer la capture
            MediaFrameReaderStartStatus status = await _frameReader.StartAsync();
            AppendLogMessage($"Caméra démarrée: {_videoDevice.Name}");
            AppendLogMessage($"FrameReader status: {status}");

            if (status == MediaFrameReaderStartStatus.Success)
                videoSource_isRunning = true;
        }

        void StopCamera()
        {
            StopCapturingAsync();
            // Effacer l'image
            imgWebcam.Source = null;

            AppendLogMessage("Caméra arrêtée");
            videoSource_isRunning = false;
        }

        BitmapSource ConvertSoftwareBitmapToBitmapSource(SoftwareBitmap sb)
        {
            // Assurer le format Bgra8
            var converted = SoftwareBitmap.Convert(sb, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            int width = converted.PixelWidth;
            int height = converted.PixelHeight;

            // Créer un tableau pour stocker les pixels
            byte[] pixels = new byte[width * height * 4]; // 4 bytes par pixel (Bgra8)
            converted.CopyToBuffer(pixels.AsBuffer());   // CopyToBuffer nécessite un IBuffer

            // Créer le WriteableBitmap WPF
            WriteableBitmap wb = new WriteableBitmap(
                width,
                height,
                96, 96, // DPI
                System.Windows.Media.PixelFormats.Bgra32,
                null);

            // Copier les pixels dans le WriteableBitmap
            wb.WritePixels(
                new System.Windows.Int32Rect(0, 0, width, height),
                pixels,
                width * 4, // stride = width * bytes per pixel
                0);

            return wb;
        }

        void FrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            using (var frame = sender.TryAcquireLatestFrame())
            {
                if (frame?.VideoMediaFrame?.SoftwareBitmap != null)
                {
                    ProcessFrameWithOpenCV(frame.VideoMediaFrame.SoftwareBitmap);
                }
            }
        }

        // Interface nécessaire pour accéder aux données de buffer
        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }

        private void ProcessFrameWithOpenCV(Windows.Graphics.Imaging.SoftwareBitmap softwareBitmap)
        {
            try
            {
                // Convertir en Bgra8
                var bitmap = Windows.Graphics.Imaging.SoftwareBitmap.Convert(
                    softwareBitmap,
                    Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,
                    Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied
                );

                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;

                // Copier les données dans un tableau managé
                byte[] pixels = new byte[width * height * 4];
                bitmap.CopyToBuffer(pixels.AsBuffer());

                // Créer la Mat OpenCV depuis le tableau
                unsafe
                {
                    fixed (byte* ptr = pixels)
                    {
                        using (var mat = Mat.FromPixelData(height, width, MatType.CV_8UC4, (IntPtr)ptr))
                        {
                            // Clone pour éviter les problèmes de lifetime
                            var matClone = mat.Clone();
                            VideoSource_NewFrame2(this, matClone);
                            matClone.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLogMessage($"Frame error: {ex.Message}");
            }
        }

        public async Task StopCapturingAsync()
        {
            if (_frameReader != null)
            {
                await _frameReader.StopAsync();
                Console.WriteLine("Capture arrêtée");
            }
            _frameReader?.Dispose();
            _frameReader = null;
            _mediaCapture?.Dispose();
            _mediaCapture = null;
        }


        private DateTime _lastFrameUpdate = DateTime.MinValue;
        private const int MinFrameIntervalMs = 33; // ~30 fps max pour l'affichage
        private readonly object _frameLock = new object();
        private Mat _displayFrame = null;
        private bool _isProcessingFrame = false;
        void VideoSource_NewFrame2(object? sender, Mat e)
        {
            try
            {
                // Éviter le traitement simultané de plusieurs frames
                if (_isProcessingFrame) return;
                _isProcessingFrame = true;

                if (roi == null)
                {
                    int w = e.Width;
                    int h = e.Height;
                    roi = new Rectangle(w / 2 - h / 2, 0, h, h);
                    ROI_Display();
                }

                if (cvRoi == null)
                {
                    Rectangle r = (Rectangle)roi;
                    cvRoi = new OpenCvSharp.Rect(r.X, r.Y, r.Width, r.Height);
                }

                if (e.Empty())
                {
                    _isProcessingFrame = false;
                    return;
                }

                // Limitation de fréquence d'affichage
                var now = DateTime.Now;
                var timeSinceLastUpdate = (now - _lastFrameUpdate).TotalMilliseconds;

                if (timeSinceLastUpdate < MinFrameIntervalMs)
                {
                    _isProcessingFrame = false;
                    return;
                }

                lock (lock_frame)
                {
                    // Libérer l'ancienne frame d'affichage
                    _displayFrame?.Dispose();

                    originalframe?.Dispose();
                    originalframe = e.Clone();

                    // Extraction de la ROI (copie indépendante)
                    Mat roiCopy = new Mat(e, (OpenCvSharp.Rect)cvRoi).Clone();

                    _displayFrame = roiCopy.Clone();
                    frame?.Dispose();
                    frame = roiCopy;
                }

                // Mise à jour UI asynchrone
                if (timeSinceLastUpdate >= MinFrameIntervalMs)
                {
                    _lastFrameUpdate = now;

                    // Utiliser BeginInvoke au lieu de Invoke pour ne pas bloquer
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            lock (lock_frame)
                            {
                                if (_displayFrame != null && !_displayFrame.IsDisposed)
                                {
                                    // Freeze le BitmapSource pour améliorer les performances
                                    var bitmapSource = _displayFrame.ToBitmapSource();
                                    bitmapSource.Freeze(); // Important !
                                    _webcam = bitmapSource;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            AppendLogMessage($"Error updating display: {ex.Message}");
                        }
                    }), System.Windows.Threading.DispatcherPriority.Render);
                }

                Compute_image_stability();

                frame_prev?.Dispose();
                frame_prev = frame.Clone();

                _isProcessingFrame = false;
            }
            catch (Exception ex)
            {
                _isProcessingFrame = false;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    AppendLogMessage($"Error in VideoSource_NewFrame: {ex.Message}");
                }));
            }
        }
        //void VideoSource_NewFrame2(object? sender, Mat e)
        //{
        //    try
        //    {
        //        if (roi == null)
        //        {
        //            int w = e.Width;
        //            int h = e.Height;
        //            roi = new Rectangle(w / 2 - h / 2, 0, h, h);
        //            ROI_Display();
        //        }

        //        if (cvRoi == null)
        //        {
        //            Rectangle r = (Rectangle)roi;
        //            cvRoi = new OpenCvSharp.Rect(r.X, r.Y, r.Width, r.Height);
        //        }

        //        if (e.Empty())
        //            return;

        //        lock (lock_frame)
        //        {
        //            originalframe = e.Clone();

        //            // Extraction de la ROI (copie indépendante)
        //            Mat roiCopy = new Mat(e, (OpenCvSharp.Rect)cvRoi).Clone();

        //            Dispatcher.Invoke(() => { _webcam = roiCopy.ToBitmapSource(); });
        //            frame = roiCopy;
        //        }

        //        Compute_image_stability();

        //        frame_prev = frame.Clone();

        //    }
        //    catch (Exception ex)
        //    {
        //        Dispatcher.Invoke(() =>
        //        {
        //            AppendLogMessage($"Error in VideoSource_NewFrame: {ex.Message}");
        //        });
        //    }
        //}

        void Compute_image_stability()
        {
            if (!frame_prev.Empty())
            {
                if (frame.Size() != frame_prev.Size())
                {
                    frame_prev = frame;
                    return;
                }

                Mat diff = new Mat();
                // Calcule la différence entre l'image actuelle et la précédente
                Cv2.Absdiff(frame, frame_prev, diff);

                // Calcule la "quantité" de différence (moyenne)
                Scalar meanDiff = Cv2.Mean(diff);

                //image stable ?
                detector.AddValue(meanDiff.Val0);
                stability = detector.IsStable();
                _image_stability = detector._tunnel;

                _brd_image_borderbrush = (stability) ? vert : rouge;
            }
        }


        void ScreenShot(string scanfolder = null)
        {
            lock (lock_frame)
            {
                if (frame == null || frame.Empty())
                {
                    System.Windows.MessageBox.Show("Aucune image de caméra disponible.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrEmpty(_saveFolderPath))
                {
                    System.Windows.MessageBox.Show("Veuillez sélectionner un dossier de sauvegarde.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    _btn_SelectFolder_Click(null, null);
                    if (string.IsNullOrEmpty(_saveFolderPath))
                        return;
                }

                try
                {
                    string fileName;
                    string filePath;
                    if (scanfolder == null)
                    {
                        // Générer un nom de fichier unique basé sur la date et l'heure
                        fileName = $"{_x.ToString("f2")};{_y.ToString("f2")} {DateTime.Now:yyyyMMdd_HHmmss.fff}.jpg";
                        filePath = System.IO.Path.Combine(_saveFolderPath, fileName);
                    }
                    else
                    {
                        // Générer un nom de fichier unique basé sur la date et l'heure
                        fileName = $"{_x.ToString("f2")};{_y.ToString("f2")}.jpg";
                        filePath = scanfolder + fileName;
                    }

                    //créé le dossier s'il n'existe pas
                    new FileInfo(filePath).Directory.Create();

                    // Sauvegarder l'image
                    frame.SaveImage(filePath);

                    AppendLogMessage($"Image saved: {fileName}");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error when saving image to disk : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        string ShortenPath(string path, int maxLength)
        {
            if (path.Length <= maxLength)
                return path;

            return "..." + path.Substring(path.Length - maxLength);
        }

        #region Log on IHM
        void AppendLogMessage(string message)
        {
            string timeStamp = DateTime.Now.ToString("HH:mm:ss");
            Dispatcher.BeginInvoke(new Action(() =>
            {
                txtReceivedData.AppendText($"[{timeStamp}] {message}{Environment.NewLine}");
                txtReceivedData.ScrollToEnd();
            }));
        }

        void UpdateControlState(bool isConnected)
        {
            _btn_Send.IsEnabled = isConnected;
            txtSendData.IsEnabled = isConnected;
        }
        #endregion

        #region Move
        void Move_X_plus() { Send("x" + ((float)_xy_move_value).ToString("0.000").Replace(",", ".")); }
        void Move_X_moins() { Send("x-" + ((float)_xy_move_value).ToString("0.000").Replace(",", ".")); }
        void Move_Y_plus() { Send("y" + ((float)_xy_move_value).ToString("0.000").Replace(",", ".")); }
        void Move_Y_moins() { Send("y-" + ((float)_xy_move_value).ToString("0.000").Replace(",", ".")); }

        void Move_Z_plus(float factor) { Send("z" + (factor * 0.02).ToString("0.00").Replace(",", ".")); }
        void Move_Z_moins(float factor) { Send("z-" + (factor * 0.02).ToString("0.00").Replace(",", ".")); }
        #endregion

        void Scan()
        {
            List<Point2d> points = Compute_Positions();
            List<Point2d> path = FindShortestPath(points);
            Thread thread = new Thread(() => Scan(path));
            thread.Start();
        }

        void DrawPath(List<Point2d> path, List<Point2d> pathalreadyviewed, Point2d current)
        {
            if (position_max.X == -2) //variable init here
            {
                Send_get_positionMax();
                while (position_max.X == -2)
                    Thread.Sleep(100);
            }

            if (position_max.X == -1) // variable init in arduino
            {
                AppendLogMessage("DrawPath need to system to be initialized (max position)");
                return;
            }

            try
            {
                // 1 mm => 10 pixel 
                double f = 10;
                int w = (int)(position_max.X * f);
                int h = (int)(position_max.Y * f);

                Mat mat = new Mat(h, w, MatType.CV_8UC3, new Scalar(255, 255, 255));//bgr

                //dessine les zones
                foreach (Point2d p in path)
                {
                    int x = (int)((p.X - _x_range_value / 2) * f);
                    int y = (int)((p.Y - _y_range_value / 2) * f);
                    OpenCvSharp.Rect rect = new OpenCvSharp.Rect(x, y, (int)(_x_range_value * f), (int)(_y_range_value * f));
                    Cv2.Rectangle(mat, rect, Scalar.Black);
                }

                //dessine les zones déjà visitées
                foreach (Point2d p in pathalreadyviewed)
                {
                    int x = (int)((p.X - _x_range_value / 2) * f);
                    int y = (int)((p.Y - _y_range_value / 2) * f);
                    OpenCvSharp.Rect rect = new OpenCvSharp.Rect(x, y, (int)(_x_range_value * f), (int)(_y_range_value * f));
                    Cv2.Rectangle(mat, rect, Scalar.DarkCyan, -1);
                }

                //dessine le parcours
                OpenCvSharp.Point A = new OpenCvSharp.Point(path[0].X * f, path[0].Y * f);
                foreach (Point2d p in path)
                {
                    int x = (int)(p.X * f);
                    int y = (int)(p.Y * f);
                    OpenCvSharp.Point B = new OpenCvSharp.Point(x, y);
                    Cv2.Line(mat, A, B, Scalar.Red, thickness: 3);
                    A = B;
                }

                int x_current = (int)((current.X - _x_range_value / 2) * f);
                int y_current = (int)((current.Y - _y_range_value / 2) * f);
                OpenCvSharp.Rect rect_current = new OpenCvSharp.Rect(x_current, y_current, (int)(_x_range_value * f), (int)(_y_range_value * f));
                Cv2.Rectangle(mat, rect_current, Scalar.YellowGreen, -1);

                Dispatcher.Invoke(() =>
                {
                    mat_path = mat.Clone();
                    OnPropertyChanged(nameof(_map_path));
                });

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        List<Point2d> Compute_Positions()
        {
            //Premier point X le plus petit et Y le plus petit
            double x_min_edge = Math.Min(_scan_start_x_value, _scan_end_x_value);
            double y_min_edge = Math.Min(_scan_start_y_value, _scan_end_y_value);
            double x_max_edge = Math.Max(_scan_start_x_value, _scan_end_x_value);
            double y_max_edge = Math.Max(_scan_start_y_value, _scan_end_y_value);

            double x_min = x_min_edge + _x_range_value / 2;
            double y_min = y_min_edge + _y_range_value / 2;

            List<Point2d> points = new List<Point2d>();

            Point2d p_last = new Point2d(x_max_edge, y_max_edge);

            Point2d p = new Point2d(x_min, y_min);
            points.Add(p);

            Rect2d rect = new Rect2d(
                p.X - _x_range_value / 2,
                p.Y - _y_range_value / 2,
                _x_range_value,
                _y_range_value);

            while (!PointInRect(p_last, rect))
            {
                //si on augmente en X, est ce qu'on dépasse en X ?
                if (p.X > x_max_edge)
                    //on remonte en Y
                    p = new Point2d(x_min, p.Y + _y_range_value);
                else
                    //on augmente en X
                    p = new Point2d(p.X + _x_range_value, p.Y);

                rect = new Rect2d(
                    p.X - _x_range_value / 2,
                    p.Y - _y_range_value / 2,
                    _x_range_value,
                    _y_range_value);
                //ajout point
                points.Add(p);
            }
            return points;
        }

        void Scan(List<Point2d> path)
        {
            try
            {
                //Go !
                scan_abord = false;
                DateTime t0 = DateTime.Now;
                scanfolder = _saveFolderPath + $"\\Scan {t0: yyyy-MM-dd HH-mm-ss}\\";
                List<Point2d> path_viewed = new List<Point2d>();
                for (int i = 0; i < path.Count; i++)
                {
                    //get next target
                    _scan_progress = i + 1 + " / " + path.Count;
                    Point2d p = path[i];

                    DrawPath(path, path_viewed, p);
                    path_viewed.Add(p);

                    //move camera
                    Send_GoTo(p);

                    //wait to be at the point
                    while (!OnPosition(p))
                        Thread.Sleep(100);

                    //wait to get a stable image
                    while (!stability && !scan_abord)
                        Thread.Sleep(100);

                    //if abord
                    if (scan_abord)
                        break;

                    //save image
                    ScreenShot(scanfolder);

                    ////wait to take picture before move
                    //Thread.Sleep(100);
                }

                if (scan_abord)
                    _scan_progress += "\naborded";
                else
                    _scan_progress += "\ndone";

                _scan_progress += " (" + DateTime.Now.Subtract(t0).TotalSeconds.ToString("0") + "s)";

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _img_scan_start.Visibility = Visibility.Visible;
                    _img_scan_stop.Visibility = Visibility.Collapsed;

                    AppendLogMessage("Scan " + _scan_progress.Replace("\n", " "));

                    MessageBoxResult rep = MessageBox.Show("Do you want to open the scan folder ?",
                        "Scan finished",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    //propose d'ouvrir l'explorateur windows directement sur le dossier de scan
                    if (rep == MessageBoxResult.Yes)
                        OpenFolderInExplorer(scanfolder);

                    //copie dans le presse papier
                    Clipboard.SetText(scanfolder);
                }));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        bool OnPosition(Point2d p)
        {
            double delta_X = Math.Abs(p.X - x);
            double delta_Y = Math.Abs(p.Y - y);
            return delta_X < 0.01 && delta_Y < 0.01;
        }

        bool PointInRect(Point2d p, OpenCvSharp.Rect2d rect)
        {
            return rect.Contains(p);
        }

        static List<Point2d> FindShortestPath(List<Point2d> points)
        {
            if (points.Count == 0) return new List<Point2d>();

            var unvisited = new HashSet<Point2d>(points);
            var path = new List<Point2d>();
            Point2d current = points[0];
            path.Add(current);
            unvisited.Remove(current);

            while (unvisited.Count > 0)
            {
                Point2d next = unvisited.OrderBy(p => current.DistanceTo(p)).First();
                path.Add(next);
                unvisited.Remove(next);
                current = next;
            }

            return path; // Ne revient pas au point de départ. Ajoute path[0] à la fin si besoin.
        }

        void OpenFolderInExplorer(string folder)
        {
            System.Diagnostics.Process.Start("explorer.exe", folder);
        }

        void MeasureCompute()
        {
            _range_computed = frame.Width * _measure_mm / _measure_pix;
        }
    }
}