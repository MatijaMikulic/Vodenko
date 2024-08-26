from flask import Flask, request, jsonify
import numpy as np
from scipy import signal, linalg
import control
import matplotlib.pyplot as plt
import io
import os
from flask_cors import CORS
from datetime import datetime
from scipy.signal import dlsim

app = Flask(__name__)
CORS(app)

def find_asymptotes(poles, zeros):
    """
    Calculate the angles and centroid of the asymptotes for the root locus plot.
    
    Args:
    poles (array): Array of pole locations.
    zeros (array): Array of zero locations.
    
    Returns:
    tuple: Angles and centroid of the asymptotes.
    """
    num_poles = len(poles)
    num_zeros = len(zeros)
    num_asymptotes = num_poles - num_zeros
    if num_asymptotes == 0:
        return [], 0

    centroid = (np.sum(poles) - np.sum(zeros)).real / num_asymptotes
    angles = [(2 * k + 1) * 180.0 / num_asymptotes for k in range(num_asymptotes)]
    return angles, centroid

def root_locus(num, den, k_max=1000, num_points=10000):
    """
    Generate root locus data for a given transfer function.
    
    Args:
    num (array): Numerator of the transfer function.
    den (array): Denominator of the transfer function.
    k_max (float): Maximum gain value.
    num_points (int): Number of points to generate.
    
    Returns:
    tuple: Root locus data, poles, and zeros.
    """
    poles = np.roots(den)
    zeros = np.roots(num)
    locus = []
    k_values = np.linspace(0, k_max, num_points)

    for k in k_values:
        closed_loop_den = den + k * np.pad(num, (len(den) - len(num), 0))
        closed_loop_poles = np.roots(closed_loop_den)
        for idx, pole in enumerate(closed_loop_poles):
            locus.append({'real': pole.real, 'imag': pole.imag, 'gain': k, 'index': idx})

    return locus, poles, zeros

