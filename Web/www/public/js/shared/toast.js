const types = {
    error: {className: "app-toast-error", delay: 6000},
    success: {className: "app-toast-success", delay: 3500},
    info: {className: "app-toast-info", delay: 4000}
};

export function showToast(message, type = "info") {
    const container = document.getElementById("toast-container");
    const template = document.getElementById("toast-template");

    if (container === null || template === null) {
        console.error("Toast markup is missing on this page ({{toasts}}):", message);
        return;
    }

    const {className, delay} = types[type] ?? types.info;

    const toastElement = template.content.firstElementChild.cloneNode(true);
    toastElement.classList.add(className);
    toastElement.querySelector(".toast-body").textContent = message;

    container.append(toastElement);

    toastElement.addEventListener("hidden.bs.toast", () => toastElement.remove());
    bootstrap.Toast.getOrCreateInstance(toastElement, {delay}).show();
}

export function showErrorToasts(messages) {
    for (const message of messages) {
        showToast(message, "error");
    }
}
