var name = resourceGroup().name
var location = resourceGroup().location
var storageAccountName = toLower(replace(name, '-', ''))
var storageAccountId = '${resourceGroup().id}/providers/Microsoft.Storage/storageAccounts/${storageAccountName}'

resource appInsights 'microsoft.insights/components@2020-02-02-preview' = {
  kind: 'other'
  name: name
  location: location
  tags: {}
  properties: {
    ApplicationId: name
    Application_Type: 'web'
  }
  dependsOn: []
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2018-07-01' = {
  sku: {
    name: 'Standard_LRS'
    tier: 'Standard'
  }
  kind: 'StorageV2'
  name: name
  location: location
  tags: {}
  properties: {
    supportsHttpsTrafficOnly: true
    encryption: {
      services: {
        file: {
          enabled: true
        }
        blob: {
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
    accessTier: 'Hot'
  }
  dependsOn: []
}

resource hostingPlan 'Microsoft.Web/serverfarms@2018-02-01' = {
  name: name
  location: location
  sku: {
    name: 'Y1'
  }
}

resource functionApp 'Microsoft.Web/sites@2015-08-01' = {
  name: name
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: hostingPlan.id
    httpsOnly: true
    siteConfig: {
      use32BitWorkerProcess: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
    }
  }
  dependsOn: [
    storageAccount
    appInsights
  ]
}

resource functionAppAppSettings 'Microsoft.Web/sites/config@2018-11-01' = {
  name: '${functionApp.name}/appsettings'
  properties: {
    AzureWebJobsStorage: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${listKeys(storageAccountId, '2015-05-01-preview').key1}'
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${listKeys(storageAccountId, '2015-05-01-preview').key1}'
    WEBSITE_CONTENTSHARE: toLower(name)
    FUNCTIONS_EXTENSION_VERSION: '~3'
    APPINSIGHTS_INSTRUMENTATIONKEY: reference(appInsights.id, '2015-05-01').InstrumentationKey
    FUNCTIONS_WORKER_RUNTIME: 'dotnet'
    WEBSITE_RUN_FROM_PACKAGE: '1'
  }
  dependsOn: [
    storageAccount
  ]
}