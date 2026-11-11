import {getData} from "../shared/http.js";

export async function getOverview() {
    return await getData("/api/analytics/overview");
}
