import {getData, postData} from "../shared/http.js";

export async function getAllMuscles() {
    return await getData("/api/muscle_groups");
}