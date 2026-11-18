import {postData} from "../shared/http.js";

export function createImagePutUrl(entityType, fileType, contentLength) {
    return postData("/api/images/upload-url", JSON.stringify({entityType, fileType, contentLength}));
}
