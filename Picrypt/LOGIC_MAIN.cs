using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Picrypt
{
    public class LOGIC_MAIN
    {
        private static byte[] key;

        //XOR Encryption
        public static string EncryptFileName(string filename)
        {
            byte[] filenameBytes = Encoding.UTF8.GetBytes(filename);
            byte[] encryptedBytes = new byte[filenameBytes.Length];

            for (int i = 0; i < filenameBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(filenameBytes[i] ^ key[i % key.Length]);
            }
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string DecryptFileName(string encryptedfilename)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedfilename);
            byte[] decryptedBytes = new byte[encryptedBytes.Length];

            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                decryptedBytes[i] = (byte)(encryptedBytes[i] ^ key[i % key.Length]);
            }
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public static void EncryptFile()
        {
            string inputFile = "";
            string outputFile = "";
            string password = "";
            using (var dialog = new Form())
            {
                dialog.StartPosition = FormStartPosition.CenterScreen;
                dialog.Text = "Enter Parameters";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ClientSize = new Size(400, 160);

                var inputFileLabel = new Label() { Left = 20, Top = 20, Text = "Input File:" };
                var inputFileTextBox = new TextBox() { Left = 120, Top = 20, Width = 200 };
                var passwordLabel = new Label() { Left = 20, Top = 50, Text = "Password:" };
                var passwordTextBox = new TextBox() { Left = 120, Top = 50, Width = 200, PasswordChar = '*' };
                // Create a group box to contain the radio buttons
                Panel groupBox = new Panel();
                groupBox.Text = "Select an option";
                groupBox.Location = new Point(10, 80);
                groupBox.Size = new Size(200, 70);
                groupBox.BorderStyle = BorderStyle.None;

                RadioButton imageRadioButton = new RadioButton();
                imageRadioButton.Text = "Image";
                imageRadioButton.Location = new Point(10, 20);
                imageRadioButton.Checked = true;

                // Create the radio buttons and add them to the group box
                RadioButton textRadioButton = new RadioButton();
                textRadioButton.Text = "Text";
                textRadioButton.Location = new Point(100, 20);
                groupBox.Controls.Add(textRadioButton);

                var okButton = new Button() { Text = "OK", Left = 170, Top = 110, Width = 80 };
                var cancelButton = new Button() { Text = "Cancel", Left = 270, Top = 110, Width = 80 };
                okButton.Click += (sender, e) => dialog.DialogResult = DialogResult.OK;
                cancelButton.Click += (sender, e) => dialog.DialogResult = DialogResult.Cancel;

                dialog.Controls.Add(inputFileLabel);
                dialog.Controls.Add(inputFileTextBox);
                dialog.Controls.Add(passwordLabel);
                dialog.Controls.Add(passwordTextBox);
                dialog.Controls.Add(groupBox);
                groupBox.Controls.Add(imageRadioButton);
                groupBox.SendToBack();
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                okButton.BringToFront();
                cancelButton.BringToFront();

                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // Get the input file, output file, and password from the text boxes
                    inputFile = inputFileTextBox.Text;
                    password = passwordTextBox.Text;
                    key = Encoding.UTF8.GetBytes(password);
                    //Clean up for input
                    inputFile = inputFile.Replace(@"\", @"/");
                    inputFile = inputFile.Replace("\"", "");
                    //Cleanup for Output

                    //Comment 2
                    if (imageRadioButton.Checked)
                    {
                        outputFile = Path.ChangeExtension(inputFile, ".pcrim");
                        outputFile = outputFile.Replace(@"\", @"/");
                        outputFile = outputFile.Replace("\"", "");
                    }
                    else if (textRadioButton.Checked)
                    {
                        outputFile = Path.ChangeExtension(inputFile, ".pcrtx");
                        outputFile = outputFile.Replace(@"\", @"/");
                        outputFile = outputFile.Replace("\"", "");
                    }
                    //
                    // Use the input file, output file, and password
                    // ...
                }
                else if (result == DialogResult.Cancel)
                {
                    MessageBox.Show("Canceled The Operation", "Exit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                }
            }

            bool defaultfpassword = Properties.Settings.Default.useDefaulPwd;
            if (defaultfpassword)
            {
                password = "ThisIsNotSecure";
            }
            if (!File.Exists(inputFile))
            {
                MessageBox.Show("File Does Not Exist!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            try
            {
                using (FileStream fsInput = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                using (FileStream fsOutput = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    byte[] salt = new byte[16];
                    new RNGCryptoServiceProvider().GetBytes(salt);

                    var key = new Rfc2898DeriveBytes(password, salt, 10000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);

                    fsOutput.Write(salt, 0, salt.Length);

                    using (CryptoStream cs = new CryptoStream(fsOutput, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] buffer = new byte[8192];
                        int read;
                        while ((read = fsInput.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cs.Write(buffer, 0, read);
                        }
                    }
                }
                string ency = EncryptFileName(Path.GetFileNameWithoutExtension(inputFile));
                string ext = Path.GetExtension(outputFile);
                string dir = Path.GetDirectoryName(outputFile);

                File.Move($"{outputFile}", $"{dir}/{ency}{ext}");
                Array.Clear(key, 0, key.Length);
            }
            catch (CryptographicException ex)
            {
                MessageBox.Show($"ERROR: \n {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ERROR: \n {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void DecryptFile()
        {
            string inputFile = "";
            string outputFile = "";
            string password = "";
            using (var dialog = new Form())
            {
                dialog.StartPosition = FormStartPosition.CenterScreen;
                dialog.Text = "Enter Parameters";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ClientSize = new Size(400, 160);

                var inputFileLabel = new Label() { Left = 20, Top = 20, Text = "Input File:" };
                var inputFileTextBox = new TextBox() { Left = 120, Top = 20, Width = 200 };
                var passwordLabel = new Label() { Left = 20, Top = 50, Text = "Password:" };
                var passwordTextBox = new TextBox() { Left = 120, Top = 50, Width = 200, PasswordChar = '*' };
                // Create a group box to contain the radio buttons
                Panel groupBox = new Panel();
                groupBox.Text = "Select an option";
                groupBox.Location = new Point(10, 80);
                groupBox.Size = new Size(200, 70);
                groupBox.BorderStyle = BorderStyle.None;

                RadioButton imageRadioButton = new RadioButton();
                imageRadioButton.Text = "Image";
                imageRadioButton.Location = new Point(10, 20);
                imageRadioButton.Checked = true;

                // Create the radio buttons and add them to the group box
                RadioButton textRadioButton = new RadioButton();
                textRadioButton.Text = "Text";
                textRadioButton.Location = new Point(100, 20);
                groupBox.Controls.Add(textRadioButton);

                var okButton = new Button() { Text = "OK", Left = 170, Top = 110, Width = 80 };
                var cancelButton = new Button() { Text = "Cancel", Left = 270, Top = 110, Width = 80 };
                okButton.Click += (sender, e) => dialog.DialogResult = DialogResult.OK;
                cancelButton.Click += (sender, e) => dialog.DialogResult = DialogResult.Cancel;

                dialog.Controls.Add(inputFileLabel);
                dialog.Controls.Add(inputFileTextBox);
                dialog.Controls.Add(passwordLabel);
                dialog.Controls.Add(passwordTextBox);
                dialog.Controls.Add(groupBox);
                groupBox.Controls.Add(imageRadioButton);
                groupBox.SendToBack();
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                okButton.BringToFront();
                cancelButton.BringToFront();

                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // Get the input file, output file, and password from the text boxes
                    inputFile = inputFileTextBox.Text;
                    password = passwordTextBox.Text;
                    key = Encoding.UTF8.GetBytes(password);
                    //Clean up for input
                    inputFile = inputFile.Replace(@"\", @"/");
                    inputFile = inputFile.Replace("\"", "");
                    //Cleanup for Output
                    if (imageRadioButton.Checked)
                    {
                        outputFile = Path.ChangeExtension(inputFile, ".png");
                        outputFile = outputFile.Replace(@"\", @"/");
                        outputFile = outputFile.Replace("\"", "");
                    }
                    else if (textRadioButton.Checked)
                    {
                        outputFile = Path.ChangeExtension(inputFile, ".txt");
                        outputFile = outputFile.Replace(@"\", @"/");
                        outputFile = outputFile.Replace("\"", "");
                    }
                    //
                    // Use the input file, output file, and password
                    // ...
                }
                else if (result == DialogResult.Cancel)
                {
                    MessageBox.Show("Canceled The Operation", "Exit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                }
            }
            bool defaultfpassword = Properties.Settings.Default.useDefaulPwd;
            if (defaultfpassword)
            {
                password = "ThisIsNotSecure";
            }

            if (!File.Exists(inputFile))
            {
                MessageBox.Show("File Does Not Exist!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            try
            {
                using (FileStream fsInput = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                using (FileStream fsOutput = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    byte[] salt = new byte[16];
                    fsInput.Read(salt, 0, salt.Length);

                    var key = new Rfc2898DeriveBytes(password, salt, 10000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);

                    using (CryptoStream cs = new CryptoStream(fsOutput, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        byte[] buffer = new byte[8192];
                        int read;
                        while ((read = fsInput.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cs.Write(buffer, 0, read);
                        }
                    }
                }
                string dency = DecryptFileName(Path.GetFileNameWithoutExtension(inputFile));
                string ext = Path.GetExtension(outputFile);
                string dir = Path.GetDirectoryName(outputFile);

                File.Move($"{outputFile}", $"{dir}/{dency}{ext}");
                Array.Clear(key, 0, key.Length);
            }
            catch (CryptographicException ex)
            {
                MessageBox.Show($"Decryption failed, try using another password \n ERROR: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ERROR: \n {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}