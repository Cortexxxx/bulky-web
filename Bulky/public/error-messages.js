export const errorMessages = {
    empty_email: "Please enter your email.",
    long_email: "Email is too long.",
    incorrect_email: "Please enter a valid email address.",
    email_exists: "This email is already registered.",

    empty_password: "Please enter a password.",
    short_password: "Password must be at least 15 characters long.",
    long_password: "Password is too long.",
    passwords_mismatch: "Passwords do not match.",
    
    wrong_email_or_password: "Wrong email or password.",
    
    unknown_error: "Something went wrong. Please try again."
};

export const defaultError = "Something went wrong";

export function getErrorMessage(error) {
    return errorMessages[error] ?? errorMessages.unknown_error;
}