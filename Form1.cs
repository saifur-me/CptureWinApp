using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Accord.Video.DirectShow;

namespace CptureWinApp
{
    public partial class Form1 : Form
    {
        private const string ConnectionString = "User Id=TEST; Password=password1; Data Source=localhost:1521/iot";
        private ManualResetEventSlim startCaptureEvent = new ManualResetEventSlim(false);
        private Task[] captureTasks;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private VideoCapture previewCapture;
        private Thread videoCaptureThread;
        int selectedIndex;
        private PictureBox pictureBox;
        private FilterInfoCollection videoDevices;

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            btnStart.Click += btnStart_Click;
            btnStop.Click += btnStop_Click;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Detect connected cameras and populate ComboBox
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in videoDevices)
            {
                cmbCameraList.Items.Add(device.Name);
            }

            if (cmbCameraList.Items.Count > 0)
            {
                cmbCameraList.SelectedIndex = 0;
            }

            cmbCameraList.SelectedIndexChanged += cmbCameraList_SelectedIndexChanged;

            // Initialize PictureBox for camera preview
            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            panelCameraPreview.Controls.Add(pictureBox);
        }

        private void cmbCameraList_SelectedIndexChanged(object sender, EventArgs e)
        {

            this.selectedIndex = cmbCameraList.SelectedIndex;
            videoCaptureThread = new Thread(InitializeVideoCaptureAsync);
            videoCaptureThread.Start();
            //previewCapture = new VideoCapture(selectedIndex);

        }

        private async void InitializeVideoCaptureAsync()
        {
            await Task.Run(() =>
            {
                previewCapture = new VideoCapture(selectedIndex);
                WaitForInitialization();
            });
        }

        // Example method to wait for the thread to complete
        public void WaitForInitialization()
        {
            videoCaptureThread.Join(); // This will now not block the UI thread
            if (!previewCapture.IsOpened())
            {
                MessageBox.Show($"Failed to open camera {videoDevices[this.selectedIndex].Name}!");
                return;
            }

            // Run the camera preview in a separate task
            Task.Run(() => UpdateCameraPreview(previewCapture));
        }

        private void UpdateCameraPreview(VideoCapture capture)
        {
            Mat frame = new Mat();
            try
            {
                while (capture.IsOpened())
                {
                    capture.Read(frame);
                    if (frame.Empty()) continue;

                    Bitmap image = BitmapConverter.ToBitmap(frame);
                    pictureBox.Invoke((MethodInvoker)(() =>
                    {
                        pictureBox.Image?.Dispose();
                        pictureBox.Image = image;
                    }));

                    Thread.Sleep(30);
                }
            }
            catch (ObjectDisposedException)
            {
                pictureBox.Invoke((MethodInvoker)(() =>
                {
                    pictureBox.Image?.Dispose();
                    pictureBox.Image = null;
                }));
            }
            finally
            {
                frame.Dispose();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            string directory = @"D:\CaptureApp\CaptureImage";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var cameraIndices = DetectConnectedCameras();

            if (cameraIndices.Length == 0)
            {
                txtLog.AppendText("No cameras detected!" + Environment.NewLine);
                return;
            }

            int interval;
            if (!int.TryParse(txtInterval.Text, out interval))
            {
                MessageBox.Show("Please enter a valid interval.");
                return;
            }

            txtLog.AppendText("Press Stop to stop..." + Environment.NewLine);

            startCaptureEvent.Reset();

            captureTasks = new Task[cameraIndices.Length];
            for (int i = 0; i < cameraIndices.Length; i++)
            {
                int cameraIndex = cameraIndices[i];
                captureTasks[i] = Task.Run(() => CaptureFromCamera(cameraIndex, directory, startCaptureEvent, cancellationTokenSource.Token, interval));
            }

            Thread.Sleep(500);
            startCaptureEvent.Set();
        }

        private int[] DetectConnectedCameras()
        {
            var cameraIndices = new List<int>();

            for (int i = 0; i < videoDevices.Count; i++)
            {
                using (var capture = new VideoCapture(i))
                {
                    if (capture.IsOpened())
                    {
                        cameraIndices.Add(i);
                    }
                }
            }

            return cameraIndices.ToArray();
        }

        private void CaptureFromCamera(int cameraIndex, string directory, ManualResetEventSlim startCaptureEvent, CancellationToken cancellationToken, int interval)
        {
            using (var capture = new VideoCapture(cameraIndex))
            {
                if (!capture.IsOpened())
                {
                    Invoke(new Action(() => txtLog.AppendText($"Failed to open camera {videoDevices[cameraIndex].Name}!" + Environment.NewLine)));
                    return;
                }

                Mat frame = new Mat();
                string cameraName = videoDevices[cameraIndex].Name;
                while (true)
                {
                    startCaptureEvent.Wait();

                    capture.Read(frame);

                    if (frame.Empty() || cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // Add watermark in frame
                    string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string dateText = $"{dateTime}";
                    string sourceText = $"CAM {cameraIndex} - {cameraName}";

                    // Adding outline black in text
                    AddTextWithOutline(frame, dateText, new OpenCvSharp.Point(10, frame.Rows - 30), HersheyFonts.HersheySimplex, 0.5, Scalar.White, 1, Scalar.Black);
                    AddTextWithOutline(frame, sourceText, new OpenCvSharp.Point(10, frame.Rows - 10), HersheyFonts.HersheySimplex, 0.5, Scalar.White, 1, Scalar.Black);

                    string fileName = $"CAM_{cameraName}:{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                    string path = Path.Combine(directory, fileName);
                    Cv2.ImWrite(path, frame);
                    Invoke(new Action(() => txtLog.AppendText($"Succes Captured : {cameraName} , DateTime : {dateTime}" + Environment.NewLine)));

                    // Insert data into database
                    InsertCaptureData(dateTime, $"{cameraName}", fileName, path);

                    startCaptureEvent.Reset();

                    Thread.Sleep(interval * 1000);

                    startCaptureEvent.Set();
                }
            }
        }

        private void AddTextWithOutline(Mat img, string text, OpenCvSharp.Point point, HersheyFonts fontFace, double fontScale, Scalar fontColor, int thickness, Scalar outlineColor)
        {
            Cv2.PutText(img, text, point, fontFace, fontScale, outlineColor, thickness + 2);
            Cv2.PutText(img, text, point, fontFace, fontScale, fontColor, thickness);
        }

        private void InsertCaptureData(string dateTime, string sourceAsset, string fileName, string fileLoc)
        {
            using (var connection = new OracleConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO CAPTURE (DATETIME, SOURCEASSET, FILENAME, FILELOC) VALUES (:dateTime, :sourceAsset, :fileName, :fileLoc)";

                        DateTime parsedDateTime;
                        if (DateTime.TryParse(dateTime, out parsedDateTime))
                        {
                            command.Parameters.Add("dateTime", OracleDbType.TimeStamp).Value = parsedDateTime;
                        }
                        else
                        {
                            Invoke(new Action(() => txtLog.AppendText("Failed to parse dateTime." + Environment.NewLine)));
                            return;
                        }

                        command.Parameters.Add("sourceAsset", OracleDbType.Varchar2).Value = sourceAsset;
                        command.Parameters.Add("fileName", OracleDbType.Varchar2).Value = fileName;
                        command.Parameters.Add("fileLoc", OracleDbType.Varchar2).Value = fileLoc;

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Invoke(new Action(() => txtLog.AppendText($"Failed to insert data: {ex.Message}" + Environment.NewLine)));
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            cancellationTokenSource.Cancel();

            if (captureTasks != null)
            {
                try
                {
                    Task.WaitAll(captureTasks);
                }
                catch (AggregateException ex)
                {
                    foreach (var innerEx in ex.InnerExceptions)
                    {
                        txtLog.AppendText($"Capture task failed: {innerEx.Message}" + Environment.NewLine);
                    }
                }
            }

            startCaptureEvent.Set();
            txtLog.AppendText("Capture stopped." + Environment.NewLine);

            if (previewCapture != null)
            {
                previewCapture.Release();
                previewCapture.Dispose();
                previewCapture = null;
            }
        }
    }
}