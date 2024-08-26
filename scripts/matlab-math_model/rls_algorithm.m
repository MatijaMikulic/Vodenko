close all;
%% Parametri procesa
K1= 0.0582;
K2= 0.1232;
K3= 0.0702;
K4= 0.0203 - 0.007;

Tv = 1.74;   % s
Kv = 3.0519 + 0.2; % cm^3/s
Km = 6.25;   % %/mA
A1 = 450;    % cm^2
A2 = 450;    % cm^2
g = 981;     % cm/s^2
K1d  =   K4*A1/sqrt(2*g); % cm^2
K2d  =   K4*A1/sqrt(2*g); % cm^2
K12A =   K3*A1/sqrt(2*g); % cm^2
K12B =   K2*A1/sqrt(2*g); % cm^2
Ki   =   K1*A1/sqrt(2*g); % cm^2
x = -12;
Ts=0.1;

% Radna točka
h10=4.69;
h20=3.39;
qu0=82;
delta_xv=25;
xv0= 25;

% Calculate constants C1, C2, C3
C1 = ((K12A + K12B) * g) / (sqrt(2 * g * (h10 - h20)));
C2 = K1d * g / sqrt(2 * g * h10);
C3 = K2d * g / sqrt(2 * g * h20) + Ki * g / sqrt(2 * g * (h20-x)) ;


% State space representation
A = [-1/Tv, 0, 0;
     1/A1, -(C1 + C2)/A1, C1/A1;
     0, C1/A2, -(C1 + C3)/A2];

B = [Kv/Tv; 0; 0];
C = [0, 0, 1];
D = 0;

% Create state-space model
ssModel = ss(A, B, C, D);



%% RLS algoithm
close all;
sys_disc_zoh = c2d(ssModel, Ts, 'zoh');
sys_disc_zoh.A
sys_disc_zoh.B
%data:
h1_ = out.h1_lin; % vector h1 with dimensions 4001x1
h2_ = out.h2_lin; % vector h2 with dimensions 4001x1
qu_ = out.qu_lin; % vector qu with dimensions 4001x1
u_ = out.xv_lin;  % input vector with dimensions 4001x1

mean_h1_noise = -0.0019;
std_h1_noise = 0.0926;
mean_h2_noise = -0.0017;
std_h2_noise = 0.0615;
mean_qu_noise = -0.0005;
std_qu_noise = 0.3605;

% Generating noise using normal distribution
noise_h1 = std_h1_noise * randn(size(h1_)) + mean_h1_noise;
noise_h2 = std_h2_noise * randn(size(h2_)) + mean_h2_noise;
noise_qu = std_qu_noise * randn(size(qu_)) + mean_qu_noise;
h1_ = h1_ + noise_h1;
h2_ = h2_ + noise_h2;
qu_ = qu_ + noise_qu;


windowSizeMA = 10; % Moving average window size
h1= movmean(h1_, windowSizeMA); 
h2= movmean(h2_, windowSizeMA); 
qu= movmean(qu_, windowSizeMA); 
u= u_;

% Number of samples
N = length(h1);

% System dimensions
n = size(sys_disc_zoh.A, 1); % number of states (3)
m = size(sys_disc_zoh.B, 2); % number of inputs (1)
p=3;  % number of outputs (3)


theta = [0.7502,0.0009,0.9579,0.0405,0.0405 ,0.9541,0.8122];
%theta = 0.1 * randn(1,7);

P = eye(7) * 1000;          % 
lambda = 1;               % 
% Define the weighting factor 
alpha_weight = 0.8;

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
    
    % Extract estimated parameters
    A_est = [theta(1), 0, 0; theta(2), theta(3), theta(4); 0, theta(5), theta(6)];
    B_est = [theta(7); 0; 0];
    C_est = sys_disc_zoh.C;
    D_est = sys_disc_zoh.D;

    estimated = ss(A_est, B_est, C_est, D_est, Ts);
    estimated_system = d2c(estimated, 'zoh');
    estimated_time_const = -1 ./ eig(estimated_system.A)

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

% Plotting comparison of outputs
t = (0:N-1) * Ts; % Time vector based on sample time

figure;

subplot(3,1,1);
plot(t, h1_, 'b'); % Plot real data with default LineWidth
hold on;
plot(t, y_est_hist(2,:), 'r--', 'LineWidth', 1.5); % Plot model data with thicker LineWidth
hold off;
legend('Real h1', 'Estimated h1');
title('Comparison of Real and Estimated Outputs for h1');

subplot(3,1,2);
plot(t, h2_, 'b'); % Plot real data with default LineWidth
hold on;
plot(t, y_est_hist(3,:), 'r--', 'LineWidth', 1.5); % Plot model data with thicker LineWidth
hold off;
legend('Real h2', 'Estimated h2');
title('Comparison of Real and Estimated Outputs for h2');

subplot(3,1,3);
plot(t, qu_, 'b'); % Plot real data with default LineWidth
hold on;
plot(t, y_est_hist(1,:), 'r--', 'LineWidth', 1.5); % Plot model data with thicker LineWidth
hold off;
legend('Real qu', 'Estimated qu');
title('Comparison of Real and Estimated Outputs for qu');


estimated_system
real_system





% Function to apply threshold
function A_thresh = apply_threshold(A, threshold)
    A_thresh = A;
    A_thresh(abs(A) < threshold) = 0;
end



