import {getData, postData, putData} from "../shared/http.js";

export function getAllMuscles(includeArchived) {
    return includeArchived ? getData("/api/admin/muscle_groups") : getData("/api/muscle_groups");
}

export function getMuscleById(includeArchived, id) {
    return includeArchived ? getData(`/api/admin/muscle_groups/${id}`) : getData(`/api/muscle_groups/${id}`);
}

export function archiveMuscle(id) {
    return postData(`/api/muscle_groups/${id}/archive`);
}

export function unarchiveMuscle(id) {
    return postData(`/api/muscle_groups/${id}/unarchive`);
}

// data: { title, description }
export function createMuscle(data) {
    return postData("/api/muscle_groups", JSON.stringify(data));
}

export function updateMuscle(id, data) {
    return putData(`/api/muscle_groups/${id}`, JSON.stringify(data));
}
