import {getData, postData, putData} from "../shared/http.js";

export async function getAllMuscles() {
    return await getData("/api/muscle_groups");
}

export async function getMuscleById(id) {
    return await getData(`/api/muscle_groups/${id}`)
}

export async function archiveMuscle(id) {
    return await postData(`/api/muscle_groups/${id}/archive`);
}

export async function unarchiveMuscle(id) {
    return await postData(`/api/muscle_groups/${id}/archive`);
}

export async function createMuscle(title, description) {
    return await postData(`/api/muscle_groups`, buildMuscleRequestBody(title, description));
}

export async function updateMuscle(id, title, description) {
    return await putData(`/api/muscle_groups/${id}/`, buildMuscleRequestBody(title, description));
}

function buildMuscleRequestBody(title, description) {
    return JSON.stringify(
        {
            title: title,
            description: description
        }
    );
}