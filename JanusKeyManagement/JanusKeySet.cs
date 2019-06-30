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
    public class JanusKeySet : IJanusKeySet
    {
        private const string NoKeyErrorMessage = "There are no keys listed as a the primary key, either no keys were passed or all keys failed.";
        private const string NoClassErrorMessage = "No matching service type was found. Ensure that the service referenced in the JanusKeyEngine section of the app config has a value matching '{0}'";
        private readonly List<KeyToken> _tokens = new List<KeyToken>();

        public JanusKeySet(Dictionary<string, string> keyVaultCredentials)
        {
            CreateCredentialDictionary(keyVaultCredentials);
        }

        public JanusKeySet(string[] keyVaultUrls)
        {
            CreateTokenDictionary(keyVaultUrls);
        }

        private void CreateCredentialDictionary(Dictionary<string, string> keyVaultCredentials)
        {
            foreach (var item in keyVaultCredentials)
            {
                var secretBundle = FetchKeyVault(item.Value).Result;
                _tokens.Add(new KeyToken()
                {
                    Identifier = item.Key,
                    IsPrimary = _tokens.Count == 0,
                    Token = secretBundle.Value
                });
            }
        }

        private void CreateTokenDictionary(string[] keyVaultUrls)
        {
                foreach (var item in keyVaultUrls)
                {
                    var secretBundle = FetchKeyVault(item).Result;
                _tokens.Add(new KeyToken()
                    {
                        Identifier = item,
                        IsPrimary = _tokens.Count == 0,
                        Token = secretBundle.Value
                    });
                }
        }

        public KeyToken Active {
            get
            {
                var activeToken = _tokens.FirstOrDefault(item => item.IsPrimary);
                if (activeToken == null) { throw new NullReferenceException(NoKeyErrorMessage); }
                return activeToken;
            }
        }

        private async Task<SecretBundle> FetchKeyVault(string keyVaultUrl)
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            return await keyVaultClient.GetSecretAsync(keyVaultUrl).ConfigureAwait(false);
        }

        public async Task Refresh()
        {
            var badTokens = _tokens.Where(item => item.DeadToken);
            foreach (var badToken in badTokens)
            {
                var secretBundle = await FetchKeyVault(badToken.Identifier);
                badToken.Token = secretBundle.Value;
                badToken.DeadToken = false;
            }
        }

        public void Rotate()
        {
            var badToken = _tokens.Find(item => item.IsPrimary);
            var keyToken = _tokens.Find(item => !item.DeadToken && !item.IsPrimary);
            badToken.IsPrimary = false;
            badToken.DeadToken = true;
            keyToken.IsPrimary = true;
        }
    }
}
