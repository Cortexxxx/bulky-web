export const errorMessages = {
    // Auth: username
    empty_username: "Please enter a username.",
    short_username: "Username must be at least 3 characters long.",
    long_username: "Username is too long (256 characters max).",

    // Auth: email
    empty_email: "Please enter your email.",
    long_email: "Email is too long.",
    incorrect_email: "Please enter a valid email address.",
    email_exists: "This email is already registered.",

    // Auth: password
    empty_password: "Please enter a password.",
    short_password: "Password must be at least 15 characters long.",
    long_password: "Password is too long.",
    passwords_mismatch: "Passwords do not match.",

    // Auth: login, session, access
    wrong_email_or_password: "Wrong email or password.",
    unauthorized: "Please log in to continue.",
    forbidden: "You don't have permission to do this.",
    already_authenticated: "You are already logged in.",
    unknown_user: "User not found.",

    // Title and description (workouts, exercises, muscle groups)
    empty_title: "Please enter a title.",
    short_title: "Title must be at least 3 characters long.",
    long_title: "Title is too long (256 characters max).",
    null_description: "Description is required.",
    long_description: "Description is too long (500 characters max).",

    // Exercises and muscle groups
    exercise_exists: "An exercise with this title already exists.",
    unknown_exercise: "Exercise not found.",
    muscle_group_exists: "A muscle group with this title already exists.",
    unknown_muscle_group: "Muscle group not found.",
    empty_muscles: "Select at least one muscle group.",
    muscle_repeat: "The same muscle group is selected more than once.",
    null_muscle: "Muscle group is invalid.",
    empty_muscle_id: "Select a muscle group.",
    invalid_role: "Invalid muscle group role.",
    unsupported_image_type: "Choose a JPEG, PNG, WebP, GIF, or AVIF image.",
    unsupported_image_entity_type: "Image uploads are unavailable for this item.",

    // Workouts
    no_current_workout: "You don't have an active workout.",
    not_active_workout: "You don't have an active workout.",
    workout_exercise_not_found: "Exercise not found in this workout.",

    // Sets
    set_not_found: "Set not found.",
    invalid_repeats: "Reps can't be negative.",
    too_many_repeats: "Too many reps (1000 max).",
    invalid_weight: "Weight can't be negative.",
    too_heavy_weight: "Weight is too large (999 kg max).",

    // Request errors
    bad_request: "Invalid request.",
    null_url: "Invalid request address.",
    body_required: "The request is empty.",
    not_null_required: "The request is empty.",
    invalid_json: "The request has an invalid format.",
    long_payload: "The request is too large.",
    not_found: "Not found.",
    method_not_allowed: "This action is not allowed.",

    unknown_error: "Something went wrong. Please try again."
};

export const defaultError = "Something went wrong";

export function getErrorMessage(error) {
    return errorMessages[error] ?? errorMessages.unknown_error;
}
