import "chart";
import "signalr";
//import { updateWaterLevel } from './threeModule';

const data_h2 = JSON.parse(document.getElementById('data').innerHTML);
const data_h1 = JSON.parse(document.getElementById('data-h1').innerHTML);
const data_qu = JSON.parse(document.getElementById('data-qu').innerHTML);
const data_qi = JSON.parse(document.getElementById('data-qi').innerHTML);

const ctx_h2 = document.getElementById('lineChart-h2').getContext('2d');
const ctx_h1 = document.getElementById('lineChart-h1').getContext('2d');
const ctx_qu = document.getElementById('lineChart-qu').getContext('2d');
const ctx_qi = document.getElementById('lineChart-qi').getContext('2d');



const lineChart_h2 = new Chart(ctx_h2, {
    type: 'line',
    data: data_h2,
    options: {
        scales: {
            y: {
                suggestedMax: 10,
                suggestedMin: 1
            }
        }
    }
});

const lineChart_h1 = new Chart(ctx_h1, {
    type: 'line',
    data: data_h1,
    options: {
        scales: {
            y: {
                suggestedMax:10,
                suggestedMin: 1
            }
        }
    }
});

const lineChart_qu = new Chart(ctx_qu, {
    type: 'line',
    data: data_qu,
    options: {
        scales: {
            y: {
                suggestedMax: 200,
                suggestedMin: 1
            }
        }
    }
});

const lineChart_qi = new Chart(ctx_qi, {
    type: 'line',
    data: data_qi,
    options: {
        scales: {
            y: {
                suggestedMax: 200,
                suggestedMin: 1
            }
        }
        
    }
});




