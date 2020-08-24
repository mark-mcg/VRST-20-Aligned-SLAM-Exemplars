The logging testbed scene was used for capturing data on different MR devices. Supports any device with a Prefab that has a Player component attached and correctly setup.

# For Oculus Quest 
* Go to project settings, XR Plugin Management, tick initialize on startup and add the Oculus XR Plugin provider
* Find the ZED folder `Assets\Research\UIST\Dependencies\ZED\SDK\Helpers\Shaders` and add a "~" symbol to the end of that folder so it is ignored for compilation (breaks under Android)
* Under `Mixed Reality Devices` disable all but the Unity XR device

# For ARCORE
* Go to project settings, XR Plugin Management, untick initialize on startup, and make sure ARCore is enabled in project settings under `Google ARCore`
* Find the ZED folder `Assets\Research\UIST\Dependencies\ZED\SDK\Helpers\Shaders` and add a "~" symbol to the end of that folder so it is ignored for compilation (breaks under Android)
* Under `Mixed Reality Devices` disable all but AR Core object


# For every device
* Change the DeviceToLog field of the LoggingManager attached to `Logging` to the device you enabled (each has a DeviceToLog component attached)

# For ZED cameras (need to re-create a Player prefab using the ZED prefab, was stripped out for this commit to remove the ZED dependencies)
* Go to project settings, XR Plugin Management, untick initialize on startup
* Find the ZED folder `Assets\Research\UIST\Dependencies\ZED\SDK\Helpers\Shaders` and remove the "~" if present
* Under `Mixed Reality Devices` disable all but the ZED_Rig_Stereo device