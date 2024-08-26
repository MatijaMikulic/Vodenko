%% Parametri procesa
K1= 0.0582;
K2= 0.1232;
K3= 0.0702;
K4= 0.0203 - 0.007;

Tv = 1.74;   % s
Kv = 3.0519 + 0.3; % cm^3/s
Km = 6.25;   % %/mA
A1 = 450;    % cm^2
A2 = 450;    % cm^2
g = 981;     % cm/s^2
K1d  =   K4*A1/sqrt(2*g); % cm^2
K2d  =   K4*A1/sqrt(2*g); % cm^2
K12A =   K3*A1/sqrt(2*g); % cm^2
K12B =   K2*A1/sqrt(2*g); % cm^2
Ki   =   K1*A1/sqrt(2*g); % cm^2
x = -12.2;
Ts= 0.1;

%% Radna točka
h10=4.69;%12.7360%6.18;%4.69;
h20=3.39;%11.3950 %4.9;%3.39;
qu0=93;%120%120.7360%93;
delta_xv=25;
xv0= 25;


% Calculate constants C1, C2, C3
C1 = ((K12A + K12B) * g) / (sqrt(2 * g * (h10 - h20)));
C2 = K1d * g / sqrt(2 * g * h10);
C3 = K2d * g / sqrt(2 * g * h20) + Ki * g / sqrt(2 * g * (h20-x)) ;


%% State space representation
A = [-1/Tv, 0, 0;
     1/A1, -(C1 + C2)/A1, C1/A1;
     0, C1/A2, -(C1 + C3)/A2];

B = [Kv/Tv; 0; 0];
C = [0, 0, 1];
D = 0;

% Create state-space model
ssModel = ss(A, B, C, D);

% Calculate and display open-loop poles
openLoopPoles = eig(A);
disp('Open-loop poles:');
disp(openLoopPoles);

% Convert state-space to transfer function
[NUM, DEN] = ss2tf(A, B, C, D);

% Plot root locus
figure;
rlocus(ssModel);
title('Root Locus of the State-Space Model');

% Izračunavanje matrice upravljivosti
Ctrb = ctrb(ssModel);

% Provjera ranga matrice upravljivosti
rankCtrb = rank(Ctrb);

% Ispis rezultata
fprintf('Rang matrice upravljivosti: %d\n', rankCtrb);
if rankCtrb == size(A, 1)
    disp('Sustav je upravljiv.');
else
    disp('Sustav nije upravljiv.');
end

% Izračunavanje matrice opažljivosti
Obsv = obsv(ssModel);

% Provjera ranga matrice opažljivosti
rankObsv = rank(Obsv);

% Ispis rezultata
fprintf('Rang matrice opažljivosti: %d\n', rankObsv);
if rankObsv == size(A, 1)
    disp('Sustav je opažljiv.');
else
    disp('Sustav nije opažljiv.');
end

sys_disc_zoh = c2d(ssModel, 0.1, 'zoh');


%% Plot
% Plot h1

figure;
hold on;
plot(out.t, out.xv, 'r', 'LineWidth', 1.5);
xlabel('Time [s]');
ylabel('Xv [%]');
title('Valve openness');
hold off;


figure;
hold on;
plot(out.t, out.h1_nelin, 'r', 'LineWidth', 1.5);
plot(out.t, out.h1_lin, 'g', 'LineWidth', 1.5);
%plot(out.t, out.h1_lin1, 'k', 'LineWidth', 1.5);
xlabel('Time [s]');
ylabel('h1 [cm]');
title('Water level - Tank 1');
legend('Nonlinear Model h1', 'Linear model h1', 'RLS Estimated h1');
hold off;

% Plot h2
figure;
hold on;
plot(out.t, out.h2_nelin, 'r', 'LineWidth', 1.5);
plot(out.t, out.h2_lin, 'g', 'LineWidth', 1.5);
%plot(out.t, out.h2_lin1, 'k', 'LineWidth', 1.5);
xlabel('Time [s]');
ylabel('h2 [cm]');
title('Water level - Tank 2');
legend('Nonlinear Model h2', 'Linear model h2', 'RLS Estimated h2');
hold off;

% Plot qu
figure;
hold on;
plot(out.t, out.qu_nelin, 'r', 'LineWidth', 1.5);
plot(out.t, out.qu_lin, 'g', 'LineWidth', 1.5);
%plot(out.t, out.qu_lin1, 'k', 'LineWidth', 1.5);
xlabel('Time [s]');
ylabel('qu [cm^3/s]');
title('Inlet flow');
legend('Nonlinear Model qu', 'Linear model qu', 'RLS Estimated qu');
hold off;

