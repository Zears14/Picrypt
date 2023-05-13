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
            string binPath = Path.Combine(Directory.GetCurrentDirectory(), "bin");
            string keypairPath = Path.Combine(binPath, "keypair");
            string publicXmlPath = Path.Combine(keypairPath, "public.xml");
            string privateXmlPath = Path.Combine(keypairPath, "private.xml");

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

                    // Export the private key to XML
                    string privateKey = rsa.ToXmlString(true);
                    File.WriteAllText(privateXmlPath, privateKey);
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}


