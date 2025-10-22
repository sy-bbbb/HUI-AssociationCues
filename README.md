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
* This work was supported by Institute of Information & communications Technology Planning & Evaluation(IITP) grant funded by the Korea government(MSIT) (RS-2019-II191270, WISE AR UI/UX Platform Development for Smartglasses)
* 이 논문은 2025년도 정부(과학기술정보통신부)의 재원으로 정보통신기획평가원의 지원을 받아 수행된 연구임 (RS-2019-II191270, (SW 스타랩) 스마트 안경을 위한 WISE AR UI/UX 플랫폼 개발)
