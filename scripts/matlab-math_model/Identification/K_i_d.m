% Load the data
file_path = 'qi.csv';
data = readmatrix(file_path, 'Delimiter', ';');

% Extract time and h2 columns
time = data(:,1);
h2 = data(:,2);

A=450;
g=981;

% Shift down the whole graph
if strcmp(file_path, 'qi.csv')
    h2 = h2 - h2(16084);
end;

% Initial condition
h0 = h2(1);

% Error function for K
E = @(K) sum((h2 - (sqrt(h0) - 0.5 * 1/A * K*sqrt(2*g) * time).^2).^2);

% Initial guess for K
K_initial = 1.0;

% Minimize the error function
K_optimal = fminsearch(E, K_initial);

% Display the optimal K
disp(['Optimal K: ', num2str(K_optimal)]);

% Calculate predicted values
h2_pred = (sqrt(h0) - 0.5 * 1/A * K_optimal*sqrt(2*g) * time).^2;

% Calculate MSE
mse = mean((h2 - h2_pred).^2);

% Calculate R²
SS_res = sum((h2 - h2_pred).^2);
SS_tot = sum((h2 - mean(h2)).^2);
R2 = 1 - (SS_res / SS_tot);

% Plot the results
figure;
plot(time, h2, 'b', 'DisplayName', 'Stvarni podatci');
hold on;
plot(time, h2_pred, 'r--', 'DisplayName', 'Model');
xlabel('Vrijeme (s)');
ylabel('Razina tekućine u drugom spremniku [cm]');
legend;
title(sprintf('Usporedba stvarnih podataka i modela\nMSE: %.4f, R²: %.4f, K: %.4f', mse, R2, K_optimal));
hold off;

% Print results
fprintf('Optimal K: %.4f\n', K_optimal);
fprintf('MSE: %.4f\n', mse);
fprintf('R²: %.4f\n', R2);
