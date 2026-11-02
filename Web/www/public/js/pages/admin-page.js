import {
    archiveMuscle,
    createMuscle,
    getAllMuscles,
    getMuscleById,
    unarchiveMuscle,
    updateMuscle
} from "../api/muscle-api.js";
import {handleSubmit} from "../shared/http.js";
async function setupMuscleList() {
    const muscleList = document.getElementById("muscle-group-list");
    document.addEventListener('DOMContentLoaded', async () => {
        const data = await getAllMuscles();
        await renderElementsList("muscle-element-template", muscleList, data, 'muscle');
    });
}

await setupMuscleList();

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
            archiveButton.dataset.action = 'unarchive';
            archiveButton.textContent = "Archive";
        }

        listElement.addEventListener('click', async (e) => {
            e.preventDefault();
            const elementId = e.target.closest("li").dataset.id;
            const pressedButton = e.target.closest("button");

            if (pressedButton.dataset.action === 'select')
            {
                if (type === 'muscle') {
                    await enableMuscleEditForm(elementId);
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

    const titleField = editForm.elements.title;
    const descriptionField = editForm.elements.description;

    createForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const title = titleField.value;
        const description = descriptionField.value;

        await handleSubmit(errorBox, async () => await createMuscle(title, description), "/admin")
    });
}

async function enableMuscleEditForm(muscleId) {
    const editForm = document.getElementById("muscle-group-edit-form");
    const errorBox = document.getElementById("muscle-group-edit-error");
    
    editForm.querySelector('fieldset').disabled = false;
    
    const titleField = editForm.elements.title;
    const descriptionField = editForm.elements.description;
    const muscle = await getMuscleById(muscleId);
    titleField.value = muscle.title;
    descriptionField.value = muscle.value;

    editForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const title = titleField.value;
        const description = descriptionField.value;
        
        await handleSubmit(errorBox, async () => await updateMuscle(muscleId, title, description), "/admin")
    });
}