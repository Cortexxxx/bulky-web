import {getData} from "../shared/http.js";

export function getGuides(category) {
    return getData(`/api/guides?category=${encodeURIComponent(category)}`);
}
