import {defaultError, getErrorMessage} from "./error-messages.js";
import {clearErrors, showErrors} from "./ui.js";

async function request(method, path, body) {
    const options = {method};

    if (body !== undefined) {
        options.headers = {'Content-Type': 'application/json'};
        options.body = body;
    }

    return await handleResponse(await fetch(path, options));
}

export function getData(path) {
    return request("GET", path);
}

export function postData(path, body = "") {
    return request("POST", path, body);
}

export function putData(path, body = "") {
    return request("PUT", path, body);
}

async function handleResponse(response) {
    const responseBody = await getResponseBody(response);

    if (!response.ok) {
        throw new HttpError(response.status, responseBody);
    }

    return responseBody;
}

async function getResponseBody(response) {
    try {
        return JSON.parse(await response.text());
    } catch (e) {
        return null;
    }
}

export function handleErrors(exception) {
    if (exception instanceof HttpError && exception.body != null) {
        return Object.values(exception.body).flat().map(getErrorMessage);
    }

    return [defaultError];
}

export async function handleSubmit(errorBox, callback, redirectPath = null) {
    try {
        clearErrors(errorBox);
        await callback();

        if (redirectPath !== null) {
            location.replace(redirectPath);
        }
    } catch (ex) {
        showErrors(errorBox, handleErrors(ex));
    }
}

export class HttpError extends Error {
    constructor(status, body) {
        super(`ERROR ${status}`);
        this.status = status;
        this.body = body;
    }
}
