using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ukubectl
{
    class BouncyCastleHelper
    {
        public static Org.BouncyCastle.X509.X509Certificate CertificateFromBase64Encoded(string text)
        {
            byte[] cert = Convert.FromBase64String(text);

            X509CertificateParser p = new X509CertificateParser();
            return p.ReadCertificate(cert);
        }

        public static string InstallCertificate(string userName, string password, string userKey, string userCrt)
        {
            StringReader sr = new StringReader(userKey);
            PemReader pemReader = new PemReader(sr);
            AsymmetricCipherKeyPair keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
            AsymmetricKeyParameter KeyParameter = keyPair.Private;

            Org.BouncyCastle.X509.X509Certificate cert = BouncyCastleHelper.CertificateFromBase64Encoded(userCrt);

            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            AsymmetricKeyEntry keyEntry = new AsymmetricKeyEntry(KeyParameter);
            X509CertificateEntry certEntry = new X509CertificateEntry(cert);

            string cnName = cert.SubjectDN.GetValueList(X509Name.CN).Cast<string>().FirstOrDefault() ?? userName;
            store.SetKeyEntry(cnName, keyEntry, new X509CertificateEntry[] { certEntry });

            using (MemoryStream ms = new MemoryStream())
            {
                store.Save(ms, password.ToCharArray(), new SecureRandom());

                byte[] pfxBytes = ms.ToArray();

                X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                certStore.Open(OpenFlags.ReadWrite);
                certStore.Add(new X509Certificate2(pfxBytes, password.ToSecureString()));
                certStore.Close();
            }

            return cnName;
        }

        public static string InstallCertificate(string certData)
        {
            X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(certData));

            using (MemoryStream ms = new MemoryStream())
            {
                X509Store certStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                certStore.Open(OpenFlags.ReadWrite);
                certStore.Add(cert);
                certStore.Close();
            }

            return cert.SubjectName.Name;
        }
    }
}
