using System;
using System.Windows.Forms;

namespace ChatApplication
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            LoginForm loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new Form1(loginForm.Username)); // Pass the username to Form1
            }
            else
            {
                Application.Exit();
            }
        }
    }
}
