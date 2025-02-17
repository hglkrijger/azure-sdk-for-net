# Release History

## 4.0.0-preview.6 (2019-11)

### Breaking changes

- `Certificate` and `CertificateWithPolicy` have been renamed to `KeyVaultCertificate` and `KeyVaultCertificateWithPolicy` to avoid ambiquity with other libraries and to yield better search results.
- `AdministratorDetails` has been renamed to `AdministratorContact`.
- `Action` has been renamed to `CertificatePolicyAction` to avoid ambiquity with other libraries.
- `Contact` has been renamed to `CertificateContact` to avoid ambiquity with other libraries.
- `Error` has been renamed to `CertificateOperationError` to avoid ambiquity with other libraries.
- `Issuer` has been renamed to `CertificateIssuer` to avoid ambiquity with other libraries.
- `CertificateClientOptions.Default` has been removed. Use `CertificatePolicy.Default` instead.
- Starting a certificate creation operation with `CertificateClient` now requires a `CertificatePolicy`.
- On `DeletedCertificate`, `DeletedDate` has been renamed to `DeletedOn`.
- `Hsm` properties and `hsm` parameters have been renamed to `HardwareProtected` and `hardwareProtected` respectively.
- `Certificate.CER` has been renamed to `KeyVaultCertificate.Cer`.
- `CertificateClient.RestoreCertificate` has been renamed to `CertificateClient.RestoreCertificateBackup` to better associate it with `CertificateClient.BackupCertificate`.

### Major changes

- A new `CertificatePolicy.Default` property returns a new policy suitable for self-signed certificate requests.
- `CertificateClient.VaultUri` has been added with the original value pass to `CertificateClient`.
- `CertificateClient.GetDeletedCertificates` includes an optional `includePending` parameter to include certificates in a delete pending state.

## 4.0.0-preview.5 (2019-10-07)

### Breaking changes

- `CertificateBase` has been renamed to `CertificateProperties`.
- `Certificate` no longer extends `CertificateProperties`, but instead contains a `CertificateProperties` property named `Properties`.
- `IssuerBase` has been renamed to `IssuerProperties`.
- `Issuer` no longer extends `IssuerProperties`, but instead contains a `IssuerProperties` property named `Properties`.
- `CertificatePolicy` has been flattened to include all properties from `KeyOptions` and derivative classes.
- `KeyOptions` and derivative classes have been removed.
- `CertificateKeyType` members have been aligned with `Azure.Security.KeyVault.Keys.KeyType` members.
- `CertificateImport` has been renamed to `CertificateImportOptions`.
