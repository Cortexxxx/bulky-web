import {defaultError, getErrorMessage} from "./error-messages.js";

export async function postData(path, body) {
    const response = await fetch(path, {
        method: "POST",
        headers: {'Content-Type': 'application/json'},
        body: body
    });

    let responseBody = await getResponseBody(response);
    throwIfNotOk(response, responseBody);
    
    return responseBody;
}

export async function getData(path) {
    const response = await fetch(path, {
        method: "GET"
    });

    let responseBody = await getResponseBody(response);
    throwIfNotOk(response, responseBody);

    return responseBody;
}

async function getResponseBody(response) {
    try {
        return JSON.parse(await response.text())
    } catch (e) {
        return null;
    }
}

function throwIfNotOk(response, responseBody) {
    if (!response.ok) {
        throw new HttpError(response.status, responseBody);
    }
}

export function handleErrors(exception) {
    let errors;

    if (exception instanceof HttpError) {
        if (exception.body != null) {
            errors = Object.values(exception.body).flat().map(getErrorMessage);
        } else {
            errors = [defaultError]
        }
    }
    else {
        errors = [defaultError]
    }
    
    return errors;
}

export class HttpError extends Error {
    constructor(status, body) {
        super(`ERROR ${status}`);
        this.status = status;
        this.body = body;
    }
}
