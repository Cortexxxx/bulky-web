class HttpError extends Error {
    constructor(status) {
        super(`HTTP ${status}`);
        this.status = status;
    }
}

const getButton = document.getElementById("getButton");
const text = document.getElementById("infoText");
let programCounter = 0;

getButton.addEventListener("click", async (e) => {
    e.preventDefault();

    if (programCounter === 5) {
        programCounter = 0;
    }

    try {
        const data = await getData("/api/programs/" + programCounter);
        text.textContent = data.title;
    } catch (ex) {
        if (ex instanceof HttpError) {
            if (ex.status === 404) {
                text.textContent = "Not found";
            }
        } else {
            text.textContent = "Не удалось загрузить данные";
            console.error(ex);
        }
    } finally {
        programCounter++;
    }
});

async function getData(url) {
    const response = await fetch(url, { method: "GET" });

    if (!response.ok) {
        throw new HttpError(response.status);
    }

    return await response.json();
}