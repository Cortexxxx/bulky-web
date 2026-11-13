import {getMuscleGroupOverview, getOverview} from "../api/analytics-api.js";
import {withErrorToasts} from "../shared/ui.js";
import {getAllMuscles} from "../api/muscle-api.js";

const averageDurationValue = document.getElementById('average-duration-value');
const muscleGroups = await withErrorToasts(getAllMuscles);
let selectedMuscle;

const WEEKS = 12;
const dateFormat = new Intl.DateTimeFormat('en-US', {month: 'short', day: 'numeric'});
let overviewVolumeChart, overviewSetsChart, muscleChart;
let muscleOverview = null;      
let muscleMetric = 'volume';

setupChartDefaults();
await setupOverview();
setupMuscleMetricToggle();
await setupMuscleOverview();

async function setupOverview() {
    const overview = await withErrorToasts(getOverview);

    if (overview == null) return;

    setStat('workouts-week', overview.lastWeekWorkouts, overview.lastWeekWorkoutsIncrease);
    setStat('sets-week', overview.lastWeekSets, overview.lastWeekSetsIncrease);
    setStat('workouts-month', overview.lastMonthWorkouts, overview.lastMonthWorkoutsIncrease);
    setStat('sets-month', overview.lastMonthSets, overview.lastMonthSetsIncrease);

    averageDurationValue.textContent = formatDuration(overview.averageDurationMinutes);
    
    overviewVolumeChart ??= createBarChart('overview-volume-chart', 'kg');
    updateBarChart(overviewVolumeChart, overview.weeksVolumes, 'kg');

    overviewSetsChart ??= createBarChart('overview-sets-chart', 'sets');
    updateBarChart(overviewSetsChart, overview.weeksSets, 'sets');
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

    muscleOverview = overview;
    renderMuscleChart();
}

function setupMuscleMetricToggle() {
    for (const radio of document.querySelectorAll('input[name="muscle-metric"]')) {
        radio.addEventListener('change', () => {
            muscleMetric = radio.value;
            renderMuscleChart();
        });
    }
}

function renderMuscleChart() {
    if (muscleOverview === null) return;

    const isVolume = muscleMetric === 'volume';

    muscleChart ??= createBarChart('muscle-chart', 'kg');
    updateBarChart(
        muscleChart,
        isVolume ? muscleOverview.weeksVolumes : muscleOverview.weeksSets,
        isVolume ? 'kg' : 'sets');

    document.getElementById('muscle-chart-title').textContent = isVolume ? 'Volume by week, kg' : 'Sets by week';
}


function cssVar(name) {
    return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
}

function setupChartDefaults() {
    Chart.defaults.font.family = cssVar('--font-body');
    Chart.defaults.font.size = 12;
    Chart.defaults.color = cssVar('--bulky-muted');

    Chart.defaults.plugins.legend.display = false; 

    const tooltip = Chart.defaults.plugins.tooltip;
    tooltip.backgroundColor = cssVar('--bulky-ink');
    tooltip.titleFont = {family: cssVar('--font-body'), weight: '600'};
    tooltip.bodyFont = {family: cssVar('--font-numbers'), weight: '500'};
    tooltip.cornerRadius = 4;
    tooltip.padding = 10;
    tooltip.displayColors = false;


    document.fonts.ready.then(() => {
        for (const chart of [overviewVolumeChart, overviewSetsChart, muscleChart]) {
            chart?.update('none');
        }
    });
}


function weekPeriods() {
    const day = 24 * 60 * 60 * 1000;

    return Array.from({length: WEEKS}, (_, i) => {
        const start = new Date(Date.now() - (WEEKS - i) * 7 * day);
        const end = new Date(start.getTime() + 6 * day);

        return {
            label: dateFormat.format(start),
            range: `${dateFormat.format(start)} – ${dateFormat.format(end)}`
        };
    });
}

function shortNumber(value) {
    return value >= 1000 ? `${value / 1000}k` : value;
}

function createBarChart(canvasId, unit) {
    const periods = weekPeriods();
    const lastIndex = periods.length - 1;
    const accent = cssVar('--bulky-accent');
    const accentPale = cssVar('--accent-pale');

    return new Chart(document.getElementById(canvasId), {
        type: 'bar',
        data: {
            labels: periods.map(p => p.label),
            datasets: [{
                data: [],
                unit,
                backgroundColor: context => context.dataIndex === lastIndex ? accent : accentPale,
                hoverBackgroundColor: cssVar('--bulky-accent-dark'),
                borderRadius: 3,
                maxBarThickness: 40
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false, 
            scales: {
                x: {
                    grid: {display: false},
                    border: {color: cssVar('--line')},
                    ticks: {maxRotation: 0, autoSkipPadding: 12}
                },
                y: {
                    beginAtZero: true,
                    grid: {color: cssVar('--line-soft')},
                    border: {display: false},
                    ticks: {font: {family: cssVar('--font-numbers')}, callback: shortNumber}
                }
            },
            plugins: {
                tooltip: {
                    callbacks: {
                        title: items => periods[items[0].dataIndex].range,
                        label: item => `${item.parsed.y.toLocaleString('ru-RU')} ${item.dataset.unit}`
                    }
                }
            }
        }
    });
}

function updateBarChart(chart, valuesNewestFirst, unit) {
    chart.data.datasets[0].data = valuesNewestFirst.toReversed();
    chart.data.datasets[0].unit = unit;
    chart.update();
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
