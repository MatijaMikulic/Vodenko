% Load your data (assuming out is already available in your workspace)
h1_ = out.h1_lin; % vector h1 with dimensions 4001x1
h2_ = out.h2_lin; % vector h2 with dimensions 4001x1
qu_ = out.qu_lin; % vector qu with dimensions 4001x1

% Parameters for Gaussian noise
mean_h1 = 0.0001;
std_h1 = 0.1114;
mean_h2 = 0.0001;
std_h2 = 0.0426;
mean_qu = -0.0003;
std_qu = 0.3673;

% Generate Gaussian noise
noise_h1 = mean_h1 + std_h1 .* randn(size(h1_));
noise_h2 = mean_h2 + std_h2 .* randn(size(h2_));
noise_qu = mean_qu + std_qu .* randn(size(qu_));

% Combine the original data with the generated noise
h1_combined = h1_ + noise_h1;
h2_combined = h2_ + noise_h2;
qu_combined = qu_ + noise_qu;

% Generate time vector
t = linspace(0, 10, length(h1_));

% Plot the results
figure;

subplot(3, 1, 1);
plot(t, h1_combined);
title('Modeled Noise in h1');
xlabel('Time (s)');
ylabel('Noise');

subplot(3, 1, 2);
plot(t, h2_combined);
title('Modeled Noise in h2');
xlabel('Time (s)');
ylabel('Noise');

subplot(3, 1, 3);
plot(t, qu_combined);
title('Modeled Noise in qu');
xlabel('Time (s)');
ylabel('Noise');

