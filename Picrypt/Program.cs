using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Picrypt
{
    internal static class Program
    {
        public static string binPath;
        public static string keypairPath;
        public static string publicXmlPath;

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            binPath = Path.Combine(Directory.GetCurrentDirectory(), "bin");
            keypairPath = Path.Combine(binPath, "keypair");
            publicXmlPath = Path.Combine(keypairPath, "public.xml");
            string privateXmlPath = Path.Combine(keypairPath, "private.p8");
            string passphrase = null;

            if (!Directory.Exists(binPath))
            {
                Directory.CreateDirectory(binPath);
            }

            if (!Directory.Exists(keypairPath))
            {
                Directory.CreateDirectory(keypairPath);
            }

            if (!File.Exists(publicXmlPath) || !File.Exists(privateXmlPath))
            {
                try
                {
                    // Generate a new RSA key pair
                    using (RSA rsa = RSA.Create())
                    {   // Export the public key to XML
                        rsa.KeySize = 4096;
                        string publicKey = rsa.ToXmlString(false);
                        File.WriteAllText(publicXmlPath, publicKey);

                        // Export the private key to XML encrypted with a passphrase
                        using (var form = new Form())
                        {
                            form.Text = "Enter Passphrase";
                            form.FormBorderStyle = FormBorderStyle.FixedDialog;
                            form.MaximizeBox = false;
                            form.MinimizeBox = false;
                            form.StartPosition = FormStartPosition.CenterParent;
                            var passwordLabel = new Label() { Left = 20, Top = 20, Text = "Passphrase:" };
                            var passwordTextBox = new TextBox() { Left = 100, Top = 20, Width = 200, PasswordChar = '*' };
                            var cancelButton = new Button() { Text = "Cancel", Left = 235, Width = 70, Top = 60 };
                            var okButton = new Button() { Text = "OK", Left = 160, Width = 70, Top = 60 };
                            okButton.DialogResult = DialogResult.OK;
                            cancelButton.DialogResult = DialogResult.Cancel;
                            form.AcceptButton = okButton;
                            form.CancelButton = cancelButton;
                            form.ClientSize = new Size(320, 100);
                            form.Controls.Add(okButton);
                            form.Controls.Add(cancelButton);
                            form.Controls.Add(passwordLabel);
                            form.Controls.Add(passwordTextBox);
                            if (form.ShowDialog() == DialogResult.OK)
                            {
                                passphrase = passwordTextBox.Text;
                            }
                            else if (form.ShowDialog() == DialogResult.Cancel)
                            {
                                Application.Exit();
                            }
                        }

                        var pbeParams = new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA512, 10000);

                        byte[] encryptedPrivateKey = rsa.ExportEncryptedPkcs8PrivateKey(Encoding.UTF8.GetBytes(passphrase), pbeParams);
                        File.WriteAllBytes(privateXmlPath, encryptedPrivateKey);
                        Console.WriteLine("Success");
                    }
                }
                catch (CryptographicException e)
                {
                    MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                }
            }
            Application.Run(new Main());
        }
    }
}