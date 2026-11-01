import {handleErrors, HttpError} from "../shared/http.js";
import {login, me, register} from "../api/auth-api.js";
import {getErrorMessage} from "../shared/error-messages.js";
import {showErrors, clearErrors} from "../shared/ui.js";

async function setupRegisterForm() {
    const registerForm = document.getElementById("register-form");
    const registerErrors = document.getElementById("register-error");

    const emailField = registerForm.elements.email;
    const passwordField = registerForm.elements.password;
    const passwordRepeatField = registerForm.elements.passwordRepeat;
    
    registerForm.addEventListener('submit', async (event) => {
        event.preventDefault();
        
        const email = emailField.value;
        const password = passwordField.value;
        const passwordRepeat = passwordRepeatField.value;
        
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

    const emailField = loginForm.elements.email;
    const passwordField = loginForm.elements.password;
    
    loginForm.addEventListener('submit', async (event)=> {
        event.preventDefault();
        
        const email = emailField.value;
        const password = passwordField.value;
        
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

await setupRegisterForm();
await setupLoginForm();
