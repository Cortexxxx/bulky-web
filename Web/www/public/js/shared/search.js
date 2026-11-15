export function searchByTitle(items, query) {
    const searchTerm = query.trim().toLowerCase();
    return Array.from(items).filter(item => item.title.trim().toLowerCase().includes(searchTerm));
}
