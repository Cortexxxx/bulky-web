
const getButton = document.getElementById("getButton");
const text = document.getElementById("infoText");
let programCounter = 0;

getButton.addEventListener("click", async (e) => {
    let title = "Not found";
    
    try {
        e.preventDefault();

        if (programCounter === 5)
        {
            programCounter = 0;
        }

        const data = await getData("http://localhost:8080/api/programs/" + programCounter);
        title = data.title;
    }
    finally {
        text.textContent = title;
        programCounter++;
    }

});

async function getData(url = "", data = {}) {
    const response = await fetch(url, {
        method: "GET",
    });
    
    if (!response.ok) {
        throw new Error();
    }
    
    return await response.json();
}