filename = 'Data_20240823083258-test.xlsx';
%filename = 'Data_20240823092300.xlsx';
data = readtable(filename);
data.DateTime = datetime(data.DateTime, 'InputFormat', 'yyyy-MM-dd HH:mm:ss.SSS');

% Define the reference datetime as the first entry in the DateTime column
referenceDateTime = data.DateTime(1);

% Calculate the relative time in seconds from the reference datetime
relativeTimeInSeconds = seconds(data.DateTime - referenceDateTime);

% Plotting the data
figure;
hold on;
plot(relativeTimeInSeconds, data.WaterLevelTank2, 'b-', 'DisplayName', 'Razina tekućine u 2. spremniku');
plot(relativeTimeInSeconds, data.Target, 'r--', 'DisplayName', 'Referentna vrijednost'); % Dashed line for Target
%plot(relativeTimeInSeconds, data.ValvePositionFeedback, 'g-', 'DisplayName', 'Otvorenost Ventila'); % Adding Model data to the plot
%plot(relativeTimeInSeconds, data.InletFlowNonLinModel, 'black-', 'DisplayName', 'Lin Model Data'); % Adding Model data to the plot
xlabel('Vrijeme [s]');
ylabel('Razina tekućine [cm]');
%ylabel('Otvorenost ventila [%]')
%title('Regulacija razine tekućine u spremniku');
title ('Regulacija razine tekućine u spremniku');
legend('show');
grid on;

start_index = 1;

% Define the continuous-time state-space model
A = [-0.0299243213669892 0.0256028069981396; 
     0.0520081207314797 -0.0595592322719047];
B = [0.00171378115848002; 0];
C = [0, 1];
D = 0;

% Convert to a discrete-time state-space model with a sample time of 0.1 seconds
ssModel = ss(A, B, C, D);
dssModel = c2d(ssModel, 0.1, 'zoh');

% Extract the discrete-time matrices
Ad = dssModel.A;
Bd = dssModel.B;

% Initialize the first values based on initial conditions
h10 = data.WaterLevelTank1(start_index);
h20 = data.WaterLevelTank2(start_index);
qu0 = data.InletFlow(start_index)*(1000/60);

h2Hist = zeros(1, length(data.WaterLevelTank2));
h1Hist = zeros(1, length(data.WaterLevelTank2));

% Initial state vector
x_current = [0; 0];
qu_current = qu0;
for i = start_index+1:length(data.WaterLevelTank2)
    % Input vector
    u_current = qu_current - qu0;
    
    % Calculate the next state using the discrete state-space model
    x_next = Ad * x_current + Bd * u_current;
    
    % Store the predictions in history arrays
    h1Hist(i) = x_next(1) + h10; % Predicted value for h1
    h2Hist(i) = x_next(2) + h20; % Predicted value for h2
    
    % Update the current state
    x_current = x_next;
    qu_current = data.InletFlow(i)*(1000/60);
end
%plot(relativeTimeInSeconds, h1Hist, 'black-', 'DisplayName', 'Estimirani model'); % Adding Model data to the plot

