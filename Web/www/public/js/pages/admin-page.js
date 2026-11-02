import {
    archiveMuscle,
    createMuscle,
    getAllMuscles,
    getMuscleById,
    unarchiveMuscle,
    updateMuscle
} from "../api/muscle-api.js";
import {handleSubmit} from "../shared/http.js";
let selectedId;
let selectedElement;
await setupMuscleList();
await setupCreateForm();
await setupEditForm();

async function setupMuscleList() {
    const muscleList = document.getElementById("muscle-group-list");
    document.addEventListener('DOMContentLoaded', async () => {
        const data = await getAllMuscles();
        await renderElementsList("muscle-element-template", muscleList, data, 'muscle');
    });
}

async function renderElementsList(templateId, list, elements, type) {
    const template = document.getElementById(templateId);
    list.replaceChildren();

    for (const element of elements) {
        const fragment = template.content.cloneNode(true);
        const listElement = fragment.querySelector("li");
        const selectButton = listElement.querySelector("[data-action='select']");
        const archiveButton = listElement.querySelector("[data-action='archive']");
        listElement.dataset.id = element.id;
        selectButton.textContent = element.title;

        if (element.isArchived) {
            listElement.classList.add("archived");
            archiveButton.dataset.action = 'unarchive';
            archiveButton.textContent = "Unarchive";
        } else
        {
            listElement.classList.remove("archived");
            archiveButton.dataset.action = 'archive';
            archiveButton.textContent = "Archive";
        }

        listElement.addEventListener('click', async (e) => {
            e.preventDefault();
            const elementId = e.target.closest("li").dataset.id;
            const pressedButton = e.target.closest("button");

            if (pressedButton.dataset.action === 'select')
            {
                if (type === 'muscle') {
                    selectedId = elementId;
                    
                    if (selectedElement !== undefined) {
                        selectedElement.classList.remove('active')
                    }
                    
                    selectedElement = listElement;
                    selectedElement.classList.add('active')
                    const data = await getMuscleById(selectedId);
                    const editForm = document.getElementById("muscle-group-edit-form");

                    editForm.querySelector('fieldset').disabled = false;

                    editForm.elements.title.value = data.title;
                    editForm.elements.description.value = data.description;
                }
            } else if (pressedButton.dataset.action === 'archive') {
                if (type === 'muscle') {
                    await archiveMuscle(elementId);
                    location.replace("/admin");
                }
            } else if (pressedButton.dataset.action === 'unarchive') {
                if (type === 'muscle') {
                    await unarchiveMuscle(elementId);
                    location.replace("/admin");
                }
            }
        });

        list.append(listElement);
    }
}
async function setupCreateForm() {
    const createForm = document.getElementById("muscle-group-create-form");
    const errorBox = document.getElementById("muscle-group-create-error");

    const titleField = createForm.elements.title;
    const descriptionField = createForm.elements.description;

    createForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const title = titleField.value;
        const description = descriptionField.value;

        await handleSubmit(errorBox, async () => await createMuscle(title, description), "/admin")
    });
}
// тут не работает нормально система. каждый раз при выборе элемента добавляется новый обработчик но старый не пропадает
async function setupEditForm(muscleId) {
    const editForm = document.getElementById("muscle-group-edit-form");
    const errorBox = document.getElementById("muscle-group-edit-error");
    
    const titleField = editForm.elements.title;
    const descriptionField = editForm.elements.description;

    editForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const title = titleField.value;
        const description = descriptionField.value;
        
        await handleSubmit(errorBox, async () => await updateMuscle(selectedId, title, description));
    });
}