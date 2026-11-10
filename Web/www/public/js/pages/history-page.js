import {getAllWorkouts} from "../api/workout-api.js";
import {withErrorToasts} from "../shared/ui.js";

const historyList = document.getElementById('history-list');
const historyEmpty = document.getElementById('history-empty');
const cardTemplate = document.getElementById('history-card-template');
const historyDialog = document.getElementById('history-dialog');

await setupHistory();

async function setupHistory() {
    const workouts = await withErrorToasts(getAllWorkouts);

    // Запрос упал, тост с ошибкой уже показан: не выдаём это за «тренировок нет»
    if (workouts == null) return;

    // В истории только завершённые, у активной тренировки нет длительности.
    // Порядок «новые сверху» приходит из SQL (ORDER BY started_at DESC).
    const completed = workouts.filter(w => w.endedAt != null);

    if (completed.length === 0) {
        historyList.classList.add('d-none');
        historyEmpty.classList.remove('d-none');
        return;
    }

    historyEmpty.classList.add('d-none');
    historyList.classList.remove('d-none');

    const cards = [];

    for (const [index, workout] of completed.entries()) {
        const card = cardTemplate.content.firstElementChild.cloneNode(true);
        const startedAt = new Date(workout.startedAt);
        const endedAt = new Date(workout.endedAt);

        card.dataset.id = workout.id;
        // Самая старая тренировка получает номер 1
        card.querySelector('.history-card-number').textContent = completed.length - index;
        card.querySelector('.history-card-title').textContent = workout.title;
        card.querySelector('.history-card-date').textContent = startedAt.toLocaleString('en-US', {
            dateStyle: 'medium',
            timeStyle: 'short'
        });
        card.querySelector('.history-card-duration').textContent = formatDuration(endedAt - startedAt);

        card.addEventListener('click', () => openWorkout(workout.id));

        cards.push(card);
    }

    historyList.replaceChildren(...cards);
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

function openWorkout(id) {
    // TODO: когда будет GET /api/workouts/{id}, загрузить тренировку по id и заполнить модалку
    historyDialog.showModal();
}
