
# Vodenko

Vodenko is a project focused on the development and implementation of an advanced control system for managing the water level in a tank. This system utilizes a combination of MATLAB/Simulink modeling, PLC programming, and real-time data processing to achieve precise control and monitoring of water levels.

## Table of Contents
- [Features](#features)
- [Project Structure](#project-structure)
- [Installation](#installation)
- [Usage](#usage)
- [MATLAB/Simulink Models](#matlabsimulink-models)
- [PLC Integration](#plc-integration)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgements](#acknowledgements)

## Features
- **Advanced Control Algorithms**: Implementation of PID, LQR, and adaptive control algorithms.
- **Real-Time Data Processing**: Continuous monitoring and control of water levels using real-time data.
- **PLC Integration**: Communication with PLCs for robust and industrial-grade control.
- **Simulation and Modeling**: MATLAB/Simulink models for testing and validating control strategies.
- **User Interface**: Simple and intuitive UI for monitoring system status and alarms.

## Project Structure
```
Vodenko/
│
├── Matlab_Simulink/       # MATLAB and Simulink models for system simulation
│   ├── models/
│   ├── scripts/
│   └── results/
│
├── PLC_Code/              # PLC program and configuration files
│   ├── main_program/
│   ├── configurations/
│   └── diagnostics/
│
├── Data/                  # Datasets and logs from the system
│   ├── raw_data/
│   ├── processed_data/
│   └── logs/
│
└── Docs/                  # Documentation and reports
    ├── user_manual/
    ├── technical_specifications/
    └── diagrams/
```

## Installation

### Prerequisites
- **MATLAB**: Ensure that you have MATLAB installed with Simulink support.
- **PLC Software**: Required to interface with the PLC hardware (e.g., TIA Portal for Siemens PLCs).
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
