import {getData, postData, putData} from "../shared/http.js";

export function getAllExercises(includeArchived) {
    return includeArchived ? getData("/api/admin/exercises") : getData("/api/exercises");
}

export function getExerciseById(includeArchived, id) {
    return includeArchived ? getData(`/api/admin/exercises/${id}`) : getData(`/api/exercises/${id}`);
}

export function archiveExercise(id) {
    return postData(`/api/exercises/${id}/archive`);
}

export function unarchiveExercise(id) {
    return postData(`/api/exercises/${id}/unarchive`);
}

// data: { title, description, muscleGroups }
export function createExercise(data) {
    return postData("/api/exercises", JSON.stringify(data));
}

export function updateExercise(id, data) {
    return putData(`/api/exercises/${id}`, JSON.stringify(data));
}
