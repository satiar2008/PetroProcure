# Production security checklist

- Supply a random JWT signing key of at least 32 characters from a secret store.
- Verify JWT issuer, audience, lifetime, and signing-key rotation procedures.
- Keep admin bootstrap disabled after initial provisioning.
- If bootstrap is enabled, supply a strong one-time password through a secret source.
- Change the bootstrap password immediately and enable the organization's password/MFA policy.
- Use encrypted SQL connections and a least-privilege database login.
- Apply all EF Core migrations before accepting traffic.
- Restrict CORS and reverse-proxy trusted networks.
- Use HTTPS only and terminate TLS with approved certificates.
- Configure upload size limits, extension/MIME allowlists, malware scanning, quotas, and retention.
- Protect the file repository with operating-system ACLs and backups.
- Store OpenAI-compatible provider keys in a secret manager.
- Enable centralized audit logs, security monitoring, rate limiting, and alerting.
- Review every new API endpoint for authentication, permission, and department scope.
