import {
    createSet,
    createWorkout,
    createWorkoutExercise,
    deleteSet,
    deleteWorkout, deleteWorkoutExercise,
    endCurrentWorkout,
    getCurrentWorkout, getExerciseHint, updateSet
} from "../api/workout-api.js";
import {withErrorToasts} from "../shared/ui.js";
import {getAllExercises} from "../api/exercise-api.js";
import {getAllMuscles} from "../api/muscle-api.js";
import {searchByTitle} from "../shared/search.js";

const createWorkoutDialog = document.getElementById('create-workout-dialog');
const confirmDialog = document.getElementById('confirm-dialog');
const workoutPanel = document.getElementById('workout-panel');
const createWorkoutForm = document.getElementById('create-workout-form');

const startWorkoutButton = document.getElementById('workout-start');
const finishWorkoutButton = document.getElementById('finish-workout-btn');
const deleteWorkoutButton = document.getElementById('delete-workout-btn');
const workoutTitle = workoutPanel.querySelector('#workout-title');

const workoutTimer = document.getElementById('workout-timer');
let timerId = null;
const addExerciseButton = document.getElementById('add-exercise-btn');
const addExerciseDialog = document.getElementById('add-exercise-dialog');
const exerciseSearch = document.getElementById('exercise-search');
const exercisePickerList = document.getElementById("exercise-picker");
const pickerGroupTemplate = document.getElementById("picker-group-template");
const pickerItemTemplate = document.getElementById("picker-item-template");
const rowTemplate = document.getElementById("row-template");
const exerciseTemplate = document.getElementById('exercise-template');

const exerciseHintStyles = new Map([
    ['increase', {label: 'Increase weight', icon: 'M12 19V5m-6 6 6-6 6 6'}],
    ['hold', {label: 'Keep weight', icon: 'M5 12h14m-6-6 6 6-6 6'}],
    ['decrease', {label: 'Decrease weight', icon: 'M12 5v14m-6-6 6 6 6-6'}]
]);
const exerciseHintRequests = new Map();
let hintWorkoutId = null;

let currentWorkout = null;
let renderedSetIds = null;
let renderedExerciseIds = null;
updateTimer();
await setupWorkout();
await setupCreateDialog();
await setupExercisePicker();

async function setupWorkout() {
    
    currentWorkout = await getCurrentWorkout();
    timerId = setInterval(updateTimer, 1000);
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

function updateTimer() {
    if (currentWorkout === null) return;
    const elapsed = Date.now() - new Date(currentWorkout.startedAt);
    const seconds = Math.floor(elapsed / 1000) % 60;
    const minutes = Math.floor(elapsed / 1000 / 60) % 60;
    const hours = Math.floor(elapsed / 1000 / 60 / 60) % 24;
    // Часы показываем только когда они есть. Строка собирается в одну линию: переводы строк внутри шаблонной
    // строки попадали в текст и после двоеточий появлялись пробелы
    const parts = hours > 0 ? [hours, minutes, seconds] : [minutes, seconds];
    workoutTimer.textContent = parts.map(part => String(part).padStart(2, '0')).join(':');
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
    const workoutId = currentWorkout?.id ?? null;
    if (workoutId !== hintWorkoutId) {
        exerciseHintRequests.clear();
        hintWorkoutId = workoutId;
    }

    updateTimer();
    if (timerId !== null) {
        clearInterval(timerId);
    }
    
    if (currentWorkout === null) {
        startWorkoutButton.classList.remove('d-none');
        workoutPanel.classList.add('d-none');
    } else {
        timerId = setInterval(updateTimer, 1000);
        startWorkoutButton.classList.add('d-none');
        workoutPanel.classList.remove('d-none');
        workoutTitle.textContent = currentWorkout.title;
        
        renderWorkoutExercises();
    }
}

function renderWorkoutExercises() {
    const exerciseList = document.getElementById('exercise-list');
    exerciseList.replaceChildren();
    const currentSetIds = new Set();
    const currentExerciseIds = new Set();

    for (const [exerciseIndex, exercise] of currentWorkout.exercises.entries()) {
        const exerciseCard = exerciseTemplate.content.firstElementChild.cloneNode(true);
        exerciseCard.dataset.id = exercise.id;

        currentExerciseIds.add(exercise.id);
        if (renderedExerciseIds !== null && !renderedExerciseIds.has(exercise.id)) {
            exerciseCard.classList.add('exercise-card-new');
            exerciseCard.addEventListener('animationend', (e) => {
                if (e.target === exerciseCard) exerciseCard.classList.remove('exercise-card-new');
            }, {once: true});
        }

        exerciseCard.querySelector('.exercise-number').textContent = exerciseIndex + 1;
        exerciseCard.querySelector('.exercise-name').textContent = exercise.exerciseTitle;
        
        exerciseCard.querySelector('.add-set-btn').addEventListener('click', async (e) => {
            e.preventDefault();
            await withErrorToasts(() => createSet(exercise.id));
            await refreshCurrent();
        });
        
        exerciseCard.querySelector('.delete-exercise-btn').addEventListener('click', async (e) => {
            e.preventDefault();
            e.currentTarget.disabled = true;
            const deleted = await withErrorToasts(async () => {
                await deleteWorkoutExercise(exercise.id);
                return true;
            });
            if (deleted) await playRemoval(exerciseCard);
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
                e.currentTarget.disabled = true;
                const deleted = await withErrorToasts(async () => {
                    await deleteSet(set.id);
                    return true;
                });
                if (deleted) await playRemoval(setRow);
                await refreshCurrent();
            });

            const repeatsField = setRow.querySelector('.set-reps');
            const weightField = setRow.querySelector('.set-weight');

            repeatsField.value = set.repeats;
            weightField.value = set.weight;
            
            repeatsField.addEventListener('change', async () => {
                await withErrorToasts(() => updateSet(set.id, repeatsField.value, weightField.value));
            });
            
            weightField.addEventListener('change', async () => {
                await withErrorToasts(() => updateSet(set.id, repeatsField.value, weightField.value));
            });
            
            currentSetIds.add(set.id);
            
            if (renderedSetIds !== null && !renderedSetIds.has(set.id)) {
                setRow.classList.add('set-row-new');
                setRow.addEventListener('animationend', () => setRow.classList.remove('set-row-new'), {once: true});
            }

            rowsList.append(setRow);
        }

        exerciseList.append(exerciseCard);
        void renderExerciseHint(exerciseCard, exercise.exerciseId);
    }

    renderedSetIds = currentSetIds;
    renderedExerciseIds = currentExerciseIds;
}

