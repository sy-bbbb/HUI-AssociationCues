# HUI-AssociationCues
A Unity-based research platform for exploring association cues between AR head-mounted displays (HMDs) and mobile devices in hybrid user interfaces.

# Overview
This repository includes three interconnected Unity projects designed for testing different association cue methods in hybrid user interfaces:
* **desktopApp** - desktop application for experiment control
* **hmdApp** - AR HMD application that connects with the mobile device via association cues
* **phoneApp** - mobile device application that connects with the AR HMD via association cues

# Prerequisites
* Unity 2022.3.60f1 of later
* **AR HMD**: HoloLens 2
* **Mobile device**: Android (API level 22 or higher)
* **Tracking system**: OptiTrack (for mobile device tracking)

# Running an Experiment
1. Launch ```desktopApp``` to configure experiment parameters
2. Deploy and start ```hmdApp``` on the AR headset
3. Deploy and start ```phoneApp``` on the mobile device
4. Ensure OptiTrack system is calibrated and tracking the mobile device
5. Use the desktop interface to monitor and control the experiment session

# Dependencies
* MRTK v2.8.0
* OptiTrack Unity Plugin 1.6.1

# Acknowledgements
* 
