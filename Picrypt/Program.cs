using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace Picrypt
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string binPath = Path.Combine(Directory.GetCurrentDirectory(), "bin");
            string keypairPath = Path.Combine(binPath, "keypair");
            string publicXmlPath = Path.Combine(keypairPath, "public.xml");
            string privateXmlPath = Path.Combine(keypairPath, "private.xml");
            string passphrase;

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
                // Generate a new RSA key pair
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096)
                {
                    // Export the public key to XML
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
                    }
                    CspParameters cspParams = new CspParameters();
                    cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
                    cspParams.KeyContainerName = "RKC";
                    cspParams.ProviderType = 1; // PROV_RSA_AES
                    cspParams.KeyNumber = (int)KeyNumber.Exchange;
                    rsa.PersistKeyInCsp = false;

                    byte[] encryptedPrivateKey = rsa.ExportEncryptedPkcs8PrivateKey(passphrase, cspParams);
                    File.WriteAllBytes(privateXmlPath, encryptedPrivateKey);
                }
            }
            Application.Run(new Main());
        }
    }
}


