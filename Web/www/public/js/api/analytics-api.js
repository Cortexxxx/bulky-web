import {getData} from "../shared/http.js";

export async function getOverview() {
    return await getData("/api/analytics/overview");
}

export async function getMuscleGroupOverview(id) {
    return await getData(`/api/analytics/muscle/${id}`);
}

export async function getExerciseOverview(id) {
    return await getData(`/api/analytics/exercise/${id}`);
}
