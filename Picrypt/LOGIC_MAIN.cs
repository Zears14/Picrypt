using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Picrypt
{
    public class LOGIC_MAIN
    {
        public byte[] key { get; set; }




        /// <summary>
        /// XOR Encryption for filename
        /// </summary>
        /// <param name="filename">the name of the file</param>
        /// <param name="iteration">not used</param>
        /// <returns>the filename encrypted with XOR Encryption</returns>
        public string EncryptFileName(string filename, int iteration)
        {
            byte[] filenameBytes = Encoding.UTF8.GetBytes(filename);
            byte[] encryptedBytes = new byte[filenameBytes.Length];

            for (int u = 0; u < filenameBytes.Length; u++)
            {
                encryptedBytes[u] = (byte)(filenameBytes[u] ^ key[u % key.Length]);
            }

            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// Decrypt the XOR Encrypted File name
        /// </summary>
        /// <param name="encryptedfilename">the Encrypted file name</param>
        /// <param name="iteration">not used</param>
        /// <returns>the decrypted filename</returns>
        public string DecryptFileName(string encryptedfilename, int iteration)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedfilename);
            byte[] decryptedBytes = new byte[encryptedBytes.Length];

            for (int u = 0; u < encryptedBytes.Length; u++)
            {
                decryptedBytes[u] = (byte)(encryptedBytes[u] ^ key[u % key.Length]);
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        /// <summary>
        /// encrypt file
        /// </summary>
        public void EncryptFile()
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
                dialog.Size = new Size(400, 160);
                dialog.Visible = true;

                var inputFileLabel = new Label() { Left = 20, Top = 20, Text = "Input File:" };
                var inputFileTextBox = new TextBox() { Left = 120, Top = 20, Width = 200 };
                var passwordLabel = new Label() { Left = 20, Top = 50, Text = "Encryption Key:" };
                var passwordTextBox = new TextBox() { Left = 120, Top = 50, Width = 200, PasswordChar = '*' };
                // Create a group box to contain the radio buttons
                Panel panel = new()
                {
                    Text = "Select an option",
                    Location = new Point(10, 80),
                    Size = new Size(200, 70),
                    BorderStyle = BorderStyle.None
                };

                RadioButton imageRadioButton = new()
                {
                    Text = "Image",
                    Location = new Point(10, 20),
                    Checked = true
                };

                // Create the radio buttons and add them to the group box
                RadioButton textRadioButton = new()
                {
                    Text = "Text",
                    Location = new Point(100, 20)
                };
                panel.Controls.Add(textRadioButton);

                var okButton = new Button() { Text = "OK", Left = 170, Top = 110, Width = 80 };
                var cancelButton = new Button() { Text = "Cancel", Left = 270, Top = 110, Width = 80 };
                okButton.Click += (sender, e) => dialog.DialogResult = DialogResult.OK;
                cancelButton.Click += (sender, e) => dialog.DialogResult = DialogResult.Cancel;

                dialog.Controls.Add(inputFileLabel);
                dialog.Controls.Add(inputFileTextBox);
                dialog.Controls.Add(passwordLabel);
                dialog.Controls.Add(passwordTextBox);
                dialog.Controls.Add(panel);
                panel.Controls.Add(imageRadioButton);
                panel.SendToBack();
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                okButton.BringToFront();
                cancelButton.BringToFront();
                dialog.Activate();


                if (dialog.ShowDialog() == DialogResult.OK)
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
                        outputFile = Path.ChangeExtension(inputFile, ".pcrim");
                        outputFile = outputFile.Replace(@"\", @"/");
                        outputFile = outputFile.Replace("\"", "");
                    }
                    else if (textRadioButton.Checked)
                    {
                        outputFile = Path.ChangeExtension(inputFile, ".pctx");
                        outputFile = outputFile.Replace(@"\", @"/");
                        outputFile = outputFile.Replace("\"", "");
                    }
                }
                else if (dialog.ShowDialog() == DialogResult.Cancel)
                {
                    MessageBox.Show("Canceled The Operation", "Exit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                }
            }

            if (!File.Exists(inputFile))
            {
                MessageBox.Show("File Does Not Exist!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            try
            {
                string ency = EncryptFileName(Path.GetFileNameWithoutExtension(inputFile), 8);
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

        public void DecryptFile()
        {
            string inputFile = "";
            string outputFile = "";
            string password = "";

            if (!File.Exists(inputFile))
            {
                MessageBox.Show("File Does Not Exist!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            try
            {

                string dency = DecryptFileName(Path.GetFileNameWithoutExtension(inputFile), 8);
                string ext = Path.GetExtension(outputFile);
                string dir = Path.GetDirectoryName(outputFile);

                File.Move($"{outputFile}", $"{dir}/{dency}{ext}");
                Array.Clear(key, 0, key.Length);
            }
            catch (CryptographicException ex)
            {
                MessageBox.Show($"ERROR: {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ERROR: \n {ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}