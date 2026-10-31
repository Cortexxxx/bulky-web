import {handleErrors, HttpError} from "../shared/http.js";
import {login, me, register} from "../api/auth-api.js";
import {getErrorMessage} from "../shared/error-messages.js";
import {showErrors, clearErrors} from "../shared/ui.js";

async function setupRegisterForm() {
    const registerForm = document.getElementById("register-form");
    const registerErrors = document.getElementById("register-error");
    const email = registerForm.elements.email.value;
    const password = registerForm.elements.password.value;
    const passwordRepeat = registerForm.elements.passwordRepeat.value;

    registerForm.addEventListener('submit', async (event)=> {
        event.preventDefault();
        
        await handleSubmit(registerErrors,async () => {
            
            if (password !== passwordRepeat) {
                showErrors(registerErrors, [getErrorMessage("passwords_mismatch")]);
                return;
            }

            await register(email, password);
        });
    });
}

async function setupLoginForm() {
    const loginForm = document.getElementById("login-form");
    const loginErrors = document.getElementById("login-error");
    const email = loginForm.elements.email.value;
    const password = loginForm.elements.password.value;

    loginForm.addEventListener('submit', async (event)=> {
        event.preventDefault();

        await handleSubmit(loginErrors,async () => await login(email, password));
    });
}

async function handleSubmit(errorBox, callback){
    try {
        clearErrors(errorBox);
        await callback();
        location.replace("/account");
    }
    catch (ex) {
        const errors = handleErrors(ex);
        showErrors(errorBox, errors);
    }
}

async function redirectIfAuthorized() {
    try {
        if ((await me()) !== null) {
            location.replace("/account");
        }
    } catch (e) {
        if (e instanceof HttpError && e.status === 401) return;
        throw e;
    }
}

await redirectIfAuthorized();
await setupRegisterForm();
await setupLoginForm();
