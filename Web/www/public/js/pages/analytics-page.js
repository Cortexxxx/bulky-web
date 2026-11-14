import {getExerciseOverview, getMuscleGroupOverview, getOverview} from "../api/analytics-api.js";
import {withErrorToasts} from "../shared/ui.js";
import {getAllMuscles} from "../api/muscle-api.js";
import {getAllExercises} from "../api/exercise-api.js";

const averageDurationValue = document.getElementById('average-duration-value');
const muscleGroups = await withErrorToasts(getAllMuscles);
const exercises = await withErrorToasts(getAllExercises);

let selectedMuscle;
let selectedExercise;

let overviewVolumeChart, overviewSetsChart, muscleChart, exerciseChart;

let muscleOverview = null;
let muscleMetric = 'volume';

let exerciseOverview = null;
let exerciseMetric = 'volume';

const weekLabels = Array.from({length: 12}, (_, i) => `${12 - i}w`);

await setupOverview();
setupMuscleMetricToggle();
setupExerciseMetricToggle();
await setupMuscleOverview();
await setupExerciseOverview();

async function setupOverview() {
    const overview = await withErrorToasts(getOverview);

    if (overview == null) return;

    setStat('workouts-week', overview.lastWeekWorkouts, overview.lastWeekWorkoutsIncrease);
    setStat('sets-week', overview.lastWeekSets, overview.lastWeekSetsIncrease);
    setStat('workouts-month', overview.lastMonthWorkouts, overview.lastMonthWorkoutsIncrease);
    setStat('sets-month', overview.lastMonthSets, overview.lastMonthSetsIncrease);

    averageDurationValue.textContent = formatDuration(overview.averageDurationMinutes);

    overviewVolumeChart ??= createBarChart('overview-volume-chart');
    updateBarChart(overviewVolumeChart, overview.weeksVolumes);

    overviewSetsChart ??= createBarChart('overview-sets-chart');
    updateBarChart(overviewSetsChart, overview.weeksSets);
}

async function setupMuscleOverview() {
    const elements = muscleGroups;
    const selector = document.getElementById('muscle-select');
    const selectorTemplate = document.getElementById('muscle-select-template');
    if (elements === undefined) return;

    for (const element of elements) {
        const option = selectorTemplate.content.firstElementChild.cloneNode(true);
        option.dataset.muscleId = element.id;
        option.textContent = element.title;
        option.value = element.id;

        selector.append(option);
    }


    selectedMuscle = selector.value;
    await renderMuscleOverview();

    selector.addEventListener('change', async (e) => {
        e.preventDefault();
        selectedMuscle = selector.value;
        await renderMuscleOverview();
    });
}

async function setupExerciseOverview() {
    const selector = document.getElementById('exercise-select');
    const selectorTemplate = document.getElementById('exercise-select-template');
    if (exercises === undefined) return;

    for (const exercise of exercises) {
        const option = selectorTemplate.content.firstElementChild.cloneNode(true);
        option.dataset.exerciseId = exercise.id;
        option.textContent = exercise.title;
        option.value = exercise.id;

        selector.append(option);
    }


    selectedExercise = selector.value;
    await renderExerciseOverview();

    selector.addEventListener('change', async (e) => {
        e.preventDefault();
        selectedExercise = selector.value;
        await renderExerciseOverview();
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

    muscleOverview = overview;
    renderMuscleChart();
}
async function renderExerciseOverview() {
    const overview = await withErrorToasts(() => getExerciseOverview(selectedExercise));

    if (overview == null) return;
    document.getElementById('exercise-month-orm-delta')
    setStat('exercise-week-orm', formatNumber(overview.lastWeekOrm), overview.lastWeekOrmIncrease);
    setStat('exercise-month-orm', formatNumber(overview.lastMonthOrm), overview.lastMonthOrmIncrease);

    document.getElementById('exercise-max-orm-value').textContent = formatNumber(overview.maxOrm);
    document.getElementById('exercise-max-volume-value').textContent = formatNumber(overview.maxVolume);
    document.getElementById('exercise-max-weight-value').textContent = formatNumber(overview.maxWeight);

    exerciseOverview = overview;
    renderExerciseChart();
}

function setupMuscleMetricToggle() {
    for (const radio of document.querySelectorAll('input[name="muscle-metric"]')) {
        radio.addEventListener('change', () => {
            muscleMetric = radio.value;
            renderMuscleChart();
        });
    }
}

function setupExerciseMetricToggle() {
    for (const radio of document.querySelectorAll('input[name="exercise-metric"]')) {
        radio.addEventListener('change', () => {
            exerciseMetric = radio.value;
            renderExerciseChart();
        });
    }
}

function renderMuscleChart() {
    if (muscleOverview === null) return;

    const isVolume = muscleMetric === 'volume';

    muscleChart ??= createBarChart('muscle-chart');
    updateBarChart(muscleChart, isVolume ? muscleOverview.weeksVolumes : muscleOverview.weeksSets);

    document.getElementById('muscle-chart-title').textContent = isVolume ? 'Volume by week, kg' : 'Sets by week';
}

function renderExerciseChart() {
    if (exerciseOverview === null) return;

    const isVolume = exerciseMetric === 'volume';

    exerciseChart ??= createBarChart('exercise-chart');
    updateBarChart(exerciseChart, isVolume ? exerciseOverview.volumes : exerciseOverview.orms);

    document.getElementById('exercise-chart-title').textContent = isVolume ? 'Volume by week, kg' : '1RM by week';
}

function createBarChart(canvasId) {
    const accent = getComputedStyle(document.documentElement).getPropertyValue('--bulky-accent').trim();

    return new Chart(document.getElementById(canvasId), {
        type: 'bar',
        data: {
            labels: weekLabels,
            datasets: [{data: [], backgroundColor: accent}]
        },
        options: {
            maintainAspectRatio: false, 
            plugins: {legend: {display: false}}
        }
    });
}

function updateBarChart(chart, valuesNewestFirst) {
    chart.data.datasets[0].data = valuesNewestFirst.toReversed();
    chart.update();
}

function setStat(prefix, value, increase, suffix = '') {
    document.getElementById(`${prefix}-value`).textContent = value;

    const delta = document.getElementById(`${prefix}-delta`);
    const trunkIncrease = formatNumber(increase);
    delta.textContent = formatIncrease(increase, trunkIncrease) + suffix;
    delta.classList.toggle('stat-delta-down', increase <= 0);
}

function formatNumber(value) {
    return value.toLocaleString('ru-RU', {
        maximumFractionDigits: 1,
        roundingMode: 'trunc',
    });
}

function formatIncrease(increase, trunkIncrease) {
    return increase > 0 ? `+${trunkIncrease}` : String(trunkIncrease);
}

function formatDuration(totalMinutes) {
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    if (hours === 0) return String(minutes);

    return `${hours} h ${String(minutes).padStart(2, '0')}`;
}
