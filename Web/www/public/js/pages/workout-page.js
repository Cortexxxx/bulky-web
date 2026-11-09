import {
    createSet,
    createWorkout,
    createWorkoutExercise,
    deleteSet,
    deleteWorkout, deleteWorkoutExercise,
    endCurrentWorkout,
    getCurrentWorkout
} from "../api/workout-api.js";
import {withErrorToasts} from "../shared/ui.js";
import {getAllExercises} from "../api/exercise-api.js";
import {getAllMuscles} from "../api/muscle-api.js";

const createWorkoutDialog = document.getElementById('create-workout-dialog');
const confirmDialog = document.getElementById('confirm-dialog');
const workoutPanel = document.getElementById('workout-panel');
const createWorkoutForm = document.getElementById('create-workout-form');

const startWorkoutButton = document.getElementById('workout-start');
const finishWorkoutButton = document.getElementById('finish-workout-btn');
const deleteWorkoutButton = document.getElementById('delete-workout-btn');
const workoutTitle = workoutPanel.querySelector('#workout-title');

const addExerciseButton = document.getElementById('add-exercise-btn');
const addExerciseDialog = document.getElementById('add-exercise-dialog');
const exercisePickerList = document.getElementById("exercise-picker");
const pickerGroupTemplate = document.getElementById("picker-group-template");
const pickerItemTemplate = document.getElementById("picker-item-template");
const rowTemplate = document.getElementById("row-template");
const exerciseTemplate = document.getElementById('exercise-template');

let currentWorkout;

await setupWorkout();
await setupCreateDialog();
await setupExercisePicker();

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

    addExerciseButton.addEventListener('click', async (e) => {
        e.preventDefault();

        addExerciseDialog.returnValue = "";
        addExerciseDialog.showModal();
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
        renderWorkoutExercises();
    }
}

function renderWorkoutExercises() {
    const exerciseList = document.getElementById('exercise-list');
    exerciseList.replaceChildren();

    for (const [exerciseIndex, exercise] of currentWorkout.exercises.entries()) {
        const exerciseCard = exerciseTemplate.content.firstElementChild.cloneNode(true);
        exerciseCard.dataset.id = exercise.id;
        exerciseCard.querySelector('.exercise-number').textContent = exerciseIndex + 1;
        exerciseCard.querySelector('.exercise-name').textContent = exercise.exerciseTitle;
        
        exerciseCard.querySelector('.add-set-btn').addEventListener('click', async (e) => {
            e.preventDefault();
            await withErrorToasts(() => createSet(exercise.id));
            await refreshCurrent();
        });
        
        exerciseCard.querySelector('.delete-exercise-btn').addEventListener('click', async (e) => {
            e.preventDefault();
            await withErrorToasts(() => deleteWorkoutExercise(exercise.id));
            await refreshCurrent();
        });

        const rowsList = exerciseCard.querySelector('.exercise-sets');

        for (const [setIndex, set] of exercise.sets.entries()) {
            const setRow = rowTemplate.content.firstElementChild.cloneNode(true);
            const setNumber = setIndex + 1;
            const weight = setRow.querySelector('.set-weight');
            const reps = setRow.querySelector('.set-reps');

            setRow.dataset.id = set.id;
            setRow.querySelector('.set-number').textContent = setNumber;
            weight.value = set.weight;
            weight.setAttribute('aria-label', `Weight, set ${setNumber}`);
            reps.value = set.repeats;
            reps.setAttribute('aria-label', `Reps, set ${setNumber}`);

            const deleteSetButton = setRow.querySelector('.delete-set-btn');
            deleteSetButton.setAttribute('aria-label', `Delete set ${setNumber}`);
            deleteSetButton.addEventListener('click', async (e) => {
                e.preventDefault();
                await withErrorToasts(() => deleteSet(set.id));
                await refreshCurrent();
            });

            rowsList.append(setRow);
        }

        exerciseList.append(exerciseCard);
    }
}

async function setupExercisePicker() {
    const exercises = await getAllExercises();
    const muscleGroups = await getAllMuscles();
    
    const grouped = new Map();
    
    for (const exercise of exercises) {
        const groupId = exercise.muscleGroups.find(g => g.role === "primary")?.muscleGroupId ?? "other";
        
        if (!grouped.has(groupId)) {
            grouped.set(groupId, []);
        }
        grouped.get(groupId).push(exercise);
    }

    for (const group of muscleGroups.toSorted((a, b) => a.title.localeCompare(b.title))) {
        const groupExercises = grouped.get(group.id);
        if (groupExercises === undefined) continue;
        const groupNode = pickerGroupTemplate.content.firstElementChild.cloneNode(true);
        groupNode.dataset.id = group.id;
        groupNode.querySelector('h3').textContent = group.title;

        for (const exercise of groupExercises) {
            const exerciseNode = pickerItemTemplate.content.firstElementChild.cloneNode(true);

            exerciseNode.dataset.id = exercise.id;
            exerciseNode.textContent = exercise.title;
            
            exerciseNode.addEventListener('click', async (e) => {
                e.preventDefault();
                await withErrorToasts(() => createWorkoutExercise(exercise.id));
                await refreshCurrent();
                addExerciseDialog.close();
            });
            
            groupNode.append(exerciseNode);
        }

        exercisePickerList.append(groupNode);
    }
}

async function refreshCurrent() {
    currentWorkout = await getCurrentWorkout();
    renderCurrentWorkout();
}