import {createWorkout, getCurrentWorkout} from "../api/workout-api.js";

const createWorkoutDialog = document.getElementById('create-workout-dialog');
const endWorkoutDialog = document.getElementById('create-workout-dialog');
const deleteWorkoutDialog = document.getElementById('create-workout-dialog');
const workoutPanel = document.getElementById('workout-panel');
const createWorkoutForm = document.getElementById('create-workout-form');
const startWorkoutButton = document.getElementById('workout-start');
const workoutTitle = workoutPanel.querySelector('#workout-title');
let currentWorkout;

await setupWorkout();
await setupCreateDialog();

async function setupWorkout() {

    currentWorkout = await getCurrentWorkout();
    
    if (currentWorkout === null) {
        startWorkoutButton.classList.remove('d-none');
        workoutPanel.classList.add('d-none');
        startWorkoutButton.addEventListener('click', (e) => {
            e.preventDefault();
            createWorkoutForm.elements.title.value = new Date().toLocaleString('en-US', {
                dateStyle: 'medium',
                timeStyle: 'medium'
            });
            createWorkoutDialog.showModal();
            
        })
    } else {
        renderCurrentWorkout();
    }
}
async function setupCreateDialog() {
    createWorkoutDialog.addEventListener('close', async (e) => {
        e.preventDefault();
        if (createWorkoutDialog.returnValue === 'start') {
            const title = createWorkoutForm.elements.title.value;
            await createWorkout(title);
            currentWorkout = await getCurrentWorkout();
            renderCurrentWorkout();
        }
    })
}
function renderCurrentWorkout() {
    startWorkoutButton.classList.add('d-none');
    workoutPanel.classList.remove('d-none');
    workoutTitle.textContent = currentWorkout.title;
}