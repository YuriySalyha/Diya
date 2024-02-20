﻿using Diia.CustomComponents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Shapes;

namespace Diia
{
    public partial class LoginForm : Form
    {
        bool registerMode = false;

        string path = System.IO.Path.Combine(Environment.CurrentDirectory, "Logins.txt");

        string configPath = System.IO.Path.Combine(Environment.CurrentDirectory, "config.json");
        private int borderSize = 2;
        Size formSize;
        public LoginForm()
        {
            InitializeComponent();
            this.Padding = new Padding(borderSize); // Border size
            this.BackColor = Color.Black; // Border color            
        }

        [DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();


        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);


        protected override void WndProc(ref Message m)
        {
            const int WM_NCCALCSIZE = 0x0083;
            const int WM_NCHITTEST = 0x0084;
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MINIMIZE = 0xF020; //Minimize form (Before)
            const int SC_RESTORE = 0xF120; //Restore form (Before)
            const int resizeAreaSize = 10;

            // Represents client area of the window
            const int HTCLIENT = 1;
            // Represents every border
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;

            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                if (this.WindowState == FormWindowState.Normal)//Resize the form if it is in normal state
                {
                    if ((int)m.Result == HTCLIENT)//If the result of the mouse pointer is in the client area of the window
                    {
                        Point screenPoint = new Point(m.LParam.ToInt32()); // Gets screen point coordinates                       
                        Point clientPoint = this.PointToClient(screenPoint);
                        if (clientPoint.Y <= resizeAreaSize)
                        {
                            if (clientPoint.X <= resizeAreaSize)
                                m.Result = (IntPtr)HTTOPLEFT;
                            else if (clientPoint.X < (this.Size.Width - resizeAreaSize))
                                m.Result = (IntPtr)HTTOP;
                            else
                                m.Result = (IntPtr)HTTOPRIGHT;
                        }
                        else if (clientPoint.Y <= (this.Size.Height - resizeAreaSize)) //If the pointer is inside the form at the Y coordinate
                        {
                            if (clientPoint.X <= resizeAreaSize)
                                m.Result = (IntPtr)HTLEFT;
                            else if (clientPoint.X > (this.Width - resizeAreaSize))
                                m.Result = (IntPtr)HTRIGHT;
                        }
                        else
                        {
                            if (clientPoint.X <= resizeAreaSize)
                                m.Result = (IntPtr)HTBOTTOMLEFT;
                            else if (clientPoint.X < (this.Size.Width - resizeAreaSize))
                                m.Result = (IntPtr)HTBOTTOM;
                            else
                                m.Result = (IntPtr)HTBOTTOMRIGHT;
                        }
                    }
                }
                return;
            }
            if (m.Msg == WM_NCCALCSIZE && m.WParam.ToInt32() == 1)
            {
                return;
            }
            if (m.Msg == WM_SYSCOMMAND)
            {

                int wParam = (m.WParam.ToInt32() & 0xFFF0);
                if (wParam == SC_MINIMIZE)  //Before
                    formSize = this.ClientSize;
                if (wParam == SC_RESTORE)// Restored form(Before)
                    this.Size = formSize;
            }
            base.WndProc(ref m);
        }

        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void iconButton1_Click(object sender, EventArgs e)
        {
            formSize = this.ClientSize;
            this.WindowState = FormWindowState.Minimized;
        }

        private void LoginForm_ResizeEnd(object sender, EventArgs e)
        {

        }

        private void LoginForm_Resize(object sender, EventArgs e)
        {
            AdjustForm();
        }

