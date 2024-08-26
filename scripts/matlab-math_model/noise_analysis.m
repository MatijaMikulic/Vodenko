% Close all open figures
close all;

% Load data from CSV file
file_path = 'eksperiment.csv';
data = readmatrix(file_path, 'Delimiter', ';');

% Sampling every 50th data point
sampling_interval = 50;
time = data(1:sampling_interval:end, 1);
h1 = data(1:sampling_interval:end, 2); 
qu = data(1:sampling_interval:end, 3) * (1000 / 60); % Convert from L/min to cm^3/s
h2 = data(1:sampling_interval:end, 4); 
qi = data(1:sampling_interval:end, 5) * (1000 / 60); % Convert from L/min to cm^3/s
pv = data(1:sampling_interval:end, 6);

% Define the time range of interest
start_time = 600;
end_time = 3000;

% Find indices corresponding to the time range
indices = time >= start_time & time <= end_time;

% Filter the data
time_filtered = time(indices);
h1_filtered = h1(indices);
h2_filtered = h2(indices);
qu_filtered = qu(indices);

% Different window sizes for smoothing
windowSizeMA_h1 = 30; % Adjust as needed
windowSizeMA_h2 = 30; % Adjust as needed
windowSizeMA_qu = 5; % Adjust as needed, considering faster dynamics

h1_smooth = movmean(h1_filtered, windowSizeMA_h1);
h2_smooth = movmean(h2_filtered, windowSizeMA_h2);
qu_smooth = movmean(qu_filtered, windowSizeMA_qu);

% Isolate noise by subtracting smoothed data
h1_noise = h1_filtered - h1_smooth;
h2_noise = h2_filtered - h2_smooth;
qu_noise = qu_filtered - qu_smooth;

% Detect and remove peaks in the qu_noise signal
threshold = 5 * std(qu_noise); % Set a threshold value based on standard deviation
peak_indices = find(abs(qu_noise) > threshold);

% Replace peaks with median of surrounding values
for i = 1:length(peak_indices)
    idx = peak_indices(i);
    if idx > 1 && idx < length(qu_noise)
        qu_noise(idx) = median(qu_noise(max(1, idx-5):min(length(qu_noise), idx+5)));
    end
end

% Plot the noise after removing peaks
figure;
subplot(3,1,1);
plot(time_filtered, h1_noise);
title('Šum - h1');
xlabel('Vrijeme [s]');
ylabel('Vrijednost [cm]');

subplot(3,1,2);
plot(time_filtered, h2_noise);
title('Šum - h2');
xlabel('Vrijeme [s]');
ylabel('Vrijednost [cm]');

subplot(3,1,3);
plot(time_filtered, qu_noise);
title('Šum - qu');
xlabel('Vrijeme [s]');
ylabel('Vrijednost [cm^3/s]');

% Calculate statistical properties of the noise
h1_noise_mean = mean(h1_noise);
h1_noise_std = std(h1_noise);
h2_noise_mean = mean(h2_noise);
h2_noise_std = std(h2_noise);
qu_noise_mean = mean(qu_noise);
qu_noise_std = std(qu_noise);

% Display statistical properties
disp('Statistička svojstva šuma:');
fprintf('h1 noise: mean = %.4f, std = %.4f\n', h1_noise_mean, h1_noise_std);
fprintf('h2 noise: mean = %.4f, std = %.4f\n', h2_noise_mean, h2_noise_std);
fprintf('qu noise: mean = %.4f, std = %.4f\n', qu_noise_mean, qu_noise_std);

% Plot histograms of the noise and overlay Gaussian distribution
figure;
subplot(3,1,1);
histogram(h1_noise, 50, 'Normalization', 'pdf'); 
hold on;
x = linspace(min(h1_noise), max(h1_noise), 100);
pdf = normpdf(x, h1_noise_mean, h1_noise_std);
plot(x, pdf, 'r-', 'LineWidth', 1.5);
title('Histogram šuma - h1 s Gaussovim prilagodbom.');
xlabel('');
ylabel('Frekvencija');
hold off;

subplot(3,1,2);
histogram(h2_noise, 50, 'Normalization', 'pdf'); 
hold on;
x = linspace(min(h2_noise), max(h2_noise), 100);
pdf = normpdf(x, h2_noise_mean, h2_noise_std);
plot(x, pdf, 'r-', 'LineWidth', 1.5);
title('Histogram šuma - h2 s Gaussovim prilagodbom.');
xlabel('Šum');
ylabel('Frekvencija');
hold off;

subplot(3,1,3);
histogram(qu_noise, 50, 'Normalization', 'pdf'); 
hold on;
x = linspace(min(qu_noise), max(qu_noise), 100);
pdf = normpdf(x, qu_noise_mean, qu_noise_std);
plot(x, pdf, 'r-', 'LineWidth', 1.5);
title('Histogram šuma - qu s Gaussovim prilagodbom.');
xlabel('');
ylabel('Frekvencija');
hold off;

% Perform frequency analysis using Fourier Transform
h1_noise_fft = fft(h1_noise);
h2_noise_fft = fft(h2_noise);
qu_noise_fft = fft(qu_noise);

% Frequency vector
Fs = 1 / mean(diff(time_filtered)); % Sampling frequency
N = length(time_filtered); % Number of data points
f = (0:N-1) * (Fs/N); % Frequency vector

% Plot Fourier Transform of the noise
figure;
subplot(3,1,1);
plot(f, abs(h1_noise_fft));
title('Frekvencijska analiza šuma - h1');
xlabel('Frekvencija (Hz)');
ylabel('Amplituda');

subplot(3,1,2);
plot(f, abs(h2_noise_fft));
title('Frekvencijska analiza šuma - h2');
xlabel('Frekvencija (Hz)');
ylabel('Amplituda');

subplot(3,1,3);
plot(f, abs(qu_noise_fft));
title('Frekvencijska analiza šuma - qu');
xlabel('Frekvencija (Hz)');
ylabel('Amplituda');

% Calculate signal power
P_signal_h1 = mean(h1_filtered .^ 2);
P_signal_h2 = mean(h2_filtered .^ 2);
P_signal_qu = mean(qu_filtered .^ 2);

% Calculate noise power
P_noise_h1 = mean(h1_noise .^ 2);
P_noise_h2 = mean(h2_noise .^ 2);
P_noise_qu = mean(qu_noise .^ 2);

% Calculate SNR in dB
SNR_h1 = 10 * log10(P_signal_h1 / P_noise_h1);
SNR_h2 = 10 * log10(P_signal_h2 / P_noise_h2);
SNR_qu = 10 * log10(P_signal_qu / P_noise_qu);

% Display the SNR values
disp('Signal-to-Noise Ratio (SNR):');
fprintf('SNR for h1: %.2f dB\n', SNR_h1);
fprintf('SNR for h2: %.2f dB\n', SNR_h2);
fprintf('SNR for qu: %.2f dB\n', SNR_qu);
