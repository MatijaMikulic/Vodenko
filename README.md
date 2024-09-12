
# Vodenko

Vodenko is a project focused on the development and implementation of an advanced control system for managing the water level in a tank. This system utilizes a combination of MATLAB/Simulink modeling, PLC programming, and real-time data processing to achieve precise control and monitoring of water levels.

## Table of Contents
- [Features](#features)
- [Project Structure](#project-structure)
- [Installation](#installation)
- [Usage](#usage)
- [MATLAB/Simulink Models](#matlabsimulink-models)
- [PLC Integration](#plc-integration)


## Features
- **Advanced Control Algorithms**: Implementation of PID, LQR, and adaptive control algorithms.
- **Real-Time Data Processing**: Continuous monitoring and control of water levels using real-time data.
- **PLC Integration**: Communication with PLCs for robust and industrial-grade control.
- **Simulation and Modeling**: MATLAB/Simulink models for testing and validating control strategies.
- **User Interface**: Simple and intuitive UI for monitoring system status and alarms.

## Project Structure
```
Diplomski/
├── Vodenko/                    # Main application folder
│   ├── DataAccess/             # Data access layer
│   ├── ModelProvider/          # Provides models for the application
│   ├── VodenkoWeb/             # Web interface for the application
│
├── Shared/                     # Shared resources or libraries
│
├── scripts/                    # Scripts for various utilities
│   ├── matlab-math_model/      # MATLAB models related to the project
│   └── python-odbc/            # Python scripts for fetching data from db (results of rls)
│
├── CommunicationL1L2/          # Communication services layer
│   ├── Common/                 # Common libraries or utilities
│   ├── DataAccess/             # Data access for communication layer
│   ├── Libraries/              # Libraries for communication services
│   ├── Shared/                 # Shared components with other services
│   ├── WindowsServices/        # Windows services related to communication
│
├── API/                        # API for programmatic access
│   └── app.py                  # Main API script

## Installation

### Prerequisites
- **MATLAB**: Ensure that you have MATLAB installed with Simulink support.
- **Git**: To clone the repository.
- **Rabbit MQ** : Required for message sending.

### Clone the Repository
```bash
git clone https://github.com/MatijaMikulic/Vodenko.git
```

### MATLAB Setup
- Navigate to the `Matlab_Simulink` directory.
- Open MATLAB and set the current directory to the path where the project is cloned.
- Run the required simulation scripts in the `scripts` folder.

### PLC Setup
- Open the `PLC_Code` directory.
- Import the PLC program into your PLC programming environment.
- Configure the PLC hardware according to the specifications in the `configurations` folder.

## Usage

### MATLAB/Simulink
- Open the required Simulink models from the `models` directory.
- Run the simulations to analyze the control strategies.
- Use the provided scripts to visualize results and generate reports.

### PLC Integration
- Deploy the PLC program to your hardware.
- Monitor the system using the integrated UI or through your PLC environment.
- Ensure that the sensors and actuators are correctly interfaced with the PLC.

### Data Logging and Analysis
- Logs and real-time data are stored in the `Data/logs` directory.
- Use the provided scripts to process and analyze data for further improvements.

## MATLAB/Simulink Models
The project includes several MATLAB/Simulink models for simulating the water tank control system. These models are located in the `Matlab_Simulink/models` directory and include:

- **LQR Control**: Linear-Quadratic Regulator for optimal control.
- **Adaptive Control**: Dynamic adjustment of control parameters.

## PLC Integration
The project supports integration with PLCs for industrial applications. The PLC code provided in the `PLC_Code` directory can be used to control actual hardware. Make sure to follow the instructions in the `configurations` folder for proper setup.


## Images

![image](https://github.com/user-attachments/assets/f1f2418b-c46c-4b2d-af60-f6f5003d7380)

