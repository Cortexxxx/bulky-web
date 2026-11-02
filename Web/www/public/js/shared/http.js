import {defaultError, getErrorMessage} from "./error-messages.js";
import {clearErrors, showErrors} from "./ui.js";

export async function postData(path, body = "") {
    const response = await fetch(path, {
        method: "POST",
        headers: {'Content-Type': 'application/json'},
        body: body
    });

    return await handleResponse(response);
}

export async function putData(path, body = "") {
    const response = await fetch(path, {
        method: "PUT",
        headers: {'Content-Type': 'application/json'},
        body: body
    });

    return await handleResponse(response);
}

export async function getData(path) {
    const response = await fetch(path, {
        method: "GET"
    });

    return await handleResponse(response);
}

async function handleResponse(response) {
    let responseBody = await getResponseBody(response);
    
    if (!response.ok) {
        throw new HttpError(response.status, responseBody);
    }

    return responseBody;
}

async function getResponseBody(response) {
    try {
        return JSON.parse(await response.text())
    } catch (e) {
        return null;
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

export async function handleSubmit(errorBox, callback, redirectPath = null){
    try {
        clearErrors(errorBox);
        await callback();
        if (redirectPath !== null) {
            location.replace(redirectPath);

        }
    }
    catch (ex) {
        const errors = handleErrors(ex);
        showErrors(errorBox, errors);
    }
}

export class HttpError extends Error {
    constructor(status, body) {
        super(`ERROR ${status}`);
        this.status = status;
        this.body = body;
    }
}
