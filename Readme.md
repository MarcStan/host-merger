# HostMerger

Pi-hole, DNS66, uBlock, etc. all support multiple Host files as sources for blocklists.

NetGuard does not. It only accepts a single hostfile.

To still get the same level of blocking, this tool merges them all into one.

It takes a list of input files and periodically merges them into a single global list.

## Setup

Run the ARM deployment via Deploy.ps1 script with the desired resourcegroup name.

**Manually** create a keyvault and grant the MSI of the function app secret read+list permissions.

**Manually** fill the keyvault with the needed secrets (check Configuration.cs for the values needed).

**Manually** configure the function to use the keyvault (set KeyVaultName in app settings).

Run function code deployment.

**Manually** set keyvault name in configuration (e.g. vsts app settings override).
