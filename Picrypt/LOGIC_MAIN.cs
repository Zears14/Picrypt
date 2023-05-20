using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Picrypt
{
    public class LOGIC_MAIN
    {
        public static byte[] key { get; set; }

        public static void GetInputForEncryption(out string IP, out string OP, out string P)
        {
            string inputFile = "";
            string outputFile = "";
            string password = "";
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
                    IP = inputFile;
                    OP = outputFile;
                    P = password;
                }
                else if (result == DialogResult.Cancel)
                {
                    MessageBox.Show("Canceled The Operation", "Exit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                }
            }
        }

        //XOR Encryption
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

        public void EncryptFile()
        {

            string inputFile;
            string outputFile;
            string password;

            GetInputForEncryption(out inputFile, out outputFile, out password);

            if (!File.Exists(inputFile))
            {
                MessageBox.Show("File Does Not Exist!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            try
            {
                using (FileStream fsInput = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                using (FileStream fsOutput = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                //Encryption Begin
                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    byte[] salt = new byte[16];
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        rng.GetBytes(salt);
                    }

                    var key = new Rfc2898DeriveBytes(password, salt, 10000);
                    byte[] tobeency = File.ReadAllBytes(inputFile);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    byte[] keyenc = aes.Key;
                    byte[] IVenc = aes.IV;
                    fsOutput.Close();

                    using (ICryptoTransform EN_TSN = aes.CreateEncryptor())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (CryptoStream cs = new CryptoStream(ms, EN_TSN, CryptoStreamMode.Write))
                            {
                                cs.Write(tobeency, 0, tobeency.Length);
                                cs.Dispose();
                                cs.Close();
                            }
                            tobeency = ms.ToArray();
                            ms.Dispose();
                            ms.Close();
                        }
                    }

                    byte[] aesEnKey;
                    byte[] aesIVEnc;
                    byte[] pack;

                    using (RSA rsa = RSA.Create())
                    {
                        string bin = Path.Combine(Directory.GetCurrentDirectory(), "bin");
                        string keypair = Path.Combine(bin, "keypair");
                        string pth = Path.Combine(keypair, "public.xml");
                        string puk = null;
                        //using (StreamReader sr = new StreamReader(pth))
                        //{
                        //    puk = sr.ReadToEnd();
                        //    sr.Dispose();
                        //    sr.Close();
                        //}

                        XmlDocument doc = new XmlDocument();
                        using (FileStream stream = new FileStream(pth, FileMode.Open))
                        {
                            doc.Load(stream);
                            puk = doc.OuterXml;
                            stream.Dispose();
                            stream.Close();
                        }
                        XmlElement modulusElement = (XmlElement)doc.SelectSingleNode("//Modulus");
                        byte[] modulusBytes = Convert.FromBase64String(modulusElement.InnerText);

                        XmlElement exponentElement = (XmlElement)doc.SelectSingleNode("//Exponent");
                        byte[] exponentBytes = Convert.FromBase64String(exponentElement.InnerText);

                        rsa.ImportParameters(new RSAParameters { Modulus = modulusBytes, Exponent = exponentBytes });
                        aesEnKey = rsa.Encrypt(keyenc, RSAEncryptionPadding.OaepSHA512);
                        aesIVEnc = rsa.Encrypt(IVenc, RSAEncryptionPadding.OaepSHA512);
                        //Packaging

                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (BinaryWriter bw = new BinaryWriter(ms))
                            {
                                bw.Write(aesEnKey.Length);
                                bw.Write(aesEnKey);
                                bw.Write(aesIVEnc.Length);
                                bw.Write(aesIVEnc);
                                bw.Write(tobeency.Length);
                                bw.Write(tobeency);
                                bw.Dispose();
                                bw.Close();
                            }
                            pack = ms.ToArray();
                            ms.Dispose();
                            ms.Close();
                        }
                    }
                    using (FileStream fs = new FileStream(outputFile, FileMode.Create))
                    {
                        fs.Write(pack);
                        fs.Dispose();
                        fs.Close();
                    }
                }
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
                byte[] passBytes = Encoding.UTF8.GetBytes(password);
                byte[] encryptedData = File.ReadAllBytes(inputFile);
                byte[] encryptedFileData;
                byte[] aesEnKey;
                byte[] aesIVEnc;
                byte[] decryptedAesKey;
                byte[] decryptedAesIV;

                using (RSA rsa = RSA.Create())
                {
                    string bin = Path.Combine(Directory.GetCurrentDirectory(), "bin");
                    string keypair = Path.Combine(bin, "keypair");
                    string privatePath = Path.Combine(keypair, "private.p8");
                    byte[] privateKeyBytes = File.ReadAllBytes(inputFile);

                    rsa.ImportEncryptedPkcs8PrivateKey(passBytes, privateKeyBytes, out _);
                    using (MemoryStream ms = new MemoryStream(encryptedData))
                    {
                        using (BinaryReader br = new BinaryReader(ms))
                        {
                            int AesKeyLength = br.ReadInt32() * 8;
                            aesEnKey = br.ReadBytes(AesKeyLength / 8);

                            int AesIVLength = br.ReadInt32() * 8;
                            aesIVEnc = br.ReadBytes(AesIVLength / 8);

                            int EncryptedFileDatalength = br.ReadInt32();
                            encryptedFileData = br.ReadBytes(EncryptedFileDatalength);
                            br.Dispose();
                            br.Close();
                        }
                        ms.Dispose();
                        ms.Close();
                    }
                    decryptedAesKey = rsa.Decrypt(aesEnKey, RSAEncryptionPadding.OaepSHA512);
                    decryptedAesIV = rsa.Decrypt(aesIVEnc, RSAEncryptionPadding.OaepSHA512);

                    using (Aes aes = Aes.Create())
                    {
                        aes.KeySize = 256;
                        aes.BlockSize = 128;
                        aes.Padding = PaddingMode.PKCS7;
                        aes.Key = decryptedAesKey;
                        aes.IV = decryptedAesIV;

                        using (MemoryStream ms = new MemoryStream(encryptedFileData))
                        {
                            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                            {
                                using (FileStream fs = new FileStream(outputFile, FileMode.Create))
                                {
                                    cs.CopyTo(fs);
                                    fs.Dispose();
                                    fs.Close();
                                }
                                cs.Dispose();
                                cs.Close();
                            }
                            ms.Dispose();
                            ms.Close();
                        }
                    }
                }
                string dency = DecryptFileName(Path.GetFileNameWithoutExtension(inputFile), 8);
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