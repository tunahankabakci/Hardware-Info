# Hardware Info

Hardware Info is a tool to see hardware identifiers.

### Used win32 functions
- GetVolumeInformationW
- GetAdaptersInfo 
- GetCurrentHwProfileW
- RegGetValueW

### GetVolumeInformation
Retrieves the serial of all the partitions on the computer.

### GetAdaptersInfo
Retrieves the information of all adapters on the computer (Mac adress, Guid, name etc.)

### GetCurrentHwProfile
Retrieves the current user HW Profile Guid

### RegGetValue
Retrieves the any value from regedit. 
Written for the following key and values.
> - SusClientId => Guid
> - ProductId => 4x5 Key ( xxxxx-xxxxx-xxxxx-xxxxx ) 
> - InstallDate => timestamp
> - MachineGuid => Guid