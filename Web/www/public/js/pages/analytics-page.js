import {getMuscleGroupOverview, getOverview} from "../api/analytics-api.js";
import {withErrorToasts} from "../shared/ui.js";
import {getAllMuscles} from "../api/muscle-api.js";

const averageDurationValue = document.getElementById('average-duration-value');
const muscleGroups = await withErrorToasts(getAllMuscles);
let selectedMuscle;

await setupOverview();
await setupMuscleOverview();

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

async function setupMuscleOverview() {
    const muscleSelect = document.getElementById('muscle-select');
    const muscleTemplate = document.getElementById('muscle-select-template');
    if (muscleGroups === undefined) return;
    
    for (const muscleGroup of muscleGroups) {
        const option = muscleTemplate.content.firstElementChild.cloneNode(true);
        option.dataset.muscleId = muscleGroup.id;
        option.textContent = muscleGroup.title;
        option.value = muscleGroup.id;
        
        muscleSelect.append(option);
    }


    selectedMuscle = muscleSelect.value;
    await renderMuscleOverview();
    
    muscleSelect.addEventListener('change', async (e) => {
        e.preventDefault();
        selectedMuscle = muscleSelect.value;
        await renderMuscleOverview();
    });
}

async function renderMuscleOverview() {
    const overview = await withErrorToasts(() => getMuscleGroupOverview(selectedMuscle));

    if (overview == null) return;

    setStat('muscle-week-volume', formatNumber(overview.lastWeekVolume), overview.lastWeekVolumeIncrease, '%');
    setStat('muscle-month-volume', formatNumber(overview.lastMonthVolume), overview.lastMonthVolumeIncreasePercents, '%');
    setStat('muscle-week-sets', overview.lastWeekSets, overview.lastWeekSetsIncrease);

    document.getElementById('muscle-average-volume-value').textContent = formatNumber(overview.averageWeekVolume);
    document.getElementById('muscle-average-sets-value').textContent = overview.averageWeekSets;
}

function setStat(prefix, value, increase, suffix = '') {
    document.getElementById(`${prefix}-value`).textContent = value;

    const delta = document.getElementById(`${prefix}-delta`);
    delta.textContent = formatIncrease(increase) + suffix;
    delta.classList.toggle('stat-delta-down', increase <= 0);
}

function formatNumber(value) {
    return value.toLocaleString('ru-RU');
}

function formatIncrease(increase) {
    return increase > 0 ? `+${increase}` : String(increase);
}

function formatDuration(totalMinutes) {
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    if (hours === 0) return String(minutes);

    return `${hours} h ${String(minutes).padStart(2, '0')}`;
}
