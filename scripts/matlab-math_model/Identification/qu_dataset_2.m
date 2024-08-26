clear all;
file_path = 'pv-step-flow2.csv';
data = readmatrix(file_path, 'Delimiter', ';');

% Učitani podatci
time = data(:, 1);
qu = data(:, 2) * (1000 / 60); % Pretvorba iz L/min u cm^3/s
step = data(:, 3);

% Interval u kojem gledam step pobudu i odziv
start_time = 15;
end_time = 25;

% Uzimam te podatke u tom intervalu
filtered_indices = (time >= start_time) & (time <= end_time);
filtered_time = time(filtered_indices);
filtered_qu = qu(filtered_indices);
filtered_step = step(filtered_indices);

% Gledam kada se dogodila promjena stepa pa da uzmem od toga trenutka
% (gledam odziv)
step_change_indices = find(diff(filtered_step) ~= 0);
if isempty(step_change_indices)
    error('No step change.');
end
step_change_time = filtered_time(step_change_indices(1) + 1);

% Još jednom radim mijenjam interval kako bi mi bila pobuda praktički u 0
filtered_indices = filtered_time >= step_change_time;
filtered_time = filtered_time(filtered_indices);
filtered_qu = filtered_qu(filtered_indices);

window_size = 10;
smoothed_qu = movmean(filtered_qu, window_size);

% K računam kao omjer promjene izlaza i ulaza
step_change = filtered_step(end) - filtered_step(step_change_indices(1));
output_change = smoothed_qu(end) - smoothed_qu(1);
K_v = output_change / step_change;

% Za T sam stavio da mi bude vrijeme kada odziv poprimi 63% vrijednosti
initial_value = smoothed_qu(1);
final_value = smoothed_qu(end);
value_63_percent = initial_value + 0.53 * (final_value - initial_value);

% Ovdje pronalazim vrijeme kada se postigne tih 63%
time_index = find(smoothed_qu >= value_63_percent, 1, 'first');
T_v = filtered_time(time_index) - filtered_time(1);

% Ovo je PT1 član u vremenskom obliku (rješenje diferencijalne jednadžbe) -
simulated_qu = initial_value + K_v * (1 - exp(-(filtered_time - filtered_time(1)) / T_v)) * step_change;

% MSE
mse = mean((smoothed_qu - simulated_qu).^2);

% R2
SS_res = sum((smoothed_qu - simulated_qu).^2);
SS_tot = sum((smoothed_qu - mean(smoothed_qu)).^2);
R2 = 1 - (SS_res / SS_tot);

% Rezultati
figure;
plot(filtered_time, smoothed_qu, 'b', 'DisplayName', 'Stvarni podatci');
hold on;
plot(filtered_time, simulated_qu, 'r--', 'DisplayName', 'PT1 Model Output');
xlabel('Vrijeme (s)');
ylabel('Ulazni protok ({cm}^3/s)');
legend;
title(sprintf('Usporedba PT1 Modela sa stvarnim podatcima\nMSE: %.4f, R2: %.4f', mse, R2));

hold off;

% K i T vrijednosti na grafu
dim = [0.2 0.5 0.3 0.3];
str = sprintf('K = %.4f {cm}^3/s\nT = %.4f s', K_v, T_v);
annotation('textbox', dim, 'String', str, 'FitBoxToText', 'on', 'BackgroundColor', 'w');

fprintf('K: %.4f {cm}^3/s\n', K_v);
fprintf('T: %.4f s\n', T_v);
fprintf('Mean Squared Error (MSE): %.4f\n', mse);
fprintf('R2 score: %.4f\n', R2);
