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

export class HttpError extends Error {
    constructor(status, body) {
        super(`ERROR ${status}`);
        this.status = status;
        this.body = body;
    }
}