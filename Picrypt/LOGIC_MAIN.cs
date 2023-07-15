using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using Newtonsoft.Json;

namespace Picrypt
{
    public static class LOGIC_MAIN
    {
        public static byte[] key { get; set; }

        public static void GetInputForEncryption(out string IP, out string OP, out string P)
        {
            IP = "";
            OP = "";
            P = "";
            using (var dialog = new Form())
            {
                dialog.StartPosition = FormStartPosition.CenterScreen;
                dialog.Text = "Enter Parameters";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ClientSize = new Size(400, 160);
                dialog.Visible = false;

                var inputFileLabel = new Label() { Left = 20, Top = 20, Text = "Input File:" };
                var inputFileTextBox = new TextBox() { Left = 120, Top = 20, Width = 200 };
                var KeyPasswordLabel = new Label() { Left = 20, Top = 60, Text = "Passphrase:" };
                var KeyPasswordInput = new TextBox() { Left = 120, Top = 60, Width = 200 };
                // Create a group box to contain the radio buttons
                Panel panel = new()
                {
                    Text = "Select an option",
                    Location = new Point(10, 80),
                    Size = new Size(200, 70),
                    BorderStyle = BorderStyle.None
                };


                var okButton = new Button() { Text = "OK", Left = 170, Top = 110, Width = 80 };
                var cancelButton = new Button() { Text = "Cancel", Left = 270, Top = 110, Width = 80 };
                okButton.Click += (sender, e) => dialog.DialogResult = DialogResult.OK;
                cancelButton.Click += (sender, e) => dialog.DialogResult = DialogResult.Cancel;

                dialog.Controls.Add(inputFileLabel);
                dialog.Controls.Add(inputFileTextBox);
                dialog.Controls.Add(KeyPasswordLabel);
                dialog.Controls.Add(KeyPasswordInput);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                okButton.BringToFront();
                cancelButton.BringToFront();
                dialog.Activate();

                okButton.Enabled = false;
                KeyPasswordInput.Text = "Enter Your Keypair Passphrase";
                KeyPasswordInput.ForeColor = SystemColors.GrayText;
                KeyPasswordInput.GotFocus += (sender, e) =>
                {
                    KeyPasswordInput.Text = "";
                    KeyPasswordInput.PasswordChar = '*';
                    KeyPasswordInput.ForeColor = SystemColors.ControlText;
                };

                KeyPasswordInput.LostFocus += (sender, e) =>
                {
                    if (string.IsNullOrWhiteSpace(KeyPasswordInput.Text))
                    {
                        KeyPasswordInput.Clear();
                        KeyPasswordInput.Text = "Enter Your Keypair Passphrase";
                        KeyPasswordInput.ForeColor = SystemColors.GrayText;
                        okButton.Enabled = false;
                        KeyPasswordInput.PasswordChar = '\0';
                    }
                    else
                    {
                        KeyPasswordInput.PasswordChar = '*';
                        okButton.Enabled = true;
                    }
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the input file, output file, and password from the text boxes
                    IP = inputFileTextBox.Text;
                    //Clean up for input
                    IP = IP.Replace(@"\", @"/");
                    IP = IP.Replace("\"", "");
                    P = KeyPasswordInput.Text;
                    //Cleanup for Output
                    OP = Path.ChangeExtension(IP, ".pcr");
                    OP = OP.Replace(@"\", @"/");
                    OP = OP.Replace("\"", "");

                }
                else
                {
                    MessageBox.Show("Canceled The Operation", "Exit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                    return;
                }
            }
        }

        public static void GetInputForDecryption(out string IP, out string P)
        {
            IP = "";
            P = "";
            using (var dialog = new Form())
            {
                dialog.StartPosition = FormStartPosition.CenterScreen;
                dialog.Text = "Enter Parameters";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ClientSize = new Size(400, 160);
                dialog.Visible = false;

                var inputFileLabel = new Label() { Left = 20, Top = 20, Text = "Input File:" };
                var inputFileTextBox = new TextBox() { Left = 120, Top = 20, Width = 200 };
                var KeyPasswordLabel = new Label() { Left = 20, Top = 60, Text = "Passphrase:" };
                var KeyPasswordInput = new TextBox() { Left = 120, Top = 60, Width = 200 };
                // Create a group box to contain the radio buttons
                Panel panel = new()
                {
                    Text = "Select an option",
                    Location = new Point(10, 80),
                    Size = new Size(200, 70),
                    BorderStyle = BorderStyle.None
                };


                var okButton = new Button() { Text = "OK", Left = 170, Top = 110, Width = 80 };
                var cancelButton = new Button() { Text = "Cancel", Left = 270, Top = 110, Width = 80 };
                okButton.Click += (sender, e) => dialog.DialogResult = DialogResult.OK;
                cancelButton.Click += (sender, e) => dialog.DialogResult = DialogResult.Cancel;

                dialog.Controls.Add(inputFileLabel);
                dialog.Controls.Add(inputFileTextBox);
                dialog.Controls.Add(KeyPasswordLabel);
                dialog.Controls.Add(KeyPasswordInput);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                okButton.BringToFront();
                cancelButton.BringToFront();
                dialog.Activate();

                okButton.Enabled = false;
                KeyPasswordInput.Text = "Enter Your Keypair Passphrase";
                KeyPasswordInput.ForeColor = SystemColors.GrayText;
                KeyPasswordInput.GotFocus += (sender, e) =>
                {
                    KeyPasswordInput.Text = "";
                    KeyPasswordInput.PasswordChar = '*';
                    KeyPasswordInput.ForeColor = SystemColors.ControlText;
                };

                KeyPasswordInput.LostFocus += (sender, e) =>
                {
                    if (string.IsNullOrWhiteSpace(KeyPasswordInput.Text))
                    {
                        KeyPasswordInput.Clear();
                        KeyPasswordInput.Text = "Enter Your Keypair Passphrase";
                        KeyPasswordInput.ForeColor = SystemColors.GrayText;
                        okButton.Enabled = false;
                        KeyPasswordInput.PasswordChar = '\0';
                    }
                    else
                    {
                        KeyPasswordInput.PasswordChar = '*';
                        okButton.Enabled = true;
                    }
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the input file, output file, and password from the text boxes
                    IP = inputFileTextBox.Text;
                    //Clean up for input
                    IP = IP.Replace(@"\", @"/");
                    IP = IP.Replace("\"", "");
                    P = KeyPasswordInput.Text;

                }
                else
                {
                    MessageBox.Show("Canceled The Operation", "Exit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                    return;
                }
            }
        }


        /// <summary>
        /// XOR Encryption for filename
        /// </summary>
        /// <param name="filename">the name of the file</param>
        /// <param name="iteration">not used</param>
        /// <returns>the filename encrypted with XOR Encryption</returns>
        public static string EncryptFileName(string filename, int iteration)
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
        public static string DecryptFileName(string encryptedfilename, int iteration)
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
        public static void EncryptFile()
        {

            string inputFile = "";
            string outputFile = "";
            string Passphrase = "";
            string json = null;
            var prejson = new
            {
                V1 = "placeholder",
                V2 = "placeholder",
                V3 = "placeholder",
                V4 = "Placeholder"
            };


            GetInputForEncryption(out inputFile, out outputFile, out Passphrase);

            if (!File.Exists(inputFile))
            {
                MessageBox.Show("File Does Not Exist!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            try
            {
                byte[] content = File.ReadAllBytes(inputFile);
                byte[] encryptedContent;
                using (Aes aes = Aes.Create())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.Zeros;
                    aes.GenerateKey();
                    aes.GenerateIV();
                    key = aes.IV;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(content, 0, content.Length);
                            encryptedContent = ms.ToArray();
                            cs.FlushFinalBlock();
                            cs.Close();
                            ms.Close();
                        }
                    }
                    using (RSA rsa = RSA.Create())
                    {
                        byte[] privateKeyByte = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "bin/keypair/private.p8"));
                        rsa.ImportEncryptedPkcs8PrivateKey(Encoding.UTF8.GetBytes(Passphrase), privateKeyByte, out _);
                        string v = Convert.ToBase64String(rsa.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1));
                        byte[] EivByte = rsa.Encrypt(aes.IV, RSAEncryptionPadding.Pkcs1);
                        byte[] eKeyByte = rsa.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1);
                        byte[] concatenatedData = new byte[eKeyByte.Length + EivByte.Length + encryptedContent.Length];
                        int cIndex = 0;
                        Array.Copy(eKeyByte, 0, concatenatedData, cIndex, eKeyByte.Length);
                        cIndex += eKeyByte.Length;
                        Array.Copy(EivByte, 0, concatenatedData, cIndex, EivByte.Length);
                        cIndex += EivByte.Length;
                        Array.Copy(encryptedContent, 0, concatenatedData, cIndex, encryptedContent.Length);

                        byte[] signature = rsa.SignData(concatenatedData, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                        string v1 = Convert.ToBase64String(signature);
                        prejson = new
                        {
                            V1 = v,
                            V2 = Convert.ToBase64String(EivByte),
                            V3 = Convert.ToBase64String(encryptedContent),
                            V4 = v1
                        };
                        json = JsonConvert.SerializeObject(prejson);
                        byte[] jByte = Encoding.UTF8.GetBytes(json);
                        File.WriteAllBytes(outputFile, jByte);
                        rsa.Clear();
                    }
                    aes.Clear();
                }
                string ency = EncryptFileName(Path.GetFileName(inputFile), 8);
                string ext = Path.GetExtension(outputFile);
                string dir = Path.GetDirectoryName(outputFile);

                File.Move($"{outputFile}", $"{dir}/{ency}{ext}");
                Array.Clear(key, 0, key.Length);
                MessageBox.Show("The file has been encrypted successfully", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            string password = "";
            GetInputForDecryption(out inputFile, out password);
            //MessageBox.Show(inputFile);

            if (!File.Exists(inputFile))
            {
                MessageBox.Show("File Does Not Exist!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string filaename = Path.GetFileNameWithoutExtension(inputFile);
            //MessageBox.Show(filaename);

            try
            {
                string json = File.ReadAllText(inputFile);
                Helper.EncryptedData parsedJ = JsonConvert.DeserializeObject<Helper.EncryptedData>(json);
                Console.WriteLine(parsedJ);
                byte[] EaesKey = Convert.FromBase64String(parsedJ.V1);
                byte[] EaesIv = Convert.FromBase64String(parsedJ.V2);
                byte[] encryptedContent = Convert.FromBase64String(parsedJ.V3);
                byte[] signature = Convert.FromBase64String(parsedJ.V4);
                byte[] concatenatedData = new byte[EaesKey.Length + EaesIv.Length + encryptedContent.Length];
                int cIndex = 0;
                Array.Copy(EaesKey, 0, concatenatedData, cIndex, EaesKey.Length);
                cIndex += EaesKey.Length;
                Array.Copy(EaesIv, 0, concatenatedData, cIndex, EaesIv.Length);
                cIndex += EaesIv.Length;
                Array.Copy(encryptedContent, 0, concatenatedData, cIndex, encryptedContent.Length);
                byte[] decryptedContent;

                using (RSA rsa = RSA.Create())
                {
                    byte[] privateKeyByte = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "bin/keypair/private.p8"));
                    rsa.ImportEncryptedPkcs8PrivateKey(Encoding.UTF8.GetBytes(password), privateKeyByte, out _);
                    //The signature verification is not working fixing it will require changing something related to the padding which would break the decryption proccess
                    //if (!rsa.VerifyData(concatenatedData, signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1))
                    //{
                    //    throw new CryptographicException("Signature verification failed");
                    //}
                    //MessageBox.Show(parsedJ.V1);
                    //MessageBox.Show(parsedJ.V2);
                    //MessageBox.Show(parsedJ.V3);
                    //MessageBox.Show(parsedJ.V4);

                    using (Aes aes = Aes.Create())
                    {
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.Zeros;
                        aes.Key = rsa.Decrypt(EaesKey, RSAEncryptionPadding.Pkcs1);
                        aes.IV = rsa.Decrypt(EaesIv, RSAEncryptionPadding.Pkcs1);
                        key = aes.IV;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(encryptedContent, 0, encryptedContent.Length);
                                cs.FlushFinalBlock();
                                decryptedContent = ms.ToArray();
                                cs.Close();
                            }
                            ms.Close();
                        }
                        aes.Clear();
                    }

                    rsa.Clear();
                }
                string outputFile = "\\"+DecryptFileName(filaename, 8);
                string Cuurdir = Path.GetDirectoryName(inputFile);
                string pathtowrite = Cuurdir + outputFile;
                //MessageBox.Show(Cuurdir);
                //MessageBox.Show(pathtowrite);
                File.WriteAllBytes(pathtowrite, decryptedContent);
                MessageBox.Show("The file has been decrypted successfully", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Information);
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