        private void AdjustForm()
        {
            switch (this.WindowState)
            {
                case FormWindowState.Maximized:
                    this.Padding = new Padding(0, 8, 8, 0);
                    break;
                case FormWindowState.Normal:
                    if (this.Padding.Top != borderSize)
                        this.Padding = new Padding(borderSize);
                    break;
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            formSize = this.ClientSize;
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            string login = UsernameBox.Text;
            string password = PasswordBox.Text;
            //
            string id = IdBox.Text;
            //
            if (registerMode)
            {
                if (checkRegister())
                {
                    registerMode = false;

                    List<string> input = File.ReadLines(path).ToList();
                    input.Add($"{login};{password};{id}");
                    File.WriteAllLines(path,input);
                    
                    bool loginStatus = checkCredentials(login, password);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                bool loginStatus = checkCredentials(login, password);

                if (loginStatus)
                {
                    CredentialError.Visible = false;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    CredentialError.Visible = true;
                }
            }
        }


        #region RegCheck
        private bool checkRegister()
        {
            bool username = true;
            bool password = true;
            bool id = true;


            if(!checkUsername(UsernameBox.Text))
            {
                UsernameError.Text = "Username is already taken";
                UsernameError.Visible = true;
                username = false;
            }
            else if(UsernameBox.Text.Length < 8) 
            {
                UsernameError.Text = "Username length should be at least 8 characters long";
                UsernameError.Visible = true;
                username = false;
            }
            else 
            {
                UsernameError.Visible = false;
            }

            if(PasswordBox.Text.Length < 8 || !checkPassword(PasswordBox.Text)) 
            {
                PasswordError.Visible = true;
                password = false;
            }
            else 
            {
                PasswordError.Visible = false;
            }

            if (IdBox.Text.Length < 1) 
            {
                CitizenError.Text = "Please enter an citizen ID";
                CitizenError.Visible = true;
                id = false;
            }
            else if (!checkID(IdBox.Text)) 
            {
                CitizenError.Text = "This citizen ID is already registered";
                CitizenError.Visible = true;
                id = false;
            }
            else 
            {
                CitizenError.Visible = false;
            }
            return (username && password && id);
            
        }

        private bool checkUsername(string name)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
            var lines = File.ReadLines(path);
            foreach (var line in lines)
            {
                if (line.Split(';').ToList()[0] == name)
                {
                    return false;
                }
            }
            return true;
        }

        private bool checkPassword(string password)
        {
            List<char> specialSymbols = new List<char> { '(',')','{','}','[',']','|','`','¬','¦','!','"','£','$','%',
                                                         '^','&','*','\'','<','>',':',';','#','~','_','-','+','=',',',
                                                         '@' };
            foreach (char specialSymbol in specialSymbols)
            {
                if (password.Contains(specialSymbol))
                {
                    return false;
                }
            }
            return true;
        }

        private bool checkID(string id) 
        {
            var lines = File.ReadLines(path);
            foreach (var line in lines)
            {
                if (line.Split(';').ToList()[2] == id) 
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region LogCheck
        private bool checkCredentials(string usr, string pwd)
        {

            var lines = File.ReadLines(path);
            foreach (var line in lines)
            {
                if (line.Split(';').ToList()[0] == usr && line.Split(';').ToList()[1] == pwd)
                {
                    Config config = new Config(true, line.Split(';').ToList()[0], DateTime.Now);
                    string output = JsonConvert.SerializeObject(config);
                    File.WriteAllText(configPath, output);
                    return true;
                }
            }
            return false;
        }
        #endregion
        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {
            if (registerMode) { registerMode = false; }
            else { registerMode = true; }
            updateMode();
        }

        private void updateMode()
        {
            if (registerMode)
            {
                ConfirmButton.Text = "Sign Up";
                UnderText.Text = "Already have an account? Sign in";
                IdBox.Visible = true;
                CitizenError.Visible = false;
                PasswordError.Visible = false;
                UsernameError.Visible = false;
                CredentialError.Visible = false;
            }
            else
            {
                ConfirmButton.Text = "Sign in";
                UnderText.Text = "Don't have an account? Sign Up";
                IdBox.Visible = false;
                CitizenError.Visible = false;
                PasswordError.Visible = false;
                UsernameError.Visible = false;
                CredentialError.Visible = false;
            }
        }
    }
}
