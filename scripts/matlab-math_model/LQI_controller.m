clear all:
close all;
%process_parameters;

Aa = [A, [0;0;0]; -C 0];
Ba = [B; 0];
Ca = [C, 0];
% Create the augmented state-space model
new_ssModel = ss(Aa, Ba, Ca, []);

% Define Q, R, and N matrices
R = 80;
Q = diag([1, 1, 2, 2]);
N = zeros(4, 1);

% Compute LQR gain matrix
[Kpoj, ~, ~] = lqr(new_ssModel, Q, R, N);

% Display the closed-loop poles
eig(Aa - Ba*Kpoj)

% Separate feedback gain and integral gain
Kx = Kpoj(:, 1:3) % Feedback gain
Kii = Kpoj(:, 4) % Integral gain

%% discrete

% Sampling time
T_s = 0.1;  % Define your sampling time

% Discretize the continuous-time state-space model
sys_c = ss(A, B, C, []);
sys_d = c2d(sys_c, T_s, 'zoh');  % Zero-order hold discretization

Ad = sys_d.A;
Bd = sys_d.B;
Cd = sys_d.C;

% Define the augmented discrete state-space model
Aa = [Ad, zeros(size(Ad, 1), 1); -Cd*T_s, 1];
Ba = [Bd; 0];
Ca = [Cd, 0];

% Define Q, R, and N matrices
R = 10;
Q = diag([1, 1, 3, 3]);
N = zeros(4, 1);

% Create the augmented state-space model
new_ssModel = ss(Aa, Ba, Ca, [], T_s);

% Compute LQR gain matrix
[Kpoj, ~, ~] = dlqr(Aa, Ba, Q, R, N);

% Display the closed-loop poles
disp('Closed-loop poles:');
disp(eig(Aa - Ba*Kpoj));

% Separate feedback gain and integral gain
Kx = Kpoj(:, 1:3); % Feedback gain
Kii = Kpoj(:, 4); % Integral gain

disp('Feedback gain Kx:');
disp(Kx);
disp('Integral gain Kii:');
disp(Kii);