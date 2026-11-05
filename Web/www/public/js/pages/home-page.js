import {logout, me} from "../api/auth-api.js";
import {HttpError} from "../shared/http.js";

await setupHome();

async function setupHome() {
    try {
        await me();
    } catch (e) {
        if (e instanceof HttpError && e.status === 401) {
            return;
        }
    }
    
    document.getElementById('authorized-panel').classList.remove("d-none");
    document.getElementById('login-btn').classList.add("d-none");

    const logoutButton = document.getElementById('logout-btn');

    logoutButton.addEventListener('click', async (e) => {
        e.preventDefault();
        await logout();
        location.replace('/');
    })
}