const connection = new signalR.HubConnectionBuilder()

    .withUrl(data_h2.url)
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start() {
    try {

        await connection.start();
        console.log("SignalR Connected.");
        

    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
}

connection.onclose(async () => {
    await start();
});

connection.on("updateData", function (dataPoints) {
    // Log dynamic data for debugging
    console.log("Received dynamic data:", dataPoints);

    console.log("Received dynamic data:", dataPoints);

    if (!dataPoints) {
        console.error("Received dynamicData is undefined or null");
        return;
    }

    // Check if all expected properties exist in dynamicData
    const requiredProperties = [
        'sample', 'waterLevelTank2', 'waterLevelTank2Model',
        'waterLevelTank1', 'waterLevelTank1Model',
        'inletFlow', 'inletFlowModel',
        'outletFlow', 'valvePositionFeedback'
    ];

    for (const property of requiredProperties) {
        if (!(property in dataPoints)) {
            console.error(`Property ${property} is missing in dynamicData`);
            return;
        }
    }
   
    lineChart_h2.data.labels.push(dataPoints.sample);
    lineChart_h2.data.datasets[0].data.push(dataPoints.waterLevelTank2);
    lineChart_h2.data.datasets[1].data.push(dataPoints.waterLevelTank2Model);
    if (lineChart_h2.data.labels.length > data_h2.limit) {
        lineChart_h2.data.labels.splice(0, 1);
        lineChart_h2.data.datasets[0].data.splice(0, 1);
        lineChart_h2.data.datasets[1].data.splice(0, 1);
    }
    lineChart_h2.update();

    // H1
    lineChart_h1.data.labels.push(dataPoints.sample);
    lineChart_h1.data.datasets[0].data.push(dataPoints.waterLevelTank1);
    lineChart_h1.data.datasets[1].data.push(dataPoints.waterLevelTank1Model);
    if (lineChart_h1.data.labels.length > data_h1.limit) {
        lineChart_h1.data.labels.splice(0, 1);
        lineChart_h1.data.datasets[0].data.splice(0, 1);
        lineChart_h1.data.datasets[1].data.splice(0, 1);
    }
    lineChart_h1.update();

    // QU
    lineChart_qu.data.labels.push(dataPoints.sample);
    lineChart_qu.data.datasets[0].data.push(dataPoints.inletFlow);
    lineChart_qu.data.datasets[1].data.push(dataPoints.inletFlowModel);
    if (lineChart_qu.data.labels.length > data_qu.limit) {
        lineChart_qu.data.labels.splice(0, 1);
        lineChart_qu.data.datasets[0].data.splice(0, 1);
        lineChart_qu.data.datasets[1].data.splice(0, 1);
    }
    lineChart_qu.update();

    // QI
    lineChart_qi.data.labels.push(dataPoints.sample);
    lineChart_qi.data.datasets[0].data.push(dataPoints.outletFlow);
    if (lineChart_qi.data.labels.length > data_qi.limit) {
        lineChart_qi.data.labels.splice(0, 1);
        lineChart_qi.data.datasets[0].data.splice(0, 1);
    }
    lineChart_qi.update();

    //updateChart(lineChart_h2, dataPoints.sample, dataPoints.waterLevelTank2, dataPoints.waterLevelTank2Model);
    //updateChart(lineChart_h1, dataPoints.sample, dataPoints.waterLevelTank1, dataPoints.waterLevelTank1Model);
    //updateChart(lineChart_qu, dataPoints.sample, dataPoints.inletFlow, dataPoints.inletFlowModel);
    //updateChart(lineChart_qi, dataPoints.sample, dataPoints.outletFlow);

    // Update progress bars and badges
    const tank1Percentage = calculatePercentage(dataPoints.waterLevelTank1, 40); // Assuming max value is 40 cm
    updateProgressAndBadge("tank1", tank1Percentage);


    const tank2Percentage = calculatePercentage(dataPoints.waterLevelTank2, 40); // Assuming max value is 40 cm
    updateProgressAndBadge("tank2", tank2Percentage);

    updateWaterLevel(tank1Percentage, tank2Percentage);


    const inletFlowPercentage = calculatePercentage(dataPoints.inletFlow, 1); // Assuming max value is 10
    updateProgressAndBadge("inletFlow", inletFlowPercentage);

    const outletFlowPercentage = calculatePercentage(dataPoints.outletFlow, 1); // Assuming max value is 10
    updateProgressAndBadge("outletFlow", outletFlowPercentage);

    const valvePositionPercentage = calculatePercentage(dataPoints.valvePositionFeedback, 1); // Assuming max value is 10
    updateProgressAndBadge("vp", valvePositionPercentage);


    // Update real-time statistics
    updateRealTimeStatistics(dataPoints);

    // Update model prediction accuracy
    updateModelPredictionAccuracy(dataPoints);

    // Update Model Data Comparison Table
    updateModelDataComparison(dataPoints);
});

function updateWaterLevel(tank1Percentage, tank2Percentage) {
    const tank1Slider = document.getElementById('tank1-slider');
    const tank1SliderValue = document.getElementById('tank1-slider-value');
    tank1Slider.value = tank1Percentage;
    tank1SliderValue.innerText = `${tank1Percentage}%`;

    const tank2Slider = document.getElementById('tank2-slider');
    const tank2SliderValue = document.getElementById('tank2-slider-value');
    tank2Slider.value = tank2Percentage;
    tank2SliderValue.innerText = `${tank2Percentage}%`;
}
function calculatePercentage(currentValue, maxValue) {
    return Math.round((currentValue / maxValue) * 100);
}

function updateProgressAndBadge(id, percentage) {
    const progressElement = document.getElementById(`${id}-progress`);
    const badgeElement = document.getElementById(`${id}-badge`);
    progressElement.style.width = `${percentage}%`;
    badgeElement.textContent = `${percentage}%`;
}

function updateChart(chart, label, realData, modelData = null) {
    chart.data.labels.push(label);
    chart.data.datasets[0].data.push(realData);
    if (modelData !== null) {
        chart.data.datasets[1].data.push(modelData);
    }
    if (chart.data.labels.length > chart.data.limit) {
        chart.data.labels.shift();
        chart.data.datasets[0].data.shift();
        if (modelData !== null) {
            chart.data.datasets[1].data.shift();
        }
    }
    chart.update();
}
function updateRealTimeStatistics(dataPoints) {
    document.getElementById('realtime-tank1').innerText = `${dataPoints.waterLevelTank1.toFixed(2)} cm`;
    document.getElementById('realtime-tank2').innerText = `${dataPoints.waterLevelTank2.toFixed(2)} cm`;
    document.getElementById('realtime-inletFlow').innerText = `${dataPoints.inletFlow.toFixed(2)} l/min`;
}

function updateModelPredictionAccuracy(dataPoints) {
    const tank1Accuracy = calculateAccuracy(dataPoints.waterLevelTank1, dataPoints.waterLevelTank1Model);
    const tank2Accuracy = calculateAccuracy(dataPoints.waterLevelTank2, dataPoints.waterLevelTank2Model);
    const inletFlowAccuracy = calculateAccuracy(dataPoints.inletFlow, dataPoints.inletFlowModel);

    document.getElementById('accuracy-tank1').innerText = `${tank1Accuracy.toFixed(2)}%`;
    document.getElementById('accuracy-tank2').innerText = `${tank2Accuracy.toFixed(2)}%`;
    document.getElementById('accuracy-inletFlow').innerText = `${inletFlowAccuracy.toFixed(2)}%`;
}

function calculateAccuracy(realValue, modelValue) {
    return 100 - Math.abs((realValue - modelValue) / realValue * 100);
}

function updateModelDataComparison(dataPoints) {
    document.getElementById('model-tank1-real').innerText = `${dataPoints.waterLevelTank1.toFixed(2)} cm`;
    document.getElementById('model-tank1-model').innerText = `${dataPoints.waterLevelTank1Model.toFixed(2)} cm`;
    document.getElementById('model-tank1-error').innerText = `${Math.abs(dataPoints.waterLevelTank1 - dataPoints.waterLevelTank1Model).toFixed(2)} cm`;

    document.getElementById('model-tank2-real').innerText = `${dataPoints.waterLevelTank2.toFixed(2)} cm`;
    document.getElementById('model-tank2-model').innerText = `${dataPoints.waterLevelTank2Model.toFixed(2)} cm`;
    document.getElementById('model-tank2-error').innerText = `${Math.abs(dataPoints.waterLevelTank2 - dataPoints.waterLevelTank2Model).toFixed(2)} cm`;

    document.getElementById('model-inletFlow-real').innerText = `${dataPoints.inletFlow.toFixed(2)} l/min`;
    document.getElementById('model-inletFlow-model').innerText = `${dataPoints.inletFlowModel.toFixed(2)} l/min`;
    document.getElementById('model-inletFlow-error').innerText = `${Math.abs(dataPoints.inletFlow - dataPoints.inletFlowModel).toFixed(2)} l/min`;

    document.getElementById('model-outletFlow-real').innerText = `${dataPoints.outletFlow.toFixed(2)} l/min`;
    document.getElementById('model-outletFlow-model').innerText = `N/A`;
    document.getElementById('model-outletFlow-error').innerText = `N/A`;
}




// Start the connection.
start().then(() => {
});


