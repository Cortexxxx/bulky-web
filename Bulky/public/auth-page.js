import {HttpError} from "./http.js";
import {register, me} from "./auth-api.js";
import {getErrorMessage, defaultError} from "./error-messages.js";
import {showErrors, clearErrors} from "./ui.js";

const registerErrors = document.getElementById("register-error")

const registerForm = document.getElementById("register-form");
const emailField = document.getElementById("register-email");
const passwordField = document.getElementById("register-password");
const passwordRepeatField = document.getElementById("register-password-repeat");
registerForm.addEventListener('submit', async (event)=> {
    event.preventDefault();
    await onRegisterSubmit(emailField.value, passwordField.value, passwordRepeatField.value); 
});

console.log(await me());

async function onRegisterSubmit(email, password, passwordRepeat) {
    try {
        clearErrors(registerErrors);
        if (password !== passwordRepeat) {
            showErrors(registerErrors, [getErrorMessage("passwords_mismatch")]);
            return;
        }
        
        await register(email, password);
        location.replace("/account");
    }
    catch (ex) {
        let errors;
        
        if (ex instanceof HttpError) {
            if (ex.body != null || ex.body === "") {
                errors = [Object.values(ex.body)].flat().map(getErrorMessage);
            } else {
                errors = [defaultError]
            }
        } 
        else {
            errors = [defaultError]
        }
        
        showErrors(registerErrors, errors);
    }
}