using System.Security.Cryptography.X509Certificates;

namespace DemaWare.DemaIdentify.Models.Configuration; 
public class OpenIddictConfigurationModel {
    public bool UseDevelopmentCertificate { get; set; }

    public CertificateModel Certificate { get; set; } = new CertificateModel();

    public class CertificateModel {
        public string Thumbprint { get; set; } = string.Empty;
        public StoreName StoreName { get; set; } = StoreName.My;
        public StoreLocation StoreLocation { get; set; } = StoreLocation.LocalMachine;
    }
}
