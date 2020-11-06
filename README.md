# pacs-data-migration-tool
 This donet tool can be used to migrate dcm files from one PACS to another

 ## usage
 Run the make file
    `make build`

 This will generate the binaries in the bin folder in the current directory.

 Now edit the `appsettings.json` file with the Source and Destination Dicom server details
  
 To run the data migration tool, go to bin and run folling command
    `dotnet PacsDataMigrationTool.dll`
    