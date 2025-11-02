# Troubleshooting

## Port Binding Errors in Aspire

### Symptom
```
fail: Aspire.Hosting.Dcp.dcpctrl.dcpctrl.ServiceReconciler[0]
      Service api-https is configured to use a port in the ephemeral range on your machine
      {"Service": "/api-https", "Port": 7181, "error": "listen tcp [::1]:7181: bind: 
      An attempt was made to access a socket in a way forbidden by its access permissions."}
```

### Cause
Windows reserves certain port ranges for system services (Hyper-V, WSL, Docker). These excluded ports can conflict with Aspire's auto-assigned ports for HTTPS profiles.

### Check Excluded Ports
Run in PowerShell:
```powershell
netsh interface ipv4 show excludedportrange protocol=tcp
```

### Solution
Aspire automatically uses the first launch profile in `launchSettings.json`. Ensure the HTTP profile is listed first:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5130",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:5001;http://localhost:5130",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

Update any `.http` files and client code to use the HTTP port (5130) instead of the blocked HTTPS port (7181).