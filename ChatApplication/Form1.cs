using SimpleTCP; // Importing SimpleTCP library for TCP communication
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ChatApplication
{
    public partial class Form1 : Form
    {
        SimpleTcpServer server; // TCP server instance
        Dictionary<string, string> clients; // Dictionary to store connected clients
        private bool dragging = false; // Flag to indicate dragging of form
        private Point dragCursorPoint; // Cursor position during dragging
        private Point dragFormPoint; // Form position during dragging

        // Constructor for Form1
        public Form1()
        {
            InitializeComponent();
            clients = new Dictionary<string, string>(); // Initialize the clients dictionary
        }

        // Method called when Form1 is loaded
        private void Form1_Load(object sender, EventArgs e)
        {
            server = new SimpleTcpServer(); // Initialize TCP server
            server.Delimiter = 0x13; // Set message delimiter to 'enter'
            server.StringEncoder = Encoding.UTF8; // Set string encoder to UTF-8
            server.DataReceived += Server_DataReceived; // Subscribe to DataReceived event
        }

        // Event handler for data received from clients
        private void Server_DataReceived(object sender, SimpleTCP.Message e)
        {
            // Invoke the UI thread to update UI components
            txtStatus.Invoke((MethodInvoker)delegate ()
            {
                // Split received message into parts
                string[] parts = e.MessageString.Split(new char[] { ':' }, 2);
                string messageType = parts[0];
                string content = parts.Length > 1 ? parts[1] : string.Empty;

                // Handle message based on message type
                switch (messageType)
                {
                    case "LOGIN":
                        HandleLogin(e, content);
                        break;
                    case "MESSAGE":
                        HandleMessage(e, content);
                        break;
                    case "LOGOUT":
                        HandleLogout(e, content);
                        break;
                    case "FILE":
                        HandleFile(e, content);
                        break;
                }
            });
        }

        // Method to handle client login
        private void HandleLogin(SimpleTCP.Message e, string username)
        {
            // Add client to dictionary with its endpoint as key
            clients[e.TcpClient.Client.RemoteEndPoint.ToString()] = username;
            // Append connection message to status textbox
            txtStatus.AppendText($"{username} connected.{Environment.NewLine}");
            // Send acknowledgment to client
            e.ReplyLine("LOGIN:OK");
        }

        // Method to handle incoming messages from clients
        private void HandleMessage(SimpleTCP.Message e, string message)
        {
            // Get username of the client
            string username = clients[e.TcpClient.Client.RemoteEndPoint.ToString()];
            // Append message to status textbox
            txtStatus.AppendText($"{username}: {message}{Environment.NewLine}");
        }

        // Method to handle client logout
        private void HandleLogout(SimpleTCP.Message e, string username)
        {
            // Remove client from dictionary
            clients.Remove(e.TcpClient.Client.RemoteEndPoint.ToString());
            // Append disconnection message to status textbox
            txtStatus.AppendText($"{username} disconnected.{Environment.NewLine}");
        }

        // Method to handle incoming files from clients
        private void HandleFile(SimpleTCP.Message e, string base64File)
        {
            // Get username of the client
            string username = clients[e.TcpClient.Client.RemoteEndPoint.ToString()];
            // Append file transfer message to status textbox
            txtStatus.AppendText($"{username} sent a file.{Environment.NewLine}");
            // You can add logic to save the file here
        }

        // Event handler for Start button click
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                // Start the server with specified IP address and port
                txtStatus.AppendText("Server Starting..." + Environment.NewLine);
                System.Net.IPAddress ip = System.Net.IPAddress.Parse(txtHost.Text);
                server.Start(ip, Convert.ToInt32(txtPort.Text));
                txtStatus.AppendText("Server Started!" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during server start
                txtStatus.AppendText("Error: " + ex.Message + Environment.NewLine);
            }
        }

        // Event handler for Stop button click
        private void btnStop_Click(object sender, EventArgs e)
        {
            // Check if the server is started
            if (server.IsStarted)
            {
                // Stop the server
                server.Stop();
                txtStatus.AppendText("Server Stopped." + Environment.NewLine);
            }
        }

        // Event handler for window close button click
        private void windowClose_Click(object sender, EventArgs e)
        {
            // Close the form
            this.Close();
        }

        // Event handler for window minimize button click
        private void windowMin_Click(object sender, EventArgs e)
        {
            // Minimize the form
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
    }
}
