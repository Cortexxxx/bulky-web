import {postData, getData} from '../shared/http.js';

export async function register(email, password) {
    const requestBody = JSON.stringify({
        email: email,
        password: password
    })

    return await postData("/api/auth/register", requestBody);
}

export async function login(email, password) {
    const requestBody = JSON.stringify({
        email: email,
        password: password
    })

    return await postData("/api/auth/login", requestBody);
}

export async function me(){
    return await getData("api/auth/me");
}
