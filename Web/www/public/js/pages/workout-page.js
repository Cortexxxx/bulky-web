import {createWorkout, deleteWorkout, endCurrentWorkout, getCurrentWorkout} from "../api/workout-api.js";
import {withErrorToasts} from "../shared/ui.js";

const createWorkoutDialog = document.getElementById('create-workout-dialog');
const confirmDialog = document.getElementById('confirm-dialog');
const workoutPanel = document.getElementById('workout-panel');
const createWorkoutForm = document.getElementById('create-workout-form');
const startWorkoutButton = document.getElementById('workout-start');
const finishWorkoutButton = document.getElementById('finish-workout-btn');
const deleteWorkoutButton = document.getElementById('delete-workout-btn');
const workoutTitle = workoutPanel.querySelector('#workout-title');
let currentWorkout;

await setupWorkout();
await setupCreateDialog();

async function setupWorkout() {

    currentWorkout = await getCurrentWorkout();
    finishWorkoutButton.addEventListener('click',
        async (e) => setConfirmDialog(e, finishWorkoutButton, async (e) => await setFinishEvent(e)));
    deleteWorkoutButton.addEventListener('click',
        async (e) => setConfirmDialog(e, deleteWorkoutButton, async (e) => await setDeleteEvent(e)));
    startWorkoutButton.addEventListener('click', (e) => {
        e.preventDefault();
        createWorkoutForm.elements.title.value = new Date().toLocaleString('en-US', {
            dateStyle: 'medium',
            timeStyle: 'medium'
        });
        
        createWorkoutDialog.returnValue = "";
        createWorkoutDialog.showModal();
    });
    renderCurrentWorkout();
}
async function setupCreateDialog() {
    createWorkoutDialog.addEventListener('close', async (e) => {
        e.preventDefault();
        if (createWorkoutDialog.returnValue === 'start') {
            const title = createWorkoutForm.elements.title.value;
            await withErrorToasts(async () => await createWorkout(title));
            currentWorkout = await withErrorToasts(getCurrentWorkout);
            renderCurrentWorkout();
        }
    });
}

async function setConfirmDialog(e, button, callback) {
    confirmDialog.returnValue = "";
    confirmDialog.showModal();
    confirmDialog.querySelector('#confirm-dialog-title').textContent = button.dataset.confirmTitle;
    confirmDialog.querySelector('#confirm-dialog-text').textContent = button.dataset.confirmText;
    confirmDialog.querySelector('#confirm-dialog-submit').textContent = button.dataset.confirmLabel;
    confirmDialog.addEventListener('close', async (e) => callback(e), { once: true });
}

async function setFinishEvent(e) {
    e.preventDefault();
    if (confirmDialog.returnValue === 'confirm') {
        await withErrorToasts(endCurrentWorkout);
        currentWorkout = null;
        renderCurrentWorkout();
    }
}

async function setDeleteEvent(e) {
    e.preventDefault();
    if (confirmDialog.returnValue === 'confirm') {
        await withErrorToasts(() => deleteWorkout(currentWorkout.id));
        currentWorkout =    null;
        renderCurrentWorkout();
    }
}


function renderCurrentWorkout() {
    if (currentWorkout === null) {
        startWorkoutButton.classList.remove('d-none');
        workoutPanel.classList.add('d-none');
    } else {
        startWorkoutButton.classList.add('d-none');
        workoutPanel.classList.remove('d-none');
        workoutTitle.textContent = currentWorkout.title;
    }
}