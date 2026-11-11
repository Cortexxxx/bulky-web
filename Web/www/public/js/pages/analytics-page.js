import {getOverview} from "../api/analytics-api.js";
import {withErrorToasts} from "../shared/ui.js";

const averageDurationValue = document.getElementById('average-duration-value');

await setupOverview();

async function setupOverview() {
    const overview = await withErrorToasts(getOverview);

    // Запрос упал, тост с ошибкой уже показан
    if (overview == null) return;

    setStat('workouts-week', overview.lastWeekWorkouts, overview.lastWeekWorkoutsIncrease);
    setStat('sets-week', overview.lastWeekSets, overview.lastWeekSetsIncrease);
    setStat('workouts-month', overview.lastMonthWorkouts, overview.lastMonthWorkoutsIncrease);
    setStat('sets-month', overview.lastMonthSets, overview.lastMonthSetsIncrease);

    averageDurationValue.textContent = formatDuration(overview.averageDurationMinutes);
}

// prefix это начало id: <prefix>-value и <prefix>-delta
function setStat(prefix, value, increase) {
    document.getElementById(`${prefix}-value`).textContent = value;

    const delta = document.getElementById(`${prefix}-delta`);
    delta.textContent = formatIncrease(increase);
    // Зелёная плашка только для роста, ноль и падение серые
    delta.classList.toggle('stat-delta-down', increase <= 0);
}

function formatIncrease(increase) {
    return increase > 0 ? `+${increase}` : String(increase);
}

// Подпись "min" уже есть в разметке: меньше часа "45", от часа "1 h 04"
function formatDuration(totalMinutes) {
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    if (hours === 0) return String(minutes);

    return `${hours} h ${String(minutes).padStart(2, '0')}`;
}
