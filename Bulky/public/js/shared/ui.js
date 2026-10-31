export function showErrors(box, messages) {
    box.replaceChildren();
    const list = document.createElement("ul")
    list.className = 'mb-0';
    
    for (const message of messages) {
        const item = document.createElement("li");
        item.textContent = message;
        list.append(item);
    }
    
    box.append(list);
    box.classList.remove('d-none');
}

export function clearErrors(box) {
    box.replaceChildren();
    box.classList.add('d-none');
}   