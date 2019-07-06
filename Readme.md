# HostMerger

Pi-hole, DNS66, uBlock, etc. all support multiple Host files as sources for blocklists.

NetGuard does not. It only accepts a single hostfile.

To still get the same level of blocking, this tool merges them all into one.

It takes a list of input files and periodically merges them into a single global list.

## Setup

Run the ARM deployment via Deploy.ps1 script with the desired resourcegroup name then modify the `azure-pipeline.yaml` to also use said resourcegroup name andmodify the appSettings parameters in the yaml file if need be.

Run function code deployment (e.g. via `azure-pipelines.yaml`) or deploy the function manually.

It will run automatically once per day, or whenever the config/whitelist file is modified.
