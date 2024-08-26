% Load the data
file_path = 'q12B-100.csv';
data = readmatrix(file_path, 'Delimiter', ';');

% Extract time and h1, h2 columns
A=450;
g=981;
time = data(:,1);
h1_data = data(:,2);
h2_data = data(:,3);

% Define the system of differential equations
differential_equations = @(t, h, K) [
    -K * 1/A * sqrt(2*g*(h(1) - h(2)));
     K * 1/A * sqrt(2*g*(h(1) - h(2)))
];

% Define the error function
E = @(K) computeError(K, time, h1_data, h2_data, differential_equations);

% Initial guess for K
K_initial = 1.0;

% Use fminsearch to minimize the error function
K_optimal = fminsearch(E, K_initial);

% Display the optimal K
disp(['Optimal K: ', num2str(K_optimal)]);

% Calculate predicted values using the optimal K
h0 = [h1_data(1), h2_data(1)];
[t, h] = ode45(@(t, h) differential_equations(t, h, K_optimal), time, h0);
h1_solution = interp1(t, h(:,1), time);
h2_solution = interp1(t, h(:,2), time);

% Calculate MSE
mse_h1 = mean((h1_data - h1_solution).^2);
mse_h2 = mean((h2_data - h2_solution).^2);
mse_total = mse_h1 + mse_h2;

% Calculate R² for h1
SS_res_h1 = sum((h1_data - h1_solution).^2);
SS_tot_h1 = sum((h1_data - mean(h1_data)).^2);
R2_h1 = 1 - (SS_res_h1 / SS_tot_h1);

% Calculate R² for h2
SS_res_h2 = sum((h2_data - h2_solution).^2);
SS_tot_h2 = sum((h2_data - mean(h2_data)).^2);
R2_h2 = 1 - (SS_res_h2 / SS_tot_h2);

% Plot the results
figure;
plot(time, h1_data, 'b', 'DisplayName', 'Stvarni h1 podaci');
hold on;
plot(time, h2_data, 'g', 'DisplayName', 'Stvarni h2 podaci');
plot(time, h1_solution, 'r--', 'DisplayName', 'Model h1');
plot(time, h2_solution, 'k--', 'DisplayName', 'Model h2');
xlabel('Vrijeme (s)');
ylabel('Razina tekućine [cm]');
legend;
title(sprintf('Usporedba stvarnih podataka i modela\nMSE: %.4f, R² h1: %.4f, R² h2: %.4f, K: %.4f', mse_total, R2_h1, R2_h2, K_optimal));
hold off;

% Print results
fprintf('Optimal K: %.4f\n', K_optimal);
fprintf('MSE (h1): %.4f\n', mse_h1);
fprintf('MSE (h2): %.4f\n', mse_h2);
fprintf('MSE (total): %.4f\n', mse_total);
fprintf('R² (h1): %.4f\n', R2_h1);
fprintf('R² (h2): %.4f\n', R2_h2);

% Function to compute the error
function error = computeError(K, time, h1_data, h2_data, differential_equations)
    % Initial conditions
    h0 = [h1_data(1), h2_data(1)];
    
    % Solve the system of differential equations
    [t, h] = ode45(@(t, h) differential_equations(t, h, K), time, h0);
    
    % Interpolate the solution to match the time points of the data
    h1_solution = interp1(t, h(:,1), time);
    h2_solution = interp1(t, h(:,2), time);
    
    % Compute the mean squared error
    error = sum((h1_data - h1_solution).^2 + (h2_data - h2_solution).^2);
end
