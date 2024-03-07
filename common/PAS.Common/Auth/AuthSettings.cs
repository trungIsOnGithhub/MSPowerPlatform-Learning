using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;

namespace PAS.Common
{
    public class AuthSettings
    {
        private static readonly Lazy<X509Certificate2> appOnlyCertificateLazy = new Lazy<X509Certificate2>(() =>
        {
            X509Certificate2 appOnlyCertificate = null;

            X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certCollection = certStore.Certificates.Find(
                X509FindType.FindByThumbprint,
                ConfigurationManager.AppSettings["AppOnlyCertificateThumbprint"],
                false);

            if (certCollection.Count > 0)
                appOnlyCertificate = certCollection[0];

            certStore.Close();

            return appOnlyCertificate;
        });

        public static X509Certificate2 AppOnlyCertificate
        {
            get
            {
                return (appOnlyCertificateLazy.Value);
            }
        }
    }
}