def adaptive_gain_vector(k_max=500, num_points=3000, critical_gains=None):
    """
    Generate an adaptive gain vector for root locus analysis.
    
    Args:
    k_max (float): Maximum gain value.
    num_points (int): Number of points to generate.
    critical_gains (array): Critical gains for adaptive step size.
    
    Returns:
    array: Adaptive gain vector.
    """
    if critical_gains is None:
        critical_gains = []
    critical_gains = np.sort(np.unique(critical_gains))
    gains = np.linspace(0, k_max, num_points)
    adaptive_gains = []

    # Adaptive step size based on critical gains
    for i in range(len(critical_gains) - 1):
        low = critical_gains[i]
        high = critical_gains[i + 1]
        step = (high - low) / (num_points // len(critical_gains))
        adaptive_gains.extend(np.arange(low, high, step))

    adaptive_gains.extend(gains)
    adaptive_gains = np.sort(np.unique(adaptive_gains))
    
    return adaptive_gains

def settling_time(t, response, threshold=0.05):
    """
    Calculate the settling time of a response.
    
    Args:
    t (array): Time array.
    response (array): Response array.
    threshold (float): Settling threshold.
    
    Returns:
    float: Settling time.
    """
    final_value = response[-1]
    threshold_value = threshold * abs(final_value)
    
    settled = np.abs(response - final_value) < threshold_value
    for i in range(len(response) - 1, -1, -1):
        if not settled[i]:
            return t[i + 1]
    return t[0]

def overshoot(response):
    """
    Calculate the overshoot of a response.
    
    Args:
    response (array): Response array.
    
    Returns:
    float: Overshoot percentage.
    """
    if len(response) < 2:
        return -1

    initial_value = response[0]
    max_value = max(response)
    final_value = response[-1]
    
    
    if final_value == 0:
        return -1

    return ((max_value - final_value) / (final_value - initial_value) * 100)

def rise_time(t, response, threshold=0.90):
    """
    Calculate the rise time of a response.
    
    Args:
    t (array): Time array.
    response (array): Response array.
    threshold (float): Rise time threshold.
    
    Returns:
    float: Rise time.
    """
    final_value = response[-1]
    for i, value in enumerate(response):
        if value >= threshold * final_value:
            return t[i]
    return -1

def closed_loop_poles(A, B, C, D, num_pid, den_pid, gain):
    """
    Calculate the closed-loop poles of a system.
    
    Args:
    A (array): System matrix.
    B (array): Input matrix.
    C (array): Output matrix.
    D (array): Feedforward matrix.
    num_pid (array): PID numerator.
    den_pid (array): PID denominator.
    gain (float): Gain value.
    
    Returns:
    array: Closed-loop poles.
    """
    num_ol = np.polymul(num_pid, B)
    den_ol = np.polymul(den_pid, A)
    closed_loop_den = np.polyadd(den_ol, gain * num_ol)
    poles = np.roots(closed_loop_den)
    return poles

@app.route('/root_locus', methods=['POST'])
def get_root_locus_data():
    """
    Handle POST requests to generate root locus data.
    
    Returns:
    JSON: Root locus data, poles, zeros, angles, and centroid.
    """
    data = request.get_json()
    A = np.array(data['A'])
    B = np.array(data['B'])
    C = np.array(data['C'])
    D = np.array(data['D'])
    Kp = data['Kp']
    Ki = data['Ki']
    Kd = data['Kd']
    
    ss_system = control.ss(A, B, C, D)
    tf_system = control.ss2tf(ss_system)

    # Determine PID transfer function numerator and denominator
    if Ki == 0 and Kd == 0:
        num_pid = [Kp]
        den_pid = [1]
    elif Ki == 0:
        num_pid = [Kd, Kp]
        den_pid = [1]
    elif Kd == 0:
        num_pid = [Kp, Ki]
        den_pid = [1, 0]
    else:
        num_pid = [Kd, Kp, Ki]
        den_pid = [1, 0]

    num_ol = np.polymul(num_pid, tf_system.num[0][0])
    den_ol = np.polymul(den_pid, tf_system.den[0][0])
    
    poles = control.poles(control.TransferFunction(num_ol, den_ol))
    zeros = control.zeros(control.TransferFunction(num_ol, den_ol))
    critical_gains = np.concatenate([np.abs(poles), np.abs(zeros)])
    gains = adaptive_gain_vector(1000, 10000, critical_gains)

    # Calculate root locus using control.root_locus
    roots, klist = control.root_locus(control.TransferFunction(num_ol, den_ol), kvect=gains, plot=False)

    angles, centroid = find_asymptotes(poles, zeros)

    grouped_locus = {}
    for k_idx, gain in enumerate(klist):
        for pole_idx, pole in enumerate(roots[k_idx]):
            if pole_idx not in grouped_locus:
                grouped_locus[pole_idx] = {'real': [], 'imag': [], 'gain': []}
            grouped_locus[pole_idx]['real'].append(pole.real)
            grouped_locus[pole_idx]['imag'].append(pole.imag)
            grouped_locus[pole_idx]['gain'].append(gain)

    response = {
        "grouped_locus": grouped_locus,
        "poles_real": poles.real.tolist(),
        "poles_imag": poles.imag.tolist(),
        "zeros_real": zeros.real.tolist(),
        "zeros_imag": zeros.imag.tolist(),
        "angles": angles,
        "centroid": centroid
    }
    return jsonify(response)

@app.route('/bode_plot', methods=['POST'])
def get_bode_plot_data():
    """
    Handle POST requests to generate Bode plot data.
    
    Returns:
    JSON: Frequency, magnitude, phase, gain crossover frequency, phase crossover frequency, gain margin, and phase margin.
    """
    data = request.get_json()
    A = np.array(data['A'])
    B = np.array(data['B'])
    C = np.array(data['C'])
    D = np.array(data['D'])
    Kp = data['Kp']
    Ki = data['Ki']
    Kd = data['Kd']
    
    ss_system = control.ss(A, B, C, D)
    tf_system = control.ss2tf(ss_system)

    # Determine PID transfer function numerator and denominator
    if Ki == 0 and Kd == 0:
        num_pid = [Kp]
        den_pid = [1]
    elif Ki == 0:
        num_pid = [Kd, Kp]
        den_pid = [1]
    elif Kd == 0:
        num_pid = [Kp, Ki]
        den_pid = [1, 0]
    else:
        num_pid = [Kd, Kp, Ki]
        den_pid = [1, 0]

    num_ol = np.polymul(num_pid, tf_system.num[0][0])
    den_ol = np.polymul(den_pid, tf_system.den[0][0])

    system = signal.TransferFunction(num_ol, den_ol)
    w, mag, phase = signal.bode(system,n=1000)
    
    gain_cross_freq = -1
    phase_cross_freq = -1
    
    for i in range(len(mag) - 1):
        if mag[i] < 0 < mag[i + 1]:
            gain_cross_freq = w[i]
        if phase[i] > -180 > phase[i + 1]:
            phase_cross_freq = w[i]

    gain_margin = -1
    phase_margin = -1

    for i in range(len(phase) - 1):
        if phase[i] > -180 > phase[i + 1]:
            gm = 1 / np.abs(system.freqresp(w[i])[1])
            if isinstance(gm, np.ndarray):
                gm = gm[0]
            gain_margin = gm
            phase_margin = 180 + phase[i]

    response = {
        "frequency": w.tolist(),
        "magnitude": mag.tolist(),
        "phase": phase.tolist(),
        'gain_cross_freq': gain_cross_freq,
        'phase_cross_freq': phase_cross_freq,
        'gain_margin': gain_margin,
        'phase_margin': phase_margin
    }
    return jsonify(response)

@app.route('/step_response', methods=['POST'])
def get_step_response_data():
    """
    Handle POST requests to generate step response data.
    
    Returns:
    JSON: Time, response, overshoot, rise time, settling time, and error.
    """
    data = request.get_json()
    A = np.array(data['A'])
    B = np.array(data['B'])
    C = np.array(data['C'])
    D = np.array(data['D'])
    Kp = data['Kp']
    Ki = data['Ki']
    Kd = data['Kd']

    ss_system = control.ss(A, B, C, D)
    tf_system = control.ss2tf(ss_system)

    # Determine PID transfer function numerator and denominator
    if Ki == 0 and Kd == 0:
        num_pid = [Kp]
        den_pid = [1]
    elif Ki == 0:
        num_pid = [Kd, Kp]
        den_pid = [1]
    elif Kd == 0:
        num_pid = [Kp, Ki]
        den_pid = [1, 0]
    else:
        num_pid = [Kd, Kp, Ki]
        den_pid = [1, 0]

    num_ol = np.polymul(num_pid, tf_system.num[0][0])
    den_ol = np.polymul(den_pid, tf_system.den[0][0])
    
    num_cl = num_ol
    den_cl = np.polyadd(den_ol, num_ol)

    system = control.TransferFunction(num_cl, den_cl)
    
    t = np.linspace(0, 800, 1000)  # Adjust the time range and resolution as needed
    
    # Generate the reference input signal
    reference_signal = np.full_like(t, 10)
    
    # Simulate the closed-loop system response with a reference input
    t, response = control.forced_response(system, T=t, U=reference_signal)
    
    overshoot_ = overshoot(response)
    rise_time_ = rise_time(t, response)
    settling_time_ = settling_time(t, response)
    error = abs(response[-1] - 10)
    
    response_data = {
        "time": t.tolist(),
        "response": response.tolist(),
        "overshoot": overshoot_,
        "rise_time": rise_time_,
        "settling_time": settling_time_,
        "error": error
    }
    return jsonify(response_data)

@app.route('/closed_loop_poles', methods=['POST'])
def get_closed_loop_poles():
    """
    Handle POST requests to calculate closed-loop poles.
    
    Returns:
    JSON: Real and imaginary parts of the closed-loop poles.
    """
    data = request.get_json()
    A = np.array(data['A'])
    B = np.array(data['B'])
    C = np.array(data['C'])
    D = np.array(data['D'])
    Kp = data['Kp']
    Ki = data['Ki']
    Kd = data['Kd']
    gain = data['gain']

    ss_system = control.ss(A, B, C, D)
    tf_system = control.ss2tf(ss_system)

    # Determine PID transfer function numerator and denominator
    if Ki == 0 and Kd == 0:
        num_pid = [Kp]
        den_pid = [1]
    elif Ki == 0:
        num_pid = [Kd, Kp]
        den_pid = [1]
    elif Kd == 0:
        num_pid = [Kp, Ki]
        den_pid = [1, 0]
    else:
        num_pid = [Kd, Kp, Ki]
        den_pid = [1, 0]

    poles = closed_loop_poles(tf_system.num[0][0], tf_system.den[0][0], num_pid, den_pid, gain)

    response = {
        "poles_real": poles.real.tolist(),
        "poles_imag": poles.imag.tolist()
    }
    return jsonify(response)

@app.route('/calculate-lqr', methods=['POST'])
def calculate_lqr():
    """
    Handle POST requests to calculate LQR controller.
    
    Returns:
    JSON: LQR gain matrix, time response, control input.
    """
    data = request.get_json()

    A = np.array(data['A'])
    B = np.array(data['B'])
    C = np.array(data['C'])
    D = np.array(data['D'])
    Q = np.array(data['Q'])
    R = np.array(data['R'])
    
    # Define sampling time
    Ts = 0.001

    # Augment the system
    try:
        A_aug, B_aug, C_aug = augment_system(A, B, C)
    except ValueError as e:
        print("Error augmenting system:", e)
        return jsonify({'error': str(e)}), 400

    # Print the augmented matrices
    print("Augmented Matrix A:", A_aug)
    print("Augmented Matrix B:", B_aug)
    
      # Discretize the augmented system using ZOH
    sys_cont_aug = control.ss(A_aug, B_aug, C_aug, D)
    sys_disc_aug = control.c2d(sys_cont_aug, Ts, method='zoh')
    
    A_d = sys_disc_aug.A
    B_d = sys_disc_aug.B
    C_d = sys_disc_aug.C
    D_d = sys_disc_aug.D
    
    print("Discrete Matrix A:", A_d)
    print("Discrete Matrix B:", B_d)
    print("Discrete Matrix C:", C_d)
    print("Discrete Matrix D:", D_d)

    
    # Ensure Q matches the dimensions of A_aug
    if Q.shape[0] != A_aug.shape[0] or Q.shape[1] != A_aug.shape[1]:
        return jsonify({'error': 'Matrix Q dimensions do not match augmented system dimensions'}), 400

    try:
        # Calculate the LQR gain K
        K, S, E = control.dlqr(A_d, B_d, Q, R)
    except linalg.LinAlgError as e:
        print("Error in LQR calculation:", e)
        return jsonify({'error': 'Failed to find a finite solution in LQR calculation.'}), 400
    print("K:", K)
    
    C_aug = np.hstack((C, np.zeros((C.shape[0], A_aug.shape[0] - C.shape[1]))))
    
    A_d = sys_disc_aug.A
    B_d = sys_disc_aug.B
    C_d = sys_disc_aug.C
    D_d = sys_disc_aug.D

    # Define the closed-loop system matrices
    A_cl = A_aug - B_aug @ K
    B_cl = np.vstack((np.zeros((A.shape[0], 1)), np.array([[1]])))  # To handle reference input
    C_cl = C_aug
    D_cl = D

    # Create the closed-loop state-space system
    sys_cl = control.ss(A_cl, B_cl, C_cl, D_cl)

    # Define the magnitude of the reference input
    reference_magnitude = 10

    # Simulate the step response with reference input of 20
    T = np.linspace(0, 500, 100)  # time vector
    U = np.ones_like(T) * reference_magnitude  # step input of 20

    # Simulate the response
    T, y, x = control.forced_response(sys_cl, T, U, return_x=True)
    
    U_control = -K @ x
    U_control = U_control.flatten()
    """ print(y[-1])
    print(U_control.T[-1]) """
    
    overshoot_ = overshoot(y)
    rise_time_ = rise_time(T, y)
    settling_time_ = settling_time(T, y)
    error = abs(y[-1] - reference_magnitude)
    
    response = {
            "Kx1": K[0][0],
            "Kx2": K[0][1],
            "Kx3": K[0][2],
            "Ki":  K[0][3],
            "time": T.tolist(),
            "y": y.tolist(),
            "u": U_control.tolist(),
            "overshoot": overshoot_,
            "rise_time": rise_time_,
            "settling_time": settling_time_,
            "error": error
    }
    return jsonify(response)


@app.route('/calculate-lqr-only', methods=['POST'])
def calculate_lqr_only():
    """
    Handle POST requests to calculate LQR controller.
    
    Returns:
    JSON: LQR gain matrix.
    """
    data = request.get_json()

    A = np.array(data['A'])
    B = np.array(data['B'])
    C = np.array(data['C'])
    D = np.array(data['D'])
    Q = np.array(data['Q'])
    R = np.array(data['R'])

    # Augment the system
    try:
        A_aug, B_aug, C_aug = augment_system(A, B, C)
    except ValueError as e:
        print("Error augmenting system:", e)
        return jsonify({'error': str(e)}), 400
    
    # Discretize the augmented system using ZOH
    sys_cont_aug = control.ss(A_aug, B_aug, C_aug, D)
    sys_disc_aug = control.c2d(sys_cont_aug, Ts, method='zoh')
    
    A_d = sys_disc_aug.A
    B_d = sys_disc_aug.B

    # Ensure Q matches the dimensions of A_aug
    if Q.shape[0] != A_aug.shape[0] or Q.shape[1] != A_aug.shape[1]:
        return jsonify({'error': 'Matrix Q dimensions do not match augmented system dimensions'}), 400

    try:
        # Calculate the LQR gain K
        K, S, E = control.dlqr(A_d, B_d, Q, R)
    except linalg.LinAlgError as e:
        print("Error in LQR calculation:", e)
        return jsonify({'error': 'Failed to find a finite solution in LQR calculation.'}), 400
    
    response = {
            "Kx1": K[0][0],
            "Kx2": K[0][1],
            "Kx3": K[0][2],
            "Ki":  K[0][3]
    }
    return jsonify(response)

def augment_system(A, B, C):
    """
    Augment the system matrices to include integral action.

    Args:
    A (numpy.ndarray): The system matrix.
    B (numpy.ndarray): The input matrix.
    C (numpy.ndarray): The output matrix.

    Returns:
    tuple: Augmented matrices (A_aug, B_aug, C_aug).
    """
    n = A.shape[0]
    m = B.shape[1]

    # Augmented A matrix
    A_aug = np.block([
        [A, np.zeros((n, 1))],
        [-C, np.zeros((1, 1))]
    ])

    # Augmented B matrix
    B_aug = np.vstack((B, np.zeros((1, m))))

    # Augmented C matrix
    C_aug = np.hstack((C, np.zeros((C.shape[0], 1))))
    return A_aug, B_aug, C_aug

def simulate_step_response(A, B, C, D, T=1000, n_steps=1000):
    """
    Simulate the step response of the closed-loop system.

    Args:
    A (numpy.ndarray): The system matrix.
    B (numpy.ndarray): The input matrix.
    C (numpy.ndarray): The output matrix.
    D (numpy.ndarray): The feedforward matrix.
    T (float): Simulation time.
    n_steps (int): Number of simulation steps.

    Returns:
    tuple: Time array and response array.
    """
    sys = control.ss(A, B, C, D)
    T = np.linspace(0, T, n_steps)
    T, y = control.step_response(sys, T)
    return T, y

@app.route('/calculate', methods=['POST'])
def calculate():
    """
    Handle POST requests to perform calculations.

    Returns:
    JSON: Dummy calculation results.
    """
    data = request.get_json()
    
    if not data:
        return jsonify({"error": "No data provided"}), 400
    
    for d in data:
        print(d)
    # Convert string dateTime to datetime objects
    for point in data:
        point['dateTime'] = datetime.fromisoformat(point['dateTime'])
    
    # Get the initial dateTime to calculate relative times
    initial_time = data[0]['dateTime']
    
    # Calculate relative times in seconds and prepare other data for processing
    processed_data = []
    for point in data:
        time_diff = point['dateTime'] - initial_time
        relative_time = time_diff.total_seconds()
        processed_data.append({
            'relativeTime': relative_time,
            'waterLevelTank2': point['waterLevelTank2']
        })
    
    # Create NumPy arrays
    relative_times = np.array([d['relativeTime'] for d in processed_data])
    water_levels_tank2 = np.array([d['waterLevelTank2'] for d in processed_data])
    # Perform your calculations here
    settlingTime = settling_time(relative_times,water_levels_tank2)
    riseTime = rise_time(relative_times,water_levels_tank2)
    overshoot_ = overshoot(water_levels_tank2)
    error = 0.0
    # For demonstration, we'll return some dummy data
    result = {
        'settling_time': settlingTime,
        'rise_time': riseTime,
        'overshoot': overshoot_,
        'error': error
    }

    return jsonify(result)

def calculate_rise_time(time, response):
    """
    Calculate the rise time of a response.

    Args:
    time (array): Time array.
    response (array): Response array.

    Returns:
    float: Rise time.
    """
    rise_time_index = np.where(response >= 0.9 * np.max(response))[0][0]
    rise_time = time[rise_time_index]
    return rise_time

def calculate_peak_time(time, response):
    """
    Calculate the peak time of a response.

    Args:
    time (array): Time array.
    response (array): Response array.

    Returns:
    float: Peak time.
    """
    peak_time_index = np.argmax(response)
    peak_time = time[peak_time_index]
    return peak_time

def calculate_gain_and_phase_margin(time, input_signal, output_signal):
    """
    Calculate the gain and phase margin of a system.

    Args:
    time (array): Time array.
    input_signal (array): Input signal array.
    output_signal (array): Output signal array.

    Returns:
    tuple: Gain margin and phase margin.
    """
    system = control.TransferFunction(output_signal, input_signal)
    gm, pm, wg, wp = control.margin(system)
    return gm, pm

def calculate_gain(input_signal, output_signal):
    """
    Calculate the gain of a system.

    Args:
    input_signal (array): Input signal array.
    output_signal (array): Output signal array.

    Returns:
    float: Gain.
    """
    delta_output = np.max(output_signal) - np.min(output_signal)
    delta_input = np.max(input_signal) - np.min(input_signal)
    gain = delta_output / delta_input if delta_input != 0 else float('inf')
    return gain

@app.route('/calculate2', methods=['POST'])
def calculate2():
    """
    Handle POST requests to perform specific calculations.

    Returns:
    JSON: Calculation results.
    """
    data = request.get_json()
    
    """ for d in data:
        #print(d)
    if not data:
        return jsonify({"error": "No data provided"}), 400 """
    
    # Convert string dateTime to datetime objects
    for point in data:
        point['dateTime'] = datetime.fromisoformat(point['dateTime'])
    
    # Get the initial dateTime to calculate relative times
    initial_time = data[0]['dateTime']
    
    # Calculate relative times in seconds and prepare other data for processing
    processed_data = []
    for point in data:
        time_diff = point['dateTime'] - initial_time
        relative_time = time_diff.total_seconds()
        processed_data.append({
            'relativeTime': relative_time,
            'waterLevelTank2': point['waterLevelTank2'],
            'waterLevelTank1': point['waterLevelTank1'],
            'inletFlow': point['inletFlow'],
            'valvePositionFeedback': point['valvePositionFeedback']
        })
    
    # Create NumPy arrays
    relative_times = np.array([d['relativeTime'] for d in processed_data])
    water_levels_tank1 = np.array([d['waterLevelTank1'] for d in processed_data])
    water_levels_tank2 = np.array([d['waterLevelTank2'] for d in processed_data])
    inlet_flows = np.array([d['inletFlow'] for d in processed_data])
    valve_positions = np.array([d['valvePositionFeedback'] for d in processed_data])
    
    # Print the arrays
    """print("Relative Times (in seconds):", relative_times)"""
    """print("Water Levels Tank 1:", water_levels_tank1)
    print("Water Levels Tank 2:", water_levels_tank2)
    print("Inlet Flows:", inlet_flows)
    print("Valve Positions:", valve_positions) """
    

    # Perform calculations
    rise_time_tank1 = calculate_rise_time(relative_times, water_levels_tank1)
    peak_time_tank1 = calculate_peak_time(relative_times, water_levels_tank1)
    rise_time_tank2 = calculate_rise_time(relative_times, water_levels_tank2)
    peak_time_tank2 = calculate_peak_time(relative_times, water_levels_tank2)
    rise_time_iflow = calculate_rise_time(relative_times, inlet_flows)
    peak_time_iflow = calculate_peak_time(relative_times, inlet_flows)
    
    gain_tank1 = calculate_gain(valve_positions, water_levels_tank1)
    gain_tank2 = calculate_gain(valve_positions, water_levels_tank2)
    gain_iflow = calculate_gain(valve_positions, inlet_flows * 1000/60)
    
    results = {
        'riseTimeTank1': rise_time_tank1,
        'peakTimeTank1': peak_time_tank1,
        'riseTimeTank2': rise_time_tank2,
        'peakTimeTank2': peak_time_tank2,
        'riseTimeIFlow': rise_time_iflow,
        'peakTimeIFlow': peak_time_iflow,
        'gainTank1': gain_tank1,
        'gainTank2': gain_tank2,
        'gainIFlow': gain_iflow,
        'relativeTimes': relative_times.tolist()  # Include relative times in the response
    }
    return jsonify(results)


if __name__ == '__main__':
    app.run(debug=True)
