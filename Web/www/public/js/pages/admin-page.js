import {getAllMuscles} from "../api/muscle-api.js";

async function setupMuscleList() {
    const muscleList = document.getElementById("muscle-group-list");
    document.addEventListener('DOMContentLoaded', async () => {
        const data = await getAllMuscles();
        const template = document.getElementById("muscle-element-template");
        muscleList.replaceChildren();
        
        for (const muscle of data) {
            const fragment = template.content.cloneNode(true);
            const listElement = fragment.querySelector("li");
            const selectButton = listElement.querySelector("[data-action='select']");
            const archiveButton = listElement.querySelector("[data-action='archive']");
            listElement.dataset.id = muscle.id;
            selectButton.textContent = muscle.title;
            
            if (muscle.isArchived) {
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
                 const muscleId = e.target.closest("li").dataset.id;
                 const pressedButton = e.target.closest("button");
                 
                 if (pressedButton.dataset.action === 'select')
                 {
                     console.log(`Selected muscle with id ${muscleId}`)
                 } else if (pressedButton.dataset.action === 'archive') {
                     console.log(`You try to archive ${muscleId}`)
                 } else if (pressedButton.dataset.action === 'unarchive') {
                     console.log(`You try to unarchive ${muscleId}`)
                 }
            });

            muscleList.append(listElement);
        }
    });
}

await setupMuscleList();