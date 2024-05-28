using SimpleTCP; // Importing SimpleTCP library for TCP communication
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ChatApplication
{
    public partial class Form1 : Form
    {
        SimpleTcpClient client; // TCP client instance
        string username; // User's username
        Timer statusTimer; // Timer for status messages
        private bool dragging = false; // Flag to indicate dragging of form
        private Point dragCursorPoint; // Cursor position during dragging
        private Point dragFormPoint; // Form position during dragging

        // Constructor for Form1
        public Form1(string username)
        {
            InitializeComponent();
            this.username = username; // Assign the username passed from login form
            lblUser.Text = username; // Set username label

            statusTimer = new Timer(); // Initialize status timer
            statusTimer.Tick += StatusTimer_Tick; // Subscribe to Tick event
            this.Load += Form1_Load; // Subscribe to Load event
        }

        // Event handler for Form1 Load event
        private void Form1_Load(object sender, EventArgs e)
        {
            // Initialize TCP client
            client = new SimpleTcpClient();
            client.Delimiter = 0x13; // Set message delimiter to 'enter'
            client.StringEncoder = Encoding.UTF8; // Set string encoder to UTF-8
            client.DataReceived += Client_DataReceived; // Subscribe to DataReceived event
        }

        // Method to show login form
        private void ShowLoginForm()
        {
            using (LoginForm loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // Retrieve username from login form
                    username = loginForm.Username;

                    // Initialize TCP client
                    client = new SimpleTcpClient();
                    client.Delimiter = 0x13; // Set message delimiter to 'enter'
                    client.StringEncoder = Encoding.UTF8; // Set string encoder to UTF-8
                    client.DataReceived += Client_DataReceived; // Subscribe to DataReceived event
                    lblUser.Text = username; // Set username label

                    // Show the main form after successful login
                    this.Show();
                }
                else
                {
                    // Close the application if the user cancels the login
                    this.Close();
                }
            }
        }

        // Event handler for data received from server
        private void Client_DataReceived(object sender, SimpleTCP.Message e)
        {
            // Invoke the UI thread to update status textbox
            txtStatus.Invoke((MethodInvoker)delegate ()
            {
                // Append received message to status textbox
                txtStatus.AppendText(e.MessageString + Environment.NewLine);
            });
        }

        // Event handler for Connect button click
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                // Disable Connect button
                btnConnect.Enabled = false;

                // Parse IP address
                System.Net.IPAddress ip = System.Net.IPAddress.Parse(txtHost.Text);

                // Connect to server
                client.Connect(txtHost.Text, Convert.ToInt32(txtPort.Text));

                // Send login message to server
                client.WriteLine("LOGIN:" + username);

                // Display connection status
                txtStatus.AppendText("Connected to Server!" + Environment.NewLine);
                txtStatus.AppendText("Please Wait 3 Seconds For The Chat To Clear" + Environment.NewLine);

                // Show the status messages for 3 seconds
                ShowStatusMessageForSeconds("Connected to Server!", 3);
                ShowStatusMessageForSeconds("Please Wait 3 Seconds For The Chat To Clear", 3);
            }
            catch (Exception ex)
            {
                // Display error message
                txtStatus.AppendText("Error: " + ex.Message + Environment.NewLine);

                // Enable Connect button
                btnConnect.Enabled = true;
            }
        }

        // Event handler for Disconnect button click
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (client.TcpClient != null)
            {
                // Send logout message to server
                client.WriteLine("LOGOUT:" + username);

                // Disconnect from server
                client.Disconnect();

                // Enable Connect button
                btnConnect.Enabled = true;

                // Display disconnection status
                // Show the status messages for 3 seconds
                ShowStatusMessageForSeconds("Disconnected from Server.", 3);
                ShowStatusMessageForSeconds("Please Wait 3 Seconds For The Chat To Clear", 3);
            }
        }

        // Method to show status message for specified seconds
        private void ShowStatusMessageForSeconds(string message, int seconds)
        {
            // Display status message
            txtStatus.AppendText(message + Environment.NewLine);

            // Set interval for status timer
            statusTimer.Interval = seconds * 1000; // Convert seconds to milliseconds

            // Start the timer
            statusTimer.Start();
        }

        // Event handler for status timer tick
        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            // Clear status text
            txtStatus.Clear();

            // Stop the timer
            statusTimer.Stop();
        }

        // Event handler for Send button click
        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if message is not empty
                if (!string.IsNullOrEmpty(txtMessage.Text))
                {
                    // Append message to status textbox
                    txtStatus.AppendText($"{username}: {txtMessage.Text}{Environment.NewLine}");

                    // Send message to server
                    client.WriteLine("MESSAGE:" + txtMessage.Text);

                    // Clear message textbox
                    txtMessage.Clear();
                }
            }
            catch (Exception ex)
            {
                // Display error message
                txtStatus.AppendText("Error: " + ex.Message + Environment.NewLine);
            }
        }

        // Event handler for Send File button click
        private void btnSendFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                byte[] fileData = System.IO.File.ReadAllBytes(filePath);
                string base64File = Convert.ToBase64String(fileData);
                client.WriteLine("FILE:" + base64File);
                txtStatus.AppendText("File sent: " + filePath + Environment.NewLine);
            }
        }

        // Event handler for Form closing
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (client.TcpClient != null && client.TcpClient.Connected)
            {
                // Send logout message to server
                client.WriteLine("LOGOUT:" + username);

                // Disconnect from server
                client.Disconnect();
            }
            base.OnFormClosing(e);
        }

        // Event handler for Save Log button click
        private void btnSaveLog_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Save status text to a file
                System.IO.File.WriteAllText(saveFileDialog.FileName, txtStatus.Text);
            }
        }

        // Event handler for window close button click
        private void windowClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Event handler for window minimize button click
        private void windowMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        // Event handler for mouse down event on the form
        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            // Set dragging flag to true
            dragging = true;

            // Save cursor and form positions
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        // Event handler for mouse move event on the form
        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            // Check if dragging flag is true
            if (dragging)
            {
                // Calculate the difference between current cursor position and initial cursor position
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));

                // Update form position
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        // Event handler for mouse up event on the form
        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            // Set dragging flag to false
            dragging = false;
        }

        // Method to set rounded corners for a control
        private void SetRoundedCorners(Control control, int radius)
        {
            // Create a GraphicsPath for rounded corners
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(control.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(control.Width - radius, control.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, control.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();

            // Set control's region to the rounded path
            control.Region = new Region(path);
        }

        // Method to logout user
        private void Logout()
        {
            if (client.TcpClient != null && client.TcpClient.Connected)
            {
                // Send logout message to server
                client.WriteLine("LOGOUT:" + username);

                // Disconnect from server
                client.Disconnect();
            }
            username = null;

            // Clear the chat window
            txtStatus.Clear();

            // Clear the message textbox
            txtMessage.Clear();

            // Enable Connect button
            btnConnect.Enabled = true;
        }

        // Event handler for Log Out button click
        private void btnLogOut_Click(object sender, EventArgs e)
        {
            Logout(); // Logout the user

            this.Hide(); // Hide the main form

            ShowLoginForm(); // Show the login form
        }
    }
}