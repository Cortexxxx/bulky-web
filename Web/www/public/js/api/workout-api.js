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

export async function createWorkoutExercise(exerciseId) {
    return await postData(`/api/workouts/current/exercise`, JSON.stringify(exerciseId));
}

export async function deleteWorkoutExercise(id) {
    await deleteData(`/api/workouts/current/exercise/${id}`);
}

export async function createSet(workoutExerciseId) {
    return await postData("/api/workouts/current/sets", JSON.stringify(workoutExerciseId));
}

export async function deleteSet(id) {
    await deleteData(`/api/workouts/current/sets/${id}`);
}