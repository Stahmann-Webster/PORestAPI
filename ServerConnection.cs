using Microsoft.Extensions.Configuration;
using SAPbobsCOM;
using System.Runtime.InteropServices;

namespace SAP_Business_One
{
    public class ServerConnection
    {
        private readonly IConfiguration _configuration;
        private Company _company;
        private int _connectionResult;
        private int _errorCode;
        private string _errorMessage;

        public ServerConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            _company = new Company();
        }

        public int Connect()
        {
            var sapConfig = _configuration.GetSection("SAPConnection");

            _company.Server = sapConfig["Server"];
            _company.CompanyDB = sapConfig["CompanyDB"];
            _company.DbServerType = BoDataServerTypes.dst_MSSQL2019;
            _company.UserName = sapConfig["User"];
            _company.Password = sapConfig["Password"];
            _company.language = BoSuppLangs.ln_English;

            _connectionResult = _company.Connect();

            if (_connectionResult != 0)
            {
                _company.GetLastError(out _errorCode, out _errorMessage);
            }

            return _connectionResult;
        }

        public void Disconnect()
        {
            if (_company.Connected)
            {
                _company.Disconnect();
                Marshal.ReleaseComObject(_company);
                _company = null;
                GC.Collect();
            }
        }

        public Company GetCompany() => _company;
        public int GetErrorCode() => _errorCode;
        public string GetErrorMessage() => _errorMessage;
    }
}
