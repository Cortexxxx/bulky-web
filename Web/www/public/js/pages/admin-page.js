import {
    archiveMuscle,
    createMuscle,
    getAllMuscles,
    getMuscleById,
    unarchiveMuscle,
    updateMuscle
} from "../api/muscle-api.js";
import {
    archiveExercise,
    createExercise,
    getAllExercises,
    getExerciseById,
    unarchiveExercise,
    updateExercise
} from "../api/exercise-api.js";
import {handleSubmit} from "../shared/http.js";

const selected = {muscle: null, exercise: null};

const entities = {
    muscle: {
        api: {
            getAll: getAllMuscles,
            get: getMuscleById,
            create: createMuscle,
            update: updateMuscle,
            archive: archiveMuscle,
            unarchive: unarchiveMuscle
        },
        onChange: refreshExerciseMuscles
    },
    exercise: {
        api: {
            getAll: getAllExercises,
            get: getExerciseById,
            create: createExercise,
            update: updateExercise,
            archive: archiveExercise,
            unarchive: unarchiveExercise
        },
        readExtra: (mode) => ({muscleGroups: getSelectedMuscles(getMusclesBox(mode))}),
        fillEdit: (data) => fillMusclesBox(getMusclesBox('edit'), data.muscleGroups)
    }
};

for (const entity of Object.keys(entities)) {
    await setupList(entity);
    setupForm(entity, 'create');
    setupForm(entity, 'edit');
}
await renderMuscleBoxes();


// lists

async function setupList(entity) {
    const list = document.getElementById(`${entity}-list`);
    list.addEventListener('click', (e) => handleListClick(entity, e));
    await reloadList(entity);
}

async function reloadList(entity) {
    const list = document.getElementById(`${entity}-list`);
    const template = document.getElementById(`${entity}-element-template`);
    const elements = await entities[entity].api.getAll(true);

    list.replaceChildren(...elements.map(element => createListItem(entity, template, element)));
}

function createListItem(entity, template, element) {
    const item = template.content.firstElementChild.cloneNode(true);
    const selectButton = item.querySelector("[data-action='select']");
    const archiveButton = item.querySelector("[data-action='archive']");

    item.dataset.id = element.id;
    item.classList.toggle('archived', element.isArchived);
    item.classList.toggle('active', String(element.id) === selected[entity]);
    selectButton.textContent = element.title;
    archiveButton.dataset.action = element.isArchived ? 'unarchive' : 'archive';
    archiveButton.textContent = element.isArchived ? 'Unarchive' : 'Archive';

    return item;
}

async function handleListClick(entity, e) {
    const button = e.target.closest('button[data-action]');
    if (button === null) {
        return;
    }

    const id = button.closest('li').dataset.id;
    const {api} = entities[entity];

    switch (button.dataset.action) {
        case 'select':
            await selectItem(entity, id);
            break;
        case 'archive':
            await api.archive(id);
            await afterChange(entity);
            break;
        case 'unarchive':
            await api.unarchive(id);
            await afterChange(entity);
            break;
    }
}

async function selectItem(entity, id) {
    const {api, fillEdit} = entities[entity];
    const data = await api.get(true, id);
    const list = document.getElementById(`${entity}-list`);
    const form = document.getElementById(`${entity}-edit-form`);

    selected[entity] = id;
    list.querySelector('.active')?.classList.remove('active');
    list.querySelector(`li[data-id="${id}"]`)?.classList.add('active');

    form.querySelector('fieldset').disabled = false;
    form.elements.title.value = data.title;
    form.elements.description.value = data.description;
    fillEdit?.(data);
}

async function afterChange(entity) {
    await reloadList(entity);
    await entities[entity].onChange?.();
}


// forms

function setupForm(entity, mode) {
    const form = document.getElementById(`${entity}-${mode}-form`);
    const errorBox = document.getElementById(`${entity}-${mode}-error`);

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        const id = selected[entity];
        if (mode === 'edit' && id === null) {
            return;
        }

        const {api, readExtra} = entities[entity];
        const data = {
            title: form.elements.title.value,
            description: form.elements.description.value,
            ...readExtra?.(mode)
        };

        await handleSubmit(errorBox, async () => {
            if (mode === 'edit') {
                await api.update(id, data);
            } else {
                await api.create(data);
                form.reset();
            }

            await afterChange(entity);
        });
    });
}


// exercise muscles box

function getMusclesBox(mode) {
    return document.getElementById(`exercise-${mode}-muscles`);
}

async function refreshExerciseMuscles() {
    await renderMuscleBoxes();

    if (selected.exercise !== null) {
        await selectItem('exercise', selected.exercise);
    }
}

async function renderMuscleBoxes() {
    const muscles = await getAllMuscles(true);

    for (const mode of ['create', 'edit']) {
        renderMuscleRows(mode, muscles);
    }
}

function renderMuscleRows(mode, muscles) {
    const template = document.getElementById("exercise-muscles-list-template");
    const box = getMusclesBox(mode);
    box.replaceChildren();

    for (const muscle of muscles) {
        const row = template.content.firstElementChild.cloneNode(true);
        row.dataset.muscleId = muscle.id;
        row.querySelector('span').textContent = muscle.title;

        for (const input of row.querySelectorAll('input')) {
            const inputId = `${mode}-muscle-${muscle.id}-${input.value}`;
            input.name = `${mode}-muscle-${muscle.id}`;
            input.id = inputId;
            input.nextElementSibling.htmlFor = inputId;
        }

        box.append(row);
    }
}

function fillMusclesBox(box, muscleGroups) {
    clearMusclesBox(box);

    for (const group of muscleGroups) {
        const row = box.querySelector(`[data-muscle-id="${group.muscleGroupId}"]`);
        const input = row?.querySelector(`input[value="${group.role}"]`);

        if (input) {
            input.checked = true;
        }
    }
}

function clearMusclesBox(box) {
    for (const row of box.querySelectorAll("[data-muscle-id]")) {
        row.querySelector('input[value="none"]').checked = true;
    }
}

function getSelectedMuscles(box) {
    const result = [];

    for (const row of box.querySelectorAll("[data-muscle-id]")) {
        const role = row.querySelector('input:checked').value;

        if (role !== 'none') {
            result.push({muscleGroupId: row.dataset.muscleId, muscleGroupRole: role});
        }
    }

    return result;
}
