file_path = 'eksperiment.csv';
data = readmatrix(file_path, 'Delimiter', ';');
sampling_interval = 10;
time = data(1:sampling_interval:end, 1);
h1 = data(1:sampling_interval:end, 2); 
qu = data(1:sampling_interval:end, 3) * (1000 / 60); % Convert from L/min to cm^3/s
h2 = data(1:sampling_interval:end, 4); 
qi = data(1:sampling_interval:end, 5) * (1000 / 60); % Convert from L/min to cm^3/s
pv = data(1:sampling_interval:end, 6);

sample_num = (1:length(h1))';

% Combine the data into a matrix with sample numbers as the first column
data = [sample_num, h1, h2, qu, pv];

% Define the filename
filename = 'Copy_of_simulation_output.txt';

% Save the data to a text file using save
save(filename, 'data', '-ascii');

% Alternatively, you can use writematrix for better control over the output format
writematrix(data, filename, 'Delimiter', ',');


%% RLS algoithm
close all;
sys_disc_zoh = c2d(ssModel, Ts, 'zoh');
sys_disc_zoh.A
sys_disc_zoh.B


windowSizeMA = 10; % Moving average window size
h1= movmedian(h1, windowSizeMA); 
h2= movmedian(h2, windowSizeMA); 
qu= movmedian(qu, 2); 
u= pv;

% Number of samples
N = length(h1);

% System dimensions
n = size(sys_disc_zoh.A, 1); % number of states (3)
m = size(sys_disc_zoh.B, 2); % number of inputs (1)
p=3;  % number of outputs (3)


%theta = [0.7502,0.0009,0.9579,0.0405,0.0405 ,0.9541,0.8122];
theta = 0.1 * randn(1,7);

P = eye(7) * 10000;          % 
lambda = 1;               % 
% Define the weighting factor 
alpha_weight = 0.7;

%window_size = 100; % Define the window size for moving window approach

% Combine outputs into one matrix
y = [qu, h1, h2]'; % Transpose to get a 3x8001 matrix

y_est_hist = zeros(3, N);
y_hat_k_prev = zeros(3, 1);%+ [qu0,h10,h20]'; % Initialize previous estimated output

% RLS algorithm simulation
for k = 2:N
    % Get current input and output
    y_k = y(:, k)'; % output vector at time k, dimensions 3x1

    % Form regression matrix \(\Phi(k)\) using previous estimated values
    phi_k_model = [
        y_hat_k_prev(1),0,0,0,0,0,u(k-1);
        0,y_hat_k_prev(1),y_hat_k_prev(2),y_hat_k_prev(3),0,0,0;
        0,0,0,0,y_hat_k_prev(2),y_hat_k_prev(3),0
    ]; % dimensions 3x7
     
    phi_k_real = [
        qu(k-1),0,0,0,0,0,u(k-1);
        0,qu(k-1),h1(k-1),h2(k-1),0,0,0;
        0,0,0,0,h1(k-1),h2(k-1),0
    ]; % dimensions 3x7
    % Prediction
    phi_k = alpha_weight * phi_k_model + (1 - alpha_weight) * phi_k_real;  % Combine regression matrices

    % Generate measurement noise (Gaussian noise based on real data characteristics)
    %measurement_noise = [std_qu_noise, std_h1_noise, std_h2_noise] .* randn(1, 3) + [mean_qu_noise, mean_h1_noise, mean_h2_noise];
    
    y_hat_k = theta * phi_k'; % + [qu0,h10,h20];

    y_est_hist(:, k) = y_hat_k';
    
    % Prediction error
    e_k = y_k - y_hat_k;

    % Update covariance matrix
    %K_k = (P * phi_k') / ((lambda * eye(3) + phi_k * P * phi_k'));
    K_k = (P * phi_k') / (lambda * eye(size(phi_k, 1)) + phi_k * P * phi_k');
    % Update parameters
    theta = theta + e_k * K_k';

    % Update covariance matrix
    %P = (P - K_k * phi_k * P) / lambda;
    P  = (P - K_k * phi_k * P) / lambda + 0.0 * eye(size(P));  % Add regularization term

    % Update previous estimated output
    y_hat_k_prev = y_hat_k;
end

% Extract estimated parameters
A_est = [theta(1), 0, 0; theta(2), theta(3), theta(4); 0, theta(5), theta(6)];
B_est = [theta(7); 0; 0];
C_est = sys_disc_zoh.C;
D_est = sys_disc_zoh.D;

estimated = ss(A_est, B_est, C_est, D_est, Ts);

% Convert to continuous-time systems
real_system = d2c(sys_disc_zoh, 'zoh');
estimated_system = d2c(estimated, 'zoh');

threshold = 1e-4;
estimated_system.A = apply_threshold(estimated_system.A, threshold);
estimated_system.B = apply_threshold(estimated_system.B, threshold);
estimated_system.C = apply_threshold(estimated_system.C, threshold);
estimated_system.D = apply_threshold(estimated_system.D, threshold);

real_time_const = -1 ./ eig(real_system.A)
estimated_time_const = -1 ./ eig(estimated_system.A)




% estimated_system
% real_system





% Function to apply threshold
function A_thresh = apply_threshold(A, threshold)
    A_thresh = A;
    A_thresh(abs(A) < threshold) = 0;
end