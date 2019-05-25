using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JanusKeyManagement
{
    public class JanusAzureKeyEngine : IJanusKeyEngine
    {
        private const string NoKeyErrorMessage = "There are no keys listed as a the primary key, either no keys were passed or all keys failed.";
        private const string NoClassErrorMessage = "No matching service type was found. Ensure that the service referenced in the JanusKeyEngine section of the app config has a value matching '{0}'";
        private Dictionary<string, List<KeyToken>> _typeToTokenDictionary;

        public JanusAzureKeyEngine(Dictionary<string, string[]> Mappings)
        {
            _typeToTokenDictionary = new Dictionary<string, List<KeyToken>>();
            foreach (var service in Mappings)
            {
                var keyTokens = new List<KeyToken>();
                foreach (var item in service.Value)
                {
                    var secretBundle = FetchKeyVault(item).Result;
                    keyTokens.Add(new KeyToken()
                    {
                        Identifier = item,
                        IsPrimary = keyTokens.Count == 0,
                        Token = secretBundle.Value
                    });
                }
                _typeToTokenDictionary.Add(service.Key, keyTokens);
            }

        }

        public KeyToken ActiveToken
        {
            get
            {
                var activeToken = CurrentStore.FirstOrDefault(item => item.IsPrimary);
                if (activeToken == null) { throw new NullReferenceException(NoKeyErrorMessage); }
                return activeToken;
            }
        }

        private List<KeyToken> CurrentStore
        {
            get
            {
                var frame = new StackFrame(2);
                var className = frame.GetMethod().DeclaringType.Name;
                if (!_typeToTokenDictionary.ContainsKey(className)) { throw new InvalidOperationException(string.Format(NoClassErrorMessage, className)); }
                return _typeToTokenDictionary.GetValueOrDefault(className);
            }
        }

        public void RotateToken()
        {
            var badToken = CurrentStore.Find(item => item.IsPrimary);
            var keyToken = CurrentStore.Find(item => !item.DeadToken && !item.IsPrimary);
            badToken.IsPrimary = false;
            badToken.DeadToken = true;
            keyToken.IsPrimary = true;
        }

        public async Task RefreshDeadTokens()
        {
            var badTokens = CurrentStore.Where(item => item.IsPrimary);
            foreach (var badToken in badTokens)
            {
                var secretBundle = await FetchKeyVault(badToken.Identifier);
                badToken.Token = secretBundle.Value;
                badToken.DeadToken = false;
            }
        }

        private async Task<SecretBundle> FetchKeyVault(string keyVaultUrl)
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            return await keyVaultClient.GetSecretAsync(keyVaultUrl).ConfigureAwait(false);
        }
    }
}
