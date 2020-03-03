# HostMerger

Merge multiple sources into a single hostfile using azure functions.

Pi-hole, DNS66, uBlock, etc. all support multiple Host files as sources for blocklists.

NetGuard does not. It only accepts a single hostfile.

To still get the same level of blocking, this tool merges them all into one list.

It takes a list of input files and periodically merges them into a single global list.

## Features

* deduplication
* IPv4 and IPv6 format
* whitelist to override domains
* cheap thanks to Azure functions

## Setup

Run the ARM deployment via Deploy.ps1 script with the desired resourcegroup name then modify the `functionapp-azure.yml` to also use said resourcegroup name andm odify the appSettings parameters in the yaml file if need be.

Run function code deployment (e.g. via `functionapp-azure.yml`) or deploy the function manually.

It will run automatically once per day. If you modify the input or whitelist file you can manually trigger the function (or wait for it to run again).

## Example

The example folder contains files that can be uploaded. If the azure function is configured to use them (via appSettings), it will generate an output file (default "hosts.txt") that can then be used by Windows, Pi-hole, DNS66, NetGuard and many others.

Note that using many sources may result in big (20MB+) hostfiles and Windows may not be able to handle them!