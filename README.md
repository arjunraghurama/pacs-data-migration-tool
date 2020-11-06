# pacs-data-migration-tool
 This donet tool can be used to migrate dcm files from one PACS to another

## Usage
 Run the make file
    `make build`

 This will generate the binaries in the bin folder in the current directory.

 Now edit the `appsettings.json` file in bin directory with the Source and Destination Dicom server details

 To run the data migration tool, go to bin and run folling command
    `dotnet PacsDataMigrationTool.dll`

## Using Docker
The same can be acheived through docker also.
First build the code using make.
Make changes to `appsettings.json` in bin directory
Then run `docker-compose up`
To shutdown the service run `docker-compose down`
    