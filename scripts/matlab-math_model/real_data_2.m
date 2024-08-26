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
Ts=0.01;

% Radna točka
h10=4.69;
h20=3.39;
qu0=82;
delta_xv=25;
xv0= 25;

% SS model
C1 = ((K12A + K12B) * g) / (sqrt(2 * g * (h10 - h20)));
C2 = K1d * g / sqrt(2 * g * h10);
C3 = K2d * g / sqrt(2 * g * h20) + Ki * g / sqrt(2 * g * (h20-x));

A = [-1/Tv, 0, 0;
     1/A1, -(C1 + C2)/A1, C1/A1;
     0, C1/A2, -(C1 + C3)/A2];

B = [Kv/Tv; 0; 0];
C = [0, 0, 1];
D = 0;

% Create state-space model
ssModel = ss(A, B, C, D);

%% starni podatci i usporedba s modelom
close all;
file_path = 'eksperiment.csv';
data = readmatrix(file_path, 'Delimiter', ';');
% Učitani podatci
time = data(:, 1);
h1 = data(:, 2); 
qu = data(:, 3) * (1000 / 60); % Pretvorba iz L/min u cm^3/s
h2 = data(:, 4); 
qi = data(:, 5) * (1000 / 60); % Pretvorba iz L/min u cm^3/s
pv = data(:, 6); 

% Remove NaN values and use linear interpolation
h1 = fillmissing(h1, 'linear');
h2 = fillmissing(h2, 'linear');
qu = fillmissing(qu, 'linear');

% Adjust time
time = time - 85.34;

% Trim measured data to match the length of model data
n_trim = length(h1) - length(out.h1_nelin);
h1_trim = h1(end-length(out.h1_nelin)+1:end);
h2_trim = h2(end-length(out.h2_nelin)+1:end);
qu_trim = qu(end-length(out.qu_nelin)+1:end);



%% Plot
% Plot h1
figure;
plot(time, h1, 'b');
hold on;
plot(out.t, out.h1_nelin, 'r', 'LineWidth', 1.5);
plot(out.t, out.h1_lin, 'g', 'LineWidth', 1.5);
%plot(out.t, out.h1_lin1, 'k', 'LineWidth', 1.5);
xlabel('Time [s]');
ylabel('h1 [cm]');
title('Water level - Tank 1');
legend('Measured h1', 'Nonlinear Model h1', 'Linear model h1', 'RLS Estimated h1');
hold off;

% Plot h2
figure;
plot(time, h2, 'b');
hold on;
plot(out.t, out.h2_nelin, 'r', 'LineWidth', 1.5);
plot(out.t, out.h2_lin, 'g', 'LineWidth', 1.5);
%plot(out.t, out.h2_lin1, 'k', 'LineWidth', 1.5);
xlabel('Time [s]');
ylabel('h2 [cm]');
title('Water level - Tank 2');
legend('Measured h2', 'Nonlinear Model h2', 'Linear model h2', 'RLS Estimated h2');
hold off;

% Plot qu
figure;
plot(time, qu, 'b');
hold on;
plot(out.t, out.qu_nelin, 'r', 'LineWidth', 1.5);
plot(out.t, out.qu_lin, 'g', 'LineWidth', 1.5);
%plot(out.t, out.qu_lin1, 'k', 'LineWidth', 1.5);
xlabel('Time [s]');
ylabel('qu [cm^3/s]');
title('Inlet flow');
legend('Measured qu', 'Nonlinear Model qu', 'Linear model qu', 'RLS Estimated qu');
hold off;


%% Calculate MSE and R²
% Nonlinear model
mse_h1_nelin = mean((h1_trim - out.h1_nelin).^2);
r2_h1_nelin = 1 - sum((h1_trim - out.h1_nelin).^2) / sum((h1_trim - mean(h1_trim)).^2);

mse_h2_nelin = mean((h2_trim - out.h2_nelin).^2);
r2_h2_nelin = 1 - sum((h2_trim - out.h2_nelin).^2) / sum((h2_trim - mean(h2_trim)).^2);

mse_qu_nelin = mean((qu_trim - out.qu_nelin).^2);
r2_qu_nelin = 1 - sum((qu_trim - out.qu_nelin).^2) / sum((qu_trim - mean(qu_trim)).^2);

% Linear model
mse_h1_lin = mean((h1_trim - out.h1_lin).^2);
r2_h1_lin = 1 - sum((h1_trim - out.h1_lin).^2) / sum((h1_trim - mean(h1_trim)).^2);

mse_h2_lin = mean((h2_trim - out.h2_lin).^2);
r2_h2_lin = 1 - sum((h2_trim - out.h2_lin).^2) / sum((h2_trim - mean(h2_trim)).^2);

mse_qu_lin = mean((qu_trim - out.qu_lin).^2);
r2_qu_lin = 1 - sum((qu_trim - out.qu_lin).^2) / sum((qu_trim - mean(qu_trim)).^2);

% Display the results
results = table({'Nonlinear'; 'Linear'}, [mse_h1_nelin; mse_h1_lin], [r2_h1_nelin; r2_h1_lin], [mse_h2_nelin; mse_h2_lin], [r2_h2_nelin; r2_h2_lin], [mse_qu_nelin; mse_qu_lin], [r2_qu_nelin; r2_qu_lin], ...
    'VariableNames', {'Model', 'MSE_h1', 'R2_h1', 'MSE_h2', 'R2_h2', 'MSE_qu', 'R2_qu'});
disp(results);
