import {getSessions, logout, me} from "../api/auth-api.js";
await setupLogout();
await setupProfile();
async function setupLogout() {
    const logoutButton = document.getElementById('logout-btn');
    
    logoutButton.addEventListener('click', async (e) => {
        e.preventDefault();
        await logout();
        location.replace('/');
    })
}

async function setupProfile() {
    const meInfo = await me();
    const sessions = await getSessions();
    
    document.getElementById("account-email-title").textContent = meInfo.id;
    document.getElementById("account-email").textContent = meInfo.email;
    document.getElementById("account-role").textContent = meInfo.role;
    document.getElementById("account-sessions").textContent = sessions.length;

    const date = new Date(meInfo.createdAt);
    
    document.getElementById("account-registered").textContent = date.toLocaleDateString("en-GB");
    
    if (meInfo.role === 'admin') {
        document.getElementById("admin-panel-btn").classList.remove('d-none');
    }
}