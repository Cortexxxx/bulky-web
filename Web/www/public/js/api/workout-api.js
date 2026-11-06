import {deleteData, getData, postData} from "../shared/http.js";

export async function getCurrentWorkout() {
    return await getData("/api/workouts/current");
}

export async function getAllWorkouts() {
    return await getData("/api/workouts");
}

export async function endCurrentWorkout() {
    await postData("/api/workouts/current/end");
}

export async function deleteWorkout(id) {
    await deleteData(`/api/workouts/${id}`);
}

export async function createWorkout(title) {
    return await postData(`/api/workouts`, JSON.stringify({
        title: title
    }));
}