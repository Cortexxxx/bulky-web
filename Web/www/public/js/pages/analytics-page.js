import {getExerciseOverview, getMuscleGroupOverview, getOverview} from "../api/analytics-api.js";
import {withErrorToasts} from "../shared/ui.js";
import {getAllMuscles} from "../api/muscle-api.js";
import {getAllExercises} from "../api/exercise-api.js";

const averageDurationValue = document.getElementById('average-duration-value');
const muscleGroups = await withErrorToasts(getAllMuscles);
const exercises = await withErrorToasts(getAllExercises);

let selectedMuscle;
let selectedExercise;

let muscleOverview = null;
let muscleMetric = 'volume';

let exerciseOverview = null;
let exerciseMetric = 'volume';

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

    renderWeeklyBars('overview-volume-chart', overview.weeksVolumes, 'kg');
    renderWeeklyBars('overview-sets-chart', overview.weeksSets, 'sets');
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

    const values = isVolume ? muscleOverview.weeksVolumes : muscleOverview.weeksSets;
    renderWeeklyBars('muscle-chart', values, isVolume ? 'kg' : 'sets');

    document.getElementById('muscle-chart-title').textContent = isVolume ? 'Volume by week, kg' : 'Sets by week';
}

function renderExerciseChart() {
    if (exerciseOverview === null) return;

    const isVolume = exerciseMetric === 'volume';

    const values = isVolume ? exerciseOverview.volumes : exerciseOverview.orms.map(value => value === 0 ? null : value);
    renderWeeklyBars('exercise-chart', values, 'kg');

    document.getElementById('exercise-chart-title').textContent = isVolume ? 'Volume by week, kg' : '1RM by week';
}

function renderWeeklyBars(chartId, valuesNewestFirst, unit) {
    const svg = document.getElementById(chartId);
    const values = [...valuesNewestFirst].reverse();
    const plot = {left: 52, right: 472, top: 16, bottom: 216};
    const plotHeight = plot.bottom - plot.top;
    const gridIntervals = 5;
    const gridStep = Math.max(1, Math.ceil(Math.max(0, ...values) / gridIntervals));
    const scaleMax = gridStep * gridIntervals;
    const slotWidth = (plot.right - plot.left) / values.length;
    const barWidth = slotWidth * 0.68;

    svg.replaceChildren();

    for (let index = 0; index <= gridIntervals; index++) {
        const tick = index * gridStep;
        const y = plot.bottom - index / gridIntervals * plotHeight;
        svg.append(
            createSvgElement('line', {
                x1: plot.left, x2: plot.right, y1: y, y2: y,
                stroke: 'var(--line)'
            }),
            createSvgElement('text', {
                x: plot.left - 8, y,
                'text-anchor': 'end',
                'dominant-baseline': 'middle'
            }, formatNumber(tick))
        );
    }

    values.forEach((value, index) => {
        const x = plot.left + slotWidth * index;
        const centerX = x + slotWidth / 2;
        const height = (value ?? 0) / scaleMax * plotHeight;
        const week = values.length - index;
        const tooltip = value === null ? 'No data' : `${formatNumber(value)} ${unit}`;
        const column = createSvgElement('g', {});

        column.append(
            createSvgElement('title', {}, `${week}w · ${tooltip}`),
            createSvgElement('rect', {x, y: plot.top, width: slotWidth, height: plotHeight, fill: 'transparent'}),
            createSvgElement('rect', {
                x: centerX - barWidth / 2, y: plot.bottom - height,
                width: barWidth, height, rx: 3, fill: 'var(--bulky-accent)'
            }),
            createSvgElement('text', {x: centerX, y: plot.bottom + 18, 'text-anchor': 'middle'}, `${week}w`)
        );
        svg.append(column);
    });
}

function createSvgElement(tag, attributes, text) {
    const element = document.createElementNS('http://www.w3.org/2000/svg', tag);
    for (const [name, value] of Object.entries(attributes)) {
        element.setAttribute(name, value);
    }
    if (text !== undefined) element.textContent = text;
    return element;
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
