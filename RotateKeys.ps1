
$key = New-AzServiceBusKey -ResourceGroup KeyVault-POC -Namespace jplservice -Queue mainqueue -Name SendQueueMessage -RegenerateKey PrimaryKey
$newKey = $key.PrimaryKey
$Secret = ConvertTo-SecureString -String $newKey -AsPlainText -Force
Set-AzKeyVaultSecret -VaultName 'KeyVault-JPL' -Name 'ServiceBusMainFetch' -SecretValue $Secret


$key = New-AzServiceBusKey -ResourceGroup KeyVault-POC -Namespace jplservice -Queue mainqueue -Name SendQueueMessage -RegenerateKey SecondaryKey
$newKey = $key.SecondaryKey
$Secret = ConvertTo-SecureString -String $newKey -AsPlainText -Force
Set-AzKeyVaultSecret -VaultName 'KeyVault-JPL' -Name 'ServiceBusMainFetch' -SecretValue $Secret