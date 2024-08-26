import * as THREE from "https://cdn.skypack.dev/three@0.129.0/build/three.module.js";
import { OrbitControls } from "https://cdn.skypack.dev/three@0.129.0/examples/jsm/controls/OrbitControls.js";
import { GLTFLoader } from "https://cdn.skypack.dev/three@0.129.0/examples/jsm/loaders/GLTFLoader.js";


const container = document.getElementById('model-container');
const scene = new THREE.Scene();
scene.background = new THREE.Color(0xffffff);

const camera = new THREE.PerspectiveCamera(50, container.clientWidth / container.clientHeight, 0.01, 1000);
camera.position.z = -4.62;
camera.position.y = 4.185;
camera.position.x = -5.40;

let object;
let controls;

let desiredPosition_tankWater3;

const initialWaterHeightTank1 = 2.9;
let finalWaterHeightTank1 = 0.2;
const offsetX_tankWater1 = 0;
const offsetZ_tankWater1 = 0.5;
const offsetY_tankWater1 = 2 + initialWaterHeightTank1 / 2;

const initialWaterHeightTank2 = 2.9;
let finalWaterHeightTank2 = 0.3;
const offsetX_tankWater2 = 0;
const offsetZ_tankWater2 = -0.5;
const offsetY_tankWater2 = 2 + initialWaterHeightTank2 / 2;

const initialWaterHeightTank3 = 1;
const offsetX_tankWater3 = 0;
const offsetZ_tankWater3 = 0;
const offsetY_tankWater3 = initialWaterHeightTank3 / 2;

const waterMaterial1 = new THREE.MeshPhongMaterial({
    color: 0x0000ff,
    transparent: true,
    opacity: 0.5,
    shininess: 100,
    reflectivity: 0.6,
    refractionRatio: 0.98
});

const waterMaterial2 = new THREE.MeshPhongMaterial({
    color: 0x0000aa,
    transparent: true,
    opacity: 0.5,
    shininess: 100,
    reflectivity: 0.6,
    refractionRatio: 0.98
});

const cubeGeometry1 = new THREE.BoxGeometry(1.93, initialWaterHeightTank1, 0.97);
const tankWater1 = new THREE.Mesh(cubeGeometry1, waterMaterial1);
const cubeGeometry2 = new THREE.BoxGeometry(1.93, initialWaterHeightTank2, 0.97);
const tankWater2 = new THREE.Mesh(cubeGeometry2, waterMaterial2);
const cubeGeometry3 = new THREE.BoxGeometry(1.93, initialWaterHeightTank3, 0.97 * 2);
const tankWater3 = new THREE.Mesh(cubeGeometry3, waterMaterial1);

let desiredPosition_tankWater1;
let desiredPosition_tankWater2;

const loader = new GLTFLoader();
loader.load(
    '/dist/3D_model/maketa.glb',
    function (glb) {
        object = glb.scene;
        const modelInitialPosition = object.position.clone();

        desiredPosition_tankWater1 = modelInitialPosition.clone();
        desiredPosition_tankWater1.y += offsetY_tankWater1;
        desiredPosition_tankWater1.x += offsetX_tankWater1;
        desiredPosition_tankWater1.z += offsetZ_tankWater1;
        tankWater1.position.copy(desiredPosition_tankWater1);

        desiredPosition_tankWater2 = modelInitialPosition.clone();
        desiredPosition_tankWater2.y += offsetY_tankWater2;
        desiredPosition_tankWater2.x += offsetX_tankWater2;
        desiredPosition_tankWater2.z += offsetZ_tankWater2;
        tankWater2.position.copy(desiredPosition_tankWater2);

        desiredPosition_tankWater3 = modelInitialPosition.clone();
        desiredPosition_tankWater3.y += offsetY_tankWater3; // Adjust Y position based on height change
        desiredPosition_tankWater3.x += offsetX_tankWater3; // Move the cube on X-axis by offsetX units
        desiredPosition_tankWater3.z += offsetZ_tankWater3; // Move the cube on Z-axis by offsetZ units
        tankWater3.position.copy(desiredPosition_tankWater3);


        scene.add(object);
        scene.add(tankWater1);
        scene.add(tankWater2);
        scene.add(tankWater3);


        object.layers.set(0);
        tankWater1.layers.set(1);
        camera.layers.enable(1);
    },
    function (xhr) {
        console.log((xhr.loaded / xhr.total * 100) + '% loaded');
    },
    function (error) {
        console.error(error);
    }
);

const renderer = new THREE.WebGLRenderer({ antialias: true });
renderer.setSize(container.clientWidth, container.clientHeight);
container.appendChild(renderer.domElement);

const topLight = new THREE.DirectionalLight(0xffffff, 0.7);
topLight.position.set(600, 600, 600);
topLight.castShadow = true;
scene.add(topLight);

const topLight2 = new THREE.DirectionalLight(0xffffff, 0.6);
topLight2.position.set(700, 700, -700);
topLight2.castShadow = true;
scene.add(topLight2);

const topLight3 = new THREE.DirectionalLight(0xffffff, 0.5);
topLight3.position.set(-20, 10, 0);
topLight3.castShadow = true;
scene.add(topLight3);

controls = new OrbitControls(camera, renderer.domElement);

export function updateWaterLevel(tank, percentage) {
    if (tank === 1) {
        finalWaterHeightTank1 = (percentage / 100) * initialWaterHeightTank1;
        console.log(`Tank 1 water level updated to: ${percentage}%`);
    } else if (tank === 2) {
        finalWaterHeightTank2 = (percentage / 100) * initialWaterHeightTank2;
        console.log(`Tank 2 water level updated to: ${percentage}%`);
    }
}

function observeProgressElement(id, tank) {
    const progressElement = document.getElementById(`${id}-progress`);

    const observer = new MutationObserver((mutationsList) => {
        for (const mutation of mutationsList) {
            if (mutation.type === 'attributes' && mutation.attributeName === 'style') {
                const widthPercentage = parseFloat(progressElement.style.width);
                Console.log(widthPercentage);
                if (!isNaN(widthPercentage)) {
                    updateWaterLevel(tank, widthPercentage);
                }
            }
        }
    });

    observer.observe(progressElement, { attributes: true });
}



function animate() {

    requestAnimationFrame(animate);


    if (finalWaterHeightTank1 >= 0 && finalWaterHeightTank1 <= initialWaterHeightTank1) {
        tankWater1.scale.y = finalWaterHeightTank1 / initialWaterHeightTank1;
        tankWater1.position.y = desiredPosition_tankWater1.y + ((tankWater1.scale.y * initialWaterHeightTank1 - initialWaterHeightTank1) / 2);
    }

    if (finalWaterHeightTank2 >= 0 && finalWaterHeightTank2 <= initialWaterHeightTank2) {
        tankWater2.scale.y = finalWaterHeightTank2 / initialWaterHeightTank2;
        tankWater2.position.y = desiredPosition_tankWater2.y + ((tankWater2.scale.y * initialWaterHeightTank2 - initialWaterHeightTank2) / 2);
    }

    renderer.render(scene, camera);
}



document.getElementById('tank1-slider').addEventListener('input', function (event) {
    updateWaterLevel(1, event.target.value);
});

document.getElementById('tank2-slider').addEventListener('input', function (event) {
    updateWaterLevel(2, event.target.value);
});


//observeProgressElement('tank1', 1);
//observeProgressElement('tank2', 2);

window.addEventListener('resize', () => {
    camera.aspect = container.clientWidth / container.clientHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(container.clientWidth, container.clientHeight);
});

animate();
