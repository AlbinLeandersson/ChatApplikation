using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatApplication
{
    public partial class LoginForm : Form
    {
        // Property to get the entered username
        public string Username { get; private set; }
        // Flag to indicate dragging of the form
        private bool dragging = false;
        // Point to store cursor position during dragging
        private Point dragCursorPoint;
        // Point to store form position during dragging
        private Point dragFormPoint;

        // Constructor for LoginForm
        public LoginForm()
        {
            InitializeComponent();
        }

        // Event handler for Login button click
        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Check if username textbox is empty
            if (string.IsNullOrEmpty(txtUsername.Text.Trim()))
            {
                MessageBox.Show("Please enter a username.");
                return;
            }

            // Set entered username
            Username = txtUsername.Text.Trim();
            // Set DialogResult to OK
            DialogResult = DialogResult.OK;
            // Close Login Form
            this.Close();
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
    }
}
