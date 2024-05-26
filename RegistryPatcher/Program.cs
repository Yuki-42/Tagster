/*
 * Tiny app that patches a few registry keys to allow for the main app to run.
 */
 
// Check the state of the HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem\LongPathsEnabled registry key as the program will not work without it.

using Microsoft.Win32;

#pragma warning disable CA1416

RegistryKey machineKeys = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\FileSystem", true)!; // We can ignore the nullability warning here as we know the key exists.

// Now check if the key exists or is not set to 1.
if (
        machineKeys.GetValue("LongPathsEnabled") is null ||
        (int)(machineKeys.GetValue("LongPathsEnabled") ?? 0) != 1
    )
    // The key does not exist, so we need to create it.
    machineKeys.SetValue("LongPathsEnabled", 1, RegistryValueKind.DWord);
    
#pragma warning restore CA1416