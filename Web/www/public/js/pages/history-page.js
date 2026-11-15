import {deleteWorkout, getAllWorkouts, getWorkoutById} from "../api/workout-api.js";
import {withErrorToasts} from "../shared/ui.js";

const historyList = document.getElementById('history-list');
const historyEmpty = document.getElementById('history-empty');
const monthTemplate = document.getElementById('history-month-template');
const cardTemplate = document.getElementById('history-card-template');
const exerciseTemplate = document.getElementById('history-exercise-template');
const setTemplate = document.getElementById('history-set-template');

const historyDialog = document.getElementById('history-dialog');
const dialogTitle = document.getElementById('history-dialog-title');
const dialogMeta = document.getElementById('history-dialog-meta');
const dialogBody = document.getElementById('history-dialog-body');
const deleteButton = document.getElementById('history-delete-btn');

let openedWorkoutId = null;

setupDeleteButton();
await setupHistory();

function setupDeleteButton() {
    deleteButton.addEventListener('click', async () => {
        deleteButton.disabled = true;

        const deleted = await withErrorToasts(async () => {
            await deleteWorkout(openedWorkoutId);
            return true;
        });

        deleteButton.disabled = false;

        if (!deleted) return;

        historyDialog.close();
        await setupHistory();
    });
}

async function setupHistory() {
    const workouts = await withErrorToasts(getAllWorkouts);

    if (workouts == null) return;

    const completed = workouts.filter(w => w.endedAt != null);

    if (completed.length === 0) {
        historyList.classList.add('d-none');
        historyEmpty.classList.remove('d-none');
        return;
    }

    historyEmpty.classList.add('d-none');
    historyList.classList.remove('d-none');
    
    const months = new Map();

    for (const [index, workout] of completed.entries()) {
        const card = cardTemplate.content.firstElementChild.cloneNode(true);

        card.dataset.id = workout.id;
        card.querySelector('.history-card-number').textContent = completed.length - index;
        card.querySelector('.history-card-title').textContent = workout.title;
        card.querySelector('.history-card-date').textContent = formatWorkoutDate(workout.startedAt);
        card.querySelector('.history-card-duration').textContent = formatWorkoutDuration(workout);

        card.addEventListener('click', async () => await openWorkout(workout.id));

        const month = formatMonth(workout.startedAt);
        if (!months.has(month)) {
            months.set(month, []);
        }
        months.get(month).push(card);
    }

    const sections = [];

    for (const [month, cards] of months) {
        const section = monthTemplate.content.firstElementChild.cloneNode(true);
        section.querySelector('.history-month-title').textContent = month;
        section.querySelector('.history-month-count').textContent = String(cards.length);
        section.querySelector('.history-month-cards').replaceChildren(...cards);
        sections.push(section);
    }

    historyList.replaceChildren(...sections);
}

async function openWorkout(id) {
    const workout = await withErrorToasts(() => getWorkoutById(id));

    if (workout == null) return;

    openedWorkoutId = workout.id;
    renderWorkoutDialog(workout);
    historyDialog.showModal();
}

function renderWorkoutDialog(workout) {
    dialogTitle.textContent = workout.title;
    dialogMeta.textContent = `${formatWorkoutDate(workout.startedAt)} · ${formatWorkoutDuration(workout)}`;

    dialogBody.scrollTop = 0;

    if (workout.exercises.length === 0) {
        dialogBody.replaceChildren(createEmptyMessage('history-dialog-empty', 'No exercises in this workout'));
        return;
    }

    const cards = [];

    for (const [exerciseIndex, exercise] of workout.exercises.entries()) {
        const exerciseCard = exerciseTemplate.content.firstElementChild.cloneNode(true);
        exerciseCard.dataset.id = exercise.id;
        exerciseCard.querySelector('.exercise-number').textContent = exerciseIndex + 1;
        exerciseCard.querySelector('.exercise-name').textContent = exercise.exerciseTitle;

        const setsList = exerciseCard.querySelector('.sets');

        // Без подходов строка заголовков колонок не нужна, вместо таблицы показываем надпись
        if (exercise.sets.length === 0) {
            setsList.replaceChildren(createEmptyMessage('sets-empty', 'No sets'));
        }

        for (const [setIndex, set] of exercise.sets.entries()) {
            const setRow = setTemplate.content.firstElementChild.cloneNode(true);
            setRow.dataset.id = set.id;
            setRow.querySelector('.set-number').textContent = setIndex + 1;
            setRow.querySelector('.set-weight').textContent = set.weight;
            setRow.querySelector('.set-reps').textContent = set.repeats;

            setsList.append(setRow);
        }

        cards.push(exerciseCard);
    }

    dialogBody.replaceChildren(...cards);
}

function createEmptyMessage(className, text) {
    const message = document.createElement('p');
    message.className = className;
    message.textContent = text;
    return message;
}

// Название месяца вместе с годом служит и заголовком секции, и ключом группировки:
// ноябрь 2026 и ноябрь 2025 получаются разными группами
function formatMonth(startedAt) {
    return new Date(startedAt).toLocaleString('en-US', {
        month: 'long',
        year: 'numeric'
    });
}

function formatWorkoutDate(startedAt) {
    return new Date(startedAt).toLocaleString('en-US', {
        dateStyle: 'medium',
        timeStyle: 'short'
    });
}

function formatWorkoutDuration(workout) {
    return formatDuration(new Date(workout.endedAt) - new Date(workout.startedAt));
}

function formatDuration(milliseconds) {
    const totalMinutes = Math.floor(milliseconds / 60000);

    if (totalMinutes < 1) return '< 1 min';

    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    if (hours === 0) return `${minutes} min`;
    if (minutes === 0) return `${hours} h`;
    return `${hours} h ${minutes} min`;
}
