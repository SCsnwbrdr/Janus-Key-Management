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
        private Dictionary<string, Dictionary<string, KeyToken>> _typeToCredentialDictionary;

        public JanusAzureKeyEngine(Dictionary<string, string[]> Tokens, Dictionary<string, JanusCredentials[]> Credentials)
        {
            _typeToTokenDictionary = new Dictionary<string, List<KeyToken>>();
            _typeToCredentialDictionary = new Dictionary<string, Dictionary<string, KeyToken>>();
            CreateTokenDictionary(Tokens);
            CreateCredentialDictionary(Credentials);
        }

        private void CreateCredentialDictionary(Dictionary<string, JanusCredentials[]> Credentials)
        {
            foreach (var service in Credentials)
            {
                var credentials = new Dictionary<string, KeyToken>();
                foreach (var item in service.Value)
                {
                    var secretBundle = FetchKeyVault(item.KeyVault).Result;
                    credentials.Add(item.UserName, new KeyToken()
                    {
                        Identifier = item.UserName,
                        IsPrimary = credentials.Count == 0,
                        Token = secretBundle.Value
                    });
                }
                _typeToCredentialDictionary.Add(service.Key, credentials);
            }
        }

        private void CreateTokenDictionary(Dictionary<string, string[]> Tokens)
        {
            foreach (var service in Tokens)
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
                var activeToken = TokenSet.FirstOrDefault(item => item.IsPrimary);
                if (activeToken == null) { throw new NullReferenceException(NoKeyErrorMessage); }
                return activeToken;
            }
        }

        public KeyToken ActiveCredential
        {
            get
            {
                var activeToken = CredentialSet.FirstOrDefault(item => item.Value.IsPrimary);
                if (activeToken.Value == null || activeToken.Key == null) { throw new NullReferenceException(NoKeyErrorMessage); }
                return activeToken.Value;
            }
        }

        private Dictionary<string, KeyToken> CredentialSet
        {
            get
            {
                var frame = new StackFrame(2);
                var className = frame.GetMethod().DeclaringType.Name;
                if (!_typeToCredentialDictionary.ContainsKey(className)) { throw new InvalidOperationException(string.Format(NoClassErrorMessage, className)); }
                return _typeToCredentialDictionary.GetValueOrDefault(className);
            }
        }

        private List<KeyToken> TokenSet
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
            var badToken = TokenSet.Find(item => item.IsPrimary);
            var keyToken = TokenSet.Find(item => !item.DeadToken && !item.IsPrimary);
            badToken.IsPrimary = false;
            badToken.DeadToken = true;
            keyToken.IsPrimary = true;
        }

        public async Task RefreshDeadTokens()
        {
            var badTokens = TokenSet.Where(item => item.IsPrimary);
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
