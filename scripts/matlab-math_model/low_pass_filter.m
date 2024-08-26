close all;
filename = 'Data_20240823083258-test.xlsx';

% Load the data
data = readtable(filename);
data.DateTime = datetime(data.DateTime, 'InputFormat', 'yyyy-MM-dd HH:mm:ss.SSS');

% Define the reference datetime as the first entry in the DateTime column
referenceDateTime = data.DateTime(1);

% Calculate the relative time in seconds from the reference datetime
relativeTimeInSeconds = seconds(data.DateTime - referenceDateTime);

% Design a Butterworth low-pass filter for Water Level Tanks
filterOrderTank = 2;        % Filter order
cutoffFrequencyTank = 0.008;  % Cutoff frequency as a fraction of the Nyquist frequency

% Design the Butterworth filter for tanks
[bTank, aTank] = butter(filterOrderTank, cutoffFrequencyTank);

% Initialize variables for storing filtered data
filteredWaterLevelTank2 = zeros(size(data.WaterLevelTank2));
filteredWaterLevelTank1 = zeros(size(data.WaterLevelTank1));
filteredInletFlow = zeros(size(data.InletFlow));

% Apply the Butterworth filter online for WaterLevelTank2
for i = 1:length(data.WaterLevelTank2)
    if i == 1
        % Initialize the filter state
        [filteredWaterLevelTank2(i), zTank] = filter(bTank, aTank, data.WaterLevelTank2(i));
    else
        % Apply the filter to each new data point
        [filteredWaterLevelTank2(i), zTank] = filter(bTank, aTank, data.WaterLevelTank2(i), zTank);
    end
end

% Plotting the original and filtered data for WaterLevelTank2
figure;
hold on;
plot(relativeTimeInSeconds, data.WaterLevelTank2, 'b-', 'DisplayName', 'Original Water Level Tank 2');
plot(relativeTimeInSeconds, filteredWaterLevelTank2, 'r-', 'LineWidth', 2.0, 'DisplayName', 'Filtered Water Level Tank 2');
xlabel('Vrijeme [s]');
ylabel('Razina tekućine u 2. spremniku / Referentna vrijednost');
title('LQR - Praćenje referentne vrijednosti');
legend('show');
grid on;
hold off;

% Apply the Butterworth filter online for WaterLevelTank1
for i = 1:length(data.WaterLevelTank1)
    if i == 1
        % Initialize the filter state
        [filteredWaterLevelTank1(i), zTank] = filter(bTank, aTank, data.WaterLevelTank1(i));
    else
        % Apply the filter to each new data point
        [filteredWaterLevelTank1(i), zTank] = filter(bTank, aTank, data.WaterLevelTank1(i), zTank);
    end
end

% Plotting the original and filtered data for WaterLevelTank1
figure;
hold on;
plot(relativeTimeInSeconds, data.WaterLevelTank1, 'b-', 'DisplayName', 'Original Water Level Tank 1');
plot(relativeTimeInSeconds, filteredWaterLevelTank1, 'r-', 'LineWidth', 2.0, 'DisplayName', 'Filtered Water Level Tank 1');
xlabel('Vrijeme [s]');
ylabel('Razina tekućine u 1. spremniku / Referentna vrijednost');
title('LQR - Praćenje referentne vrijednosti - Tank 1');
legend('show');
grid on;
hold off;

% Design a different Butterworth low-pass filter for InletFlow
filterOrderInletFlow = 2;         % Lower filter order
cutoffFrequencyInletFlow = 0.05;  % Slightly higher cutoff frequency

% Design the Butterworth filter for InletFlow
[bInletFlow, aInletFlow] = butter(filterOrderInletFlow, cutoffFrequencyInletFlow);

% Apply the Butterworth filter online for InletFlow
for i = 1:length(data.InletFlow)
    if i == 1
        % Initialize the filter state
        [filteredInletFlow(i), zInletFlow] = filter(bInletFlow, aInletFlow, data.InletFlow(i));
    else
        % Apply the filter to each new data point
        [filteredInletFlow(i), zInletFlow] = filter(bInletFlow, aInletFlow, data.InletFlow(i), zInletFlow);
    end
end

% Plotting the original and filtered data for InletFlow
figure;
hold on;
plot(relativeTimeInSeconds, data.InletFlow, 'b-', 'DisplayName', 'Original Inlet Flow');
plot(relativeTimeInSeconds, filteredInletFlow, 'r-', 'LineWidth', 2.0, 'DisplayName', 'Filtered Inlet Flow');
xlabel('Vrijeme [s]');
ylabel('Inlet Flow / Referentna vrijednost');
title('LQR - Praćenje referentne vrijednosti - Inlet Flow');
legend('show');
grid on;
hold off;