async function renderExerciseHint(exerciseCard, exerciseId) {
    let request = exerciseHintRequests.get(exerciseId);
    if (!request) {
        request = withErrorToasts(() => getExerciseHint(exerciseId));
        exerciseHintRequests.set(exerciseId, request);
    }

    const hint = await request;
    if (exerciseHintRequests.get(exerciseId) !== request) return;
    if (!hint) {
        exerciseHintRequests.delete(exerciseId);
        return;
    }

    const style = exerciseHintStyles.get(hint?.status);
    if (!style || !exerciseCard.isConnected) return;

    const panel = exerciseCard.querySelector('.exercise-hint');
    const weight = hint.previousWeight.toLocaleString('en-US', {maximumFractionDigits: 2});
    const repeats = hint.lastSetsRepeats.join(' / ');

    panel.dataset.status = hint.status;
    panel.querySelector('.exercise-hint-icon').setAttribute('d', style.icon);
    panel.querySelector('.exercise-hint-label').textContent = style.label;
    panel.querySelector('.exercise-hint-history').textContent = `Last workout: ${weight} kg × ${repeats}`;
    panel.classList.remove('d-none');
}

function playRemoval(element) {
    if (matchMedia('(prefers-reduced-motion: reduce)').matches) return Promise.resolve();

    return new Promise((resolve) => {
        element.addEventListener('animationend', (e) => {
            if (e.target === element) resolve();
        });
        element.style.setProperty('--h', `${element.offsetHeight}px`);
        element.classList.add('removing');
    });
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

    const sortedGroups = muscleGroups.toSorted((a, b) => a.title.localeCompare(b.title));
    const renderMatches = () => renderExercisePicker(sortedGroups, grouped);
    exerciseSearch.addEventListener('input', renderMatches);
    addExerciseButton.addEventListener('click', e => {
        e.preventDefault();
        exerciseSearch.value = '';
        renderMatches();
        addExerciseDialog.returnValue = "";
        addExerciseDialog.showModal();
    });
}

function renderExercisePicker(muscleGroups, grouped) {
    const results = document.createDocumentFragment();

    for (const group of muscleGroups) {
        const groupExercises = searchByTitle(grouped.get(group.id) ?? [], exerciseSearch.value);
        if (groupExercises.length === 0) continue;
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

        results.append(groupNode);
    }

    exercisePickerList.replaceChildren(results);
}

async function refreshCurrent() {
    currentWorkout = await getCurrentWorkout();
    renderCurrentWorkout();
}
