
const getButton = document.getElementById("getButton");
const text = document.getElementById("infoText");

getButton.addEventListener("click", async (e) => {
    e.preventDefault();
    const data = await getData("http://localhost:8080/api/biceps");
    text.textContent = data.title;
});

async function getData(url = "", data = {}) {
    const response = await fetch(url, {
        method: "GET",
    });
    return response.json();
}